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
using Lykke.Service.PayAPI.Models;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Exceptions;
using Lykke.Service.PayInternal.Client.Models.Exchange;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Refit;
using Swashbuckle.AspNetCore.SwaggerGen;
using ExchangeResponse = Lykke.Service.PayAPI.Models.ExchangeResponse;
using AssetRateResponse = Lykke.Service.PayAPI.Models.AssetRateResponse;
using ExchangeClientResponse = Lykke.Service.PayInternal.Client.Models.Exchange.ExchangeResponse;
using AssetRateClientResponse = Lykke.Service.PayInternal.Client.Models.AssetRates.AssetRateResponse;

namespace Lykke.Service.PayAPI.Controllers.Mobile
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/mobile/exchange/[action]")]
    [Microsoft.AspNetCore.Authorization.Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [BearerHeader]
    public class ExchangeController : Controller
    {
        private readonly IPayInternalClient _payInternalClient;
        private readonly ILog _log;

        public ExchangeController(
            [NotNull] ILogFactory logFactory, 
            [NotNull] IPayInternalClient payInternalClient)
        {
            _payInternalClient = payInternalClient ?? throw new ArgumentNullException(nameof(payInternalClient));
            _log = logFactory?.CreateLog(this) ?? throw new ArgumentNullException(nameof(logFactory));
        }

        /// <summary>
        /// Returns current exchange rate for the asset pair
        /// </summary>
        /// <param name="baseAssetId">Base asset id</param>
        /// <param name="quotingAssetId">Quoting asset id</param>
        /// <response code="200">Asset pair rate</response>
        /// <response code="400">Bad request</response>
        [HttpGet]
        [Route("{baseAssetId}/{quotingAssetId}")]
        [SwaggerOperation(nameof(GetRate))]
        [ProducesResponseType(typeof(AssetRateResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetRate(string baseAssetId, string quotingAssetId)
        {
            baseAssetId = Uri.UnescapeDataString(baseAssetId);

            quotingAssetId = Uri.UnescapeDataString(quotingAssetId);

            try
            {
                AssetRateClientResponse response =
                    await _payInternalClient.GetCurrentAssetPairRateAsync(baseAssetId, quotingAssetId);

                return Ok(Mapper.Map<AssetRateResponse>(response));
            }
            catch (DefaultErrorResponseException e) when (e.StatusCode == HttpStatusCode.BadRequest)
            {
                var apiException = e.InnerException as ApiException;

                if (apiException?.StatusCode == HttpStatusCode.BadRequest)
                    return BadRequest(apiException.GetContentAs<ErrorResponse>());

                _log.Error(e, null, $@"request:{
                        new
                        {
                            baseAssetId,
                            quotingAssetId
                        }.ToJson()
                    }");

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (DefaultErrorResponseException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                var apiException = e.InnerException as ApiException;

                if (apiException?.StatusCode == HttpStatusCode.NotFound)
                    return BadRequest(apiException.GetContentAs<ErrorResponse>());

                _log.Error(e, null, $@"request:{
                        new
                        {
                            baseAssetId,
                            quotingAssetId
                        }.ToJson()
                    }");

                return NotFound(ErrorResponse.Create(e.Message));
            }
        }

        /// <summary>
        /// Executes exchange operation
        /// </summary>
        /// <param name="request">Exchange operation request details</param>
        /// <returns></returns>
        /// <response code="200">Exchange operation completed successfully</response>
        /// <response code="400">Bad request</response>
        [HttpPost]
        [SwaggerOperation(nameof(Execute))]
        [ProducesResponseType(typeof(ExchangeResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> Execute([FromBody] ExchangeModel request)
        {
            string merchantId = this.GetUserMerchantId();

            try
            {
                var clientRequest = Mapper.Map<ExchangeRequest>(request, opt => opt.Items["MerchantId"] = merchantId);

                ExchangeClientResponse response = await _payInternalClient.ExchangeAsync(clientRequest);

                return Ok(Mapper.Map<ExchangeResponse>(response));
            }
            catch (DefaultErrorResponseException e) when (e.StatusCode == HttpStatusCode.BadRequest)
            {
                var apiException = e.InnerException as ApiException;

                if (apiException?.StatusCode == HttpStatusCode.BadRequest)
                    return BadRequest(apiException.GetContentAs<ErrorResponse>());

                _log.Error(e, null, $"request:{request.ToJson()}");
                return BadRequest(ErrorResponse.Create(e.Message));
            }
        }

        /// <summary>
        /// Returns current exchange rate
        /// </summary>
        /// <param name="request">PreExchange operation request details</param>
        /// <returns></returns>
        /// <response code="200">PreExchange operation completed successfully</response>
        /// <response code="400">Bad request</response>
        [HttpPost]
        [SwaggerOperation(nameof(PreExchange))]
        [ProducesResponseType(typeof(ExchangeResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> PreExchange([FromBody] PreExchangeModel request)
        {
            string merchantId = this.GetUserMerchantId();

            try
            {
                var clientRequest = Mapper.Map<PreExchangeRequest>(request, opt => opt.Items["MerchantId"] = merchantId);

                ExchangeClientResponse response = await _payInternalClient.PreExchangeAsync(clientRequest);

                return Ok(Mapper.Map<ExchangeResponse>(response));
            }
            catch (DefaultErrorResponseException e) when (e.StatusCode == HttpStatusCode.BadRequest)
            {
                var apiException = e.InnerException as ApiException;

                if (apiException?.StatusCode == HttpStatusCode.BadRequest)
                    return BadRequest(apiException.GetContentAs<ErrorResponse>());

                _log.Error(e, null, $"request:{request.ToJson()}");
                return BadRequest(ErrorResponse.Create(e.Message));
            }
        }
    }
}
