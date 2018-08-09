using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.Log;
using Lykke.Service.PayAPI.Attributes;
using Lykke.Service.PayAPI.Core.Domain.Rates;
using Lykke.Service.PayAPI.Core.Exceptions;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Lykke.Service.PayAPI.Controllers
{
    [Authorize]
    [SignatureHeaders]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/rates")]
    [Produces("application/json")]
    public class RatesController : Controller
    {
        private readonly IRatesService _ratesService;
        private readonly ILog _log;

        public RatesController(
            IRatesService ratesService,
            ILogFactory logFactory)
        {
            _ratesService = ratesService ?? throw new ArgumentNullException(nameof(ratesService));
            _log = logFactory?.CreateLog(this) ?? throw new ArgumentNullException(nameof(logFactory));
        }

        /// <summary>
        /// Get asset pair rate
        /// </summary>
        /// <param name="assetPairId">Asset pair id</param>
        /// <response code="200">Result model</response>
        /// <response code="400">Problem occured</response>
        [HttpGet]
        [Route("{assetPairId}")]
        [SwaggerOperation(OperationId = "GetAssetPairRates")]
        [SwaggerXSummary("Asset pair rate")]
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
    }
}
