using System.Collections.Generic;
using Lykke.Service.PayAPI.Core.Domain.Assets;

namespace Lykke.Service.PayAPI.Core.Services
{
    public interface IAssetSettingsService
    {
        IReadOnlyList<CashoutAsset> GetCashoutAssets();
    }
}
