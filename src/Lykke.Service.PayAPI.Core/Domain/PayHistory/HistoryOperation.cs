using Lykke.Service.PayInvoice.Client.Models.Invoice;

namespace Lykke.Service.PayAPI.Core.Domain.PayHistory
{
    public class HistoryOperation : HistoryOperationView
    {
        public string MerchantName { get; set; }

        public string EmployeeEmail { get; set; }

        public string TxHash { get; set; }

        public string ExplorerUrl { get; set; }

        public long BlockHeight { get; set; }

        public long BlockConfirmations { get; set; }

        public string InvoiceNumber { get; set; }

        public string BillingCategory { get; set; }

        public InvoiceStatus? InvoiceStatus { get; set; }
    }
}
