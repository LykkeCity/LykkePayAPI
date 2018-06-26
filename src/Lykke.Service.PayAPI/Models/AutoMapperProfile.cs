using AutoMapper;
using Common;
using Lykke.Service.PayAPI.Core.Domain.MerchantWallets;
using Lykke.Service.PayAPI.Core.Domain.PaymentRequest;
using Lykke.Service.PayAPI.Core.Domain.Rates;
using Lykke.Service.PayAPI.Models.Invoice;
using Lykke.Service.PayAPI.Models.Mobile.History;
using Lykke.Service.PayCallback.Client.Models;
using Lykke.Service.PayInternal.Client.Models.Asset;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using Lykke.Service.PayInvoice.Client.Models.Invoice;

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

            CreateMap<PaymentRequestTransactionModel, PaymentResponseTransactionModel>(MemberList.Destination)
                .ForMember(dest => dest.Timestamp, opt => opt.ResolveUsing(src => src.FirstSeen.ToIsoDateTime()))
                .ForMember(dest => dest.NumberOfConfirmations, opt => opt.MapFrom(src => src.Confirmations))
                .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => src.Id));

            CreateMap<PaymentRequestRefundTransactionModel, RefundResponseTransactionModel>(MemberList.Destination)
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp.ToIsoDateTime()))
                .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => src.Hash));

            CreateMap<PaymentRequestRefundModel, RefundRequestResponseModel>(MemberList.Destination)
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.Timestamp.ToIsoDateTime()))
                .ForMember(dest => dest.ExpirationDt, opt => opt.MapFrom(src => src.DueDate.ToIsoDateTime()));

            CreateMap<GetPaymentCallbackModel, GetPaymentCallbackResponseModel>(MemberList.Destination)
                .ForMember(dest => dest.CallbackUrl, opt => opt.MapFrom(src => src.Url));

            CreateMap<AvailableAssetsResponse, AssetsResponseModel>(MemberList.Destination);

            CreateMap<InvoiceModel, InvoiceResponseModel>(MemberList.Destination)
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.IataInvoiceDate, opt => opt.Ignore())
                .ForMember(dest => dest.SettlementMonthPeriod, opt => opt.Ignore())
                .ForMember(dest => dest.LogoUrl, opt => opt.Ignore())
                .ForMember(dest => dest.SettlementAmountInBaseAsset, opt => opt.Ignore())
                .ForMember(dest => dest.MerchantName, opt => opt.Ignore());

            CreateMap<InvoiceResponseModel, InvoiceMarkedDisputeResponse>(MemberList.Destination)
                .ForMember(dest => dest.DisputeRaisedAt, opt => opt.Ignore())
                .ForMember(dest => dest.DisputeReason, opt => opt.Ignore());

            CreateMap<MerchantWalletBalanceLine, MerchantWalletConvertedBalanceResponse>(MemberList.Destination)
                .ForMember(dest => dest.WalletId, opt => opt.MapFrom(src => src.MerchantWalletId));

            CreateMobileHistoryMaps();
        }

        private void CreateMobileHistoryMaps()
        {
            CreateMap<PayHistory.Client.AutorestClient.Models.HistoryOperationViewModel, HistoryOperationViewModel>()
                .ForMember(dest => dest.MerchantLogoUrl, opt => opt.Ignore());

            CreateMap<PayHistory.Client.AutorestClient.Models.HistoryOperationModel, HistoryOperationModel>()
                .ForMember(dest => dest.MerchantLogoUrl, opt => opt.Ignore())
                .ForMember(dest => dest.MerchantName, opt => opt.Ignore())
                .ForMember(dest => dest.BlockHeight, opt => opt.Ignore())
                .ForMember(dest => dest.BlockConfirmations, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceNumber, opt => opt.Ignore())
                .ForMember(dest => dest.BillingCategory, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceStatus, opt => opt.Ignore())
                .ForMember(dest => dest.ExplorerUrl, opt => opt.Ignore());
        }
    }
}
