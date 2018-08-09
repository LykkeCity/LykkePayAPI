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
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using Lykke.Common.Log;
using HistoryOperation = Lykke.Service.PayAPI.Core.Domain.PayHistory.HistoryOperation;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayAPI.Controllers.Mobile
{
    [ApiVersion("1.0")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ValidateActionParametersFilter]
    [BearerHeader]
    [Route("api/v{version:apiVersion}/mobile/history/[action]")]
    [Produces("application/json")]
    public class HistoryController : Controller
    {
        private readonly ILog _log;
        private readonly IPayHistoryService _payHistoryService;

        public HistoryController(IPayHistoryService payHistoryService, ILogFactory logFactory)
        {
            _payHistoryService = payHistoryService;
            _log = logFactory?.CreateLog(this) ?? throw new ArgumentNullException(nameof(logFactory));
        }

        /// <summary>
        /// Get history
        /// </summary>
        /// <remarks>
        /// Receive list of history operations.
        /// </remarks>
        /// <response code="200">A collection of history operations.</response>
        /// <response code="400">Problem occured.</response>        
        [HttpGet]
        [SwaggerOperation(OperationId = "History")]
        [SwaggerXSummary("History")]
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
        /// Get history details
        /// </summary>
        /// <remarks>
        /// Receive details of the history operation.
        /// </remarks>
        /// <response code="200">A details of the history operation.</response>
        /// <response code="400">Problem occured.</response>        
        /// <response code="404">History operation is not found.</response>        
        [HttpGet]
        [SwaggerOperation(OperationId = "HistoryDetails")]
        [SwaggerXSummary("History details")]
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

        /// <summary>
        /// Get latest payment details
        /// </summary>
        /// <remarks>
        /// Receive latest payment details of the history operation.
        /// </remarks>
        /// <response code="200">A details of the history operation.</response>
        /// <response code="404">History operation is not found.</response>  
        [HttpGet]
        [SwaggerOperation(OperationId = "InvoiceLatestPaymentDetails")]
        [SwaggerXSummary("Latest payment details")]
        [ProducesResponseType(typeof(HistoryOperationModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> InvoiceLatestPaymentDetails([Required, PartitionOrRowKey] string invoiceId)
        {
            HistoryOperation operation = await _payHistoryService.GetLatestPaymentDetailsAsync(this.GetUserMerchantId(), invoiceId);

            if (operation == null)
                return NotFound(ErrorResponse.Create("There is no payment operation for the invoice to be fully paid"));

            return Ok(Mapper.Map<HistoryOperationModel>(operation));
        }
    }
}
