using AutoMapper;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayAPI.Attributes;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayAPI.Filters;
using Lykke.Service.PayAPI.Models.Mobile.History;
using Lykke.Service.PayHistory.Client.Publisher;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

namespace Lykke.Service.PayAPI.Controllers.Mobile
{
    [ApiVersion("1.0")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ValidateActionParametersFilter]
    [BearerHeader]
    [Route("api/v{version:apiVersion}/mobile/history/[action]")]
    public class HistoryController : Controller
    {
        private readonly ILog _log;
        private readonly IPayHistoryService _payHistoryService;

        public HistoryController(IPayHistoryService payHistoryService, ILog log)
        {
            _payHistoryService = payHistoryService;
            _log = log.CreateComponentScope(nameof(HistoryController)) ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>
        /// Returns list of history operations.
        /// </summary>
        /// <response code="200">A collection of history operations.</response>
        /// <response code="400">Problem occured.</response>        
        [HttpGet]
        [SwaggerOperation("History")]
        [ProducesResponseType(typeof(IReadOnlyList<HistoryOperationViewModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Index()
        {
            var merchantId = this.GetUserMerchantId();
            var historyOperations = await _payHistoryService.GetHistoryAsync(merchantId);
            var results = Mapper.Map<IReadOnlyList<HistoryOperationViewModel>>(historyOperations);
            return Ok(results);
        }

        /// <summary>
        /// Returns details of the history operation.
        /// </summary>
        /// <response code="200">A details of the history operation.</response>
        /// <response code="400">Problem occured.</response>        
        /// <response code="404">History operation is not found.</response>        
        [HttpGet]
        [SwaggerOperation("HistoryDetails")]
        [ProducesResponseType(typeof(HistoryOperationModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Details([Required, PartitionOrRowKey]string id)
        {
            var merchantId = this.GetUserMerchantId();

            var historyOperation = await _payHistoryService.GetDetailsAsync(merchantId, id);
            if (historyOperation == null)
            {
                return NotFound();
            }

            var result = Mapper.Map<HistoryOperationModel>(historyOperation);
            return Ok(result);
        }
    }
}
