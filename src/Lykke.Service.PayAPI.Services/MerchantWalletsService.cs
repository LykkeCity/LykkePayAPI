using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Service.PayAPI.Core.Domain.MerchantWallets;
using Lykke.Service.PayAPI.Core.Exceptions;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Exceptions;
using Lykke.Service.PayInternal.Client.Models.AssetRates;
using Lykke.Service.PayInternal.Client.Models.MerchantWallets;

namespace Lykke.Service.PayAPI.Services
{
    public class MerchantWalletsService : IMerchantWalletsService
    {
        private readonly IPayInternalClient _payInternalClient;
        private readonly ILog _log;

        public MerchantWalletsService(
            [NotNull] IPayInternalClient payInternalClient, 
            [NotNull] ILog log)
        {
            _payInternalClient = payInternalClient ?? throw new ArgumentNullException(nameof(payInternalClient));
            _log = log.CreateComponentScope(nameof(MerchantWalletsService)) ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task<IReadOnlyList<MerchantWalletBalanceLine>> GetBalancesAsync(string merchantId, string convertAssetId)
        {
            var balancesResult = new List<MerchantWalletBalanceLine>();

            IEnumerable<MerchantWalletBalanceResponse> balancesResponse;

            try
            {
                balancesResponse = await _payInternalClient.GetMerchantWalletBalancesAsync(merchantId);
            }
            catch (DefaultErrorResponseException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                throw new MerchantNotFoundException(merchantId);
            }
            catch (DefaultErrorResponseException e) when (e.StatusCode == HttpStatusCode.NotImplemented)
            {
                throw new BlockchainSupportNotImplemented(e.Error.ErrorMessage);
            }

            foreach (MerchantWalletBalanceResponse merchantWalletBalanceResponse in balancesResponse)
            {
                decimal assetPairRate = 0;

                if (!string.IsNullOrWhiteSpace(convertAssetId))
                {
                    try
                    {
                        AssetRateResponse assetRateResponse = await _payInternalClient.GetCurrentAssetPairRateAsync(
                            merchantWalletBalanceResponse.AssetDisplayId,
                            convertAssetId);

                        assetPairRate = assetRateResponse.BidPrice;
                    }
                    catch (DefaultErrorResponseException e) when (e.StatusCode == HttpStatusCode.NotFound)
                    {
                        _log.WriteError(nameof(GetBalancesAsync), new
                        {
                            baseAssetId = merchantWalletBalanceResponse.AssetDisplayId,
                            quotingAssetId = convertAssetId
                        }, e);
                    }
                }

                balancesResult.Add(new MerchantWalletBalanceLine
                {
                    MerchantWalletId = merchantWalletBalanceResponse.MerchantWalletId,
                    AssetId = merchantWalletBalanceResponse.AssetDisplayId,
                    BaseAssetBalance = merchantWalletBalanceResponse.Balance,
                    ConvertedBalance = merchantWalletBalanceResponse.Balance * assetPairRate
                });
            }

            return balancesResult;
        }
    }
}
