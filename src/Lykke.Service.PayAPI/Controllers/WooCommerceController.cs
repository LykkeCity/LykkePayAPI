using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WooCommerceInvoiceModel = Lykke.Service.PayAPI.Models.WooCommerceInvoiceModel;
using Swashbuckle.AspNetCore.SwaggerGen;
using Lykke.Service.PayAPI.Models;
using System.Net;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayAPI.Attributes;
using Lykke.Service.PayInvoice.Client;
using Lykke.SettingsReader;
using Lykke.Service.PayAPI.Core.Settings;
using Microsoft.AspNetCore.Authorization;

namespace Lykke.Service.PayAPI.Controllers
{
    [Authorize]
    [SignatureHeaders]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/wooCommerce")]
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

        [HttpPost("create")]
        [SwaggerOperation("Create")]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Create(WooCommerceInvoiceModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse().AddErrors(ModelState));

            if (!model.IsValid())
                return BadRequest(ErrorResponse.Create($"{nameof(model)} has invalid value"));

            var response = new WooCommerceResponse();
            try
            {
                var invoice = await _invoicesServiceClient.CreateInvoiceAsync(model.MerchantId, new PayInvoice.Client.Models.Invoice.CreateInvoiceModel
                {
                    Number = model.InvoiceNumber,
                    ClientName = model.ClientName,
                    ClientEmail = model.ClientEmail,
                    Amount = model.Amount,
                    DueDate = DateTime.Now.AddDays(1),
                    PaymentAssetId = "BTC",
                    SettlementAssetId = model.Currency
                });
                response.InvoiceURL =
                    $"{_settings.CurrentValue.PayInvoicePortal.SiteUrl}invoice/{invoice.Id}?callback={WebUtility.UrlEncode(model.CallbackUrl)}";
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

        
        [HttpPost("status")]
        [SwaggerOperation("Status")]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
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
