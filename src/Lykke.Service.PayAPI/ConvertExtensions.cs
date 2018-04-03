using System;
using System.Linq;
using AutoMapper;
using Common;
using Lykke.Service.PayAPI.Models;
using Lykke.Service.PayInternal.Client.Exceptions;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using Lykke.Service.PayInternal.Contract.PaymentRequest;
using PaymentRequestProcessingError = Lykke.Service.PayInternal.Client.Models.PaymentRequest.PaymentRequestProcessingError;
using PaymentRequestStatus = Lykke.Service.PayInternal.Client.Models.PaymentRequest.PaymentRequestStatus;

namespace Lykke.Service.PayAPI
{
    public static class ConvertExtensions
    {
        public static PaymentStatusResponseModel ToStatusApiModel(this PaymentRequestDetailsModel src)
        {
            PaymentStatusResponseModel response = new PaymentStatusResponseModel
            {
                Id = src.Id,
                PaymentAsset = src.PaymentAssetId,
                SettlementAsset = src.SettlementAssetId,
                OrderId = src.OrderId,
                PaymentRequest = new PaymentRequestResponseModel
                {
                    Amount = src.Order?.PaymentAmount,
                    Address = src.WalletAddress,
                    CreatedAt = src.Timestamp.ToIsoDateTime(),
                    ExchangeRate = src.Order?.ExchangeRate,
                    ExpirationDt = src.DueDate.ToIsoDateTime(),
                    Transactions = src.Transactions.Any() ? src.Transactions.Select(Mapper.Map<PaymentResponseTransactionModel>).ToList() : null
                }
            };

            switch (src.Status)
            {
                case PaymentRequestStatus.New:

                    response.PaymentStatus = PaymentRequestPublicStatuses.PaymentRequestCreated;

                    break;
                case PaymentRequestStatus.Confirmed:

                    response.PaymentStatus = PaymentRequestPublicStatuses.PaymentConfirmed;

                    break;
                case PaymentRequestStatus.InProcess:

                    response.PaymentStatus = PaymentRequestPublicStatuses.PaymentInProgress;

                    break;
                case PaymentRequestStatus.Error:

                    switch (src.ProcessingError)
                    {
                        case PaymentRequestProcessingError.PaymentAmountAbove:

                            response.PaymentStatus = PaymentRequestPublicStatuses.PaymentError;

                            response.PaymentRequest.Error = PaymentRequestErrorPublicCodes.PaymentAmountAbove;

                            break;
                        case PaymentRequestProcessingError.PaymentAmountBelow:

                            response.PaymentStatus = PaymentRequestPublicStatuses.PaymentError;

                            response.PaymentRequest.Error = PaymentRequestErrorPublicCodes.PaymentAmountBelow;

                            break;
                        case PaymentRequestProcessingError.PaymentExpired:

                            response.PaymentStatus = PaymentRequestPublicStatuses.PaymentError;

                            response.PaymentRequest.Error = PaymentRequestErrorPublicCodes.PaymentExpired;

                            break;
                        case PaymentRequestProcessingError.RefundNotConfirmed:

                            response.PaymentStatus = PaymentRequestPublicStatuses.RefundError;

                            response.RefundRequest = Mapper.Map<RefundRequestResponseModel>(src.Refund);

                            if (response.RefundRequest != null)
                                response.RefundRequest.Error = PaymentRequestErrorPublicCodes.TransactionNotConfirmed;

                            break;
                        case PaymentRequestProcessingError.UnknownPayment:

                            response.PaymentStatus = PaymentRequestPublicStatuses.PaymentError;

                            break;
                        case PaymentRequestProcessingError.UnknownRefund:

                            response.PaymentStatus = PaymentRequestPublicStatuses.RefundError;

                            response.RefundRequest = Mapper.Map<RefundRequestResponseModel>(src.Refund);

                            if (response.RefundRequest != null)
                                response.RefundRequest.Error = PaymentRequestErrorPublicCodes.RefundIsNotAvailable;

                            break;
                        default:
                            throw new Exception("Unknown payment request error type");
                    }

                    break;
                case PaymentRequestStatus.RefundInProgress:

                    response.PaymentStatus = PaymentRequestPublicStatuses.RefundInProgress;

                    response.RefundRequest = Mapper.Map<RefundRequestResponseModel>(src.Refund);

                    break;
                case PaymentRequestStatus.Refunded:

                    response.PaymentStatus = PaymentRequestPublicStatuses.RefundConfirmed;

                    response.RefundRequest = Mapper.Map<RefundRequestResponseModel>(src.Refund);

                    break;
                default:
                    throw new Exception("Unknown payment request status");
            }

            return response;
        }

        public static PaymentErrorResponseModel ToErrorModel(this RefundErrorResponseException src)
        {
            switch (src.Error.Code)
            {
                case RefundErrorType.Unknown:
                    return PaymentErrorResponseModel.Create(PaymentErrorType.RefundIsNotAvailable);
                case RefundErrorType.InvalidDestinationAddress:
                    return PaymentErrorResponseModel.Create(PaymentErrorType.InvalidDestinationAddress);
                case RefundErrorType.MultitransactionNotSupported:
                    return PaymentErrorResponseModel.Create(PaymentErrorType. RefundIsNotAvailable);
                case RefundErrorType.NoPaymentTransactions:
                    return PaymentErrorResponseModel.Create(PaymentErrorType.NoPaymentTransactions);
                case RefundErrorType.NotAllowedInStatus:
                    return PaymentErrorResponseModel.Create(PaymentErrorType.RefundIsNotAvailable);
                case RefundErrorType.PaymentRequestNotFound:
                    return PaymentErrorResponseModel.Create(PaymentErrorType.InvalidPaymentId);
                default:
                    return PaymentErrorResponseModel.Create(PaymentErrorType.RefundIsNotAvailable);
            }
        }
    }
}
