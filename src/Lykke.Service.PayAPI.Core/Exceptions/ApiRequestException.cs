using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayAPI.Core.Exceptions
{
    public class ApiRequestException : Exception
    {
        public ApiRequestException()
        {
        }

        public ApiRequestException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ApiRequestException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ApiRequestException(string message, string appStatusCode, int? httpStatusCode = null) : base(message)
        {
            HttpStatusCode = httpStatusCode;
            AppStatusCode = appStatusCode;
        }

        public int? HttpStatusCode { get; set; }

        public string AppStatusCode { get; set; }
    }
}
