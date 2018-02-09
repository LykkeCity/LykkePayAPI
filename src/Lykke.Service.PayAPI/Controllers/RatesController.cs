﻿using System;
using System.Net;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.PayAPI.Core.Domain.Rates;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using Lykke.Service.PayAPI.Attributes;

namespace Lykke.Service.PayAPI.Controllers
{
    [Route("api/[controller]")]
    [SignatureVerification]
    public class RatesController : Controller
    {
        private readonly IRatesService _ratesService;
        private readonly ILog _log;

        public RatesController(
            IRatesService ratesService,
            ILog log)
        {
            _ratesService = ratesService ?? throw new ArgumentNullException(nameof(ratesService));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>
        /// Get asset pair rates
        /// </summary>
        /// <param name="assetPairId">Asset pair identifier.</param>
        /// <returns>Asset pair rates</returns>
        [HttpGet]
        [Route("{assetPairId}")]
        [SwaggerOperation("GetAssetPairRates")]
        [ProducesResponseType(typeof(AssetPairResponseModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAssetPairRates(string assetPairId)
        {
            try
            {
                AssetPairRate rate = await _ratesService.Get(assetPairId);

                return Ok(rate.ToApiModel());
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(RatesController), nameof(GetAssetPairRates),
                    new {AssetPairId = assetPairId}.ToJson(), ex);
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }
    }
}