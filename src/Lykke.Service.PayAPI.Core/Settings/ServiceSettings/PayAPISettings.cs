using System;

namespace Lykke.Service.PayAPI.Core.Settings.ServiceSettings
{
    public class PayAPISettings
    {
        public DbSettings Db { get; set; }
        public TimeSpan PaymentRequestDueDate { get; set; }
        public string PayInvoicePortalUrl { get; set; }
        public JwtSecuritySettings JwtSecurity { get; set; }
        public CacheExpirationPeriodsSettings CacheExpirationPeriods { get; set; }
        public IataApiSettings IataApi { get; set; }
    }

    public class JwtSecuritySettings
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Key { get; set; }
        public TimeSpan TokenLifetime { get; set; }
    }

    public class IataApiSettings
    {
        public string Url { get; set; }
        public string LykkeStaffKey { get; set; }
    }
}
