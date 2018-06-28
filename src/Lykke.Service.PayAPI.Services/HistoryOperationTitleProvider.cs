using Common;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayAPI.Core.Services;
using System.Threading.Tasks;

namespace Lykke.Service.PayAPI.Services
{
    public class HistoryOperationTitleProvider : IHistoryOperationTitleProvider
    {
        private readonly CachedDataDictionary<string, Asset> _assetsCache;

        public HistoryOperationTitleProvider(CachedDataDictionary<string, Asset> assetsCache)
        {
            _assetsCache = assetsCache;
        }

        public async Task<string> GetTitleAsync(string assetId, string type)
        {
            Asset asset = await _assetsCache.GetItemAsync(assetId);

            return $"{asset.Name} {GetTypeTitle(type)}";
        }

        private string GetTypeTitle(string type)
        {
            return HistoryOperationType.ResourceManager.GetString(type);
        }
    }
}
