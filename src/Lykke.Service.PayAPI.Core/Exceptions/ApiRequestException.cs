using System;
using System.Net;
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

        public ApiRequestException(string message, string appStatusCode, HttpStatusCode? httpStatusCode = null) : base(message)
        {
            HttpStatusCode = httpStatusCode;
            AppStatusCode = appStatusCode;
        }

        public HttpStatusCode? HttpStatusCode { get; set; }

        public string AppStatusCode { get; set; }
    }
}
