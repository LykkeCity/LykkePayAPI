using AutoMapper;
using Common;
using Lykke.Service.PayAPI.Core.Domain.PaymentRequest;
using Lykke.Service.PayAPI.Core.Domain.Rates;
using Lykke.Service.PayCallback.Client.Models;
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

            CreateMap<PaymentRequestTransactionModel, PaymentResponseTransactionModel>(MemberList.Destination)
                .ForMember(dest => dest.Timestamp, opt => opt.ResolveUsing(src => src.FirstSeen.ToIsoDateTime()))
                .ForMember(dest => dest.NumberOfConfirmations, opt => opt.MapFrom(src => src.Confirmations))
                .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => src.Id));

            CreateMap<PaymentRequestRefundTransactionModel, RefundResponseTransactionModel>(MemberList.Destination)
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp.ToIsoDateTime()))
                .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => src.Hash));

            CreateMap<PaymentRequestRefundModel, RefundRequestResponseModel>(MemberList.Destination)
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.Timestamp.ToIsoDateTime()))
                .ForMember(dest => dest.ExpirationDt, opt => opt.MapFrom(src => src.DueDate.ToIsoDateTime()))
                .ForMember(dest => dest.Error, opt => opt.Ignore());

            CreateMap<GetPaymentCallbackModel, GetPaymentCallbackResponseModel>(MemberList.Destination)
                .ForMember(dest => dest.CallbackUrl, opt => opt.MapFrom(src => src.Url));
        }
    }
}
