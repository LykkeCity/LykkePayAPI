using System;
using Lykke.Service.PayAPI.Core.Domain.PaymentRequest;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;

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
    }
}
