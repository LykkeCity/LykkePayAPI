using AutoMapper;
using Common;
using Common.Log;
using Lykke.Service.EthereumCore.Client.Models;
using Lykke.Service.PayAPI.Core.Domain.Invoice;
using Lykke.Service.PayAPI.Core.Domain.PayHistory;
using Lykke.Service.PayAPI.Core.Exceptions;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayHistory.Client;
using Lykke.Service.PayHistory.Client.AutorestClient.Models;
using Lykke.Service.PayInvoice.Client;
using MoreLinq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.PayAPI.Core.Settings.ServiceSettings;

namespace Lykke.Service.PayAPI.Services
{
    public class PayHistoryService : IPayHistoryService
    {
        private const int BatchPieceSize = 15;

        private readonly IPayHistoryClient _payHistoryClient;
        private readonly IMerchantService _merchantService;
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly IExplorerUrlResolver _explorerUrlResolver;
        private readonly IEthereumCoreClient _ethereumCoreClient;
        private readonly IIataService _iataService;
        private readonly IHistoryOperationTitleProvider _historyOperationTitleProvider;
        private readonly string _merchantDefaultLogoUrl;
        private readonly ILog _log;

        public PayHistoryService(IPayHistoryClient payHistoryClient, ILog log,
            IMerchantService merchantService, IPayInvoiceClient payInvoiceClient,
            IExplorerUrlResolver explorerUrlResolver, IEthereumCoreClient ethereumCoreClient,
            IIataService iataService, IHistoryOperationTitleProvider historyOperationTitleProvider,
            string merchantDefaultLogoUrl)
        {
            _payHistoryClient = payHistoryClient;
            _merchantService = merchantService;
            _payInvoiceClient = payInvoiceClient;
            _explorerUrlResolver = explorerUrlResolver;
            _ethereumCoreClient = ethereumCoreClient;
            _iataService = iataService;
            _historyOperationTitleProvider = historyOperationTitleProvider;
            _merchantDefaultLogoUrl = merchantDefaultLogoUrl;
            _log = log;
        }

        public async Task<IReadOnlyList<HistoryOperationView>> GetHistoryAsync(string merchantId)
        {
            var historyOperations = (await _payHistoryClient.GetHistoryAsync(merchantId)).ToArray();

            return await GetHistoryAsync(historyOperations);
        }

        public async Task<IReadOnlyList<HistoryOperationView>> GetHistoryByInvoiceAsync(string invoiceId)
        {
            var historyOperations = (await _payHistoryClient.GetHistoryByInvoiceAsync(invoiceId)).ToArray();

            return await GetHistoryAsync(historyOperations);
        }

        private async Task<IReadOnlyList<HistoryOperationView>> GetHistoryAsync(
            HistoryOperationViewModel[] historyOperations, string merchantId = null)
        {
            var merchantLogosTask = GetMerchantLogosAsync(historyOperations, merchantId);
            var iataSpecificDataTask = GetIataSpecificDataAsync(historyOperations);
            var titlesTask = GetTitlesAsync(historyOperations);
            await Task.WhenAll(merchantLogosTask, iataSpecificDataTask, titlesTask);

            var results = new List<HistoryOperationView>();
            foreach (var historyOperation in historyOperations)
            {
                var result = Mapper.Map<HistoryOperationView>(historyOperation);

                string logoKey = GetLogoKey(historyOperation);
                result.MerchantLogoUrl = merchantLogosTask.Result[logoKey];
                result.Title = titlesTask.Result[historyOperation.Id];
                result.Amount = GetAmount(historyOperation.Type, historyOperation.Amount);

                if (!string.IsNullOrEmpty(historyOperation.InvoiceId))
                {
                    result.SettlementMonthPeriod =
                        iataSpecificDataTask.Result[historyOperation.InvoiceId]?.SettlementMonthPeriod;
                    result.IataInvoiceDate =
                        ParseIataInvoiceDate(iataSpecificDataTask.Result[historyOperation.InvoiceId]?.IataInvoiceDate);
                }

                results.Add(result);
            }

            return results;
        }

