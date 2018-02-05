using System.Collections.Generic;
using System.Linq;
using Lykke.Common.Api.Contract.Responses;
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
    }
}
