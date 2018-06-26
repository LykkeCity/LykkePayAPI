using Lykke.Common.Api.Contract.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Lykke.Service.PayAPI.Filters
{
    public class ValidateActionParametersFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var descriptor = context.ActionDescriptor as ControllerActionDescriptor;

            if (descriptor != null)
            {
                var parameters = descriptor.MethodInfo.GetParameters();

                foreach (var parameter in parameters)
                {
                    context.ActionArguments.TryGetValue(parameter.Name, out var argument);

                    EvaluateValidationAttributes(parameter, argument, context.ModelState);
                }
            }

            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(GetErrorResponse(context.ModelState));
            }

            base.OnActionExecuting(context);
        }

        private void EvaluateValidationAttributes(ParameterInfo parameter, object argument, ModelStateDictionary modelState)
        {
            var validationAttributes = parameter.CustomAttributes;

            foreach (var attributeData in validationAttributes)
            {
                var attributeInstance = CustomAttributeExtensions.GetCustomAttribute(parameter, attributeData.AttributeType);

                var validationAttribute = attributeInstance as ValidationAttribute;

                if (validationAttribute != null)
                {
                    var isValid = validationAttribute.IsValid(argument);
                    if (!isValid)
                    {
                        modelState.AddModelError(parameter.Name, validationAttribute.FormatErrorMessage(parameter.Name));
                    }
                }
            }
        }

        private ErrorResponse GetErrorResponse(ModelStateDictionary modelState)
        {
            var errorResponse = ErrorResponse.Create("Model is invalid.");
            foreach (var entity in modelState)
            {
                foreach (var error in entity.Value.Errors)
                {
                    if (error.Exception != null)
                    {
                        errorResponse.AddModelError(entity.Key, error.Exception);
                    }
                    else
                    {
                        errorResponse.AddModelError(entity.Key, error.ErrorMessage);
                    }
                }
            }

            return errorResponse;
        }
    }
}
