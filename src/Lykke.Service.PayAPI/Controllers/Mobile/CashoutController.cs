using System;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.PayAPI.Attributes;
using Lykke.Service.PayAPI.Models.Mobile.Cashout;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayAPI.Controllers.Mobile
{
    [ApiVersion("1.0")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [BearerHeader]
    [Route("api/v{version:apiVersion}/mobile/cashout")]
    public class CashoutController : ControllerBase
    {
        /// <summary>
        /// Executes cashout request
        /// </summary>
        /// <param name="request">Cashout request details</param>
        /// <response code="200">Cashout operation has been successfully executed</response>
        [HttpPost]
        [SwaggerOperation(nameof(Execute))]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        [ValidateModel]
        public async Task<IActionResult> Execute([FromBody] CashoutModel request)
        {
            throw new NotImplementedException();
        }
    }
}
