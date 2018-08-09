using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.Swagger;
using System.Linq;
using Lykke.Service.PayAPI.Attributes;
using Lykke.Service.PayAPI.Core;

namespace Lykke.Service.PayAPI.SwaggerFilters
{
    public class SwaggerExtensionsFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var filterPipeline = context.ApiDescription.ActionDescriptor.FilterDescriptors;
            var swaggerXSummaryAttribute = filterPipeline.Select(filterInfo => filterInfo.Filter).FirstOrDefault(filter => filter is SwaggerXSummaryAttribute) as SwaggerXSummaryAttribute;
            
            if (swaggerXSummaryAttribute != null)
            {
                operation.Extensions.Add("x-summary", swaggerXSummaryAttribute.XSummary);
            }
        }
    }
}
