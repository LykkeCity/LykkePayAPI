using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.PayAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    sealed public class SignatureVerificationAttribute : ActionFilterAttribute
    {
        public const string MerchantIdHeaderName = "Lykke-Merchant-Id";
        public const string SignHeaderName = "Lykke-Merchant-Sign";

        public SignatureVerificationAttribute()
        {
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var merchantHeader = context.HttpContext.Request.Headers[MerchantIdHeaderName];
            var headerValue = merchantHeader.FirstOrDefault();
            var signHeader = context.HttpContext.Request.Headers[SignHeaderName];
            var signValue = signHeader.FirstOrDefault();
            if (string.IsNullOrEmpty(headerValue) && string.IsNullOrEmpty(signValue) || context.HttpContext.Response.StatusCode == 401)
            {
                SetError(context, "access is not provided");
                return;
            }
            await next();
        }

        private void SetError(ActionExecutingContext context, string error)
        {
            context.Result =
                new JsonResult(error);
        }
    }
}
