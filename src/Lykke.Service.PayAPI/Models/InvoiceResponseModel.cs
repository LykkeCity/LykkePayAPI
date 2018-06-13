using System;
using System.Collections.Generic;
using System.Text;
using Lykke.Service.PayInvoice.Client.Models.Invoice;

namespace Lykke.Service.PayAPI.Models
{
    public class InvoiceResponseModel : InvoiceModel
    {
        /// <summary>
        /// Display name of the merchant
        /// </summary>
        public string MerchantName { get; set; }

        /// <summary>
        /// Status name
        /// </summary>
        public new string Status { get; set; }
    }
}
