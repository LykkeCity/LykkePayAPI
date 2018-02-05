using Lykke.Service.PayAPI.Models;
using CreatePaymentRequestModelClient = Lykke.Service.PayInternal.Client.Models.PaymentRequest.CreatePaymentRequestModel;

namespace Lykke.Service.PayAPI
{
    public static class ConvertExtensions
    {
        public static CreatePaymentRequestModelClient ToServiceClientModel(this CreatePaymentRequestModel src)
        {
            return new CreatePaymentRequestModelClient
            {
                Amount = src.Amount,
                MarkupPercent = src.Percent,
                MarkupPips = src.Pips,
                MarkupFixedFee = src.FixedFee,
                MerchantId = "",
                OrderId = src.OrderId,
                PaymentAssetId = src.PaymentAsset,
                SettlementAssetId = src.SettlementAsset,
                DueDate = src
            }
        }
    }
}
