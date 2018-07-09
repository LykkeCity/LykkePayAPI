using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Lykke.Service.PayAPI.Core.Services;

namespace Lykke.Service.PayAPI.Validation
{
    public class CashoutAssetExistsAttribute : ValidationAttribute
    {
        private const string Message = "Cashout asset not found";

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var assetSettingsService =
                (IAssetSettingsService) validationContext.GetService(typeof(IAssetSettingsService));

            if (assetSettingsService == null)
                throw new ArgumentNullException(nameof(assetSettingsService));

            string cashoutAsset = (string) value;

            if (string.IsNullOrEmpty(cashoutAsset))
                return ValidationResult.Success;

            bool exists = assetSettingsService.GetCashoutAssets().Any(x => x.Name.Equals(cashoutAsset));

            return exists ? ValidationResult.Success : new ValidationResult(Message);
        }
    }
}
