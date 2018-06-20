using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.Common.Cache;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayAPI.Core.Settings.ServiceSettings;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models.MerchantGroups;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.MerchantSetting;
using Microsoft.Extensions.Caching.Memory;

namespace Lykke.Service.PayAPI.Services
{
    public class MerchantService : IMerchantService
    {
        private readonly IPayInternalClient _payInternalClient;
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly MerchantSettings _merchantSettings;
        private readonly OnDemandDataCache<string> _merchantNamesCache;
        private readonly CacheExpirationPeriodsSettings _cacheExpirationPeriods;

        public MerchantService(
            IPayInternalClient payInternalClient,
            IPayInvoiceClient payInvoiceClient,
            IMemoryCache memoryCache,
            MerchantSettings merchantSettings,
            CacheExpirationPeriodsSettings cacheExpirationPeriods)
        {
            _payInternalClient = payInternalClient;
            _payInvoiceClient = payInvoiceClient;
            _merchantSettings = merchantSettings;
            _merchantNamesCache = new OnDemandDataCache<string>(memoryCache);
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
            //TODO: implement getting logo url later from PayInternal service
            var merchantLogoUrl = await Task.FromResult(_merchantSettings.MerchantDefaultLogoUrl);

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

        public async Task<string> GetBaseAssetAsync(string merchantId)
        {
            try
            {
                return await _payInvoiceClient.GetBaseAssetAsync(merchantId);
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }
    }
}
