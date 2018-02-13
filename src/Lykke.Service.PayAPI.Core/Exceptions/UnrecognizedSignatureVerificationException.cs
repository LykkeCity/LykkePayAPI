using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayAPI.Core.Exceptions
{
    public class UnrecognizedSignatureVerificationException : Exception
    {
        public UnrecognizedSignatureVerificationException()
        {
        }

        public UnrecognizedSignatureVerificationException(string status) : base("Unexpected verification status")
        {
            Status = status;
        }

        public UnrecognizedSignatureVerificationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnrecognizedSignatureVerificationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string Status { get; set; }
    }
}
