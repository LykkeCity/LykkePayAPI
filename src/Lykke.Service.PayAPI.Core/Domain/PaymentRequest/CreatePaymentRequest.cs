namespace Lykke.Service.PayAPI.Core.Domain.PaymentRequest
{
    public class CreatePaymentRequest
    {
        public string SettlementAssetId { get; set; }

        public decimal Amount { get; set; }

        public string PaymentAssetId { get; set; }

        public string CallbackUrl { get; set; }

        public string OrderId { get; set; }

        public double Percent { get; set; }

        public int Pips { get; set; }

        public double FixedFee { get; set; }

        public string MerchantId { get; set; }
    }
}
