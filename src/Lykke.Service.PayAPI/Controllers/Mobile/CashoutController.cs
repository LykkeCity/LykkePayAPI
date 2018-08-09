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
using Lykke.Service.PayAPI.Models.Mobile.Cashout;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Exceptions;
using Lykke.Service.PayInternal.Client.Models.Cashout;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Refit;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayAPI.Controllers.Mobile
{
    [ApiVersion("1.0")]
    [Microsoft.AspNetCore.Authorization.Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [BearerHeader]
    [Route("api/v{version:apiVersion}/mobile/cashout")]
    [Produces("application/json")]
    public class CashoutController : Controller
    {
        private readonly IPayInternalClient _payInternalClient;
        private readonly ILog _log;

        public CashoutController(
            [NotNull] IPayInternalClient payInternalClient, 
            [NotNull] ILogFactory logFactory)
        {
            _payInternalClient = payInternalClient ?? throw new ArgumentNullException(nameof(payInternalClient));
            _log = logFactory?.CreateLog(this) ?? throw new ArgumentNullException(nameof(logFactory));
        }

        /// <summary>
        /// Execute cashout
        /// </summary>
        /// <remarks>
        /// Execute cashout request for current merchant.
        /// </remarks>
        /// <param name="request">Cashout request details</param>
        /// <response code="200">Cashout operation has been successfully executed</response>
        /// <response code="400">Bad request</response>
        [HttpPost]
        [SwaggerOperation(OperationId = nameof(Execute))]
        [SwaggerXSummary("Execute cashout")]
        [ProducesResponseType(typeof(CashoutResponseModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> Execute([FromBody] CashoutModel request)
        {
            try
            {
                var clientRequest = Mapper.Map<CashoutRequest>(request, opt =>
                {
                    opt.Items["MerchantId"] = this.GetUserMerchantId();
                    opt.Items["EmployeeEmail"] = this.GetUserEmail();
                });

                CashoutResponse response = await _payInternalClient.CashoutAsync(clientRequest);

                return Ok(Mapper.Map<CashoutResponseModel>(response));
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
