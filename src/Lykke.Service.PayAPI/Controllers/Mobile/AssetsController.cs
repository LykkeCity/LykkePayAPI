﻿using System;
using System.Collections.Generic;
using System.Net;
using AutoMapper;
using JetBrains.Annotations;
using Lykke.Service.PayAPI.Attributes;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayAPI.Models.Mobile.Assets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayAPI.Controllers.Mobile
{
    [ApiVersion("1.0")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [BearerHeader]
    [Route("api/v{version:apiVersion}/mobile/assets")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [NonController]
    public class AssetsController : ControllerBase
    {
        private readonly IAssetSettingsService _assetSettingsService;

        public AssetsController([NotNull] IAssetSettingsService assetSettingsService)
        {
            _assetSettingsService = assetSettingsService ?? throw new ArgumentNullException(nameof(assetSettingsService));
        }

        /// <summary>
        /// Get cashout assets
        /// </summary>
        /// <remarks>
        /// Receive list of available assets for bank cash out.
        /// </remarks>
        /// <response code="200">List of asset names</response>
        [HttpGet]
        [Route("cashout")]
        [SwaggerOperation(OperationId = nameof(GetCashoutAssets))]
        [SwaggerXSummary("Cashout assets")]
        [ProducesResponseType(typeof(IReadOnlyList<CashoutAssetResponse>), (int) HttpStatusCode.OK)]
        public IActionResult GetCashoutAssets()
        {
            var assets = Mapper.Map<IReadOnlyList<CashoutAssetResponse>>(_assetSettingsService.GetCashoutAssets());

            return Ok(assets);
        }
    }
}
