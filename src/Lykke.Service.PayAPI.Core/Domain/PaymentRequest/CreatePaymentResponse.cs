namespace Lykke.Service.PayAPI.Core.Domain.PaymentRequest
{
    public class CreatePaymentResponse
    {
        public string Id { get; set; }

        public string Timestamp { get; set; }

        public string Address { get; set; }

        public string OrderId { get; set; }

        public string PaymentAssetId { get; set; }

        public decimal Amount { get; set; }

        public decimal ExchangeRate { get; set; }
    }
}
