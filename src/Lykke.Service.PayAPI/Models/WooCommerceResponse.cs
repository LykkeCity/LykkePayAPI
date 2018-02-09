using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayAPI.Models
{
    public class WooCommerceResponse
    {
        public string InvoiceURL { get; set; }
        public string ErrorCode { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public string InvoiceId { get; set; }
    }
}
