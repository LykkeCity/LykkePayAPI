using System.Threading.Tasks;
using Lykke.Service.PayAPI.Core.Services;

namespace Lykke.Service.PayAPI.Services
{
    public class HistoryOperationTitleProvider : IHistoryOperationTitleProvider
    {
        public async Task<string> GetTitleAsync(string assetId, string type)
        {
            return string.Format(GetTypeTitle(type), assetId);
        }

        private string GetTypeTitle(string type)
        {
            return HistoryOperationType.ResourceManager.GetString(type);
        }
    }
}
