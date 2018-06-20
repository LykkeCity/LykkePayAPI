using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayAPI.Core.Domain.Invoice
{
    public class IataApiInvoiceResponse
    {
        public string LykkeId { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public string SettlementMonthPeriod { get; set; }
    }
}
