using System;

namespace Lykke.Service.PayAPI.Models.Mobile.History
{
    public class HistoryOperationViewModel
    {
        public string Id { get; set; }

        public string MerchantLogoUrl { get; set; }

        public string Title { get; set; }

        public DateTime CreatedOn { get; set; }

        public decimal Amount { get; set; }

        public string AssetId { get; set; }

        public string DesiredAssetId { get; set; }

        public string Type { get; set; }

        public DateTime? IataInvoiceDate { get; set; }

        public string SettlementMonthPeriod { get; set; }

        public string InvoiceStatus { get; set; }
    }
}
