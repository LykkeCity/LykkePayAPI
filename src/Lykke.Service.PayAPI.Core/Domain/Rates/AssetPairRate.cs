namespace Lykke.Service.PayAPI.Core.Domain.Rates
{
    public class AssetPairRate
    {
        public string AssetPairId { get; set; }
        public int Accuracy { get; set; }
        public decimal Ask { get; set; }
        public decimal Bid { get; set; }
    }
}
