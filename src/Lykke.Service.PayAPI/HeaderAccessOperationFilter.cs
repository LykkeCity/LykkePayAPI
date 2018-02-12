using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.Swagger;
using System.Linq;
using Lykke.Service.PayAPI.Attributes;
using Lykke.Service.PayAPI.Core;

namespace Lykke.Service.PayAPI
{
    public class HeaderAccessOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var filterPipeline = context.ApiDescription.ActionDescriptor.FilterDescriptors;
            var isSignAccess = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is SignatureHeadersAttribute);
            if (isSignAccess)
            {
                if (operation.Parameters == null)
                    operation.Parameters = new List<IParameter>();

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = LykkePayConstants.Headers.MerchantId,
                    In = "header",
                    Description = "MerchantId",
                    Required = true,
                    Type = "string"
                });

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = LykkePayConstants.Headers.MerchantSign,
                    In = "header",
                    Description = "signature",
                    Required = true,
                    Type = "string"
                });
            }
        }
    }
}
