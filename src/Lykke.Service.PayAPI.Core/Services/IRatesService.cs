using System.Threading.Tasks;
using Lykke.Service.PayAPI.Core.Domain.Rates;

namespace Lykke.Service.PayAPI.Core.Services
{
    public interface IRatesService
    {
        Task<AssetPairRate> Get(string assetPairId);
    }
}
