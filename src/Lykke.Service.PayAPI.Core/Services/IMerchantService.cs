using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayAPI.Core.Services
{
    public interface IMerchantService
    {
        Task<string> GetMerchantName(string merchantId);
        Task<IReadOnlyList<string>> GetGroupMerchants(string merchantId);
    }
}
