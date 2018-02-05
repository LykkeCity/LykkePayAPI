using System;
using System.Net;
using System.Threading.Tasks;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInternal.Client;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using CreatePaymentRequestModel = Lykke.Service.PayAPI.Models.CreatePaymentRequestModel;

namespace Lykke.Service.PayAPI.Controllers
{
    [Route("api/[controller]")]
    public class PaymentRequestController : Controller
    {
        private readonly IPayInternalClient _payInternalClient;

        public PaymentRequestController(IPayInternalClient payInternalClient)
        {
            _payInternalClient = payInternalClient ?? throw new ArgumentNullException(nameof(payInternalClient));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerOperation("CreatePaymentRequest")]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> CreatePaymentRequest([FromBody] CreatePaymentRequestModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse().AddErrors(ModelState));

            try
            {
                //var response = await _payInternalClient.CreatePaymentRequestAsync();
                throw new NotImplementedException();
            }
            catch (ErrorResponseException e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
