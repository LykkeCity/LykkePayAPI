using System.Threading.Tasks;
using Lykke.Service.PayHistory.Client.AutorestClient.Models;

namespace Lykke.Service.PayAPI.Core.Services
{
    public interface IHistoryOperationTitleProvider
    {
        Task<string> GetTitleAsync(string assetId, string type);
    }
}
