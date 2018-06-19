using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Cache;
using Lykke.Service.PayAPI.Core.Domain.Invoice;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayAPI.Core.Settings.ServiceSettings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json;

namespace Lykke.Service.PayAPI.Services
{
    public class InvoiceService : IInvoiceService, IDisposable
    {
        private readonly HttpClient _httpClientIataApi;
        private readonly CacheExpirationPeriodsSettings _cacheExpirationPeriodsSettings;
        private readonly InvoiceSettings _invoiceSettings;
        private readonly OnDemandDataCache<IReadOnlyList<string>> _iataBillingCategories;
        private readonly OnDemandDataCache<InvoiceIataSpecificData> _invoiceIataSpecificDataCache;
        private readonly ILog _log;

        public InvoiceService(
            IataApiSettings iataApiSettings,
            CacheExpirationPeriodsSettings cacheExpirationPeriodsSettings,
            InvoiceSettings invoiceSettings,
            IMemoryCache memoryCache,
            ILog log)
        {
            _httpClientIataApi = new HttpClient
            {
                BaseAddress = new Uri(iataApiSettings.Url),
                DefaultRequestHeaders =
                {
                    {
                        "User-Agent",
                        $"{PlatformServices.Default.Application.ApplicationName}/{PlatformServices.Default.Application.ApplicationVersion}"
                    },
                    {
                        "api-key",
                        iataApiSettings.LykkeStaffKey
                    }
                }
            };

            _cacheExpirationPeriodsSettings = cacheExpirationPeriodsSettings;
            _invoiceSettings = invoiceSettings;
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
                            var response = await _httpClientIataApi.GetAsync("/api/v1/invoices/billingCategories");

                            if (response.IsSuccessStatusCode)
                            {
                                string value = await response.Content.ReadAsStringAsync();
                                var deserializedResponse = JsonConvert.DeserializeObject<IReadOnlyList<string>>(value);
                                return deserializedResponse;
                            }
                            else
                            {
                                _log.WriteWarning(nameof(GetIataBillingCategoriesAsync), null, "Not success status code");
                                return null;
                            }
                        }
                        catch (Exception ex)
                        {
                            _log.WriteError(nameof(GetIataBillingCategoriesAsync), null, ex);
                            return null;
                        }
                    },
                    _cacheExpirationPeriodsSettings.IataBillingCategories
                );

            return iataBillingCategories?.ToDictionary(x => x, x => x);
        }

        public IReadOnlyDictionary<string, string> GetIataAssets()
        {
            return _invoiceSettings.AssetsMap.Values.ToDictionary(x => x.Key.ToUpperInvariant(), x => x.Value);
        }

        public async Task<InvoiceIataSpecificData> GetIataSpecificDataAsync(string invoiceId)
        {
            var invoiceIataSpecificData = await _invoiceIataSpecificDataCache.GetOrAddAsync
                (
                    $"InvoiceIataSpecificData-{invoiceId}",
                    async x => {
                        try
                        {
                            var response = await _httpClientIataApi.GetAsync($"/api/v1/invoices/{invoiceId}");

                            if (response.IsSuccessStatusCode)
                            {
                                string value = await response.Content.ReadAsStringAsync();
                                var deserializedResponse = JsonConvert.DeserializeObject<IataApiInvoiceResponse>(value);
                                return new InvoiceIataSpecificData
                                {
                                    IsIataInvoice = true,
                                    IataInvoiceDate = deserializedResponse.Date.ToString("yyyy-MM-dd"),
                                    SettlementMonthPeriod = deserializedResponse.SettlementMonthPeriod
                                };
                            }
                            else
                            {
                                _log.WriteWarning(nameof(GetIataSpecificDataAsync), new { invoiceId, response.StatusCode }, "Not success status code");
                                return new InvoiceIataSpecificData { IsIataInvoice = false };
                            }
                        }
                        catch (Exception ex)
                        {
                            _log.WriteError(nameof(GetIataSpecificDataAsync), new { invoiceId }, ex);
                            return new InvoiceIataSpecificData { IsIataInvoice = false };
                        }
                    },
                    DateTime.UtcNow.AddYears(1)
                );

            return invoiceIataSpecificData.IsIataInvoice ? invoiceIataSpecificData : null;
        }

        public void Dispose()
        {
            _httpClientIataApi?.Dispose();
        }
    }
}
