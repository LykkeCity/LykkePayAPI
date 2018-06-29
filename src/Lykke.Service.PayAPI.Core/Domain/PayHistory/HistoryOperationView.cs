using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayAPI.Core.Domain.PayHistory
{
    public class HistoryOperationView
    {
        public string Id { get; set; }

        public string MerchantLogoUrl { get; set; }

        public string Title { get; set; }

        public DateTime CreatedOn { get; set; }

        public decimal Amount { get; set; }

        public string AssetId { get; set; }

        public string Type { get; set; }

        public DateTime? IataInvoiceDate { get; set; }

        public string SettlementMonthPeriod { get; set; }
    }
}
