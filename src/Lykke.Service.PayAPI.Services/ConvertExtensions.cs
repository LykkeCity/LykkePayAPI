using System;
using System.Linq;
using Lykke.Service.PayAPI.Core.Domain.PaymentRequest;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using RefundTransactionResponseDomain = Lykke.Service.PayAPI.Core.Domain.PaymentRequest.RefundTransactionResponse;
using RefundTransactionResponseService = Lykke.Service.PayInternal.Client.Models.PaymentRequest.RefundTransactionResponse;
using RefundResponseDomain = Lykke.Service.PayAPI.Core.Domain.PaymentRequest.RefundResponse;
using RefundResponseService = Lykke.Service.PayInternal.Client.Models.PaymentRequest.RefundResponse;

namespace Lykke.Service.PayAPI.Services
{
    public static class ConvertExtensions
    {
        public static CreatePaymentRequestModel ToServiceClientModel(this CreatePaymentRequest src, DateTime dueDate)
        {
            return new CreatePaymentRequestModel
            {
                Amount = src.Amount,
                MarkupPercent = src.Percent,
                MarkupPips = src.Pips,
                MarkupFixedFee = src.FixedFee,
                MerchantId = src.MerchantId,
                OrderId = src.OrderId,
                PaymentAssetId = src.PaymentAssetId,
                SettlementAssetId = src.SettlementAssetId,
                DueDate = dueDate
            };
        }

        public static RefundRequestModel ToServiceClientModel(this RefundRequest src)
        {
            return new RefundRequestModel
            {
                MerchantId = src.MerchantId,
                PaymentRequestId = src.PaymentRequestId,
                CallbackUrl = src.CallbackUrl,
                DestinationAddress = src.DestinationAddress
            };
        }

        public static RefundTransactionResponseDomain ToDomain(this RefundTransactionResponseService src)
        {
            return new RefundTransactionResponseDomain
            {
                Amount = src.Amount,
                AssetId = src.AssetId,
                Blockchain = src.Blockchain,
                DestinationAddress = src.DestinationAddress,
                Hash = src.Hash,
                SourceAddress = src.SourceAddress
            };
        }

        public static RefundResponseDomain ToDomain(this RefundResponseService src)
        {
            return new RefundResponseDomain
            {
                Amount = src.Amount,
                PaymentRequestId = src.PaymentRequestId,
                AssetId = src.AssetId,
                DueDate = src.DueDate,
                Transactions = src.Transactions.Select(x => x.ToDomain())
            };
        }
    }
}
