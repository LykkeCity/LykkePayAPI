using System.Collections.Generic;

namespace Lykke.Service.PayAPI.Models
{
    /// <summary>
    /// Available merchants for billing response
    /// </summary>
    public class AvailableMerchantsForBillingResponse
    {
        /// <summary>
        /// Gets or sets merchant list
        /// </summary>
        public IEnumerable<string> Merchants { get; set; }
    }
}
