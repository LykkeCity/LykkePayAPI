using Lykke.Service.PayInvoice.Client.Models.Invoice;

namespace Lykke.Service.PayAPI.Models.Mobile.History
{
    public class HistoryOperationInvoiceModel : HistoryOperationModel
    {
        public string InvoiceNumber { get; set; }

        public string BillingCategory { get; set; }

        public InvoiceStatus InvoiceStatus { get; set; }
    }
}
