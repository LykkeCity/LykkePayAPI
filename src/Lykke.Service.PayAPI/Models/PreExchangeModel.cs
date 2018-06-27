using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayAPI.Models
{
    /// <summary>
    /// Exchange operation details
    /// </summary>
    public class PreExchangeModel
    {
        /// <summary>
        /// Gets or sets source asset id
        /// </summary>
        [Required]
        public string SourceAssetId { get; set; }

        /// <summary>
        /// Gets or sets source amount
        /// </summary>
        [Required]
        public decimal SourceAmount { get; set; }

        /// <summary>
        /// Gets or sets destination asset id
        /// </summary>
        [Required]
        public string DestAssetId { get; set; }
    }
}
