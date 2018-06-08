using System;
using System.Net;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayAPI.Attributes;
using Lykke.Service.PayAPI.Models;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Exceptions;
using Lykke.Service.PayInternal.Client.Models.MerchantGroups;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.Employee;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayAPI.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/mobile")]
    public class MerchantsController : Controller
    {
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly IPayInternalClient _payInternalClient;

        public MerchantsController(
            [NotNull] IPayInvoiceClient payInvoiceClient,
            [NotNull] IPayInternalClient payInternalClient)
        {
            _payInvoiceClient = payInvoiceClient ?? throw new ArgumentNullException(nameof(payInvoiceClient));
            _payInternalClient = payInternalClient ?? throw new ArgumentNullException(nameof(payInternalClient));
        }

        /// <summary>
        /// Returns list of merchants available to be billed
        /// </summary>
        /// <returns>List of merchants</returns>
        /// <response code="200">List of merchants</response>
        /// <reponse code="404">Employee or merchant not found</reponse>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [BearerHeader]
        [HttpGet]
        [Route("billing/availableMerchants")]
        [SwaggerOperation(nameof(GetAvailableMerchantsForBilling))]
        [ProducesResponseType(typeof(AvailableMerchantsForBillingResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetAvailableMerchantsForBilling()
        {
            string email = this.GetUserEmail();

            MerchantsByUsageResponse merchantsResponse;

            try
            {
                EmployeeModel employee = await _payInvoiceClient.GetEmployeeByEmailAsync(email);

                merchantsResponse = await _payInternalClient.GetMerchantsByUsageAsync(
                    new GetMerchantsByUsageRequest
                    {
                        MerchantId = employee.MerchantId,
                        MerchantGroupUse = MerchantGroupUse.Billing
                    });
            }
            catch (ErrorResponseException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                    return NotFound(ErrorResponse.Create("Employee not found"));

                throw;
            }
            catch (DefaultErrorResponseException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                    return NotFound(ErrorResponse.Create("Merchant not found"));

                throw;
            }

            return Ok(new AvailableMerchantsForBillingResponse {Merchants = merchantsResponse.Merchants});
        }
    }
}
