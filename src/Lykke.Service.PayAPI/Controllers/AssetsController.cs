﻿using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Common.Log;
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
        private readonly ILog _log;

        public AssetsController(
            IPayInternalClient payInternalClient,
            IHeadersHelper headersHelper,
            ILog log)
        {
            _payInternalClient = payInternalClient ?? throw new ArgumentNullException(nameof(payInternalClient));
            _headersHelper = headersHelper ?? throw new ArgumentNullException(nameof(headersHelper));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>
        /// Returns list of settlement assets available for merchant
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("settlement")]
        [SwaggerOperation("GetSettlementAssets")]
        [ProducesResponseType(typeof(AssetsResponseModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
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
                await _log.WriteErrorAsync(nameof(AssetsController), nameof(GetSettlementAssets),
                    new {_headersHelper.MerchantId}.ToJson(), ex);
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
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
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPaymentAssets(string settlementAssetId)
        {
            try
            {
                AvailableAssetsResponse response =
                    await _payInternalClient.GetAvailablePaymentAssetsAsync(_headersHelper.MerchantId, settlementAssetId);

                return Ok(Mapper.Map<AssetsResponseModel>(response));

            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(AssetsController), nameof(GetPaymentAssets),
                    new { _headersHelper.MerchantId, settlementAssetId }.ToJson(), ex);
            }

            return StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }
}