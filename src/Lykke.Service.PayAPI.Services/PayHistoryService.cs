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
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Lykke.Service.PayAPI.Services
{
    public class PayHistoryService : IPayHistoryService
    {
        private readonly IPayHistoryClient _payHistoryClient;
        private readonly IMerchantService _merchantService;
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly IExplorerUrlResolver _explorerUrlResolver;
        private readonly IEthereumCoreClient _ethereumCoreClient;
        private readonly ILog _log;

        public PayHistoryService(IPayHistoryClient payHistoryClient, ILog log,
            IMerchantService merchantService, IPayInvoiceClient payInvoiceClient,
            IExplorerUrlResolver explorerUrlResolver, IEthereumCoreClient ethereumCoreClient)
        {
            _payHistoryClient = payHistoryClient;
            _merchantService = merchantService;
            _payInvoiceClient = payInvoiceClient;
            _explorerUrlResolver = explorerUrlResolver;
            _ethereumCoreClient = ethereumCoreClient;
            _log = log;
        }

        public async Task<IReadOnlyList<HistoryOperationView>> GetHistoryAsync(string merchantId)
        {
            var historyOperations = (await _payHistoryClient.GetHistoryAsync(merchantId)).ToArray();

            var merchantIds = historyOperations.Select(o => o.OppositeMerchantId).Where(id => !string.IsNullOrEmpty(id))
                .Distinct().ToList();
            if (!merchantIds.Contains(merchantId, StringComparer.OrdinalIgnoreCase))
            {
                merchantIds.Add(merchantId);
            }

            var merchantLogoUrlTasks =
                merchantIds.ToDictionary(id => id, id => _merchantService.GetMerchantLogoUrlAsync(id));
            await Task.WhenAll(merchantLogoUrlTasks.Values);

            var results = new List<HistoryOperationView>();
            foreach (var historyOperation in historyOperations)
            {
                var result = Mapper.Map<HistoryOperationView>(historyOperation);

                string logoKey = string.IsNullOrEmpty(historyOperation.OppositeMerchantId)
                    ? merchantId
                    : historyOperation.OppositeMerchantId;

                result.MerchantLogoUrl = merchantLogoUrlTasks[logoKey].Result;
                results.Add(result);
            }

            return results;
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
                await Task.WhenAll(FillFromMerchantServiceAsync(result, detailsMerchantId),
                    FillFromPayInvoiceAsync(result, historyOperation.InvoiceId),
                    FillFromEthereumCoreAsync(result));
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

        private async Task FillFromMerchantServiceAsync(HistoryOperation historyOperation, string merchantId)
        {
            var getMerchantNameTask = _merchantService.GetMerchantNameAsync(merchantId);
            var getMerchantLogoUrlTask = _merchantService.GetMerchantLogoUrlAsync(merchantId);

            await Task.WhenAll(getMerchantNameTask, getMerchantLogoUrlTask);

            historyOperation.MerchantName = getMerchantNameTask.Result;
            historyOperation.MerchantLogoUrl = getMerchantLogoUrlTask.Result;
        }

        private async Task FillFromPayInvoiceAsync(HistoryOperation historyOperation, string invoiceId)
        {
            if (string.IsNullOrEmpty(invoiceId))
            {
                return;
            }

            InvoiceModel invoice = await _payInvoiceClient.GetInvoiceAsync(invoiceId);

            historyOperation.InvoiceNumber = invoice?.Number;
            historyOperation.BillingCategory = invoice?.BillingCategory;
            historyOperation.InvoiceStatus = invoice?.Status;
        }

        private async Task FillFromEthereumCoreAsync(HistoryOperation historyOperation)
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
        }
    }
}
