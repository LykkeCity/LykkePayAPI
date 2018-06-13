using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayAPI.Core.Exceptions
{
    public class MerchantNotFoundException : Exception
    {
        public MerchantNotFoundException()
        {
        }

        public MerchantNotFoundException(string merchantId) : base("Merchant not found")
        {
            MerchantId = merchantId;
        }

        public MerchantNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MerchantNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string MerchantId { get; set; }
    }
}
