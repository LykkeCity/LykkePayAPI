﻿using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Service.PayAPI.Attributes;
using Lykke.Service.PayAPI.Core.Domain.PaymentRequest;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayAPI.Models;
using Lykke.Service.PayCallback.Client;
using Lykke.Service.PayCallback.Client.Models;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using CreatePaymentRequestModel = Lykke.Service.PayAPI.Models.CreatePaymentRequestModel;

namespace Lykke.Service.PayAPI.Controllers
{
    [Authorize]
    [SignatureHeaders]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/paymentRequest")]
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
        [ProducesResponseType(typeof(PaymentStatusResponseModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(PaymentErrorResponseModel), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreatePaymentRequest([FromBody] CreatePaymentRequestModel request)
        {
            if (string.IsNullOrWhiteSpace(request.SettlementAsset))
                return BadRequest(PaymentErrorResponseModel.Create(PaymentErrorType.InvalidSettlementAsset));

            try
            {
                var domainRequest =
                    Mapper.Map<CreatePaymentRequest>(request,
                        opt => opt.Items["MerchantId"] = _headersHelper.MerchantId);

                CreatePaymentResponse createResponse =
                    await _paymentRequestService.CreatePaymentRequestAsync(domainRequest);

                PaymentRequestDetailsModel paymentRequestDetails =
                    await _paymentRequestService.GetPaymentRequestDetailsAsync(_headersHelper.MerchantId,
                        createResponse.Id);

                return Ok(paymentRequestDetails.ToStatusApiModel());
            }
            catch (Exception ex)
            {
                _log.WriteError(nameof(CreatePaymentRequest), request, ex);
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
        [ProducesResponseType(typeof(PaymentStatusResponseModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(PaymentErrorResponseModel), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPaymentRequestStatus(string paymentRequestId)
        {
            if (!paymentRequestId.IsValidPaymentRequestId())
                return BadRequest(PaymentErrorResponseModel.Create(PaymentErrorType.InvalidPaymentId));

            try
            {
                var paymentRequestDetails =
                    await _paymentRequestService.GetPaymentRequestDetailsAsync(_headersHelper.MerchantId,
                        paymentRequestId);

                return Ok(paymentRequestDetails.ToStatusApiModel());
            }
            catch (Exception ex)
            {
                _log.WriteError(nameof(GetPaymentRequestStatus), new {paymentRequestId}, ex);
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
        [ProducesResponseType(typeof(PaymentStatusResponseModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(PaymentErrorResponseModel), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Refund(string paymentRequestId, [FromQuery] string destinationAddress)
        {
            if (!paymentRequestId.IsValidPaymentRequestId())
                return BadRequest(PaymentErrorResponseModel.Create(PaymentErrorType.InvalidPaymentId));

            try
            {
                PaymentRequestDetailsModel paymentRequestDetails = await _paymentRequestService.RefundAsync(
                    new RefundRequest
                    {
                        MerchantId = _headersHelper.MerchantId,
                        PaymentRequestId = paymentRequestId,
                        DestinationAddress = destinationAddress
                    });

                return Ok(paymentRequestDetails.ToStatusApiModel());
            }
            catch (Exception ex)
            {
                _log.WriteError(nameof(Refund), new
                {
                    paymentRequestId,
                    destinationAddress
                }, ex);

                if (ex is PayInternal.Client.Exceptions.RefundErrorResponseException refundEx)
                {
                    return BadRequest(refundEx.ToErrorModel());
                }

                return BadRequest(PaymentErrorResponseModel.Create(PaymentErrorType.RefundIsNotAvailable));
            }
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
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(PaymentErrorResponseModel), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SetCallbackUrl(string paymentRequestId, [FromQuery] string callbackUrl)
        {
            if (!paymentRequestId.IsValidPaymentRequestId())
                return BadRequest(PaymentErrorResponseModel.Create(PaymentErrorType.InvalidPaymentId));

            if (string.IsNullOrWhiteSpace(callbackUrl))
                return BadRequest(PaymentErrorResponseModel.Create(PaymentErrorType.InvalidCallbackUrl));

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
                _log.WriteError(nameof(SetCallbackUrl), new
                {
                    paymentRequestId,
                    callbackUrl
                }, ex);
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }

        [HttpGet]
        [Route("{paymentRequestId}/callback")]
        [SwaggerOperation("GetCallback")]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(GetPaymentCallbackResponseModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(PaymentErrorResponseModel), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetCallbackUrl(string paymentRequestId)
        {
            if (!paymentRequestId.IsValidPaymentRequestId())
                return BadRequest(PaymentErrorResponseModel.Create(PaymentErrorType.InvalidPaymentId));

            try
            {
                GetPaymentCallbackModel callbackInfo =
                    await _payCallbackClient.GetPaymentCallback(_headersHelper.MerchantId, paymentRequestId);

                return Ok(Mapper.Map<GetPaymentCallbackResponseModel>(callbackInfo));
            }
            catch (Exception ex)
            {
                _log.WriteError(nameof(GetCallbackUrl), new {paymentRequestId}, ex);

                if (ex is ErrorResponseException errorEx)
                {
                    if (errorEx.StatusCode == HttpStatusCode.NotFound)
                        return NotFound();
                }
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }
    }
}
