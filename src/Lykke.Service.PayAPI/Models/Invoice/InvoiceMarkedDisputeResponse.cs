using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayAPI.Models.Invoice
{
    public class InvoiceMarkedDisputeResponse : InvoiceResponseModel
    {
        public DateTime? DisputeRaisedAt { get; set; }
        public string DisputeReason { get; set; }
    }
}
