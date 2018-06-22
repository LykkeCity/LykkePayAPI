using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.Common.Cache;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayAPI.Core.Settings.ServiceSettings;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Exceptions;
using Lykke.Service.PayInternal.Client.Models.MerchantGroups;
using Microsoft.Extensions.Caching.Memory;

namespace Lykke.Service.PayAPI.Services
{
    public class MerchantService : IMerchantService
    {
        private readonly IPayInternalClient _payInternalClient;
        private readonly MerchantSettings _merchantSettings;
        private readonly OnDemandDataCache<string> _merchantNamesCache;
        private readonly OnDemandDataCache<string> _merchantLogoUrlsCache;
        private readonly CacheExpirationPeriodsSettings _cacheExpirationPeriods;

        public MerchantService(
            IPayInternalClient payInternalClient,
            IMemoryCache memoryCache,
            MerchantSettings merchantSettings,
            CacheExpirationPeriodsSettings cacheExpirationPeriods)
        {
            _payInternalClient = payInternalClient;
            _merchantSettings = merchantSettings;
            _merchantNamesCache = new OnDemandDataCache<string>(memoryCache);
            _merchantLogoUrlsCache = new OnDemandDataCache<string>(memoryCache);
            _cacheExpirationPeriods = cacheExpirationPeriods;
        }

        public async Task<string> GetMerchantNameAsync(string merchantId)
        {
            var merchantName = await _merchantNamesCache.GetOrAddAsync
                (
                    $"MerchantName-{merchantId}",
                    async x => {
                        var merchant = await _payInternalClient.GetMerchantByIdAsync(merchantId);
                        return merchant.DisplayName;
                    },
                    _cacheExpirationPeriods.MerchantName
                );

            return merchantName;
        }

        public async Task<string> GetMerchantLogoUrlAsync(string merchantId)
        {
            var merchantLogoUrl = await _merchantLogoUrlsCache.GetOrAddAsync
                (
                    $"MerchantLogoUrl-{merchantId}",
                    async x => {
                        try
                        {
                            return await _payInternalClient.GetMerchantLogoUrl(merchantId);
                        }
                        catch (DefaultErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                        {
                            return _merchantSettings.MerchantDefaultLogoUrl;
                        }
                    },
                    _cacheExpirationPeriods.MerchantLogoUrl
                );

            return merchantLogoUrl;
        }

        public async Task<IReadOnlyList<string>> GetGroupMerchantsAsync(string merchantId)
        {
            MerchantsByUsageResponse response = await _payInternalClient.GetMerchantsByUsageAsync(
                new GetMerchantsByUsageRequest
                {
                    MerchantId = merchantId,
                    MerchantGroupUse = MerchantGroupUse.Billing
                });

            return response.Merchants.ToList();
        }
    }
}
