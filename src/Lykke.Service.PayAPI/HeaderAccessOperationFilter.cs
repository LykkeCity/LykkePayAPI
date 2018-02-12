using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Text;
using Swashbuckle.AspNetCore.Swagger;
using System.Linq;
using Lykke.Service.PayAPI.Attributes;

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
                    Name = "Lykke-Merchant-Id",
                    In = "header",
                    Description = "MerchantId",
                    Required = true,
                    Type = "string"
                });
                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "Lykke-Merchant-Sign",
                    In = "header",
                    Description = "signature",
                    Required = true,
                    Type = "string"
                });
            }
        }
    }
}
