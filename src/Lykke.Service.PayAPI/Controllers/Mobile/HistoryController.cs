using AutoMapper;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayAPI.Attributes;
using Lykke.Service.PayAPI.Models.Mobile.History;
using Lykke.Service.PayInvoice.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Lykke.Service.PayInvoice.Client.Models.Invoice;

namespace Lykke.Service.PayAPI.Controllers.Mobile
{
    [ApiVersion("1.0")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [BearerHeader]
    [Route("api/v{version:apiVersion}/mobile/history/[action]")]
    public class HistoryController : Controller
    {
        private HistoryOperationModel[] _models = new[]
        {
            new HistoryOperationModel
            {
                Id = "59f2c4bc-8edb-41e5-be25-b63c7729b885",
                MerchantLogoUrl = "https://lkedevmerchant.blob.core.windows.net/merchantfiles/iata_256.jpg",
                Title = "USD Incoming transfer",
                CreatedOn = new DateTime(2015,12,08,15,46,02),
                Amount =10000,
                AssetId = "USD",
                Type = HistoryOperationType.Recharge,
                EmployeeEmail = "tom@etihad.com",
                BlockHeight=5703355,
                BlockConfirmations=9,
                TxHash="12JhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6InRlc3RvdmVua29AdGVzdC5ydSIsIkVtcGxveWVlSWQiOiJiYWI3Yzg3NC03ZjY0LTQ3YmQtYjg3Mi0yODU4MDY1Y2RjMTAiLCJNZXJjaGFudElkIjoicGVtNiIsImV4cCI6MTUyOTU5NzY4MywiaXNzIjoiaHR0cDovL2x5a2tlLXBheS1hcGkubHlra2UtYXBpLnN2Yy5jbHVzdGVyLmxvY2FsIiwiYXVkIjoiaHR0cDovL2x5a2tlLXBheS1hcGkubHlra2UtYXBpLnN2Yy5jbHVzdGVyLmxvY2FsIn0.8p3IpujS1qZr8qvzwr_QS8WRusL-wcoCKWL0t4jiWGM"
            },
            new HistoryOperationInvoiceModel
            {
                Id = "0970e655-88ea-444e-a46f-476ba9ca1e32",
                MerchantLogoUrl = "https://lkedevmerchant.blob.core.windows.net/merchantfiles/iata_256.jpg",
                Title = "CHF Buy",
                CreatedOn = new DateTime(2015,12,09,14,35,03),
                Amount =20000,
                AssetId = "CHF",
                Type = HistoryOperationType.IncomingInvoicePayment,
                EmployeeEmail = "john@etihad.com",
                BlockHeight=4604432,
                BlockConfirmations=4,
                TxHash="34JhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6InRlc3RvdmVua29AdGVzdC5ydSIsIkVtcGxveWVlSWQiOiJiYWI3Yzg3NC03ZjY0LTQ3YmQtYjg3Mi0yODU4MDY1Y2RjMTAiLCJNZXJjaGFudElkIjoicGVtNiIsImV4cCI6MTUyOTU5NzY4MywiaXNzIjoiaHR0cDovL2x5a2tlLXBheS1hcGkubHlra2UtYXBpLnN2Yy5jbHVzdGVyLmxvY2FsIiwiYXVkIjoiaHR0cDovL2x5a2tlLXBheS1hcGkubHlra2UtYXBpLnN2Yy5jbHVzdGVyLmxvY2FsIn0.8p3IpujS1qZr8qvzwr_QS8WRusL-wcoCKWL0t4jiWGM",
                InvoiceNumber = "0043482188",
                BillingCategory = "Miscellaneous",
                InvoiceStatus = InvoiceStatus.Paid
            },
            new HistoryOperationModel
            {
                Id = "1bce8215-63e7-4bcb-8ac5-7572dba9cf5d",
                MerchantLogoUrl = "https://lkedevmerchant.blob.core.windows.net/merchantfiles/iata_256.jpg",
                Title = "USD Sell",
                CreatedOn = new DateTime(2015,12,10,13,24,04),
                Type = HistoryOperationType.Withdrawal,
                Amount =-12000,
                AssetId = "USD",
                EmployeeEmail = "sam@etihad.com",
                BlockHeight=6414512,
                BlockConfirmations=7,
                TxHash="56JhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6InRlc3RvdmVua29AdGVzdC5ydSIsIkVtcGxveWVlSWQiOiJiYWI3Yzg3NC03ZjY0LTQ3YmQtYjg3Mi0yODU4MDY1Y2RjMTAiLCJNZXJjaGFudElkIjoicGVtNiIsImV4cCI6MTUyOTU5NzY4MywiaXNzIjoiaHR0cDovL2x5a2tlLXBheS1hcGkubHlra2UtYXBpLnN2Yy5jbHVzdGVyLmxvY2FsIiwiYXVkIjoiaHR0cDovL2x5a2tlLXBheS1hcGkubHlra2UtYXBpLnN2Yy5jbHVzdGVyLmxvY2FsIn0.8p3IpujS1qZr8qvzwr_QS8WRusL-wcoCKWL0t4jiWGM"
            },

        };

        private readonly ILog _log;

        public HistoryController(
            ILog log)
        {
            _log = log.CreateComponentScope(nameof(HistoryController)) ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>
        /// Returns list of history operations.
        /// </summary>
        /// <response code="200">A collection of history operations.</response>
        /// <response code="400">Problem occured.</response>        
        [HttpGet]
        [SwaggerOperation("History")]
        [ProducesResponseType(typeof(IReadOnlyList<HistoryOperationViewModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public IActionResult Index()
        {
            var result = Mapper.Map<IReadOnlyList<HistoryOperationViewModel>>(_models);
            return Ok(result);
        }

        /// <summary>
        /// Returns details of the history operation.
        /// </summary>
        /// <response code="200">A details of the history operation.</response>
        /// <response code="400">Problem occured.</response>        
        /// <response code="404">History operation is not found.</response>        
        [HttpGet]
        [SwaggerOperation("HistoryDetails")]
        [ProducesResponseType(typeof(HistoryOperationModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public IActionResult Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest($"Identificator of the history operation (parameter \"{nameof(id)}\") is invalid..");
            }

            var model = _models.FirstOrDefault(m => string.Equals(id, m.Id, StringComparison.OrdinalIgnoreCase));
            if (model == null)
            {
                return NotFound();
            }

            return Ok(model);
        }

        /// <summary>
        /// Returns details of the invoice history operation.
        /// </summary>
        /// <response code="200">A details of the history operation.</response>
        /// <response code="400">Problem occured.</response>
        /// <response code="404">History operation is not found.</response>        
        [HttpGet]
        [SwaggerOperation("InvoiceHistoryDetails")]
        [ProducesResponseType(typeof(HistoryOperationInvoiceModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public IActionResult InvoiceDetails(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest($"Identificator of the history operation (parameter \"{nameof(id)}\") is invalid..");
            }

            var model = _models.FirstOrDefault(m => string.Equals(id, m.Id, StringComparison.OrdinalIgnoreCase))
                as HistoryOperationInvoiceModel;
            if (model == null)
            {
                return NotFound();
            }

            return Ok(model);
        }
    }
}
