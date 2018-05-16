using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayAPI.Attributes;
using Lykke.Service.PayAPI.Core.Domain.Rates;
using Lykke.Service.PayAPI.Core.Exceptions;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayAPI.Models;
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
                _log.WriteError(nameof(GetAssetPairRates), new {AssetPairId = assetPairId}, ex);

                if (ex is ApiRequestException apiException)
                {
                    return apiException.GenerateErrorResponse();
                }
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }
    }
}
