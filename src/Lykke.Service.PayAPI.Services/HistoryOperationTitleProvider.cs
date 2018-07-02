using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayAPI.Core.Services;
using System.Threading.Tasks;

namespace Lykke.Service.PayAPI.Services
{
    public class HistoryOperationTitleProvider : IHistoryOperationTitleProvider
    {
        public async Task<string> GetTitleAsync(string assetId, string type)
        {
            return $"{assetId} {GetTypeTitle(type)}";
        }

        private string GetTypeTitle(string type)
        {
            return HistoryOperationType.ResourceManager.GetString(type);
        }
    }
}
