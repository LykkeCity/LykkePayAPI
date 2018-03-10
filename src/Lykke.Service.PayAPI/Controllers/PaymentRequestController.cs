using System;
using System.Net;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayAPI.Attributes;
using Lykke.Service.PayAPI.Core.Exceptions;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using CreatePaymentRequestModel = Lykke.Service.PayAPI.Models.CreatePaymentRequestModel;

namespace Lykke.Service.PayAPI.Controllers
{
    [Authorize]
    [SignatureHeaders]
    [Route("api/[controller]")]
    public class PaymentRequestController : BaseController
    {
        private readonly IPaymentRequestService _paymentRequestService;

        public PaymentRequestController(
            IPaymentRequestService paymentRequestService,
            ILog log) : base(log)
        {
            _paymentRequestService =
                paymentRequestService ?? throw new ArgumentNullException(nameof(paymentRequestService));
        }

        /// <summary>
        /// Creates payment request and order
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerOperation("CreatePaymentRequest")]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(CreatePaymentResponseModel), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> CreatePaymentRequest([FromBody] CreatePaymentRequestModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse().AddErrors(ModelState));

            try
            {
                var domainRequest = request.ToDomain(MerchantId);

                var response = await _paymentRequestService.CreatePaymentRequestAsync(domainRequest);

                return Ok(response.ToApiModel());
            }
            catch (Exception ex)
            {
                await Log.WriteErrorAsync(nameof(PaymentRequestController), nameof(CreatePaymentRequest),
                    request.ToJson(), ex);

                if (ex is ApiRequestException apiRequestException)
                {
                    return apiRequestException.GenerateErrorResponse();
                }
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// The method returns status on a payment by the address
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{address}/status")]
        [SwaggerOperation("GetPaymentStatus")]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(PaymentStatusResponseModel), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> GetPaymentStatus(string address)
        {
            try
            {
                var paymentRequestDetails = await _paymentRequestService.GetPaymentRequestDetailsAsync(address);

                return Ok(paymentRequestDetails.ToStatusApiModel());
            }
            catch (Exception ex)
            {
                await Log.WriteErrorAsync(nameof(PaymentRequestController), nameof(GetPaymentStatus),
                    new {Address = address}.ToJson(), ex);

                if (ex is ApiRequestException apiRequestException)
                {
                    return apiRequestException.GenerateErrorResponse();
                }
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }

        [HttpPost]
        [Route("refund")]
        [SwaggerOperation("Refund")]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(RefundResponseModel), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> Refund([FromBody] RefundRequestModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse().AddErrors(ModelState));

            try
            {
                var response = _paymentRequestService.RefundAsync(request.PaymentRequestId, request.Address);

                return Ok(response);
            }
            catch (Exception ex)
            {
                await Log.WriteErrorAsync(nameof(PaymentRequestController), nameof(Refund), request.ToJson(), ex);

                if (ex is ApiRequestException apiRequestException)
                {
                    return apiRequestException.GenerateErrorResponse();
                }
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }
    }
}
