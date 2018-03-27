using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayAPI.Attributes;
using Lykke.Service.PayAPI.Core.Domain.PaymentRequest;
using Lykke.Service.PayAPI.Core.Exceptions;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayAPI.Models;
using Lykke.Service.PayCallback.Client;
using Lykke.Service.PayCallback.Client.Models;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using CreatePaymentRequestModel = Lykke.Service.PayAPI.Models.CreatePaymentRequestModel;
using RefundResponse = Lykke.Service.PayAPI.Core.Domain.PaymentRequest.RefundResponse;

namespace Lykke.Service.PayAPI.Controllers
{
    [Authorize]
    [SignatureHeaders]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class PaymentRequestController : Controller
    {
        private readonly IPaymentRequestService _paymentRequestService;
        private readonly IPayCallbackClient _payCallbackClient;
        private readonly IHeadersHelper _headersHelper;
        private readonly ILog _log;

        public PaymentRequestController(
            IPaymentRequestService paymentRequestService,
            IPayCallbackClient payCallbackClient,
            IHeadersHelper headersHelper,
            ILog log)
        {
            _paymentRequestService =
                paymentRequestService ?? throw new ArgumentNullException(nameof(paymentRequestService));
            _payCallbackClient = payCallbackClient ?? throw new ArgumentNullException(nameof(payCallbackClient));
            _headersHelper = headersHelper ?? throw new ArgumentNullException(nameof(headersHelper));
            _log = log ?? throw new ArgumentNullException(nameof(log));
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
        [ProducesResponseType(typeof(PaymentStatusResponseModel), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> CreatePaymentRequest([FromBody] CreatePaymentRequestModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse().AddErrors(ModelState));

            try
            {
                var domainRequest =
                    Mapper.Map<CreatePaymentRequest>(request, opt => opt.Items["MerchantId"] = _headersHelper.MerchantId);

                CreatePaymentResponse createResponse =
                    await _paymentRequestService.CreatePaymentRequestAsync(domainRequest);

                PaymentRequestDetailsModel paymentRequestDetails =
                    await _paymentRequestService.GetPaymentRequestDetailsAsync(_headersHelper.MerchantId, createResponse.Id);

                return Ok(paymentRequestDetails.ToStatusApiModel());
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(PaymentRequestController), nameof(CreatePaymentRequest),
                    request.ToJson(), ex);

                if (ex is ApiRequestException apiRequestException)
                {
                    return apiRequestException.GenerateErrorResponse();
                }
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Returns status of a payment request
        /// </summary>
        /// <param name="paymentRequestId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{paymentRequestId}/status")]
        [SwaggerOperation("GetPaymentRequestStatus")]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(PaymentStatusResponseModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetPaymentRequestStatus(string paymentRequestId)
        {
            if (!paymentRequestId.IsValidPaymentRequestId())
                return BadRequest(ErrorResponse.Create($"{nameof(paymentRequestId)} has invalid value"));

            try
            {
                var paymentRequestDetails =
                    await _paymentRequestService.GetPaymentRequestDetailsAsync(_headersHelper.MerchantId, paymentRequestId);

                return Ok(paymentRequestDetails.ToStatusApiModel());
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(PaymentRequestController), nameof(GetPaymentRequestStatus),
                    new {paymentRequestId}.ToJson(), ex);

                if (ex is ApiRequestException apiRequestException)
                {
                    return apiRequestException.GenerateErrorResponse();
                }
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Initiates a refund on a payment
        /// </summary>
        /// <param name="paymentRequestId"></param>
        /// <param name="destinationAddress"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{paymentRequestId}/refund")]
        [SwaggerOperation("Refund")]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(PaymentStatusResponseModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Refund(string paymentRequestId, [FromQuery] string destinationAddress)
        {
            if (!paymentRequestId.IsValidPaymentRequestId())
                return BadRequest(ErrorResponse.Create($"{nameof(paymentRequestId)} has invalid value"));

            try
            {
                RefundResponse refundResponse = await _paymentRequestService.RefundAsync(new RefundRequest
                {
                    MerchantId = _headersHelper.MerchantId,
                    PaymentRequestId = paymentRequestId,
                    DestinationAddress = destinationAddress
                });

                PaymentRequestDetailsModel paymentRequestDetails =
                    await _paymentRequestService.GetPaymentRequestDetailsAsync(_headersHelper.MerchantId, refundResponse.PaymentRequestId);

                return Ok(paymentRequestDetails.ToStatusApiModel());
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(PaymentRequestController), nameof(Refund), new
                {
                    paymentRequestId,
                    destinationAddress
                }.ToJson(), ex);

                if (ex is ApiRequestException apiRequestException)
                {
                    return apiRequestException.GenerateErrorResponse();
                }
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Adds or updates payment request callback url
        /// </summary>
        /// <param name="paymentRequestId"></param>
        /// <param name="callbackUrl"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{paymentRequestId}/callback")]
        [SwaggerOperation("SetCallback")]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> SetCallbackUrl(string paymentRequestId, [FromQuery] string callbackUrl)
        {
            if (!paymentRequestId.IsValidPaymentRequestId())
                return BadRequest(ErrorResponse.Create($"{nameof(paymentRequestId)} has invalid value"));

            if(string.IsNullOrWhiteSpace(callbackUrl))
                return BadRequest(ErrorResponse.Create($"{nameof(callbackUrl)} has invalid value"));

            try
            {
                await _payCallbackClient.SetPaymentCallback(new SetPaymentCallbackModel
                {
                    MerchantId = _headersHelper.MerchantId,
                    PaymentRequestId = paymentRequestId,
                    CallbackUrl = callbackUrl
                });

                return Ok();
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(PaymentRequestController), nameof(SetCallbackUrl), new
                {
                    paymentRequestId,
                    callbackUrl
                }.ToJson(), ex);
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }

        [HttpGet]
        [Route("{paymentRequestId}/callback")]
        [SwaggerOperation("GetCallback")]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(GetPaymentCallbackResponseModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetCallbackUrl(string paymentRequestId)
        {
            if (!paymentRequestId.IsValidPaymentRequestId())
                return BadRequest(ErrorResponse.Create($"{nameof(paymentRequestId)} has invalid value"));

            try
            {
                GetPaymentCallbackModel callbackInfo =
                    await _payCallbackClient.GetPaymentCallback(_headersHelper.MerchantId, paymentRequestId);

                return Ok(Mapper.Map<GetPaymentCallbackResponseModel>(callbackInfo));
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(PaymentRequestController), nameof(GetCallbackUrl),
                    new {paymentRequestId}.ToJson(), ex);
            }

            return NotFound();
        }
    }
}
