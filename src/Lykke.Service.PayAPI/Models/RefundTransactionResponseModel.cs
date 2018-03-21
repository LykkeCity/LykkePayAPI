namespace Lykke.Service.PayAPI.Models
{
    public class RefundTransactionResponseModel
    {
        public decimal Amount { get; set; }

        public string AssetId { get; set; }

        public string Blockchain { get; set; }

        public string Hash { get; set; }

        public string SourceAddress { get; set; }

        public string DestinationAddress { get; set; }
    }
}
