using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Newtonsoft.Json;
using Common.Log;
using Lykke.Contracts.Security;
using Lykke.Service.PayAuth.Client;
using Lykke.Service.PayAuth.Client.Models;

namespace Lykke.Service.PayAPI.Controllers
{
    public class BaseController : Controller
    {
        protected string MerchantId => HttpContext.Request.Headers["Lykke-Merchant-Id"].ToString() ?? "";
        protected string TrasterSignIn => HttpContext.Request.Headers["Lykke-Merchant-Traster-SignIn"].ToString() ?? "";

        protected readonly ILog _log;
        protected readonly IPayAuthClient _payAuthClient;

        public BaseController(ILog log, IPayAuthClient payAuthClient)
        {
            _log = log;
            _payAuthClient = payAuthClient;
        }
        protected async Task<IActionResult> ValidateRequest()
        {
            await _log.WriteInfoAsync("Lykke Pay", "Validate Trasted Sign", JsonConvert.SerializeObject(new
            {
                MerchantId,
                TrasterSignIn
            }), null);
            if (!string.IsNullOrEmpty(MerchantId) && !string.IsNullOrEmpty(TrasterSignIn))
            {
                return Ok();
            }

            string strToSign;
            Console.WriteLine($"Method {HttpContext.Request.Method}");
            if (HttpContext.Request.Method.Equals("POST"))
            {
                HttpContext.Request.EnableRewind();
                HttpContext.Request.Body.Position = 0;
                using (StreamReader reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    strToSign = reader.ReadToEnd();
                }
                HttpContext.Request.Body.Position = 0;
            }
            else
            {
                strToSign = $"{HttpContext.Request.Path.ToString().TrimEnd('/')}{HttpContext.Request.QueryString}";
            }
            Console.WriteLine($"strToSign {strToSign}");
            var strToSend = JsonConvert.SerializeObject(new MerchantAuthRequest
            {
                MerchantId = MerchantId,
                StringToSign = strToSign,
                Sign = HttpContext.Request.Headers["Lykke-Merchant-Sign"].ToString() ?? ""
            });
            Console.WriteLine($"strToSend {strToSend}");
            var request = new VerifyRequest()
            {
                ClientId = MerchantId,
                SystemId = "LykkePay",
                Signature = HttpContext.Request.Headers["Lykke-Merchant-Sign"].ToString() ?? "",
                Text = strToSign
            };

            var isValid = (SecurityErrorType)int.Parse(await _payAuthClient.VerifyAsync(request));
            Console.WriteLine($"isValid {isValid}");
            if (isValid != SecurityErrorType.Ok)
            {
                switch (isValid)
                {
                    case SecurityErrorType.AssertEmpty:
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    case SecurityErrorType.MerchantUnknown:
                    case SecurityErrorType.SignEmpty:
                        return BadRequest();
                    case SecurityErrorType.SignIncorrect:
                        return StatusCode(StatusCodes.Status401Unauthorized);
                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }
            return Ok();
        }
    }
}
