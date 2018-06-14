using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayAPI.Core.Domain.MerchantWallets;

namespace Lykke.Service.PayAPI.Core.Services
{
    public interface IMerchantWalletsService
    {
        Task<IReadOnlyList<MerchantWalletBalanceLine>> GetBalancesAsync(string merchantId, string convertAssetId);
    }
}
