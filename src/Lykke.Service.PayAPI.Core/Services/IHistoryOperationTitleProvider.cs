using System.Threading.Tasks;

namespace Lykke.Service.PayAPI.Core.Services
{
    public interface IHistoryOperationTitleProvider
    {
        Task<string> GetTitleAsync(string assetId, string type);
    }
}
