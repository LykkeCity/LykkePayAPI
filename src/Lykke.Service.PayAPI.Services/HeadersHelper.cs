using Lykke.Service.PayAPI.Core;
using Lykke.Service.PayAPI.Core.Services;
using Microsoft.AspNetCore.Http;

namespace Lykke.Service.PayAPI.Services
{
    public class HeadersHelper : IHeadersHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HeadersHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string MerchantId => _httpContextAccessor.HttpContext.Request.GetMerchantId();

        public string MerchantSign => _httpContextAccessor.HttpContext.Request.GetMerchantSign();
    }
}
