using AutoMapper;
using Common;
using Lykke.Service.PayAPI.Core.Domain.PaymentRequest;
using Lykke.Service.PayAPI.Core.Domain.Rates;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using RefundResponse = Lykke.Service.PayAPI.Core.Domain.PaymentRequest.RefundResponse;
using RefundTransactionResponse = Lykke.Service.PayAPI.Core.Domain.PaymentRequest.RefundTransactionResponse;

namespace Lykke.Service.PayAPI.Models
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<CreatePaymentRequestModel, CreatePaymentRequest>()
                .ForMember(dest => dest.MerchantId,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.MerchantId = resContext.Items["MerchantId"].ToString()))
                .ForMember(dest => dest.PaymentAssetId, opt => opt.MapFrom(src => src.PaymentAsset))
                .ForMember(dest => dest.SettlementAssetId, opt => opt.MapFrom(src => src.SettlementAsset));

            CreateMap<CreatePaymentResponse, CreatePaymentResponseModel>()
                .ForMember(dest => dest.PaymentAsset, opt => opt.MapFrom(src => src.PaymentAssetId));

            CreateMap<AssetPairRate, AssetPairResponseModel>()
                .ForMember(dest => dest.AssetPair, opt => opt.MapFrom(src => src.AssetPairId));

            CreateMap<RefundTransactionResponse, RefundTransactionResponseModel>();

            CreateMap<RefundResponse, RefundResponseModel>();

            CreateMap<PaymentRequestTransactionModel, PaymentResponseTransactionModel>()
                .ForMember(dest => dest.Timestamp, opt => opt.ResolveUsing(src => src.FirstSeen.ToIsoDateTime()))
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.NumberOfConfirmations, opt => opt.MapFrom(src => src.Confirmations));
        }
    }
}
