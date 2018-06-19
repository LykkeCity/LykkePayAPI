﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayAPI.Models.Invoice
{
    public class FilterOfMerchantResponse
    {
        public IReadOnlyList<FilterItemModel> GroupMerchants { get; set; }

        public IReadOnlyList<FilterItemModel> BillingCategories { get; set; }

        public IReadOnlyList<FilterItemModel> SettlementAssets { get; set; }
    }
}
