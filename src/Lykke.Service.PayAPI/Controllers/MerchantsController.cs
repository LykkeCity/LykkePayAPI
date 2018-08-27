using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayAPI.Attributes;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayAPI.Models;
using Lykke.Service.PayAPI.Models.Merchant;
using Lykke.Service.PayAPI.Models.Mobile;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Exceptions;
using Lykke.Service.PayMerchant.Client.Models;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.Employee;
using Lykke.Service.PayInvoice.Client.Models.MerchantSetting;
using Lykke.Service.PayMerchant.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayAPI.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/mobile")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class MerchantsController : Controller
    {
        private readonly IMerchantService _merchantService;
        private readonly IIataService _iataService;
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly IPayInternalClient _payInternalClient;
        private readonly IPayMerchantClient _payMerchantClient;

        public MerchantsController(
            [NotNull] IMerchantService merchantService,
            [NotNull] IIataService iataService,
            [NotNull] IPayInvoiceClient payInvoiceClient,
            [NotNull] IPayInternalClient payInternalClient, 
            [NotNull] IPayMerchantClient payMerchantClient)
        {
            _merchantService = merchantService ?? throw new ArgumentNullException(nameof(merchantService));
            _iataService = iataService ?? throw new ArgumentNullException(nameof(iataService));
            _payInvoiceClient = payInvoiceClient ?? throw new ArgumentNullException(nameof(payInvoiceClient));
            _payInternalClient = payInternalClient ?? throw new ArgumentNullException(nameof(payInternalClient));
            _payMerchantClient = payMerchantClient ?? throw new ArgumentNullException(nameof(payMerchantClient));
        }

        /// <summary>
        /// Get list of merchants
        /// </summary>
        /// <remarks>
        /// Receive list of merchants available to be billed.
        /// </remarks>
        /// <response code="200">List of merchants</response>
        /// <reponse code="404">Employee or merchant not found</reponse>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [BearerHeader]
        [HttpGet]
        [Route("billing/availableMerchants")]
        [SwaggerOperation(OperationId = nameof(GetAvailableMerchantsForBilling))]
        [SwaggerXSummary("Available merchants")]
        [ProducesResponseType(typeof(AvailableMerchantsForBillingResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetAvailableMerchantsForBilling()
        {
            string email = this.GetUserEmail();

            MerchantsByUsageResponse merchantsResponse;

            try
            {
                EmployeeModel employee = await _payInvoiceClient.GetEmployeeByEmailAsync(email);

                merchantsResponse = await _payMerchantClient.GroupsApi.GetMerchantsByUsageAsync(
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
        /// Get current user info
        /// </summary>
        /// <remarks>
        /// Receive information about current user (employee of the merchant).
        /// </remarks>
        /// <response code="200">User information</response>
        /// <response code="404">Employee not found</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [BearerHeader]
        [HttpGet]
        [Route("user")]
        [SwaggerOperation(OperationId = nameof(GetCurrentUserInfo))]
        [SwaggerXSummary("User info")]
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
                    Email = employee.Email,
                    IsInternalSupervisor = employee.IsInternalSupervisor
                };
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound(ErrorResponse.Create("Employee not found"));
            }

            return Ok(result);
        }

        /// <summary>
        /// Get base asset list
        /// </summary>
        /// <remarks>
        /// Receive assets which can be set as base asset.
        /// </remarks>
        /// <response code="200">Base asset</response>
        /// <response code="404">Assets not found</response>
        /// <response code="400">Problem occured</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [BearerHeader]
        [HttpGet("baseAsset/list")]
        [SwaggerOperation(OperationId = nameof(GetBaseAssetList))]
        [SwaggerXSummary("Base asset list")]
        [ProducesResponseType(typeof(IReadOnlyList<BaseAssetItemModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetBaseAssetList()
        {
            string merchantId = this.GetUserMerchantId();

            try
            {
                var iataAssets = _iataService.GetIataAssets();
                var settlementAssetsResponse = await _payInternalClient.GetAvailableSettlementAssetsAsync(merchantId);

                var validAssets = settlementAssetsResponse.Assets.Where(x => iataAssets.ContainsKey(x)).ToList();

                if (!validAssets.Any())
                    return NotFound(ErrorResponse.Create("Assets not configured for current merchant"));

                string baseAsset = await _merchantService.GetBaseAssetAsync(merchantId);

                var result = new List<BaseAssetItemModel>();

                foreach (var asset in validAssets)
                {
                    result.Add(new BaseAssetItemModel
                    {
                        Id = asset,
                        Value = iataAssets[asset],
                        IsSelected = asset == baseAsset
                    });
                }

                return Ok(result);
            }
            catch (DefaultErrorResponseException ex) 
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
        }

        /// <summary>
        /// Set base asset
        /// </summary>
        /// <remarks>
        /// Set base asset for current merchant
        /// </remarks>
        /// <response code="200">Base asset</response>
        /// <response code="404">Assets not found</response>
        /// <response code="400">Problem occured</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [BearerHeader]
        [HttpPost("baseAsset")]
        [SwaggerOperation(OperationId = nameof(SetBaseAsset))]
        [SwaggerXSummary("Set base asset")]
        [ProducesResponseType(typeof(IReadOnlyList<BaseAssetItemModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> SetBaseAsset(string baseAsset)
        {
            string merchantId = this.GetUserMerchantId();

            try
            {
                await _payInvoiceClient.SetBaseAssetAsync(new UpdateBaseAssetRequest
                {
                    MerchantId = merchantId,
                    BaseAsset = baseAsset
                });

                return Ok();
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
        }
    }
}
