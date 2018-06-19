using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayAPI.Models
{
    /// <summary>
    /// The model to pay invoices
    /// </summary>
    public class PayInvoicesRequestModel
    {
        /// <summary>
        /// Invoices identifiers
        /// </summary>
        [Required]
        public IEnumerable<string> InvoicesIds { get; set; }
        /// <summary>
        /// Amount to pay in base asset
        /// </summary>
        [Required]
        public decimal AmountInBaseAsset { get; set; }
    }
}
