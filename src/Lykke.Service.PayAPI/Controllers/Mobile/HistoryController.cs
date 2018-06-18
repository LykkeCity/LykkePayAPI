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
                Logo = "https://www.lykke.com/img/lykke_new.svg",
                Title = "USD Incoming transfer",
                TimeStamp = new DateTime(2015,12,08,15,46,02),
                Amount =10000,
                AssetId = "USD",
                SoldBy = "tom@etihad.com",
                BlockHeight=5703355,
                BlockConfirmations=9,
                TxHash="12JhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6InRlc3RvdmVua29AdGVzdC5ydSIsIkVtcGxveWVlSWQiOiJiYWI3Yzg3NC03ZjY0LTQ3YmQtYjg3Mi0yODU4MDY1Y2RjMTAiLCJNZXJjaGFudElkIjoicGVtNiIsImV4cCI6MTUyOTU5NzY4MywiaXNzIjoiaHR0cDovL2x5a2tlLXBheS1hcGkubHlra2UtYXBpLnN2Yy5jbHVzdGVyLmxvY2FsIiwiYXVkIjoiaHR0cDovL2x5a2tlLXBheS1hcGkubHlra2UtYXBpLnN2Yy5jbHVzdGVyLmxvY2FsIn0.8p3IpujS1qZr8qvzwr_QS8WRusL-wcoCKWL0t4jiWGM"
            },
            new HistoryOperationModel
            {
                Id = "0970e655-88ea-444e-a46f-476ba9ca1e32",
                Logo = "https://www.lykke.com/img/lykke_new.svg",
                Title = "CHF Buy",
                TimeStamp = new DateTime(2015,12,09,14,35,03),
                Amount =20000,
                AssetId = "CHF",
                SoldBy = "john@etihad.com",
                BlockHeight=4604432,
                BlockConfirmations=4,
                TxHash="34JhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6InRlc3RvdmVua29AdGVzdC5ydSIsIkVtcGxveWVlSWQiOiJiYWI3Yzg3NC03ZjY0LTQ3YmQtYjg3Mi0yODU4MDY1Y2RjMTAiLCJNZXJjaGFudElkIjoicGVtNiIsImV4cCI6MTUyOTU5NzY4MywiaXNzIjoiaHR0cDovL2x5a2tlLXBheS1hcGkubHlra2UtYXBpLnN2Yy5jbHVzdGVyLmxvY2FsIiwiYXVkIjoiaHR0cDovL2x5a2tlLXBheS1hcGkubHlra2UtYXBpLnN2Yy5jbHVzdGVyLmxvY2FsIn0.8p3IpujS1qZr8qvzwr_QS8WRusL-wcoCKWL0t4jiWGM"
            },
            new HistoryOperationModel
            {
                Id = "1bce8215-63e7-4bcb-8ac5-7572dba9cf5d",
                Logo = "https://www.lykke.com/img/lykke_new.svg",
                Title = "USD Sell",
                TimeStamp = new DateTime(2015,12,10,13,24,04),
                Amount =-12000,
                AssetId = "USD",
                SoldBy = "sam@etihad.com",
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
        /// <response code="500">Internal error has occured.</response>        
        [HttpGet]
        [SwaggerOperation("History")]
        [ProducesResponseType(typeof(IReadOnlyList<HistoryOperationViewModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.InternalServerError)]
        public IActionResult Index()
        {
            try
            {
                var result = Mapper.Map<IReadOnlyList<HistoryOperationViewModel>>(_models);
                return Ok(result);
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(ex.Error);
            }
            catch (Exception ex)
            {
                _log.WriteError(nameof(Index), null, ex);
            }

            return StatusCode((int)HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Returns details of the history operation.
        /// </summary>
        /// <response code="200">A details of the history operation.</response>
        /// <response code="400">Problem occured.</response>        
        /// <response code="500">Internal error has occured.</response>        
        [HttpGet]
        [SwaggerOperation("HistoryDetails")]
        [ProducesResponseType(typeof(HistoryOperationModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.InternalServerError)]
        public IActionResult Details(string id)
        {
            try
            {
                var model = _models.FirstOrDefault(m => string.Equals(id, m.Id, StringComparison.OrdinalIgnoreCase));
                if (model == null)
                {
                    return BadRequest("History operation is not found.");
                }

                return Ok(model);
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(ex.Error);
            }
            catch (Exception ex)
            {
                _log.WriteError(nameof(Index), null, ex);
            }

            return StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }
}
