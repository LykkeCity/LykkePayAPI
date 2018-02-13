using System;

namespace Lykke.Service.PayAPI.Core.Settings.ServiceSettings
{
    public class PayAPISettings
    {
        public DbSettings Db { get; set; }

        public TimeSpan PaymentRequestDueDate { get; set; }
    }
}
