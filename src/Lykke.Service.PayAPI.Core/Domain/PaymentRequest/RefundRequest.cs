namespace Lykke.Service.PayAPI.Core.Domain.PaymentRequest
{
    public class RefundRequest
    {
        public string MerchantId { get; set; }

        public string PaymentRequestId { get; set; }

        public string DestinationAddress { get; set; }

        public string CallbackUrl { get; set; }
    }
}
