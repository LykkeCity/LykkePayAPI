using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Lykke.Service.PayAPI.Attributes
{
    /// <summary>
    /// Attribute to create Swagger Extension x-summary to make readable navigation in api documentation
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SwaggerXSummaryAttribute : ActionFilterAttribute
    {
        public SwaggerXSummaryAttribute(string xSummary)
        {
            XSummary = xSummary;
        }

        public string XSummary { get; set; }
    }
}
