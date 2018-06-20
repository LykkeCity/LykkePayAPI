using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayAPI.Core.Services
{
    public interface IMerchantService
    {
        Task<string> GetMerchantNameAsync(string merchantId);
        Task<string> GetMerchantLogoUrlAsync(string merchantId);
        Task<IReadOnlyList<string>> GetGroupMerchantsAsync(string merchantId);
    }
}
