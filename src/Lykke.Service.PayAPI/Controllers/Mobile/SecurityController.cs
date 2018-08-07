using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayAPI.Attributes;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayAPI.Models;
using Lykke.Service.PayAuth.Client;
using Lykke.Service.PayAuth.Client.Models.Employees;
using Lykke.Service.PayPushNotifications.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Refit;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayAPI.Controllers.Mobile
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/mobile")]
    [Produces("application/json")]
    public class SecurityController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IPayAuthClient _payAuthClient;
        private readonly IPayPushNotificationsClient _payPushNotificationsClient;

        public SecurityController(
            [NotNull] IAuthService authService, 
            [NotNull] IPayAuthClient payAuthClient,
            [NotNull] IPayPushNotificationsClient payPushNotificationsClient)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _payAuthClient = payAuthClient ?? throw new ArgumentNullException(nameof(payAuthClient));
            _payPushNotificationsClient = payPushNotificationsClient;
        }

        /// <summary>
        /// Authenticates user
        /// </summary>
        /// <param name="request">Authorization request details</param>
        /// <response code="200">Authorization succedded</response>
        /// <response code="400">Invalid email or password</response>
        [AllowAnonymous]
        [HttpPost]
        [Route("auth")]
        [SwaggerOperation(nameof(Auth))]
        [ProducesResponseType(typeof(AuthResponseModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> Auth([FromBody] AuthRequestModel request)
        {
            ValidateResultModel validationResult;

            try
            {
                validationResult = await _payAuthClient.ValidatePasswordAsync(request.Email, request.Password);
            }
            catch (ErrorResponseException e)
            {
                var apiException = e.InnerException as ApiException;

                if (apiException?.StatusCode == HttpStatusCode.BadRequest)
                    return BadRequest(apiException.GetContentAs<ErrorResponse>());

                throw;
            }

            if (!validationResult.Success)
                return BadRequest(ErrorResponse.Create("Invalid email or password"));

            return Ok(new AuthResponseModel
            {
                Token = _authService.CreateToken(request.Email, validationResult.EmployeeId,
                    validationResult.MerchantId),
                EmployeeId = validationResult.EmployeeId,
                MerchantId = validationResult.MerchantId,
                ForcePasswordUpdate = validationResult.ForcePasswordUpdate,
                ForcePinUpdate = validationResult.ForcePinUpdate,
                NotificationIds = await _payPushNotificationsClient.GetNotificationIdsAsync(
                    validationResult.EmployeeId, validationResult.MerchantId)
            });
        }

        /// <summary>
        /// Updates user password
        /// </summary>
        /// <param name="request">Update password request details</param>
        /// <response code="200">Password updated successfully</response>
        /// <response code="400">Invalid email, password or password must be different from previous password</response>
        [Microsoft.AspNetCore.Authorization.Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [BearerHeader]
        [HttpPost]
        [Route("password")]
        [SwaggerOperation(nameof(UpdatePassword))]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequestModel request)
        {
            if (request.CurrentPasssword.Equals(request.NewPasswordHash))
                return BadRequest(
                    ErrorResponse.Create("Your new password must be different from your previous password."));

            string email = this.GetUserEmail();

            ValidateResultModel validationResult;

            try
            {
                validationResult = await _payAuthClient.ValidatePasswordAsync(email, request.CurrentPasssword);
            }
            catch (ErrorResponseException e)
            {
                var apiException = e.InnerException as ApiException;

                if (apiException?.StatusCode == HttpStatusCode.BadRequest)
                    return BadRequest(apiException.GetContentAs<ErrorResponse>());

                throw;
            }

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
        /// <response code="200">Pin updated successfully</response>
        /// <response code="400">Email is invalid or there is no credentials</response>
        [Microsoft.AspNetCore.Authorization.Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [BearerHeader]
        [HttpPost]
        [Route("pin")]
        [SwaggerOperation(nameof(UpdatePin))]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> UpdatePin([FromBody] UpdatePinRequestModel request)
        {
            string email = this.GetUserEmail();

            try
            {
                await _payAuthClient.UpdatePinHashAsync(new UpdatePinHashModel
                {
                    Email = email,
                    PinHash = request.NewPinCodeHash
                });
            }
            catch (ErrorResponseException e)
            {
                var apiException = e.InnerException as ApiException;

                if (apiException?.StatusCode == HttpStatusCode.BadRequest)
                    return BadRequest(apiException.GetContentAs<ErrorResponse>());

                throw;
            }

            return Ok();
        }

        /// <summary>
        /// Checks if pin is valid
        /// </summary>
        /// <param name="request">Pin validation request details</param>
        /// <response code="200">The pin has been successfully validated</response>
        /// <response code="400">Email is invalid</response>
        [Microsoft.AspNetCore.Authorization.Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [BearerHeader]
        [HttpPost]
        [Route("pin/validation")]
        [SwaggerOperation(nameof(ValidatePin))]
        [ProducesResponseType(typeof(ValidatePinResponseModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> ValidatePin([FromBody] ValidatePinRequestModel request)
        {
            string email = this.GetUserEmail();

            ValidateResultModel validationResult;

            try
            {
                validationResult = await _payAuthClient.ValidatePinAsync(email, request.PinCode);
            }
            catch (ErrorResponseException e)
            {
                var apiException = e.InnerException as ApiException;

                if (apiException?.StatusCode == HttpStatusCode.BadRequest)
                    return BadRequest(apiException.GetContentAs<ErrorResponse>());

                throw;
            }

            var result = new ValidatePinResponseModel
            {
                Passed = validationResult.Success
            };

            if (validationResult.Success)
            {
                result.NotificationIds = await _payPushNotificationsClient.GetNotificationIdsAsync(
                    validationResult.EmployeeId, validationResult.MerchantId);
            }

            return Ok(result);
        }
    }
}
