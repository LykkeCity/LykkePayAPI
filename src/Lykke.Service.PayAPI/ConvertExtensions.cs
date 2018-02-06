using System.Linq;
using Common;
using Lykke.Service.PayAPI.Core.Domain.PaymentRequest;
using Lykke.Service.PayAPI.Core.Domain.Rates;
using Lykke.Service.PayAPI.Models;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using CreatePaymentRequestModel = Lykke.Service.PayAPI.Models.CreatePaymentRequestModel;

namespace Lykke.Service.PayAPI
{
    public static class ConvertExtensions
    {
        public static CreatePaymentRequest ToDomain(this CreatePaymentRequestModel src, string merchantId)
        {
            return new CreatePaymentRequest
            {
                Percent = src.Percent,
                Pips = src.Pips,
                FixedFee = src.FixedFee,
                Amount = src.Amount,
                OrderId = src.OrderId,
                SettlementAssetId = src.SettlementAsset,
                PaymentAssetId = src.PaymentAsset,
                CallbackUrl = src.CallbackURL,
                MerchantId = merchantId
            };
        }

        public static CreatePaymentResponseModel ToApiModel(this CreatePaymentResponse src)
        {
            return new CreatePaymentResponseModel
            {
                OrderId = src.OrderId,
                Amount = src.Amount,
                PaymentAsset = src.PaymentAssetId,
                ExchangeRate = src.ExchangeRate,
                Address = src.Address,
                Timestamp = src.Timestamp
            };
        }

        public static PaymentStatusResponseModel ToStatusApiModel(this PaymentRequestDetailsModel src)
        {
            return new PaymentStatusResponseModel
            {
                PaymentResponse = new PaymentResponseModel
                {
                    Error = src.Error,
                    PaymentRequestId = src.Id,
                    WalletAddress = src.WalletAddress,
                    Transactions = src.Transactions.Select(x => new PaymentResponseTransactionModel
                    {
                        Id = x.Id,
                        Amount = x.Amount,
                        Timestamp = x.FirstSeen.ToIsoDateTime(),
                        Currency = x.AssetId,
                        NumberOfConfirmations = x.Confirmations,
                        Url = x.Url,
                        RefundLink = x.RefundLink,
                    }).ToList()
                },
                PaymentStatus = src.Status.ToString()
            };
        }

        public static AssetPairResponseModel ToApiModel(this AssetPairRate src)
        {
            return new AssetPairResponseModel
            {
                AssetPair = src.AssetPairId,
                Accuracy = src.Accuracy,
                Bid = src.Bid,
                Ask = src.Ask
            };
        }
    }
}
