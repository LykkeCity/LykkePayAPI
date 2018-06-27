using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayAPI.Models
{
    /// <summary>
    /// Exchange operation details
    /// </summary>
    public class ExchangeModel: PreExchangeModel
    {
        /// <summary>
        /// Gets or sets Expected Rate
        /// </summary>
        public decimal ExpectedRate { get; set; }
    }
}
