using AutoMapper;
using Common;
using Lykke.Service.PayAPI.Core.Domain.Assets;
using Lykke.Service.PayAPI.Core.Domain.MerchantWallets;
using Lykke.Service.PayAPI.Core.Domain.PayHistory;
using Lykke.Service.PayAPI.Core.Domain.PaymentRequest;
using Lykke.Service.PayAPI.Core.Domain.Rates;
using Lykke.Service.PayAPI.Models.Invoice;
using Lykke.Service.PayAPI.Models.Mobile.Assets;
using Lykke.Service.PayAPI.Models.Mobile.Cashout;
using Lykke.Service.PayAPI.Models.Mobile.History;
using Lykke.Service.PayCallback.Client.Models;
using Lykke.Service.PayInternal.Client.Models.Asset;
using Lykke.Service.PayInternal.Client.Models.Cashout;
using Lykke.Service.PayInternal.Client.Models.Exchange;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Lykke.Service.PayVolatility.Models;

namespace Lykke.Service.PayAPI.Models
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<CreatePaymentRequestModel, CreatePaymentRequest>()
                .ForMember(dest => dest.MerchantId,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.MerchantId = (string) resContext.Items["MerchantId"]))
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

            CreateMap<ExchangeModel, ExchangeRequest>(MemberList.Destination)
                .ForMember(dest => dest.SourceMerchantWalletId, opt => opt.Ignore())
                .ForMember(dest => dest.DestMerchantWalletId, opt => opt.Ignore())
                .ForMember(dest => dest.MerchantId,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.MerchantId = (string) resContext.Items["MerchantId"]));

            CreateMap<PreExchangeModel, PreExchangeRequest>(MemberList.Destination)
                .ForMember(dest => dest.MerchantId,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.MerchantId = (string)resContext.Items["MerchantId"]));

            CreateMap<PayInternal.Client.Models.Exchange.ExchangeResponse, ExchangeResponse>(MemberList.Destination);

            CreateMap<PayInternal.Client.Models.AssetRates.AssetRateResponse, AssetRateResponse>(MemberList
                .Destination);

            CreateMap<CashoutAsset, CashoutAssetResponse>(MemberList.Destination);

            CreateMap<CashoutModel, CashoutRequest>(MemberList.Destination)
                .ForMember(dest => dest.MerchantId,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.MerchantId = (string) resContext.Items["MerchantId"]))
                .ForMember(dest => dest.EmployeeEmail,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.EmployeeEmail = (string) resContext.Items["EmployeeEmail"]))
                .ForMember(dest => dest.DesiredAsset, opt => opt.MapFrom(src => src.DesiredCashoutAsset))
                .ForMember(dest => dest.SourceAmount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.SourceAssetId, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.SourceMerchantWalletId, opt => opt.Ignore());

            CreateMap<CashoutResponse, CashoutResponseModel>(MemberList.Destination);

            CreateMap<VolatilityModel, VolatilityResponseModel>(MemberList.Destination);

            CreateMobileHistoryMaps();
        }

        private void CreateMobileHistoryMaps()
        {
            CreateMap<PayHistory.AutorestClient.Models.HistoryOperationViewModel, HistoryOperationView>(
                    MemberList.Source)
                .ForSourceMember(s => s.InvoiceId, s => s.Ignore())
                .ForSourceMember(s => s.OppositeMerchantId, s => s.Ignore())
                .ForSourceMember(s => s.DesiredAssetId, s => s.Ignore())
                .ForSourceMember(s => s.InvoiceStatus, s => s.Ignore());

            CreateMap<HistoryOperationView, HistoryOperationViewModel>();

            CreateMap<PayHistory.AutorestClient.Models.HistoryOperationModel, HistoryOperation>(
                    MemberList.Source)
                .ForSourceMember(s => s.InvoiceId, s => s.Ignore())
                .ForSourceMember(s => s.MerchantId, s => s.Ignore())
                .ForSourceMember(s => s.EmployeeEmail, s => s.Ignore())
                .ForSourceMember(s => s.OppositeMerchantId, s => s.Ignore())
                .ForSourceMember(s => s.DesiredAssetId, s => s.Ignore());

            CreateMap<HistoryOperation, HistoryOperationModel>();
        }
    }
}
