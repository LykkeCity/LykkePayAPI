using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayAPI.Core.Exceptions
{
    public class BlockchainSupportNotImplemented : Exception
    {
        public BlockchainSupportNotImplemented()
        {
        }

        public BlockchainSupportNotImplemented(string message) : base(message)
        {
        }

        public BlockchainSupportNotImplemented(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BlockchainSupportNotImplemented(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
