using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.Log;
using Lykke.Service.PayAPI.Attributes;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayAPI.Models;
using Lykke.Service.PayAPI.Models.Invoice;
using Lykke.Service.PayAPI.Validation;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Exceptions;
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
    [Produces("application/json")]
    [Consumes("application/json")]
    public class InvoicesController : Controller
    {
        private readonly IIataService _iataService;
        private readonly IMerchantService _merchantService;
        private readonly IPayInternalClient _payInternalClient;
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly ILog _log;

        public InvoicesController(
            IIataService iataService,
            IMerchantService merchantService,
            IPayInternalClient payInternalClient,
            IPayInvoiceClient payInvoiceClient,
            ILogFactory logFactory)
        {
            _iataService = iataService;
            _merchantService = merchantService;
            _payInternalClient = payInternalClient;
            _payInvoiceClient = payInvoiceClient ?? throw new ArgumentNullException(nameof(payInvoiceClient));
            _log = logFactory?.CreateLog(this) ?? throw new ArgumentNullException(nameof(logFactory));
        }

        /// <summary>
        /// Get my invoices
        /// </summary>
        /// <remarks>
        /// Receive the invoices by filter which were created by me.
        /// </remarks>
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
        [SwaggerOperation(OperationId = "InvoicesGetMineByFilter")]
        [SwaggerXSummary("My invoices")]
        [ProducesResponseType(typeof(IReadOnlyList<InvoiceResponseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
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

                var invoices = await _payInvoiceClient.GetByFilter(new string[] { merchantId }, clientMerchantIds, statuses, dispute, billingCategories, null, null);

                var result = Mapper.Map<IReadOnlyList<InvoiceResponseModel>>(FilterBySettlementAssets(invoices, settlementAssets));

                await FillAdditionalData(result);

                result = await FilterByAmountInBaseAsset(result, greaterThan, lessThan, merchantId);

                return Ok(result.OrderByDescending(x => x.CreatedDate));
            }
            catch (DefaultErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound(ex.Error);
            }
            catch (ErrorResponseException ex)
            {
                switch (ex.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        return NotFound(ex.Error);
                    case HttpStatusCode.BadRequest:
                        return BadRequest(ex.Error);
                    default:
                        throw;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, null, $@"request:{
                        new
                        {
                            merchantId,
                            clientMerchantIds,
                            statuses,
                            billingCategories
                        }.ToJson()
                    }");
                throw;
            }
        }

        /// <summary>
        /// Get incoming invoices
        /// </summary>
        /// <remarks>
        /// Receive the invoices by filter which should be paid by me.
        /// </remarks>
        /// <param name="clientMerchantIds">[Optional] The merchant ids of the clients (e.g. ?clientMerchantIds=one&amp;clientMerchantIds=two)</param>
        /// <param name="statuses">[Optional] The statuses (e.g. ?statuses=one&amp;statuses=two)</param>
        /// <param name="dispute">[Optional] The dispute attribute</param>
        /// <param name="billingCategories">[Optional] The billing categories (e.g. ?billingCategories=one&amp;billingCategories=two)</param>
        /// <param name="settlementAssets">[Optional] The settlement assets (e.g. ?settlementAssets=one&amp;settlementAssets=two)</param>
        /// <param name="greaterThan">[Optional] The greater than number for filtering (can be fractional)</param>
        /// <param name="lessThan">[Optional] The less than number for filtering (can be fractional)</param>
        /// <response code="200">A collection of invoices.</response>
        /// <response code="400">Problem occured.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [BearerHeader]
        [HttpGet("inbox")]
        [SwaggerOperation(OperationId = "InvoicesGetInboxByFilter")]
        [SwaggerXSummary("Incoming invoices")]
        [ProducesResponseType(typeof(IReadOnlyList<InvoiceResponseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
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

                var invoices = await _payInvoiceClient.GetByFilter(clientMerchantIds, new string[] { merchantId }, statuses, dispute, billingCategories, null, null);

                var result = Mapper.Map<IReadOnlyList<InvoiceResponseModel>>(FilterBySettlementAssets(invoices, settlementAssets));

                await FillAdditionalData(result);

                result = await FilterByAmountInBaseAsset(result, greaterThan, lessThan, merchantId);

                return Ok(result.OrderByDescending(x => x.CreatedDate));
            }
            catch (DefaultErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound(ex.Error);
            }
            catch (ErrorResponseException ex)
            {
                switch (ex.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        return NotFound(ex.Error);
                    case HttpStatusCode.BadRequest:
                        return BadRequest(ex.Error);
                    default:
                        throw;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, null, $@"request:{
                        new
                        {
                            merchantId,
                            statuses,
                            billingCategories
                        }.ToJson()
                    }");
                throw;
            }
        }

        /// <summary>
        /// Get filter
        /// </summary>
        /// <remarks>
        /// Get filter for current merchant.
        /// </remarks>
        /// <response code="200">Filter for current merchant</response>
        /// <response code="400">Problem occured</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [BearerHeader]
        [HttpGet("filter")]
        [SwaggerOperation(OperationId = "GetFilterForCurrentMerchant")]
        [SwaggerXSummary("Filter")]
        [ProducesResponseType(typeof(FilterOfMerchantResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
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

                #region MaxRangeInBaseAsset
                var invoices = await _payInvoiceClient.GetByFilter(groupMerchants, new string[] { merchantId }, null, null, null, null, null);

                var calculatedInvoices = await CalcSettlementAmountInBaseAsset(Mapper.Map<IReadOnlyList<InvoiceResponseModel>>(invoices), merchantId);

                filter.MaxRangeInBaseAsset = calculatedInvoices.Any()
                    ? Math.Ceiling(calculatedInvoices.Max(x => x.SettlementAmountInBaseAsset))
                    : 0;

                #endregion

                return Ok(filter);
            }
            catch (DefaultErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound(ex.Error);
            }
            catch (ErrorResponseException ex)
            {
                switch (ex.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        return NotFound(ex.Error);
                    case HttpStatusCode.BadRequest:
                        return BadRequest(ex.Error);
                    default:
                        throw;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, null, $"request:{new {merchantId}.ToJson()}");
                throw;
            }
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
        [SwaggerOperation(OperationId = nameof(MarkDispute))]
        [SwaggerXSummary("Mark dispute")]
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
        [SwaggerOperation(OperationId = nameof(CancelDispute))]
        [SwaggerXSummary("Cancel dispute")]
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
        /// Get dispute invoices
        /// </summary>
        /// <remarks>
        /// Get list of my invoices which are marked as Dispute.
        /// </remarks>
        /// <response code="200">Success</response>
        /// <response code="404">Not found</response>
        /// <response code="400">Problem occured</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [BearerHeader]
        [HttpGet]
        [Route("dispute/list")]
        [SwaggerOperation(OperationId = nameof(GetMyInvoicesMarkedDispute))]
        [SwaggerXSummary("Dispute invoices")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetMyInvoicesMarkedDispute()
        {
            var merchantId = this.GetUserMerchantId();

            try
            {
                IReadOnlyList<string> groupMerchants = await _merchantService.GetGroupMerchantsAsync(merchantId);

                var disputeInvoices = await _payInvoiceClient.GetByFilter(new string[] { merchantId }, groupMerchants, null, true, null, null, null);

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

        /// <summary>
        /// 
        /// </summary>
        private async Task<IReadOnlyList<InvoiceResponseModel>> FilterByAmountInBaseAsset(IReadOnlyList<InvoiceResponseModel> invoices, decimal? greaterThan, decimal? lessThan, string merchantId)
        {
            if (greaterThan == null && lessThan == null)
                return invoices;

            var calculatedInvoices = await CalcSettlementAmountInBaseAsset(invoices, merchantId);

            if (greaterThan.HasValue)
            {
                calculatedInvoices = calculatedInvoices.Where(x => x.SettlementAmountInBaseAsset >= greaterThan).ToList();
            }

            if (lessThan.HasValue)
            {
                calculatedInvoices = calculatedInvoices.Where(x => x.SettlementAmountInBaseAsset <= lessThan).ToList();
            }

            return calculatedInvoices;
        }

        private async Task<IReadOnlyList<InvoiceResponseModel>> CalcSettlementAmountInBaseAsset(IReadOnlyList<InvoiceResponseModel> invoices, string merchantId)
        {
            var baseAsset = await _payInvoiceClient.GetBaseAssetAsync(merchantId);

            var invoiceSettlementAssets = invoices.Select(x => x.SettlementAssetId).Distinct();

            var rates = new Dictionary<string, decimal>();

            foreach (var settlementAsset in invoiceSettlementAssets)
            {
                if (baseAsset == settlementAsset)
                {
                    rates.Add(settlementAsset, 1);
                }
                else
                {
                    //the response example is
                    /*
                     "BaseAssetId": "IATAUSDT2",
                     "QuotingAssetId": "IATAEURT2",
                     "BidPrice": 0.87,
                    */
                    var assetRateResponse = await _payInternalClient.GetCurrentAssetPairRateAsync(settlementAsset, baseAsset);
                    rates.Add(settlementAsset, assetRateResponse.BidPrice);
                }
            }

            foreach (var invoice in invoices)
            {
                invoice.SettlementAmountInBaseAsset = invoice.SettlementAssetId == baseAsset
                    ? invoice.Amount
                    : invoice.Amount * rates[invoice.SettlementAssetId];
            }

            return invoices;
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

                // if an invoice partially paid, it shows the amount left to pay
                if (invoice.Status == InvoiceStatus.Underpaid.ToString() && invoice.LeftAmountToPayInSettlementAsset > 0)
                {
                    invoice.Amount = invoice.LeftAmountToPayInSettlementAsset;
                }
            }
        }

        private IReadOnlyList<InvoiceModel> FilterBySettlementAssets(IReadOnlyList<InvoiceModel> invoices, IEnumerable<string> settlementAssets)
        {
            if (settlementAssets == null || !settlementAssets.Any())
                return invoices;

            invoices = invoices.Where(x => settlementAssets.Contains(x.SettlementAssetId)).ToList();

            return invoices;
        }

        /// <summary>
        /// Pay invoices
        /// </summary>
        /// <remarks>
        /// Pay one or multiple invoices with certain amount.
        /// </remarks>
        /// <param name="model">Invoices ids and amount to pay</param>
        /// <response code="202">Accepted for further processing</response>
        /// <response code="400">Problem occured</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [BearerHeader]
        [HttpPost("pay")]
        [SwaggerOperation(OperationId = "PayInvoices")]
        [SwaggerXSummary("Pay invoices")]
        [ValidateModel]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.Accepted)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> PayInvoices([FromBody] PayInvoicesRequestModel model)
        {
            try
            {
                await _payInvoiceClient.PayInvoicesAsync(new PayInvoicesRequest
                {
                    EmployeeId = this.GetUserEmployeeId(),
                    InvoicesIds = model.InvoicesIds,
                    Amount = model.AmountInBaseAsset
                });
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound(ex.Error);
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(ex.Error);
            }

            return Accepted(true);
        }

        /// <summary>
        /// Get sum
        /// </summary>
        /// <remarks>
        /// Get sum for paying invoices
        /// </remarks>
        /// <param name="invoicesIds">The invoices ids (e.g. ?invoicesIds=one&amp;invoicesIds=two)</param>
        /// <response code="200">Sum for paying invoices</response>
        /// <response code="400">Problem occured</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [BearerHeader]
        [HttpGet("sum")]
        [SwaggerOperation(OperationId = "GetSumToPayInvoices")]
        [SwaggerXSummary("Sum to pay")]
        [ValidateModel]
        [ProducesResponseType(typeof(GetSumToPayResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetSumToPayInvoices([NotEmptyCollection] IEnumerable<string> invoicesIds)
        {
            decimal result = 0;

            try
            {
                result = await _payInvoiceClient.GetSumToPayInvoicesAsync(new GetSumToPayInvoicesRequest
                {
                    EmployeeId = this.GetUserEmployeeId(),
                    InvoicesIds = invoicesIds
                });
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound(ex.Error);
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(ex.Error);
            }

            return Accepted(new GetSumToPayResponse
            {
                AmountToPay = result
            });
        }
    }
}
