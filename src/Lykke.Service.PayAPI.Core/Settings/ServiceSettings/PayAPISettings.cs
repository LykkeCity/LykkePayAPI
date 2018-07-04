using System;
using System.Collections.Generic;

namespace Lykke.Service.PayAPI.Core.Settings.ServiceSettings
{
    public class PayAPISettings
    {
        public DbSettings Db { get; set; }
        public TimeSpan PaymentRequestDueDate { get; set; }
        public string PayInvoicePortalUrl { get; set; }
        public MerchantSettings Merchant { get; set; }
        public JwtSecuritySettings JwtSecurity { get; set; }
        public CacheExpirationPeriodsSettings CacheExpirationPeriods { get; set; }
        public IataSettings Iata { get; set; }
        public string TransactionUrl { get; set; }
    }

    public class MerchantSettings
    {
        public string MerchantDefaultLogoUrl { get; set; }
    }

    public class JwtSecuritySettings
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Key { get; set; }
        public TimeSpan TokenLifetime { get; set; }
    }

    public class IataSettings
    {
        public AssetsMapSettings AssetsMap { get; set; }
        public CashoutAssetsSettings CashoutAssets { get; set; }
    }

    public class AssetsMapSettings
    {
        public IDictionary<string, string> Values { get; set; }
    }

    public class CashoutAssetsSettings
    {
        public IReadOnlyList<string> Assets { get; set; }
    }
}
