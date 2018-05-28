using System;
using System.Net;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayAPI.Attributes;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayAPI.Models;
using Lykke.Service.PayAuth.Client;
using Lykke.Service.PayAuth.Client.Models.Employees;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayAPI.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/mobile")]
    public class SecurityController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IPayAuthClient _payAuthClient;

        public SecurityController(
            [NotNull] IAuthService authService, 
            [NotNull] IPayAuthClient payAuthClient)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _payAuthClient = payAuthClient ?? throw new ArgumentNullException(nameof(payAuthClient));
        }

        /// <summary>
        /// Authenticates user
        /// </summary>
        /// <param name="request">Authorization request details</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("auth")]
        [SwaggerOperation(nameof(Auth))]
        [ValidateModel]
        [ProducesResponseType(typeof(AuthResponseModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Auth([FromBody] AuthRequestModel request)
        {
            ValidateResultModel validationResult =
                await _payAuthClient.ValidatePasswordAsync(request.Email, request.Password);

            if (!validationResult.Success)
                return BadRequest(ErrorResponse.Create("Invalid email or password"));

            return Ok(new AuthResponseModel
            {
                Token = _authService.CreateToken(request.Email),
                ForcePasswordUpdate = validationResult.ForcePasswordUpdate
            });
        }

        /// <summary>
        /// Updates user password
        /// </summary>
        /// <param name="request">Update password request details</param>
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
            string email = this.GetUserEmail();

            ValidateResultModel validationResult =
                await _payAuthClient.ValidatePasswordAsync(email, request.CurrentPasssword);

            if (!validationResult.Success)
                return BadRequest(ErrorResponse.Create("Invalid password"));

            await _payAuthClient.UpdatePasswordHashAsync(new UpdatePasswordHashModel
            {
                Email = email,
                PasswordHash = request.NewPasswordHash
            });

            return Ok();
        }

        /// <summary>
        /// Updates pin code
        /// </summary>
        /// <param name="request">Pin code update details</param>
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
            string email = this.GetUserEmail();

            await _payAuthClient.UpdatePinHashAsync(new UpdatePinHashModel
            {
                Email = email,
                PinHash = request.NewPinCodeHash
            });

            return Ok();
        }

        /// <summary>
        /// Checks if pin is valid
        /// </summary>
        /// <param name="request">Pin validation request details</param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [BearerHeader]
        [HttpPost]
        [Route("pin/validation")]
        [SwaggerOperation(nameof(ValidatePin))]
        [ValidateModel]
        [ProducesResponseType(typeof(ValidatePinResponseModel), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> ValidatePin([FromBody] ValidatePinRequestModel request)
        {
            string email = this.GetUserEmail();

            ValidateResultModel validationResult = await _payAuthClient.ValidatePinAsync(email, request.PinCode);

            return Ok(new ValidatePinResponseModel {Passed = validationResult.Success});
        }
    }
}
