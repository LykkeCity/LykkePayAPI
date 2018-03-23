using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayAPI.Models
{
    public class WooCommerceInvoiceModel
    {
        public string InvoiceNumber { get; set; }
        public string ClientName { get; set; }
        public string ClientEmail { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public string MerchantId { get; set; }
        public string Signature { get; set; }
        public string InvoiceId { get; set; }
        public string CallbackUrl { get; set; }
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(InvoiceNumber) &&
                !string.IsNullOrWhiteSpace(ClientName) &&
                !string.IsNullOrWhiteSpace(ClientEmail) &&
                !string.IsNullOrWhiteSpace(Currency) &&
                !string.IsNullOrWhiteSpace(MerchantId) &&
                !string.IsNullOrWhiteSpace(CallbackUrl);
        }
    }
}
