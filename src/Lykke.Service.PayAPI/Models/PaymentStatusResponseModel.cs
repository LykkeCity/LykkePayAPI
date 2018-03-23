using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lykke.Service.PayAPI.Models
{
    public class PaymentStatusResponseModel
    {
        public string PaymentStatus { get; set; }
        public PaymentResponseModel PaymentResponse { get; set; }
    }

    public class PaymentResponseModel
    {
        [CanBeNull] public string Id { get; set; }
        [CanBeNull] public string Address { get; set; }
        [CanBeNull] public string OrderId { get; set; }
        [CanBeNull] public string PaymentAsset { get; set; }
        public decimal? Amount { get; set; }
        public decimal? ExchangeRate { get; set; }
        [CanBeNull] public string Timestamp { get; set; }
        [CanBeNull] public string RefundLink { get; set; }
        [CanBeNull] public string Error { get; set; }
        [CanBeNull] public string Settlement { get; set; }

        [CanBeNull] public IList<PaymentResponseTransactionModel> Transactions { get; set; }
    }

    public class PaymentResponseTransactionModel
    {
        public string Id { get; set; }
        public string Timestamp { get; set; }
        public int NumberOfConfirmations { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        [CanBeNull] public string Url { get; set; }
        [CanBeNull] public string RefundLink { get; set; }
    }
}
