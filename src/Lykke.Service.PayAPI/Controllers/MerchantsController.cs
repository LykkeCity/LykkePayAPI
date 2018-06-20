using System;
using System.Net;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayAPI.Attributes;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayAPI.Models;
using Lykke.Service.PayAPI.Models.Mobile;
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
        private readonly IMerchantService _merchantService;
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly IPayInternalClient _payInternalClient;

        public MerchantsController(
            [NotNull] IMerchantService merchantService,
            [NotNull] IPayInvoiceClient payInvoiceClient,
            [NotNull] IPayInternalClient payInternalClient)
        {
            _merchantService = merchantService ?? throw new ArgumentNullException(nameof(merchantService));
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

        /// <summary>
        /// Get information about current user (employee of the merchant)
        /// </summary>
        /// <response code="200">User information</response>
        /// <response code="404">Employee not found</reponse>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [BearerHeader]
        [HttpGet]
        [Route("user")]
        [SwaggerOperation(nameof(GetCurrentUserInfo))]
        [ProducesResponseType(typeof(CurrentUserInfoResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetCurrentUserInfo()
        {
            string merchantId = this.GetUserMerchantId();
            string employeeId = this.GetUserEmployeeId();

            CurrentUserInfoResponse result;

            try
            {
                EmployeeModel employee = await _payInvoiceClient.GetEmployeeAsync(employeeId);

                result = new CurrentUserInfoResponse
                {
                    MerchantName = await _merchantService.GetMerchantNameAsync(merchantId),
                    MerchantLogoUrl = await _merchantService.GetMerchantLogoUrlAsync(merchantId),
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Email = employee.Email
                };
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound(ErrorResponse.Create("Employee not found"));
            }

            return Ok(result);
        }
    }
}
