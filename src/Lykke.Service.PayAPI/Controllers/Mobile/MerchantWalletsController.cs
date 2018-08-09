using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.Log;
using Lykke.Service.PayAPI.Attributes;
using Lykke.Service.PayAPI.Core.Domain.MerchantWallets;
using Lykke.Service.PayAPI.Core.Exceptions;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayAPI.Models;
using Lykke.Service.PayInternal.Client.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Lykke.Service.PayAPI.Controllers.Mobile
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/mobile/merchantWallets")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [BearerHeader]
    [Produces("application/json")]
    public class MerchantWalletsController : Controller
    {
        private readonly IMerchantWalletsService _merchantWalletsService;
        private readonly ILog _log;

        public MerchantWalletsController([NotNull] IMerchantWalletsService merchantWalletsService,
            ILogFactory logFactory)
        {
            _merchantWalletsService = merchantWalletsService ?? throw new ArgumentNullException(nameof(merchantWalletsService));
            _log = logFactory?.CreateLog(this) ?? throw new ArgumentNullException(nameof(logFactory));
        }

        /// <summary>
        /// Get balances
        /// </summary>
        /// <remarks>
        /// Receive merchant wallets with balances in base asset and converted to given asset.
        /// </remarks>
        /// <param name="convertAssetId">Asset id to convert balances to</param>
        /// <response code="200">List of merchant wallet balances</response>
        /// <response code="404">Merchant not found</response>
        /// <response code="501">Blockchain support not implemented</response>
        /// <response code="502">Internal service request error</response>
        [HttpGet]
        [SwaggerOperation(OperationId = "GetMerchantWalletConvertedBalances")]
        [SwaggerXSummary("Balances")]
        [ProducesResponseType(typeof(IEnumerable<MerchantWalletConvertedBalanceResponse>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotImplemented)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadGateway)]
        public async Task<IActionResult> GetBalances([CanBeNull] [FromQuery] string convertAssetId)
        {
            string merchantId = this.GetUserMerchantId();

            try
            {
                IReadOnlyList<MerchantWalletBalanceLine> balances =
                    await _merchantWalletsService.GetBalancesAsync(merchantId, convertAssetId);

                return Ok(Mapper.Map<IEnumerable<MerchantWalletConvertedBalanceResponse>>(balances));
            }
            catch (MerchantNotFoundException e)
            {
                _log.Error(e, null, $@"request:{
                        new
                        {
                            merchantId,
                            convertAssetId
                        }.ToJson()
                    }");

                return NotFound(ErrorResponse.Create(e.Message));
            }
            catch (BlockchainSupportNotImplemented e)
            {
                _log.Error(e, null, $@"request:{
                        new
                        {
                            merchantId,
                            convertAssetId
                        }.ToJson()
                    }
                ");

                return StatusCode((int) HttpStatusCode.NotImplemented, ErrorResponse.Create(e.Message));
            }
            catch (DefaultErrorResponseException e) when (e.StatusCode == HttpStatusCode.BadGateway)
            {
                _log.Error(e, null, $@"request:{
                        new
                        {
                            merchantId,
                            convertAssetId
                        }.ToJson()
                    }
                ");

                return StatusCode((int) HttpStatusCode.BadGateway, ErrorResponse.Create(e.Message));
            }
        }
    }
}
