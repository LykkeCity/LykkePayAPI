using System.Collections.Generic;

namespace Lykke.Service.PayAPI.Core.Services
{
    public interface IAssetSettingsService
    {
        IReadOnlyList<string> GetCashoutAssets();
    }
}
