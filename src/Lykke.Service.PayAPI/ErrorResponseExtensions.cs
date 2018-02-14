using System.Collections.Generic;
using System.Linq;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayAPI.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Lykke.Service.PayAPI
{
    public static class ErrorResponseExtensions
    {
        public static ErrorResponse AddErrors(this ErrorResponse errorResponse, ModelStateDictionary modelState)
        {
            errorResponse.ModelErrors = new Dictionary<string, List<string>>();

            foreach (var state in modelState)
            {
                var messages = state.Value.Errors
                    .Where(e => !string.IsNullOrWhiteSpace(e.ErrorMessage))
                    .Select(e => e.ErrorMessage)
                    .Concat(state.Value.Errors
                        .Where(e => string.IsNullOrWhiteSpace(e.ErrorMessage))
                        .Select(e => e.Exception.Message))
                    .ToList();

                errorResponse.ModelErrors.Add(state.Key, messages);
            }

            return errorResponse;
        }

        public static ObjectResult GenerateErrorResponse(this ApiRequestException src)
        {
            var message = $"{src.Message}. Application status code: {src.AppStatusCode ?? "no information"}";

            return src.HttpStatusCode == null
                ? new BadRequestObjectResult(ErrorResponse.Create(message))
                : new ObjectResult(ErrorResponse.Create(message)) {StatusCode = (int) src.HttpStatusCode};
        }
    }
}
