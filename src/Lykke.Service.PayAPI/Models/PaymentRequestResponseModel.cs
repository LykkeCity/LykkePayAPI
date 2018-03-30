using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.PayAPI.Models
{
    public class PaymentRequestResponseModel
    {
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? ExchangeRate { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? Amount { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [CanBeNull] public string Address { get; set; }

        [JsonProperty("expiration_datetime")]
        public string ExpirationDt { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [CanBeNull] public IList<PaymentResponseTransactionModel> Transactions { get; set; }
    }
}
