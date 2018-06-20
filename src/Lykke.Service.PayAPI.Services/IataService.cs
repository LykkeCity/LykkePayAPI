using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Cache;
using Lykke.Service.IataApi.Client;
using Lykke.Service.PayAPI.Core.Domain.Invoice;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayAPI.Core.Settings.ServiceSettings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json;

namespace Lykke.Service.PayAPI.Services
{
    public class IataService : IIataService
    {
        private readonly IIataApiClient _iataApiClient;
        private readonly CacheExpirationPeriodsSettings _cacheExpirationPeriodsSettings;
        private readonly IataSettings _iataSettings;
        private readonly OnDemandDataCache<IReadOnlyList<string>> _iataBillingCategories;
        private readonly OnDemandDataCache<InvoiceIataSpecificData> _invoiceIataSpecificDataCache;
        private readonly ILog _log;

        public IataService(
            IIataApiClient iataApiClient,
            CacheExpirationPeriodsSettings cacheExpirationPeriodsSettings,
            IataSettings iataSettings,
            IMemoryCache memoryCache,
            ILog log)
        {
            _iataApiClient = iataApiClient;
            _cacheExpirationPeriodsSettings = cacheExpirationPeriodsSettings;
            _iataSettings = iataSettings;
            _iataBillingCategories = new OnDemandDataCache<IReadOnlyList<string>>(memoryCache);
            _invoiceIataSpecificDataCache = new OnDemandDataCache<InvoiceIataSpecificData>(memoryCache);
            _log = log;
        }

        public async Task<IReadOnlyDictionary<string, string>> GetIataBillingCategoriesAsync()
        {
            var iataBillingCategories = await _iataBillingCategories.GetOrAddAsync
                (
                    "IataBillingCategories",
                    async x => {
                        try
                        {
                            IReadOnlyList<string> response = await _iataApiClient.GetBillingCategoriesAsync();
                            return response;
                        }
                        catch (ErrorResponseException)
                        {
                            return new List<string>();
                        }
                    },
                    _cacheExpirationPeriodsSettings.IataBillingCategories
                );

            return iataBillingCategories?.ToDictionary(x => x, x => x);
        }

        public IReadOnlyDictionary<string, string> GetIataAssets()
        {
            return _iataSettings.AssetsMap.Values.ToDictionary(x => x.Key.ToUpperInvariant(), x => x.Value);
        }

        public async Task<InvoiceIataSpecificData> GetIataSpecificDataAsync(string invoiceId)
        {
            var invoiceIataSpecificData = await _invoiceIataSpecificDataCache.GetOrAddAsync
                (
                    $"InvoiceIataSpecificData-{invoiceId}",
                    async x => {
                        try
                        {
                            var response = await _iataApiClient.GetInvoiceByIdAsync(invoiceId);

                            return new InvoiceIataSpecificData
                            {
                                IsIataInvoice = true,
                                IataInvoiceDate = response.Date.ToString("yyyy-MM-dd"),
                                SettlementMonthPeriod = response.SettlementMonthPeriod
                            };
                        }
                        catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                        {
                            return new InvoiceIataSpecificData { IsIataInvoice = false };
                        }
                    },
                    DateTime.UtcNow.AddYears(1)
                );

            return invoiceIataSpecificData.IsIataInvoice ? invoiceIataSpecificData : null;
        }
    }
}
