using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayAPI.Core.Exceptions
{
    public class InvalidPaymentAssetException : Exception
    {
        public InvalidPaymentAssetException()
        {
        }

        public InvalidPaymentAssetException(string message) : base(message)
        {
        }

        public InvalidPaymentAssetException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidPaymentAssetException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
