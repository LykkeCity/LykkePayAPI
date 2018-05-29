using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayAPI.Attributes;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayAPI.Models;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models.Asset;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayAPI.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/invoices")]
    public class InvoicesController : Controller
    {
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly ILog _log;

        public InvoicesController(
            IPayInvoiceClient payInvoiceClient,
            ILog log)
        {
            _payInvoiceClient = payInvoiceClient ?? throw new ArgumentNullException(nameof(payInvoiceClient));
            _log = log.CreateComponentScope(nameof(InvoicesController)) ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>
        /// Returns invoices by filter
        /// </summary>
        /// <param name="merchantIds">The merchant ids (e.g. ?merchantIds=one&amp;merchantIds=two)</param>
        /// <param name="clientMerchantIds">The merchant ids of the clients (e.g. ?clientMerchantIds=one&amp;clientMerchantIds=two)</param>
        /// <param name="statuses">The statuses (e.g. ?statuses=one&amp;statuses=two)</param>
        /// <param name="dispute">The dispute attribute</param>
        /// <param name="billingCategories">The billing categories (e.g. ?billingCategories=one&amp;billingCategories=two)</param>
        /// <param name="greaterThan">The greater than number for filtering (can be fractional)</param>
        /// <param name="lessThan">The less than number for filtering (can be fractional)</param>
        /// <response code="200">A collection of invoices.</response>
        /// <response code="400">Problem occured.</response>
        [HttpGet]
        [SwaggerOperation("InvoicesGetByFilter")]
        [ProducesResponseType(typeof(IReadOnlyList<InvoiceResponseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetByFilter(IEnumerable<string> merchantIds, IEnumerable<string> clientMerchantIds, IEnumerable<string> statuses, bool? dispute, IEnumerable<string> billingCategories, decimal? greaterThan, decimal? lessThan)
        {
            try
            {
                var response = await _payInvoiceClient.GetByFilter(merchantIds, clientMerchantIds, statuses, dispute, billingCategories, greaterThan, lessThan);

                return Ok(Mapper.Map<IReadOnlyList<InvoiceResponseModel>>(response));
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(ex.Error);
            }
            catch (Exception ex)
            {
                _log.WriteError(nameof(GetByFilter), new { merchantIds, clientMerchantIds, statuses, billingCategories }, ex);
            }

            return StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }
}
