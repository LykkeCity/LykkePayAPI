namespace Lykke.Service.PayAPI.Models
{
    public class CreatePaymentResponseModel
    {
        public string Timestamp { get; set; }

        public string Address { get; set; }

        public string OrderId { get; set; }

        public string PaymentAsset { get; set; }

        public decimal Amount { get; set; }

        public decimal ExchangeRate { get; set; }
    }
}
