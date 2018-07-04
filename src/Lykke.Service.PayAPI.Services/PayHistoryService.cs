using AutoMapper;
using Common;
using Common.Log;
using Lykke.Service.EthereumCore.Client.Models;
using Lykke.Service.PayAPI.Core.Domain.PayHistory;
using Lykke.Service.PayAPI.Core.Exceptions;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayHistory.Client;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.PayAPI.Core.Domain.Invoice;
using Lykke.Service.PayHistory.Client.AutorestClient.Models;
using MoreLinq;
using System.Collections.Concurrent;

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
        private readonly ILog _log;

        public PayHistoryService(IPayHistoryClient payHistoryClient, ILog log,
            IMerchantService merchantService, IPayInvoiceClient payInvoiceClient,
            IExplorerUrlResolver explorerUrlResolver, IEthereumCoreClient ethereumCoreClient,
            IIataService iataService, IHistoryOperationTitleProvider historyOperationTitleProvider)
        {
            _payHistoryClient = payHistoryClient;
            _merchantService = merchantService;
            _payInvoiceClient = payInvoiceClient;
            _explorerUrlResolver = explorerUrlResolver;
            _ethereumCoreClient = ethereumCoreClient;
            _iataService = iataService;
            _historyOperationTitleProvider = historyOperationTitleProvider;
            _log = log;
        }

        public async Task<IReadOnlyList<HistoryOperationView>> GetHistoryAsync(string merchantId)
        {
            var historyOperations = (await _payHistoryClient.GetHistoryAsync(merchantId)).ToArray();

            var merchantLogosTask = GetMerchantLogosAsync(merchantId, historyOperations);
            var iataSpecificDataTask = GetIataSpecificDataAsync(historyOperations);
            var titlesTask = GetTitlesAsync(historyOperations);
            await Task.WhenAll(merchantLogosTask, iataSpecificDataTask, titlesTask);

            var results = new List<HistoryOperationView>();
            foreach (var historyOperation in historyOperations)
            {
                var result = Mapper.Map<HistoryOperationView>(historyOperation);

                string logoKey = string.IsNullOrEmpty(historyOperation.OppositeMerchantId)
                    ? merchantId
                    : historyOperation.OppositeMerchantId;


                result.MerchantLogoUrl = merchantLogosTask.Result[logoKey];
                result.Title = titlesTask.Result[historyOperation.Id];

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

        private async Task<IDictionary<string,string>> GetMerchantLogosAsync(string merchantId, HistoryOperationViewModel[] historyOperations)
        {
            var merchantIds = historyOperations.Select(o => o.OppositeMerchantId).Where(id => !string.IsNullOrEmpty(id))
                .Distinct().ToList();
            if (!merchantIds.Contains(merchantId, StringComparer.OrdinalIgnoreCase))
            {
                merchantIds.Add(merchantId);
            }

            var results = new ConcurrentDictionary<string, string>();
            foreach (var batch in merchantIds.Batch(BatchPieceSize))
            {
                await Task.WhenAll(batch.Select(id =>
                    _merchantService.GetMerchantLogoUrlAsync(id).ContinueWith(t => results[id] = t.Result)));
            }

            return results;
        }

        private async Task<IDictionary<string, InvoiceIataSpecificData>> GetIataSpecificDataAsync(HistoryOperationViewModel[] historyOperations)
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

            string detailsMerchantId = string.IsNullOrEmpty(historyOperation.OppositeMerchantId)
                ? merchantId
                : historyOperation.OppositeMerchantId;            

            try
            {
                FillEmployeeEmail(historyOperation, result);
                await Task.WhenAll(FillTitleAsync(result),
                    FillFromMerchantServiceAsync(result, detailsMerchantId),
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

        private void FillEmployeeEmail(HistoryOperationModel model, HistoryOperation result)
        {
            if (model.Type == PayHistory.Client.AutorestClient.Models.HistoryOperationType.CashOut)
            {
                result.RequestedBy = model.EmployeeEmail;
            }
            else if(string.IsNullOrEmpty(model.InvoiceId))
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

        private async Task FillFromMerchantServiceAsync(HistoryOperation historyOperation, string merchantId)
        {
            var getMerchantNameTask = _merchantService.GetMerchantNameAsync(merchantId);
            var getMerchantLogoUrlTask = _merchantService.GetMerchantLogoUrlAsync(merchantId);

            await Task.WhenAll(getMerchantNameTask, getMerchantLogoUrlTask);

            historyOperation.MerchantName = getMerchantNameTask.Result;
            historyOperation.MerchantLogoUrl = getMerchantLogoUrlTask.Result;
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