        private decimal GetAmount(PayHistory.Client.AutorestClient.Models.HistoryOperationType type, double amount)
        {
            switch (type)
            {
                case PayHistory.Client.AutorestClient.Models.HistoryOperationType.CashOut:
                case PayHistory.Client.AutorestClient.Models.HistoryOperationType.OutgoingInvoicePayment:
                case PayHistory.Client.AutorestClient.Models.HistoryOperationType.OutgoingExchange:
                    return (decimal) -amount;
                default:
                    return (decimal) amount;
            }
        }

        private string GetLogoKey(HistoryOperationViewModel historyOperation)
        {
            return historyOperation.OppositeMerchantId ?? string.Empty;
        }

        private async Task<IDictionary<string, string>> GetMerchantLogosAsync(
            HistoryOperationViewModel[] historyOperations, string merchantId = null)
        {
            var merchantIds = historyOperations.Select(GetLogoKey).Where(k => !string.IsNullOrEmpty(k))
                .Distinct().ToList();

            if (!string.IsNullOrEmpty(merchantId)
                && !merchantIds.Contains(merchantId, StringComparer.OrdinalIgnoreCase))
            {
                merchantIds.Add(merchantId);
            }

            var results = new ConcurrentDictionary<string, string>();
            foreach (var batch in merchantIds.Batch(BatchPieceSize))
            {
                await Task.WhenAll(batch.Select(id =>
                    _merchantService.GetMerchantLogoUrlAsync(id).ContinueWith(t => results[id] = t.Result)));
            }

            results[string.Empty] = _merchantDefaultLogoUrl;

            return results;
        }

        private async Task<IDictionary<string, InvoiceIataSpecificData>> GetIataSpecificDataAsync(
            HistoryOperationViewModel[] historyOperations)
        {
            var invoiceIds = historyOperations.Where(o => !string.IsNullOrEmpty(o.InvoiceId)).Select(o => o.InvoiceId)
                .Distinct();

            var results = new ConcurrentDictionary<string, InvoiceIataSpecificData>();
            foreach (var batch in invoiceIds.Batch(BatchPieceSize))
            {
                await Task.WhenAll(batch.Select(id =>
                    _iataService.GetIataSpecificDataAsync(id).ContinueWith(t => results[id] = t.Result)));
            }

            return results;
        }

        private async Task<IDictionary<string, string>> GetTitlesAsync(HistoryOperationViewModel[] historyOperations)
        {
            var results = new ConcurrentDictionary<string, string>();
            foreach (var batch in historyOperations.Batch(BatchPieceSize))
            {
                await Task.WhenAll(batch.Select(o =>
                    _historyOperationTitleProvider.GetTitleAsync(o.AssetId, o.Type.ToString())
                        .ContinueWith(t => results[o.Id] = t.Result)));
            }

            return results;
        }

        private DateTime? ParseIataInvoiceDate(string iataInvoiceDate)
        {
            if (DateTime.TryParseExact(iataInvoiceDate, "yyyy-MM-dd", CultureInfo.InvariantCulture.DateTimeFormat,
                DateTimeStyles.None, out var result))
            {
                return result;
            }

            return null;
        }

