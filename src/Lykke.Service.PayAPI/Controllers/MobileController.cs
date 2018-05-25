using System;
using System.Net;
using System.Threading.Tasks;
using Common;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayAPI.Attributes;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayAPI.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/mobile")]
    public class MobileController : Controller
    {
        private readonly IAuthService _authService;

        public MobileController([NotNull] IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        /// <summary>
        /// Authenticates user
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("auth")]
        [SwaggerOperation(nameof(Auth))]
        [ValidateModel]
        [ProducesResponseType(typeof(AuthResponseModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public IActionResult Auth([FromBody] AuthRequestModel request)
        {
            if (!request.Email.IsValidEmail())
                return BadRequest(ErrorResponse.Create("Invalid email"));

            //todo
            return Ok(new AuthResponseModel
            {
                Token = _authService.CreateToken(),
                FirstTime = true
            });
        }

        /// <summary>
        /// Updates user password
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [BearerHeader]
        [HttpPost]
        [Route("password")]
        [SwaggerOperation(nameof(UpdatePassword))]
        [ValidateModel]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequestModel request)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates pin code
        /// </summary>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [BearerHeader]
        [HttpPost]
        [Route("pin")]
        [SwaggerOperation(nameof(UpdatePin))]
        [ValidateModel]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdatePin([FromBody] UpdatePinRequestModel request)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if pin is valid
        /// </summary>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [BearerHeader]
        [HttpPost]
        [Route("pin/validation")]
        [SwaggerOperation(nameof(ValidatePin))]
        [ValidateModel]
        [ProducesResponseType(typeof(ValidatePinResponseModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ValidatePin([FromBody] ValidatePinRequestModel request)
        {
            throw new NotImplementedException();
        }
    }
}
