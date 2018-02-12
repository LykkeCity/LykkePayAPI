using System;
using System.Net;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayAPI.Models;
using Lykke.Service.PayAuth.Client;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using CreatePaymentRequestModel = Lykke.Service.PayAPI.Models.CreatePaymentRequestModel;
using ErrorResponseException = Lykke.Service.PayInternal.Client.ErrorResponseException;

namespace Lykke.Service.PayAPI.Controllers
{
    [Route("api/[controller]")]
    public class PaymentRequestController : BaseController
    {
        private readonly IPaymentRequestService _paymentRequestService;

        public PaymentRequestController(
            IPaymentRequestService paymentRequestService,
            IPayAuthClient payAuthClient,
            ILog log) : base(log, payAuthClient)
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
            await ValidateRequest();

            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse().AddErrors(ModelState));

            try
            {
                var domainRequest = request.ToDomain(MerchantId);

                var response = await _paymentRequestService.CreatePaymentRequestAsync(domainRequest);

                return Ok(response.ToApiModel());
            }
            catch (ErrorResponseException ex)
            {
                await _log.WriteErrorAsync(nameof(PaymentRequestController), nameof(CreatePaymentRequest),
                    request.ToJson(), ex);
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
            await ValidateRequest();

            try
            {
                var paymentRequestDetails = await _paymentRequestService.GetPaymentRequestDetailsAsync(address);

                return Ok(paymentRequestDetails.ToStatusApiModel());
            }
            catch (ErrorResponseException ex)
            {
                await _log.WriteErrorAsync(nameof(PaymentRequestController), nameof(GetPaymentStatus),
                    new {Address = address}.ToJson(), ex);

                return StatusCode((int) ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(PaymentRequestController), nameof(GetPaymentStatus),
                    new {Address = address}.ToJson(), ex);
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }
    }
}
