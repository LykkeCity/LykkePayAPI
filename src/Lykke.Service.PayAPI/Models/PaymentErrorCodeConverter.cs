using System;
using Newtonsoft.Json;

namespace Lykke.Service.PayAPI.Models
{
    public class PaymentErrorCodeConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            PaymentErrorType errorCode = (PaymentErrorType) value;

            switch (errorCode)
            {
                case PaymentErrorType.InvalidPaymentId:
                    writer.WriteValue("INVALID_PAYMENT_ID");
                    break;
                case PaymentErrorType.InvalidDestinationAddress:
                    writer.WriteValue("INVALID_DESTINATION_ADDRESS");
                    break;
                case PaymentErrorType.NoPaymentTransactions:
                    writer.WriteValue("NO_PAYMENT_TRANSACTIONS");
                    break;
                case PaymentErrorType.RefundIsNotAvailable:
                    writer.WriteValue("REFUND_IS_NOT_AVAILABLE");
                    break;
                case PaymentErrorType.InvalidSettlementAsset:
                    writer.WriteValue("INVALID_SETTLEMENTASSET");
                    break;
                case PaymentErrorType.InvalidPaymentAsset:
                    writer.WriteValue("INVALID_PAYMENTASSET");
                    break;
                case PaymentErrorType.InvalidCallbackUrl:
                    writer.WriteValue("INVALID_CALLBACKURL");
                    break;
                default:
                    throw new Exception("Unexpected payment error type");
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var enumString = (string)reader.Value;

            return Enum.Parse(typeof(PaymentErrorType), enumString, true);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }
    }
}
