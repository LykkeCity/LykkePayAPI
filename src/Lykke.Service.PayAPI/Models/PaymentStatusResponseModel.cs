using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.PayAPI.Models
{
    public class PaymentStatusResponseModel
    {
        public string Id { get; set; }

        public string PaymentStatus { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [CanBeNull] public ErrorResponseModel Error { get; set; }

        public string OrderId { get; set; }

        public string PaymentAsset { get; set; }

        public string SettlementAsset { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [CanBeNull] public PaymentRequestResponseModel PaymentRequest { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [CanBeNull] public RefundRequestResponseModel RefundRequest { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [CanBeNull] public SettlementRequestResponseModel SettlementResponse { get; set; }
    }
}
