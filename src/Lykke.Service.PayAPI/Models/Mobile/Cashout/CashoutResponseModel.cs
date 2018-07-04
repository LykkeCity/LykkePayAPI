namespace Lykke.Service.PayAPI.Models.Mobile.Cashout
{
    /// <summary>
    /// Cashout operation result details
    /// </summary>
    public class CashoutResponseModel
    {
        /// <summary>
        /// Gets or sets source wallet address
        /// </summary>
        public string SourceWalletAddress { get; set; }

        /// <summary>
        /// Gets or sets asset id
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// Gets or sets amount
        /// </summary>
        public decimal Amount { get; set; }
    }
}
