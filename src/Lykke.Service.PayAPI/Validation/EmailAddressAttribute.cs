using System;
using System.ComponentModel.DataAnnotations;
using Common;

namespace Lykke.Service.PayAPI.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class EmailAddressAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return value?.ToString().IsValidEmail() ?? false;
        }
    }
}