        public async Task<HistoryOperation> GetDetailsAsync(string merchantId, string id)
        {
            var historyOperation = await _payHistoryClient.GetDetailsAsync(merchantId, id);
            if (historyOperation == null)
            {
                return null;
            }

            var result = Mapper.Map<HistoryOperation>(historyOperation);
            try
            {
                FillEmployeeEmail(historyOperation, result);
                result.Amount = GetAmount(historyOperation.Type, historyOperation.Amount);

                await Task.WhenAll(FillTitleAsync(result),
                    FillFromMerchantServiceAsync(historyOperation, result),
                    FillByInvoiceAsync(result, historyOperation.InvoiceId),
                    FillByTxHashAsync(result));
            }
            catch (EthereumCoreApiException ex)
            {
                await _log.WriteErrorAsync(nameof(PayHistoryService), nameof(GetDetailsAsync),
                    new {historyOperation.TxHash}.ToJson(), ex);
                return null;
            }
            catch (Exception ex)
            {
                var apiEx = ex.InnerException as Refit.ApiException;
                if (apiEx?.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw;
            }

            return result;
        }

        public async Task<HistoryOperation> GetLatestPaymentDetailsAsync(string merchantId, string invoiceId)
        {
            IEnumerable<HistoryOperationViewModel> invoiceOperations =
                await _payHistoryClient.GetHistoryByInvoiceAsync(invoiceId);

            var filteredOperations = invoiceOperations.Where(x =>
                x.Type == ClientHistoryOperationType.OutgoingInvoicePayment &&
                x.InvoiceStatus == InvoiceStatus.Paid.ToString());

            HistoryOperationViewModel operation = filteredOperations.MaxBy(x => x.CreatedOn);

            if (operation == null) return null;

            return await GetDetailsAsync(merchantId, operation.Id);
        }

        private void FillEmployeeEmail(HistoryOperationModel model, HistoryOperation result)
        {
            if (model.Type == PayHistory.Client.AutorestClient.Models.HistoryOperationType.CashOut)
            {
                result.RequestedBy = model.EmployeeEmail;
            }
            else if (string.IsNullOrEmpty(model.InvoiceId))
            {
                result.SoldBy = model.EmployeeEmail;
            }
            else
            {
                result.PaidBy = model.EmployeeEmail;
            }
        }

        private async Task FillTitleAsync(HistoryOperation historyOperation)
        {
            historyOperation.Title = await
                _historyOperationTitleProvider.GetTitleAsync(historyOperation.AssetId, historyOperation.Type);
        }

        private async Task FillFromMerchantServiceAsync(HistoryOperationModel model, HistoryOperation result)
        {
            string detailsMerchantId = string.IsNullOrEmpty(model.OppositeMerchantId)
                ? model.MerchantId
                : model.OppositeMerchantId;

            result.MerchantName = await _merchantService.GetMerchantNameAsync(detailsMerchantId);
            result.MerchantLogoUrl = string.IsNullOrEmpty(model.OppositeMerchantId)
                ? _merchantDefaultLogoUrl
                : await _merchantService.GetMerchantLogoUrlAsync(model.OppositeMerchantId);
        }

        private async Task FillByInvoiceAsync(HistoryOperation historyOperation, string invoiceId)
        {
            if (string.IsNullOrEmpty(invoiceId))
            {
                return;
            }

            var invoiceTask = _payInvoiceClient.GetInvoiceAsync(invoiceId);
            var iataSpecificDataTask = _iataService.GetIataSpecificDataAsync(invoiceId);
            await Task.WhenAll(invoiceTask, iataSpecificDataTask);

            historyOperation.InvoiceNumber = invoiceTask.Result?.Number;
            historyOperation.BillingCategory = invoiceTask.Result?.BillingCategory;
            historyOperation.SettlementMonthPeriod = iataSpecificDataTask.Result?.SettlementMonthPeriod;
            historyOperation.IataInvoiceDate = ParseIataInvoiceDate(iataSpecificDataTask.Result?.IataInvoiceDate);
        }

        private async Task FillByTxHashAsync(HistoryOperation historyOperation)
        {
            if (string.IsNullOrEmpty(historyOperation.TxHash))
            {
                return;
            }

            Task<CurrentBlockModel> getBlockTask = _ethereumCoreClient.GetBlockAsync();
            Task<TransactionResponse> getTransactionTask =
                _ethereumCoreClient.GetTransactionAsync(historyOperation.TxHash);
            await Task.WhenAll(getBlockTask, getTransactionTask);

            historyOperation.ExplorerUrl = _explorerUrlResolver.GetExplorerUrl(historyOperation.TxHash);
            historyOperation.BlockHeight = getTransactionTask.Result.BlockNumber;
            historyOperation.BlockConfirmations = getBlockTask.Result.LatestBlockNumber - historyOperation.BlockHeight;
            historyOperation.TimeStamp = getTransactionTask.Result.BlockTimeUtc;
        }
    }
}
