using Lykke.Service.EthereumCore.Client.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Lykke.Service.PayAPI.Core.Exceptions
{
    [Serializable]
    public class EthereumCoreApiException : Exception
    {
        public IDictionary<string, IList<string>> ModelErrors { get; }

        public EthereumCoreApiException()
        {
        }

        public EthereumCoreApiException(ErrorResponse errorResponse)
            : base(errorResponse.ErrorMessage)
        {
            ModelErrors = errorResponse.ModelErrors;
        }

        public EthereumCoreApiException(string message, IDictionary<string, IList<string>> modelErrors = null)
            : base(message)
        {
            ModelErrors = modelErrors;
        }

        public EthereumCoreApiException(string message, Exception innerException,
            IDictionary<string, IList<string>> modelErrors = null)
            : base(message, innerException)
        {
            ModelErrors = modelErrors;
        }

        protected EthereumCoreApiException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
