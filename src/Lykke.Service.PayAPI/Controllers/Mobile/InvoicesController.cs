using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayAPI.Attributes;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayAPI.Models;
using Lykke.Service.PayAPI.Models.Invoice;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayAPI.Controllers.Mobile
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/mobile/invoices")]
    public class InvoicesController : Controller
    {
        private readonly IIataService _iataService;
        private readonly IMerchantService _merchantService;
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly ILog _log;

        public InvoicesController(
            IIataService iataService,
            IMerchantService merchantService,
            IPayInvoiceClient payInvoiceClient,
            ILog log)
        {
            _iataService = iataService;
            _merchantService = merchantService;
            _payInvoiceClient = payInvoiceClient ?? throw new ArgumentNullException(nameof(payInvoiceClient));
            _log = log.CreateComponentScope(nameof(InvoicesController)) ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>
        /// Returns invoices by filter
        /// </summary>
        /// <param name="clientMerchantIds">The merchant ids of the clients (e.g. ?clientMerchantIds=one&amp;clientMerchantIds=two)</param>
        /// <param name="statuses">The statuses (e.g. ?statuses=one&amp;statuses=two)</param>
        /// <param name="dispute">The dispute attribute</param>
        /// <param name="billingCategories">The billing categories (e.g. ?billingCategories=one&amp;billingCategories=two)</param>
        /// <param name="settlementAssets">The settlement assets (e.g. ?settlementAssets=one&amp;settlementAssets=two)</param>
        /// <param name="greaterThan">The greater than number for filtering (can be fractional)</param>
        /// <param name="lessThan">The less than number for filtering (can be fractional)</param>
        /// <response code="200">A collection of invoices.</response>
        /// <response code="400">Problem occured.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [BearerHeader]
        [HttpGet("mine")]
        [SwaggerOperation("InvoicesGetMineByFilter")]
        [ProducesResponseType(typeof(IReadOnlyList<InvoiceResponseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetMineByFilter(IEnumerable<string> clientMerchantIds, IEnumerable<string> statuses, bool? dispute, IEnumerable<string> billingCategories, IEnumerable<string> settlementAssets, decimal? greaterThan, decimal? lessThan)
        {
            var merchantId = this.GetUserMerchantId();

            try
            {
                IReadOnlyList<string> groupMerchants = await _merchantService.GetGroupMerchantsAsync(merchantId);

                // should be only from merchants inside group
                if (clientMerchantIds.Any())
                {
                    foreach (var clientMerchantId in clientMerchantIds)
                    {
                        if (!groupMerchants.Contains(clientMerchantId))
                        {
                            return BadRequest(ErrorResponse.Create($"ClientMerchantId {clientMerchantId} is not in the group"));
                        }
                    }
                }
                else
                {
                    clientMerchantIds = groupMerchants;
                }

                var invoices = await _payInvoiceClient.GetByFilter(new string[] { merchantId }, clientMerchantIds, statuses, dispute, billingCategories, greaterThan, lessThan);

                var result = Mapper.Map<IReadOnlyList<InvoiceResponseModel>>(FilterBySettlementAssets(invoices, settlementAssets));
                await FillAdditionalData(result);
                return Ok(result.OrderByDescending(x => x.CreatedDate));
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(ex.Error);
            }
            catch (Exception ex)
            {
                _log.WriteError(nameof(GetMineByFilter), new { merchantId, clientMerchantIds, statuses, billingCategories }, ex);
            }

            return StatusCode((int)HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Returns invoices by filter
        /// </summary>
        /// <param name="clientMerchantIds">The merchant ids of the clients (e.g. ?clientMerchantIds=one&amp;clientMerchantIds=two)</param>
        /// <param name="statuses">The statuses (e.g. ?statuses=one&amp;statuses=two)</param>
        /// <param name="dispute">The dispute attribute</param>
        /// <param name="billingCategories">The billing categories (e.g. ?billingCategories=one&amp;billingCategories=two)</param>
        /// <param name="settlementAssets">The settlement assets (e.g. ?settlementAssets=one&amp;settlementAssets=two)</param>
        /// <param name="greaterThan">The greater than number for filtering (can be fractional)</param>
        /// <param name="lessThan">The less than number for filtering (can be fractional)</param>
        /// <response code="200">A collection of invoices.</response>
        /// <response code="400">Problem occured.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [BearerHeader]
        [HttpGet("inbox")]
        [SwaggerOperation("InvoicesGetInboxByFilter")]
        [ProducesResponseType(typeof(IReadOnlyList<InvoiceResponseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetInboxByFilter(IEnumerable<string> clientMerchantIds, IEnumerable<string> statuses, bool? dispute, IEnumerable<string> billingCategories, IEnumerable<string> settlementAssets, decimal? greaterThan, decimal? lessThan)
        {
            var merchantId = this.GetUserMerchantId();

            try
            {
                IReadOnlyList<string> groupMerchants = await _merchantService.GetGroupMerchantsAsync(merchantId);

                // should be only from merchants inside group
                if (clientMerchantIds.Any())
                {
                    foreach (var clientMerchantId in clientMerchantIds)
                    {
                        if (!groupMerchants.Contains(clientMerchantId))
                        {
                            return BadRequest(ErrorResponse.Create($"ClientMerchantId {clientMerchantId} is not in the group"));
                        }
                    }
                }
                else
                {
                    clientMerchantIds = groupMerchants;
                }

                var invoices = await _payInvoiceClient.GetByFilter(clientMerchantIds, new string[] { merchantId }, statuses, dispute, billingCategories, greaterThan, lessThan);

                var result = Mapper.Map<IReadOnlyList<InvoiceResponseModel>>(FilterBySettlementAssets(invoices, settlementAssets));
                await FillAdditionalData(result);
                return Ok(result.OrderByDescending(x => x.CreatedDate));
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(ex.Error);
            }
            catch (Exception ex)
            {
                _log.WriteError(nameof(GetInboxByFilter), new { merchantId, statuses, billingCategories }, ex);
            }

            return StatusCode((int)HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Get filter for current merchant
        /// </summary>
        /// <response code="200">Filter for current merchant</response>
        /// <response code="400">Problem occured</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [BearerHeader]
        [HttpGet("filter")]
        [SwaggerOperation("GetFilterForCurrentMerchant")]
        [ProducesResponseType(typeof(FilterOfMerchantResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetFilterForCurrentMerchant()
        {
            var merchantId = this.GetUserMerchantId();

            try
            {
                var filter = new FilterOfMerchantResponse();

                IReadOnlyList<string> groupMerchants = await _merchantService.GetGroupMerchantsAsync(merchantId);

                var merchantsDictionary = new Dictionary<string, string>();
                foreach (var groupMerchantId in groupMerchants)
                {
                    var merchantName = await _merchantService.GetMerchantNameAsync(groupMerchantId);
                    if (!string.IsNullOrEmpty(merchantName))
                    {
                        merchantsDictionary.TryAdd(groupMerchantId, merchantName);
                    }
                }

                var groupMerchantsFilterItems = new List<MerchantFilterItemModel>();

                foreach (var item in merchantsDictionary.ToListOfFilterItems())
                {
                    groupMerchantsFilterItems.Add(new MerchantFilterItemModel
                    {
                        Id = item.Id,
                        Value = item.Value,
                        MerchantLogoUrl = await _merchantService.GetMerchantLogoUrlAsync(item.Id)
                    });
                }

                filter.GroupMerchants = groupMerchantsFilterItems;

                filter.BillingCategories = (await _iataService.GetIataBillingCategoriesAsync()).ToListOfFilterItems();

                filter.SettlementAssets = _iataService.GetIataAssets().ToListOfFilterItems();

                return Ok(filter);
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(ex.Error);
            }
            catch (Exception ex)
            {
                _log.WriteError(nameof(GetFilterForCurrentMerchant), new { merchantId }, ex);
            }

            return StatusCode((int)HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Mark dispute
        /// </summary>
        /// <param name="model">The model</param>
        /// <response code="200">Success</response>
        /// <response code="404">Not found</response>
        /// <response code="400">Invalid model</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [BearerHeader]
        [HttpPost]
        [Route("dispute/mark")]
        [SwaggerOperation(nameof(MarkDispute))]
        [ValidateModel]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> MarkDispute([FromBody] Models.Invoice.MarkInvoiceDisputeRequest model)
        {
            var employeeId = this.GetUserEmployeeId();

            try
            {
                await _payInvoiceClient.MarkDisputeAsync(new PayInvoice.Client.Models.Invoice.MarkInvoiceDisputeRequest
                {
                    InvoiceId = model.InvoiceId,
                    Reason = model.Reason,
                    EmployeeId = employeeId
                });

                return Ok();
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound(ex.Error);
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(ex.Error);
            }
        }

        /// <summary>
        /// Cancel dispute
        /// </summary>
        /// <param name="model">The model</param>
        /// <response code="200">Success</response>
        /// <response code="404">Not found</response>
        /// <response code="400">Invalid model</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [BearerHeader]
        [HttpPost]
        [Route("dispute/cancel")]
        [SwaggerOperation(nameof(CancelDispute))]
        [ValidateModel]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CancelDispute([FromBody] Models.Invoice.CancelInvoiceDisputeRequest model)
        {
            var employeeId = this.GetUserEmployeeId();

            try
            {
                await _payInvoiceClient.CancelDisputeAsync(new PayInvoice.Client.Models.Invoice.CancelInvoiceDisputeRequest
                {
                    InvoiceId = model.InvoiceId,
                    EmployeeId = employeeId
                });

                return Ok();
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound(ex.Error);
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(ex.Error);
            }
        }

        /// <summary>
        /// Get list of my invoices which are marked as Dispute
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="404">Not found</response>
        /// <response code="400">Problem occured</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [BearerHeader]
        [HttpGet]
        [Route("dispute/list")]
        [SwaggerOperation(nameof(GetMyInvoicesMarkedDispute))]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetMyInvoicesMarkedDispute()
        {
            var merchantId = this.GetUserMerchantId();

            try
            {
                IReadOnlyList<string> groupMerchants = await _merchantService.GetGroupMerchantsAsync(merchantId);

                var disputeInvoices = await _payInvoiceClient.GetByFilter(new string[] { merchantId }, groupMerchants, null, dispute: true, null, null, null);

                var result = Mapper.Map<IReadOnlyList<InvoiceMarkedDisputeResponse>>(Mapper.Map<IReadOnlyList<InvoiceResponseModel>>(disputeInvoices));

                await FillAdditionalData(result, isDispute: true);

                // Fill dispute info
                foreach (var invoice in result)
                {
                    var info = await GetInvoiceDisputeInfo(invoice.Id);

                    if (info != null)
                    {
                        invoice.DisputeRaisedAt = info.CreatedAt;
                        invoice.DisputeReason = info.Reason;
                    }
                }

                return Ok(result.OrderByDescending(x => x.CreatedDate));
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(ex.Error);
            }
        }

        private async Task<InvoiceDisputeInfoResponse> GetInvoiceDisputeInfo(string invoiceId)
        {
            try
            {
                var info = await _payInvoiceClient.GetInvoiceDisputeInfoAsync(invoiceId);
                return info;
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        private async Task FillAdditionalData(IReadOnlyList<InvoiceResponseModel> result, bool isDispute = false)
        {
            foreach (var invoice in result)
            {
                invoice.MerchantName = await _merchantService.GetMerchantNameAsync(invoice.MerchantId);

                var iataSpecificData = await _iataService.GetIataSpecificDataAsync(invoice.Id);
                if (iataSpecificData != null)
                {
                    invoice.IataInvoiceDate = iataSpecificData.IataInvoiceDate;
                    invoice.SettlementMonthPeriod = iataSpecificData.SettlementMonthPeriod;
                }

                invoice.LogoUrl = isDispute
                    ? await _merchantService.GetMerchantLogoUrlAsync(invoice.ClientName)
                    : await _merchantService.GetMerchantLogoUrlAsync(invoice.MerchantId);
            }
        }

        private IReadOnlyList<InvoiceModel> FilterBySettlementAssets(IReadOnlyList<InvoiceModel> invoices, IEnumerable<string> settlementAssets)
        {
            if (settlementAssets == null || !settlementAssets.Any())
                return invoices;

            invoices = invoices.Where(x => settlementAssets.Contains(x.SettlementAssetId)).ToList();

            return invoices;
        }
    }
}
