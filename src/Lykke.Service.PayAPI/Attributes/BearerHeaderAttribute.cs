using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Lykke.Service.PayAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class BearerHeaderAttribute : ActionFilterAttribute
    {
        
    }
}
