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

                    response.PaymentStatus = PaymentRequestPublicStatuses.PaymentError;

                    switch (src.Error)
                    {
                        case PaymentRequestErrorType.PaymentAmountAbove:

                            response.PaymentRequest.Error = PaymentRequestErrorPublicCodes.PaymentAmountAbove;

                            break;
                        case PaymentRequestErrorType.PaymentAmountBelow:

                            response.PaymentRequest.Error = PaymentRequestErrorPublicCodes.PaymentAmountBelow;

                            break;
                        case PaymentRequestErrorType.PaymentExpired:

                            response.PaymentRequest.Error = PaymentRequestErrorPublicCodes.PaymentExpired;

                            break;
                        case PaymentRequestErrorType.RefundNotConfirmed:

                            response.RefundRequest = Mapper.Map<RefundRequestResponseModel>(src.Refund);

                            if (response.RefundRequest != null)
                                response.RefundRequest.Error = PaymentRequestErrorPublicCodes.TransactionNotConfirmed;

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
