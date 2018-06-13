namespace Lykke.Service.PayAPI.Core.Domain.MerchantWallets
{
    public class MerchantWalletBalanceLine
    {
        public string MerchantWalletId { get; set; }
        public string AssetId { get; set; }
        public decimal BaseAssetBalance { get; set; }
        public decimal ConvertedBalance { get; set; }
    }
}
