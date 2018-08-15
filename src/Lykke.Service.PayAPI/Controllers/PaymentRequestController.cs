using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Common.Log;
using Lykke.Common.Log;
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

namespace Lykke.Service.PayAPI.Controllers
{
    [Authorize]
    [SignatureHeaders]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/paymentRequest")]
    [Produces("application/json")]
    [Consumes("application/json")]
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
            ILogFactory logFactory)
        {
            _paymentRequestService =
                paymentRequestService ?? throw new ArgumentNullException(nameof(paymentRequestService));
            _payCallbackClient = payCallbackClient ?? throw new ArgumentNullException(nameof(payCallbackClient));
            _headersHelper = headersHelper ?? throw new ArgumentNullException(nameof(headersHelper));
            _log = logFactory?.CreateLog(this) ?? throw new ArgumentNullException(nameof(logFactory));
        }

        /// <summary>
        /// Create payment request and order
        /// </summary>
        /// <param name="request">Request model</param>
        /// <response code="200">Result model</response>
        /// <response code="400">Problem occured</response>
        [HttpPost]
        [SwaggerOperation(OperationId = "CreatePaymentRequest")]
        [SwaggerXSummary("Create")]
        [ProducesResponseType(typeof(PaymentStatusResponseModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(PaymentErrorResponseModel), (int) HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> CreatePaymentRequest([FromBody] CreatePaymentRequestModel request)
        {
            if (string.IsNullOrWhiteSpace(request.SettlementAsset))
                return BadRequest(PaymentErrorResponseModel.Create(PaymentErrorType.InvalidSettlementAsset));

            if (string.IsNullOrWhiteSpace(request.PaymentAsset))
                return BadRequest(PaymentErrorResponseModel.Create(PaymentErrorType.InvalidPaymentAsset));

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
            catch (InvalidSettlementAssetException ex)
            {
                _log.Error(ex, null, $"request: {request.ToJson()}");

                return BadRequest(PaymentErrorResponseModel.Create(PaymentErrorType.InvalidSettlementAsset));
            }
            catch (InvalidPaymentAssetException ex)
            {
                _log.Error(ex, null, $"request: {request.ToJson()}");

                return BadRequest(PaymentErrorResponseModel.Create(PaymentErrorType.InvalidPaymentAsset));
            }
            catch (Exception ex)
            {
                _log.Error(ex, null, $"request: {request.ToJson()}");

                throw;
            }
        }

        /// <summary>
        /// Get status
        /// </summary>
        /// <remarks>
        /// Receive status of a payment request.
        /// </remarks>
        /// <param name="paymentRequestId">Payment request id</param>
        /// <response code="200">Result model</response>
        /// <response code="400">Problem occured</response>
        [HttpGet]
        [Route("{paymentRequestId}/status")]
        [SwaggerOperation(OperationId = "GetPaymentRequestStatus")]
        [SwaggerXSummary("Status")]
        [ProducesResponseType(typeof(PaymentStatusResponseModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(PaymentErrorResponseModel), (int) HttpStatusCode.BadRequest)]
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
                _log.Error(ex, null, $"request: {new {paymentRequestId}.ToJson()}");
                throw;
            }
        }

        /// <summary>
        /// Initiate a refund on a payment
        /// </summary>
        /// <param name="paymentRequestId">Payment request id</param>
        /// <param name="destinationAddress">Destination address</param>
        /// <response code="200">Result model</response>
        /// <response code="400">Problem occured</response>
        [HttpPost]
        [Route("{paymentRequestId}/refund")]
        [SwaggerOperation(OperationId = "Refund")]
        [SwaggerXSummary("Refund")]
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
                _log.Error(ex, null, $@"request: {
                        new
                        {
                            paymentRequestId,
                            destinationAddress
                        }.ToJson()
                    }");

                if (ex is PayInternal.Client.Exceptions.RefundErrorResponseException refundEx)
                {
                    return BadRequest(refundEx.ToErrorModel());
                }

                return BadRequest(PaymentErrorResponseModel.Create(PaymentErrorType.RefundIsNotAvailable));
            }
        }

        /// <summary>
        /// Set callback url
        /// </summary>
        /// <remarks>
        /// Add or update payment request callback url.
        /// </remarks>
        /// <param name="paymentRequestId">Payment request id</param>
        /// <param name="callbackUrl">Callback url</param>
        /// <response code="200">Result model</response>
        /// <response code="400">Problem occured</response>
        [HttpPost]
        [Route("{paymentRequestId}/callback")]
        [SwaggerOperation(OperationId = "SetCallback")]
        // There should be the only one x-summary for route which is used for GET, POST etc
        [SwaggerXSummary("Callback")]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(PaymentErrorResponseModel), (int) HttpStatusCode.BadRequest)]
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
                _log.Error(ex, null, $@"request:{
                        new
                        {
                            paymentRequestId,
                            callbackUrl
                        }.ToJson()
                    }");
                throw;
            }
        }

        /// <summary>
        /// Get callback url
        /// </summary>
        /// <remarks>
        /// Receive payment request callback url.s
        /// </remarks>
        /// <param name="paymentRequestId">Payment request id</param>
        /// <response code="200">Result model</response>
        /// <response code="400">Problem occured</response>
        [HttpGet]
        [Route("{paymentRequestId}/callback")]
        [SwaggerOperation(OperationId = "GetCallback")]
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
                _log.Error(ex, null, $"request:{new {paymentRequestId}.ToJson()}");

                if (ex is ErrorResponseException errorEx)
                {
                    if (errorEx.StatusCode == HttpStatusCode.NotFound)
                        return NotFound();
                }
                throw;
            }
        }
    }
}
