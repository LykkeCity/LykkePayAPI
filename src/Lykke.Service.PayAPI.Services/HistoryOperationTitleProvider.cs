using System;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayAPI.Core.Services;
using System.Threading.Tasks;

namespace Lykke.Service.PayAPI.Services
{
    public class HistoryOperationTitleProvider : IHistoryOperationTitleProvider
    {
        public async Task<string> GetTitleAsync(string assetId, string type)
        {
            if (string.Equals(type, PayHistory.Core.Domain.HistoryOperationType.CashOut.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return $"{GetTypeTitle(type)} {assetId}";
            }
            else
            {
                return $"{assetId} {GetTypeTitle(type)}";
            }            
        }

        private string GetTypeTitle(string type)
        {
            return HistoryOperationType.ResourceManager.GetString(type);
        }
    }
}
