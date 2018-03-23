using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.PayAPI.Models
{
    public class PaymentStatusResponseModel
    {
        public string PaymentStatus { get; set; }
        public PaymentResponseModel PaymentResponse { get; set; }
    }

    public class PaymentResponseModel
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [CanBeNull] public string Id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [CanBeNull] public string Address { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [CanBeNull] public string OrderId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [CanBeNull] public string PaymentAsset { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? Amount { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? ExchangeRate { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [CanBeNull] public string Timestamp { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [CanBeNull] public string RefundLink { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [CanBeNull] public string Error { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [CanBeNull] public string Settlement { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [CanBeNull] public IList<PaymentResponseTransactionModel> Transactions { get; set; }
    }

    public class PaymentResponseTransactionModel
    {
        public string Id { get; set; }

        public string Timestamp { get; set; }

        public int NumberOfConfirmations { get; set; }

        public string Currency { get; set; }

        public decimal Amount { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [CanBeNull] public string Url { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [CanBeNull] public string RefundLink { get; set; }
    }
}
