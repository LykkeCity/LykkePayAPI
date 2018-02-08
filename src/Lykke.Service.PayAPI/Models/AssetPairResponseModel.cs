namespace Lykke.Service.PayAPI.Models
{
    public class AssetPairResponseModel
    {
        public string AssetPair { get; set; }
        public decimal Ask { get; set; }
        public decimal Bid { get; set; }
        public int Accuracy { get; set; }
    }
}
