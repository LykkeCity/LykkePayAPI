﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayAPI.Core.Settings.ServiceSettings
{
    public class CacheExpirationPeriodsSettings
    {
        public TimeSpan MerchantName { get; set; }
        public TimeSpan MerchantLogoUrl { get; set; }
        public TimeSpan IataBillingCategories { get; set; }
    }
}
