using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;

namespace Lykke.Service.PayAPI.Core
{
    public static class HttpContextExtensions
    {
        public static string GetMerchantId(this HttpRequest src)
        {
            return src.GetHeader(LykkePayConstants.Headers.MerchantId);
        }

        public static string GetMerchantSign(this HttpRequest src)
        {
            return src.GetHeader(LykkePayConstants.Headers.MerchantSign);
        }

        public static string GetHeader(this HttpRequest src, string headerName)
        {
            StringValues headerValue = src.Headers[headerName];

            return headerValue == StringValues.Empty ? string.Empty : headerValue.ToString();
        }

        public static string ReadBody(this HttpRequest request)
        {
            request.EnableRewind();

            string body;

            using (StreamReader reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
            {
                body = reader.ReadToEnd();
            }

            request.Body.Position = 0;

            return body;
        }
    }
}
