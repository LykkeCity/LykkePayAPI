using System;
using System.Net;
using System.Threading.Tasks;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayAPI.Attributes;
using Lykke.Service.PayAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayAPI.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/mobile")]
    public class MobileController : Controller
    {
        /// <summary>
        /// Authenticates user
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("auth")]
        [SwaggerOperation(nameof(Auth))]
        [ValidateModel]
        [ProducesResponseType(typeof(AuthResponseModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Auth([FromBody] AuthRequestModel request)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates user password
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("password")]
        [SwaggerOperation(nameof(UpdatePassword))]
        [ValidateModel]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequestModel request)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates pin code
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("pin")]
        [SwaggerOperation(nameof(UpdatePin))]
        [ValidateModel]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdatePin([FromBody] UpdatePinRequestModel request)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if pin is valid
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("pin/validation")]
        [SwaggerOperation(nameof(ValidatePin))]
        [ValidateModel]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ValidatePin([FromBody] ValidatePinRequestModel request)
        {
            throw new NotImplementedException();
        }
    }
}
