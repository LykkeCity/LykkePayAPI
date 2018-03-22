using Common;

namespace Lykke.Service.PayAPI
{
    public static class ValidationExtensions
    {
        public static bool IsValidPaymentRequestId(this string src)
        {
            return !string.IsNullOrWhiteSpace(src) && src.IsGuid();
        }
    }
}
