using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayAPI.Validation
{
    /// <summary>
    /// For nullable types should be used Required
    /// </summary>
    public abstract class UseWithRequiredAttribute : ValidationAttribute
    {
        protected bool IsRequiredResponsibility(object value)
        {
            // Automatically pass if value is null or empty. RequiredAttribute should be used to assert a value is not empty.
            if (value == null)
            {
                return true;
            }
            var s = value as string;
            if (s != null && string.IsNullOrEmpty(s))
            {
                return true;
            }

            return false;
        }
    }
}
