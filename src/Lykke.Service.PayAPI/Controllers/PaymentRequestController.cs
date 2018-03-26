﻿using System;
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
using RabbitMQ.Client.Apigen.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using CreatePaymentRequestModel = Lykke.Service.PayAPI.Models.CreatePaymentRequestModel;
using RefundResponse = Lykke.Service.PayAPI.Core.Domain.PaymentRequest.RefundResponse;

namespace Lykke.Service.PayAPI.Controllers
{
    [Authorize]
    [SignatureHeaders]
    [Route("api/[controller]")]
    public class PaymentRequestController : BaseController
    {
        private readonly IPaymentRequestService _paymentRequestService;
        private readonly IPayCallbackClient _payCallbackClient;

        public PaymentRequestController(
            IPaymentRequestService paymentRequestService,
            IPayCallbackClient payCallbackClient,
            ILog log) : base(log)
        {
            _paymentRequestService =
                paymentRequestService ?? throw new ArgumentNullException(nameof(paymentRequestService));
            _payCallbackClient = payCallbackClient ?? throw new ArgumentNullException(nameof(payCallbackClient));
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
                    Mapper.Map<CreatePaymentRequest>(request, opt => opt.Items["MerchantId"] = MerchantId);

                CreatePaymentResponse createResponse =
                    await _paymentRequestService.CreatePaymentRequestAsync(domainRequest);

                PaymentRequestDetailsModel paymentRequestDetails =
                    await _paymentRequestService.GetPaymentRequestDetailsAsync(MerchantId, createResponse.Id);

                return Ok(paymentRequestDetails.ToStatusApiModel());
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
        /// Returns status of a payment request
        /// </summary>
        /// <param name="paymentRequestId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{paymentRequestId}/status")]
        [SwaggerOperation("GetPaymentRequestStatus")]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(PaymentStatusResponseModel), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> GetPaymentRequestStatus(string paymentRequestId)
        {
            try
            {
                var paymentRequestDetails = await _paymentRequestService.GetPaymentRequestDetailsAsync(MerchantId, paymentRequestId);

                return Ok(paymentRequestDetails.ToStatusApiModel());
            }
            catch (Exception ex)
            {
                await Log.WriteErrorAsync(nameof(PaymentRequestController), nameof(GetPaymentRequestStatus),
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
        public async Task<IActionResult> Refund(string paymentRequestId, [FromQuery] string destinationAddress)
        {
            try
            {
                RefundResponse refundResponse = await _paymentRequestService.RefundAsync(new RefundRequest
                {
                    MerchantId = MerchantId,
                    PaymentRequestId = paymentRequestId,
                    DestinationAddress = destinationAddress
                });

                PaymentRequestDetailsModel paymentRequestDetails =
                    await _paymentRequestService.GetPaymentRequestDetailsAsync(MerchantId, refundResponse.PaymentRequestId);

                return Ok(paymentRequestDetails.ToStatusApiModel());
            }
            catch (Exception ex)
            {
                await Log.WriteErrorAsync(nameof(PaymentRequestController), nameof(Refund), new
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
        public async Task<IActionResult> SetCallbackUrl(string paymentRequestId, [FromQuery] string callbackUrl)
        {
            try
            {
                await _payCallbackClient.SetPaymentCallback(new SetPaymentCallbackModel
                {
                    MerchantId = MerchantId,
                    PaymentRequestId = paymentRequestId,
                    CallbackUrl = callbackUrl
                });

                return Ok();
            }
            catch (Exception ex)
            {
                await Log.WriteErrorAsync(nameof(PaymentRequestController), nameof(SetCallbackUrl), new
                {
                    paymentRequestId,
                    callbackUrl
                }.ToJson(), ex);
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }
    }
}
