using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Lykke.Service.PayAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class SignatureHeadersAttribute : ActionFilterAttribute
    {
        
    }
}
