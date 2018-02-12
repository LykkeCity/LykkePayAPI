namespace Lykke.Service.PayAPI.Core
{
    public static class LykkePayConstants
    {
        public static class Headers
        {
            public const string  MerchantId = "Lykke-Merchant-Id";
            public const string MerchantSign = "Lykke-Merchant-Sign";
        }

        public const string SystemId = "LykkePay";
        public const string AuthenticationScheme = "Lykke Pay Auth Scheme";
    }
}
