using System;
using System.Collections.Generic;
using System.Text;
using Lykke.Service.PayInvoice.Client.Models.Invoice;

namespace Lykke.Service.PayAPI.Models
{
    public class InvoiceResponseModel : InvoiceModel
    {
        /// <summary>
        /// Status name
        /// </summary>
        public new string Status { get; set; }
    }
}
