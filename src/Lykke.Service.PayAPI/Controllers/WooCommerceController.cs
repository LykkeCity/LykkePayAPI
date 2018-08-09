using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WooCommerceInvoiceModel = Lykke.Service.PayAPI.Models.WooCommerceInvoiceModel;
using Lykke.Service.PayAPI.Models;
using System.Net;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayAPI.Attributes;
using Lykke.Service.PayInvoice.Client;
using Lykke.SettingsReader;
using Lykke.Service.PayAPI.Core.Settings;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace Lykke.Service.PayAPI.Controllers
{
    [Authorize]
    [SignatureHeaders]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/wooCommerce")]
    [Produces("application/json")]
    public class WooCommerceController : Controller
    {
        private readonly IReloadingManager<AppSettings> _settings;
        private readonly IPayInvoiceClient _invoicesServiceClient;

        public WooCommerceController(
            IPayInvoiceClient invoicesServiceClient, 
            IReloadingManager<AppSettings> settings)
        {
            _invoicesServiceClient = invoicesServiceClient;
            _settings = settings;
        }

        /// <summary>
        /// Create invoice
        /// </summary>
        /// <param name="model">Request model</param>
        /// <response code="200">Result model</response>
        /// <response code="400">Problem occured</response>
        [HttpPost("create")]
        [SwaggerOperation(OperationId = "Create")]
        [SwaggerXSummary("Create invoice")]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(WooCommerceResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Create(WooCommerceInvoiceModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse().AddErrors(ModelState));

            if (!model.IsValid())
                return BadRequest(ErrorResponse.Create($"{nameof(model)} has invalid value"));

            var response = new WooCommerceResponse();
            try
            {
                var invoice = await _invoicesServiceClient.CreateInvoiceAsync(new PayInvoice.Client.Models.Invoice.CreateInvoiceModel
                {
                    MerchantId = model.MerchantId,
                    Number = model.InvoiceNumber,
                    ClientName = model.ClientName,
                    ClientEmail = model.ClientEmail,
                    Amount = model.Amount,
                    DueDate = DateTime.Now.AddDays(1),
                    PaymentAssetId = "BTC",
                    SettlementAssetId = model.Currency
                });
                response.InvoiceURL =
                    $"{_settings.CurrentValue.PayAPI.PayInvoicePortalUrl.TrimEnd('/')}/invoice/{invoice.Id}?callback={WebUtility.UrlEncode(model.CallbackUrl)}";
                response.InvoiceId = invoice.Id;
                response.ErrorCode = "0";
            }
            catch(Exception e)
            {
                response.ErrorCode = "1";
                response.Message = e.Message;
            }
            return new JsonResult(response);
        }

        /// <summary>
        /// Get invoice status
        /// </summary>
        /// <param name="model">Request model</param>
        /// <response code="200">Result model</response>
        /// <response code="400">Problem occured</response>
        [HttpPost("status")]
        [SwaggerOperation(OperationId = "Status")]
        [SwaggerXSummary("Invoice status")]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(WooCommerceResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Status(WooCommerceInvoiceModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse().AddErrors(ModelState));
            if (!string.IsNullOrWhiteSpace(model.InvoiceId))
                return BadRequest(ErrorResponse.Create($"{nameof(model.InvoiceId)} has invalid value"));

            var response = new WooCommerceResponse();
            try
            {
                var invoice = await _invoicesServiceClient.GetInvoiceAsync(model.InvoiceId);
                response.Status = invoice.Status.ToString();
                response.ErrorCode = "0";
            }
            catch (Exception e)
            {
                response.ErrorCode = "1";
                response.Message = e.Message;
            }
            return new JsonResult(response);
        }
    }
}
