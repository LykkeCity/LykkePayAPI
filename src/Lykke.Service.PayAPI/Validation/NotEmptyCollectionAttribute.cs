using System;
using System.Collections;

namespace Lykke.Service.PayAPI.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class NotEmptyCollectionAttribute : UseWithRequiredAttribute
    {
        public override bool IsValid(object value)
        {
            if (IsRequiredResponsibility(value)) return true;

            if (value is ICollection collection)
            {
                return collection.Count != 0;
            }

            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"The field {name} must contain at least one item.";
        }
    }
}
