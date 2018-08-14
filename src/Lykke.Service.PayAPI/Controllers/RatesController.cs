using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.Log;
using Lykke.Service.PayAPI.Attributes;
using Lykke.Service.PayAPI.Core.Domain.Rates;
using Lykke.Service.PayAPI.Core.Exceptions;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayAPI.Models;
using Lykke.Service.PayVolatility.Client;
using Lykke.Service.PayVolatility.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayAPI.Controllers
{
    [Authorize]
    [SignatureHeaders]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/rates")]
    public class RatesController : Controller
    {
        private readonly IRatesService _ratesService;
        private readonly IPayVolatilityClient _payVolatilityClient;
        private readonly ILog _log;

        public RatesController(
            [NotNull] IRatesService ratesService,
            [NotNull] ILogFactory logFactory,
            [NotNull] IPayVolatilityClient payVolatilityClient)
        {
            _ratesService = ratesService ?? throw new ArgumentNullException(nameof(ratesService));
            _payVolatilityClient = payVolatilityClient ?? throw new ArgumentNullException(nameof(payVolatilityClient));
            _log = logFactory.CreateLog(this) ?? throw new ArgumentNullException(nameof(logFactory));
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
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAssetPairRates(string assetPairId)
        {
            if (string.IsNullOrWhiteSpace(assetPairId))
                return BadRequest(ErrorResponse.Create($"{nameof(assetPairId)} has invalid value"));

            try
            {
                AssetPairRate rate = await _ratesService.Get(assetPairId);

                return Ok(Mapper.Map<AssetPairResponseModel>(rate));
            }
            catch (Exception ex)
            {
                _log.Error(ex, null, $"request: {new {assetPairId}.ToJson()}");

                if (ex is ApiRequestException apiException)
                {
                    return apiException.GenerateErrorResponse();
                }
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Get asset pair today's volatility
        /// </summary>
        /// <param name="assetPairId">Asset pair id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("volatility/{assetPairId}")]
        [SwaggerOperation(nameof(GetVolatility))]
        [ProducesResponseType(typeof(VolatilityResponseModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetVolatility(string assetPairId)
        {
            if (string.IsNullOrWhiteSpace(assetPairId))
                return BadRequest(ErrorResponse.Create($"{nameof(assetPairId)} has invalid value"));

            try
            {
                VolatilityModel volatility =
                    await _payVolatilityClient.GetDailyVolatilityAsync(DateTime.Today, assetPairId);

                return Ok(Mapper.Map<VolatilityResponseModel>(volatility));
            }
            catch (Exception ex)
            {
                _log.Error(ex, null, assetPairId);
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }
    }
}
