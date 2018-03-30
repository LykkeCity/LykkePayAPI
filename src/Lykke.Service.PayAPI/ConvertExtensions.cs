using System;
using System.Linq;
using AutoMapper;
using Common;
using Lykke.Service.PayAPI.Models;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using Lykke.Service.PayInternal.Contract.PaymentRequest;
using PaymentRequestErrorType = Lykke.Service.PayInternal.Client.Models.PaymentRequest.PaymentRequestErrorType;
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

                    switch (src.Error)
                    {
                        case PaymentRequestErrorType.PaymentAmountAbove:

                            response.PaymentStatus = PaymentRequestPublicStatuses.PaymentError;

                            response.Error = new ErrorResponseModel
                                {Code = PaymentRequestErrorPublicCodes.PaymentAmountAbove};

                            break;
                        case PaymentRequestErrorType.PaymentAmountBelow:

                            response.PaymentStatus = PaymentRequestPublicStatuses.PaymentError;

                            response.Error = new ErrorResponseModel
                                {Code = PaymentRequestErrorPublicCodes.PaymentAmountBelow};

                            break;
                        case PaymentRequestErrorType.PaymentExpired:

                            response.PaymentStatus = PaymentRequestPublicStatuses.PaymentError;

                            response.Error = new ErrorResponseModel
                                {Code = PaymentRequestErrorPublicCodes.PaymentExpired};

                            break;
                        case PaymentRequestErrorType.RefundNotConfirmed:

                            response.PaymentStatus = PaymentRequestPublicStatuses.RefundError;

                            response.RefundRequest = Mapper.Map<RefundRequestResponseModel>(src.Refund);

                            response.Error = new ErrorResponseModel
                                {Code = PaymentRequestErrorPublicCodes.TransactionNotConfirmed};

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
    }
}
