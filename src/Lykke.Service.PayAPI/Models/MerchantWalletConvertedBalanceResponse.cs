namespace Lykke.Service.PayAPI.Models
{
    /// <summary>
    /// Merchant wallet balance with converted value
    /// </summary>
    public class MerchantWalletConvertedBalanceResponse
    {
        /// <summary>
        /// Gets or sets wallet id
        /// </summary>
        public string WalletId { get; set; }

        /// <summary>
        /// Gets or sets asset id
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// Gets or sets base asset balance
        /// </summary>
        public decimal BaseAssetBalance { get; set; }

        /// <summary>
        /// Gets or sets converted asset balance
        /// </summary>
        public decimal ConvertedBalance { get; set; }
    }
}
