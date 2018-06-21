using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Lykke.Service.PayAPI.Models.Invoice
{
    public class CancelInvoiceDisputeRequest
    {
        [Required]
        public string InvoiceId { get; set; }
    }
}
