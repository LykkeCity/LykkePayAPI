using System;
using System.ComponentModel.DataAnnotations;
using Common;

namespace Lykke.Service.PayAPI.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class EmailAddressAndRowKeyAttribute : ValidationAttribute
    {
        private const string Message = "The {0} field is not a valid e-mail address.";

        public override bool IsValid(object value)
        {
            return value?.ToString().IsValidEmailAndRowKey() ?? false;
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(Message, name);
        }
    }
}
