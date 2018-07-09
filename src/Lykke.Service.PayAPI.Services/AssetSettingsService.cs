using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Lykke.Service.PayAPI.Core.Domain.Assets;
using Lykke.Service.PayAPI.Core.Services;

namespace Lykke.Service.PayAPI.Services
{
    public class AssetSettingsService : IAssetSettingsService
    {
        private readonly IReadOnlyList<string> _cashoutAssets;

        public AssetSettingsService(
            [NotNull] IReadOnlyList<string> cashoutAssets)
        {
            _cashoutAssets = cashoutAssets ?? throw new ArgumentNullException(nameof(cashoutAssets));
        }
        
        public IReadOnlyList<CashoutAsset> GetCashoutAssets()
        {
            return _cashoutAssets.Select(x => new CashoutAsset {Name = x}).ToList();
        }
    }
}
