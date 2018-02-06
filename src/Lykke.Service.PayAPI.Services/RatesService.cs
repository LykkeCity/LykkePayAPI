using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.MarketProfile.Client;
using Lykke.Service.MarketProfile.Client.Models;
using Lykke.Service.PayAPI.Core.Domain.Rates;
using Lykke.Service.PayAPI.Core.Services;

namespace Lykke.Service.PayAPI.Services
{
    public class RatesService : IRatesService
    {
        private readonly ILykkeMarketProfile _marketProfileService;
        private readonly CachedDataDictionary<string, AssetPair> _assetPairsCache;

        public RatesService(
            ILykkeMarketProfile marketProfileService,
            CachedDataDictionary<string, AssetPair> assetPairsCache)
        {
            _marketProfileService =
                marketProfileService ?? throw new ArgumentNullException(nameof(marketProfileService));
            _assetPairsCache = assetPairsCache ?? throw new ArgumentNullException(nameof(assetPairsCache));
        }

        public async Task<AssetPairRate> Get(string assetPairId)
        {
            var response = await _marketProfileService.ApiMarketProfileByPairCodeGetAsync(assetPairId);

            if (response is ErrorModel error)
            {
                throw new Exception(error.Message);
            }

            if (response is AssetPairModel assetPairRate)
            {
                IReadOnlyDictionary<string, AssetPair> assetPairs = await _assetPairsCache.GetDictionaryAsync();

                var assetPair = assetPairs.ContainsKey(assetPairId) ? assetPairs[assetPairId] : null;

                if (assetPair == null)
                    throw new Exception($"Asset pair {assetPairId} doesn't exist");

                return new AssetPairRate
                {
                    AssetPairId = assetPair.Id,
                    Ask = (decimal) assetPairRate.AskPrice,
                    Bid = (decimal) assetPairRate.BidPrice,
                    Accuracy = assetPair.Accuracy
                };
            }

            throw new Exception("Unknown MarketProfile API response");
        }
    }
}
