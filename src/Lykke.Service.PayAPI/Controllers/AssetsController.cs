using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using JetBrains.Annotations;
using Lykke.Service.PayAPI.Attributes;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayAPI.Models;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models.Asset;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayAPI.Controllers
{
    [Authorize]
    [SignatureHeaders]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/assets")]
    public class AssetsController : Controller
    {
        private readonly IPayInternalClient _payInternalClient;
        private readonly IHeadersHelper _headersHelper;

        public AssetsController(
            [NotNull] IPayInternalClient payInternalClient,
            [NotNull] IHeadersHelper headersHelper)
        {
            _payInternalClient = payInternalClient ?? throw new ArgumentNullException(nameof(payInternalClient));
            _headersHelper = headersHelper ?? throw new ArgumentNullException(nameof(headersHelper));
        }

        /// <summary>
        /// Returns list of settlement assets available for merchant
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("settlement")]
        [SwaggerOperation("GetSettlementAssets")]
        [ProducesResponseType(typeof(AssetsResponseModel), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> GetSettlementAssets()
        {
            AvailableAssetsResponse response =
                await _payInternalClient.GetAvailableSettlementAssetsAsync(_headersHelper.MerchantId);

            return Ok(Mapper.Map<AssetsResponseModel>(response));
        }

        /// <summary>
        /// Returns list of payment assets available for merchant
        /// </summary>
        /// <param name="settlementAssetId">Settlement asset id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("payment/{settlementAssetId}")]
        [SwaggerOperation("GetPaymentAssets")]
        [ProducesResponseType(typeof(AssetsResponseModel), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> GetPaymentAssets(string settlementAssetId)
        {
            AvailableAssetsResponse response =
                await _payInternalClient.GetAvailablePaymentAssetsAsync(_headersHelper.MerchantId,
                    settlementAssetId);

            return Ok(Mapper.Map<AssetsResponseModel>(response));
        }
    }
}
