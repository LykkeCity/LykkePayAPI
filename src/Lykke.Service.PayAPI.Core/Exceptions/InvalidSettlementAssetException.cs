using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayAPI.Core.Exceptions
{
    public class InvalidSettlementAssetException : Exception
    {
        public InvalidSettlementAssetException()
        {
        }

        public InvalidSettlementAssetException(string message) : base(message)
        {
        }

        public InvalidSettlementAssetException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidSettlementAssetException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
