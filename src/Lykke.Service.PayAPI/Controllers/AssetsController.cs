using System;
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
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Exceptions;
using Lykke.Service.PayInternal.Client.Models.Asset;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayAPI.Controllers
{
    /// <summary>
    /// Assets API
    /// </summary>
    /// <remarks>
    /// Provide information about assets availability
    /// Assets are configured per merchant depending of needs
    /// </remarks>
    [Authorize]
    [SignatureHeaders]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/assets")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class AssetsController : Controller
    {
        private readonly IPayInternalClient _payInternalClient;
        private readonly IHeadersHelper _headersHelper;
        private readonly ILog _log;

        public AssetsController(
            IPayInternalClient payInternalClient,
            IHeadersHelper headersHelper,
            ILogFactory logFactory)
        {
            _payInternalClient = payInternalClient ?? throw new ArgumentNullException(nameof(payInternalClient));
            _headersHelper = headersHelper ?? throw new ArgumentNullException(nameof(headersHelper));
            _log = logFactory?.CreateLog(this) ?? throw new ArgumentNullException(nameof(logFactory));
        }

        /// <summary>
        /// Get settlement assets
        /// </summary>
        /// <remarks>
        /// Receive list of settlement assets available for merchant.
        /// </remarks>
        /// <response code="200">List of settlement assets</response>
        [HttpGet]
        [Route("settlement")]
        [SwaggerOperation(OperationId = "GetSettlementAssets")]
        [SwaggerXSummary("Settlement assets")]
        [ProducesResponseType(typeof(AssetsResponseModel), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> GetSettlementAssets()
        {
            try
            {
                AvailableAssetsResponse response =
                    await _payInternalClient.GetAvailableSettlementAssetsAsync(_headersHelper.MerchantId);

                return Ok(Mapper.Map<AssetsResponseModel>(response));
            }
            catch (Exception ex)
            {
                _log.Error(ex, null, $"request: {new {_headersHelper.MerchantId}.ToJson()}");
                throw;
            }
        }

        /// <summary>
        /// Get payment assets
        /// </summary>
        /// <remarks>
        /// Receive list of payment assets available for merchant
        /// </remarks>
        /// <param name="settlementAssetId">Settlement asset id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("payment/{settlementAssetId}")]
        [SwaggerOperation(OperationId = "GetPaymentAssets")]
        [SwaggerXSummary("Payment assets")]
        [ProducesResponseType(typeof(AssetsResponseModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetPaymentAssets(string settlementAssetId)
        {
            try
            {
                AvailableAssetsResponse response =
                    await _payInternalClient.GetAvailablePaymentAssetsAsync(_headersHelper.MerchantId,
                        settlementAssetId);

                return Ok(Mapper.Map<AssetsResponseModel>(response));
            }
            catch (Exception ex)
            {
                _log.Error(ex, null, $@"request: {
                        new
                        {
                            _headersHelper.MerchantId,
                            settlementAssetId
                        }.ToJson()
                    }");

                if (ex is DefaultErrorResponseException clientEx)
                {
                    if (clientEx.StatusCode == HttpStatusCode.NotFound)
                        return NotFound(clientEx.Error);

                    if (clientEx.StatusCode == HttpStatusCode.BadRequest)
                        return BadRequest(clientEx.Error);
                }

                throw;
            }
        }
    }
}
