﻿using Common.Log;
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
    [Route("api/[controller]")]
    public class WooCommerceController : BaseController
    {
        private readonly IReloadingManager<AppSettings> _settings;
        private readonly IPayInvoiceClient _invoicesServiceClient;
        public WooCommerceController(ILog log, IPayInvoiceClient invoicesServiceClient, IReloadingManager<AppSettings> settings) : base(log)
        {
            _invoicesServiceClient = invoicesServiceClient;
            _settings = settings;
        }

        [HttpPost("Create")]
        [SwaggerOperation("Create")]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Create(WooCommerceInvoiceModel model)
        {
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

        
        [HttpPost("Status")]
        [SwaggerOperation("Status")]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Status(WooCommerceInvoiceModel model)
        {
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
