using AutoMapper;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayAPI.Attributes;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayAPI.Filters;
using Lykke.Service.PayAPI.Models.Mobile.History;
using Lykke.Service.PayHistory.Client;
using Lykke.Service.PayHistory.Client.Publisher;
using Lykke.Service.PayInvoice.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Lykke.Service.PayAPI.Controllers.Mobile
{
    [ApiVersion("1.0")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ValidateActionParametersFilter]
    [BearerHeader]
    [Route("api/v{version:apiVersion}/mobile/history/[action]")]
    public class HistoryController : Controller
    {
        private readonly ILog _log;
        private readonly IPayHistoryClient _payHistoryClient;
        private readonly IMerchantService _merchantService;
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly IExplorerUrlResolver _explorerUrlResolver;

        public HistoryController(IPayHistoryClient payHistoryClient,
            IMerchantService merchantService,
            IPayInvoiceClient payInvoiceClient,
            IExplorerUrlResolver explorerUrlResolver,
            ILog log)
        {
            _payHistoryClient = payHistoryClient;
            _merchantService = merchantService;
            _payInvoiceClient = payInvoiceClient;
            _explorerUrlResolver = explorerUrlResolver;
            _log = log.CreateComponentScope(nameof(HistoryController)) ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>
        /// Returns list of history operations.
        /// </summary>
        /// <response code="200">A collection of history operations.</response>
        /// <response code="400">Problem occured.</response>        
        [HttpGet]
        [SwaggerOperation("History")]
        [ProducesResponseType(typeof(IReadOnlyList<HistoryOperationViewModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Index()
        {
            var merchantId = this.GetUserMerchantId();

            var historyOperations = (await _payHistoryClient.GetHistoryAsync(merchantId)).ToArray();

            var merchantIds = historyOperations.Select(o => o.OppositeMerchantId).Where(id => !string.IsNullOrEmpty(id))
                .Distinct().ToList();
            if (!merchantIds.Contains(merchantId, StringComparer.OrdinalIgnoreCase))
            {
                merchantIds.Add(merchantId);
            }

            var merchantLogoUrlTasks = merchantIds.ToDictionary(id=>id, id => _merchantService.GetMerchantLogoUrlAsync(id));
            await Task.WhenAll(merchantLogoUrlTasks.Values);
            
            var results = new List<HistoryOperationViewModel>();
            foreach (var historyOperation in historyOperations)
            {
                var result = Mapper.Map<HistoryOperationViewModel>(historyOperation);
                string logoKey = historyOperation.OppositeMerchantId ?? merchantId;
                result.MerchantLogoUrl = merchantLogoUrlTasks[logoKey].Result;
                results.Add(result);
            }
            return Ok(results);
        }

        /// <summary>
        /// Returns details of the history operation.
        /// </summary>
        /// <response code="200">A details of the history operation.</response>
        /// <response code="400">Problem occured.</response>        
        /// <response code="404">History operation is not found.</response>        
        [HttpGet]
        [SwaggerOperation("HistoryDetails")]
        [ProducesResponseType(typeof(HistoryOperationModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Details([Required, PartitionOrRowKey]string id)
        {
            var merchantId = this.GetUserMerchantId();

            var historyOperation = await _payHistoryClient.GetDetailsAsync(merchantId, id);
            if (historyOperation == null)
            {
                return NotFound();
            }

            var result = Mapper.Map<HistoryOperationModel>(historyOperation);

            string detailsMerchantId = historyOperation.OppositeMerchantId ?? merchantId;
            result.MerchantName = await _merchantService.GetMerchantNameAsync(detailsMerchantId);
            result.MerchantLogoUrl = await _merchantService.GetMerchantLogoUrlAsync(detailsMerchantId);

            if (!string.IsNullOrEmpty(historyOperation.InvoiceId))
            {
                var invoice = await _payInvoiceClient.GetInvoiceAsync(historyOperation.InvoiceId);
                if (invoice != null)
                {
                    result.InvoiceNumber = invoice.Number;
                    result.BillingCategory = invoice.BillingCategory;
                    result.InvoiceStatus = invoice.Status;
                }
            }

            if (!string.IsNullOrEmpty(historyOperation.TxHash))
            {
                result.ExplorerUrl = _explorerUrlResolver.GetExplorerUrl(historyOperation.TxHash);
            }

            return Ok(result);
        }
    }
}
