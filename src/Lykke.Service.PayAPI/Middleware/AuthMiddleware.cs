using Common.Log;
using Lykke.Contracts.Security;
using Lykke.Service.PayAuth.Client;
using Lykke.Service.PayAuth.Client.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Lykke.Service.PayAPI.Middleware
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;

        protected readonly ILog _log;
        protected readonly IPayAuthClient _payAuthClient;
        public AuthMiddleware(RequestDelegate next, ILog log, IPayAuthClient payAuthClient)
        {
            _next = next;
            _log = log;
            _payAuthClient = payAuthClient;
        }
        public async Task Invoke(HttpContext context)
        {
            var MerchantId = context.Request.Headers["Lykke-Merchant-Id"].ToString() ?? "";
            var TrasterSignIn = context.Request.Headers["Lykke-Merchant-Sign"].ToString() ?? "";
            await _log.WriteInfoAsync("Lykke Pay", "Validate Sign", JsonConvert.SerializeObject(new
            {
                MerchantId,
                TrasterSignIn
            }), null);

            string strToSign;
            Console.WriteLine($"Method {context.Request.Method}");
            if (context.Request.Method.Equals("POST"))
            {
                context.Request.EnableRewind();
                context.Request.Body.Position = 0;
                using (StreamReader reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    strToSign = reader.ReadToEnd();
                }
                context.Request.Body.Position = 0;
                Console.WriteLine($"strToSign {strToSign}");
                var strToSend = JsonConvert.SerializeObject(new MerchantAuthRequest
                {
                    MerchantId = MerchantId,
                    StringToSign = strToSign,
                    Sign = context.Request.Headers["Lykke-Merchant-Sign"].ToString() ?? ""
                });
                Console.WriteLine($"strToSend {strToSend}");
                var request = new VerifyRequest()
                {
                    ClientId = MerchantId,
                    SystemId = "LykkePay",
                    Signature = context.Request.Headers["Lykke-Merchant-Sign"].ToString() ?? "",
                    Text = strToSign
                };

                var isValid = (SecurityErrorType)int.Parse(await _payAuthClient.VerifyAsync(request));
                Console.WriteLine($"isValid {isValid}");
                if (isValid != SecurityErrorType.Ok)
                {
                    switch (isValid)
                    {
                        case SecurityErrorType.AssertEmpty:
                            await CreateErrorResponse(context, StatusCodes.Status500InternalServerError);
                            break;
                        case SecurityErrorType.MerchantUnknown:
                        case SecurityErrorType.SignEmpty:
                            await CreateErrorResponse(context, StatusCodes.Status500InternalServerError);
                            break;
                        case SecurityErrorType.SignIncorrect:
                            await CreateErrorResponse(context, StatusCodes.Status401Unauthorized);
                            break;
                        default:
                            await CreateErrorResponse(context, StatusCodes.Status500InternalServerError);
                            break;
                    }
                }
            }
            await _next(context);
        }
        private async Task CreateErrorResponse(HttpContext ctx, int statusCode)
        {
            ctx.Response.ContentType = "application/json";
            ctx.Response.StatusCode = statusCode;
            return;
        }
    }
    public static class AuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuth(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthMiddleware>();
        }
    }
}
