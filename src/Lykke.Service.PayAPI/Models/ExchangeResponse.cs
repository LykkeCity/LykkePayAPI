namespace Lykke.Service.PayAPI.Models
{
    /// <summary>
    /// Completed exchange operation details
    /// </summary>
    public class ExchangeResponse
    {
        /// <summary>
        /// Gets or sets source asset id
        /// </summary>
        public string SourceAssetId { get; set; }

        /// <summary>
        /// Gets or sets source amount
        /// </summary>
        public decimal SourceAmount { get; set; }

        /// <summary>
        /// Gets or sets destination asset id
        /// </summary>
        public string DestAssetId { get; set; }

        /// <summary>
        /// Gets or sets destination amount
        /// </summary>
        public decimal DestAmount { get; set; }

        /// <summary>
        /// Gets or sets rate
        /// </summary>
        public decimal Rate { get; set; }
    }
}
