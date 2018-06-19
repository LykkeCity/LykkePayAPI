using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayAPI.Core.Domain.Invoice
{
    public class InvoiceIataSpecificData
    {
        public bool IsIataInvoice { get; set; }
        public string IataInvoiceDate { get; set; }
        public string SettlementMonthPeriod { get; set; }
    }
}
