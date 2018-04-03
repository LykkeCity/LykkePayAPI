using Newtonsoft.Json;

namespace Lykke.Service.PayAPI.Models
{
    public class PaymentErrorDetailsModel
    {
        [JsonConverter(typeof(PaymentErrorCodeConverter))]
        public PaymentErrorType Code { get; set; }
    }
}
