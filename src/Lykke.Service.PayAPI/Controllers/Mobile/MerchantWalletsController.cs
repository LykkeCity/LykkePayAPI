using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayAPI.Attributes;
using Lykke.Service.PayAPI.Core.Domain.MerchantWallets;
using Lykke.Service.PayAPI.Core.Exceptions;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayAPI.Models;
using Lykke.Service.PayInternal.Client.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayAPI.Controllers.Mobile
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/mobile/merchantWallets")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [BearerHeader]
    public class MerchantWalletsController : Controller
    {
        private readonly IMerchantWalletsService _merchantWalletsService;
        private readonly ILog _log;

        public MerchantWalletsController([NotNull] IMerchantWalletsService merchantWalletsService, ILog log)
        {
            _merchantWalletsService = merchantWalletsService ?? throw new ArgumentNullException(nameof(merchantWalletsService));
            _log = log.CreateComponentScope(nameof(MerchantWalletsController)) ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>
        /// Returns merchant wallets with balances in base asset and converted to given asset
        /// </summary>
        /// <param name="convertAssetId">Asset id to convert balances to</param>
        /// <response code="200">List of merchant wallet balances</response>
        /// <response code="404">Merchant not found</response>
        /// <response code="501">Blockchain support not implemented</response>
        /// <response code="502">Internal service request error</response>
        [HttpGet]
        [SwaggerOperation("GetMerchantWalletConvertedBalances")]
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
                _log.WriteError(nameof(GetBalances), new
                {
                    merchantId,
                    convertAssetId
                }, e);

                return NotFound(ErrorResponse.Create(e.Message));
            }
            catch (BlockchainSupportNotImplemented e)
            {
                _log.WriteError(nameof(GetBalances), new
                {
                    merchantId,
                    convertAssetId
                }, e);

                return StatusCode((int) HttpStatusCode.NotImplemented, ErrorResponse.Create(e.Message));
            }
            catch (DefaultErrorResponseException e) when (e.StatusCode == HttpStatusCode.BadGateway)
            {
                _log.WriteError(nameof(GetBalances), new
                {
                    merchantId,
                    convertAssetId
                }, e);

                return StatusCode((int) HttpStatusCode.BadGateway, ErrorResponse.Create(e.Message));
            }
        }
    }
}
