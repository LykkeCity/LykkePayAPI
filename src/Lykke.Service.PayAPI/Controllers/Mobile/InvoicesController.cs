using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayAPI.Attributes;
using Lykke.Service.PayAPI.Models;
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
        /// <param name="clientMerchantIds">The merchant ids of the clients (e.g. ?clientMerchantIds=one&amp;clientMerchantIds=two)</param>
        /// <param name="statuses">The statuses (e.g. ?statuses=one&amp;statuses=two)</param>
        /// <param name="dispute">The dispute attribute</param>
        /// <param name="billingCategories">The billing categories (e.g. ?billingCategories=one&amp;billingCategories=two)</param>
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
        public async Task<IActionResult> GetMineByFilter(IEnumerable<string> clientMerchantIds, IEnumerable<string> statuses, bool? dispute, IEnumerable<string> billingCategories, decimal? greaterThan, decimal? lessThan)
        {
            var merchantId = this.GetUserMerchantId();

            try
            {
                //TODO: get group merchants
                IEnumerable<string> groupMerchants = new List<string> { "Begun1" };

                foreach (var clientMerchantId in clientMerchantIds)
                {
                    if (!groupMerchants.Contains(clientMerchantId))
                    {
                        return BadRequest($"ClientMerchantId {clientMerchantId} is not in the group");
                    }
                }

                var response = await _payInvoiceClient.GetByFilter(new string[] { merchantId }, clientMerchantIds, statuses, dispute, billingCategories, greaterThan, lessThan);

                return Ok(Mapper.Map<IReadOnlyList<InvoiceResponseModel>>(response));
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
        /// <param name="statuses">The statuses (e.g. ?statuses=one&amp;statuses=two)</param>
        /// <param name="dispute">The dispute attribute</param>
        /// <param name="billingCategories">The billing categories (e.g. ?billingCategories=one&amp;billingCategories=two)</param>
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
        public async Task<IActionResult> GetInboxByFilter(IEnumerable<string> statuses, bool? dispute, IEnumerable<string> billingCategories, decimal? greaterThan, decimal? lessThan)
        {
            var merchantId = this.GetUserMerchantId();

            try
            {
                //TODO: get group merchants
                IEnumerable<string> groupMerchants = new List<string> { "Begun1" };

                var response = await _payInvoiceClient.GetByFilter(Enumerable.Empty<string>(), new string[] { merchantId }, statuses, dispute, billingCategories, greaterThan, lessThan);

                return Ok(Mapper.Map<IReadOnlyList<InvoiceResponseModel>>(response));
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
    }
}
