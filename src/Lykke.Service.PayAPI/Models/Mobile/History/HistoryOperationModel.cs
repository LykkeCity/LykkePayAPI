using System;
using Lykke.Service.PayInvoice.Client.Models.Invoice;

namespace Lykke.Service.PayAPI.Models.Mobile.History
{
    public class HistoryOperationModel
    {
        public string Id { get; set; }

        public string MerchantName { get; set; }

        public string MerchantLogoUrl { get; set; }

        public string Title { get; set; }

        public DateTime CreatedOn { get; set; }

        public decimal Amount { get; set; }

        public string AssetId { get; set; }

        public string Type { get; set; }

        public string EmployeeEmail { get; set; }

        public string TxHash { get; set; }

        public string ExplorerUrl { get; set; }

        public int BlockHeight { get; set; }

        public int BlockConfirmations { get; set; }

        public string InvoiceNumber { get; set; }

        public string BillingCategory { get; set; }

        public InvoiceStatus? InvoiceStatus { get; set; }
    }
}
