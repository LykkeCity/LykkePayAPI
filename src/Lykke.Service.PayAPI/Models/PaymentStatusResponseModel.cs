using System.Collections.Generic;

namespace Lykke.Service.PayAPI.Models
{
    public class PaymentStatusResponseModel
    {
        public string PaymentStatus { get; set; }
        public PaymentResponseModel PaymentResponse { get; set; }
    }

    public class PaymentResponseModel
    {
        public string Id { get; set; }
        public string Address { get; set; }
        public string OrderId { get; set; }
        public string PaymentAsset { get; set; }
        public decimal Amount { get; set; }
        public decimal ExchangeRate { get; set; }
        public string Timestamp { get; set; }
        public string RefundLink { get; set; }
        public string Error { get; set; }
        public string Settlement { get; set; }

        public IList<PaymentResponseTransactionModel> Transactions { get; set; }
    }

    public class PaymentResponseTransactionModel
    {
        public string Id { get; set; }
        public string Timestamp { get; set; }
        public int NumberOfConfirmations { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public string Url { get; set; }
        public string RefundLink { get; set; }
    }
}
