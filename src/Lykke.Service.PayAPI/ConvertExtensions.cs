using System;
using System.Linq;
using AutoMapper;
using Common;
using Lykke.Service.PayAPI.Models;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;

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

                    response.PaymentStatus = "PAYMENT_REQUEST_CREATED";

                    break;
                case PaymentRequestStatus.Confirmed:

                    response.PaymentStatus = "PAYMENT_CONFIRMED";

                    break;
                case PaymentRequestStatus.InProcess:

                    response.PaymentStatus = "PAYMENT_INPROGRESS";

                    break;
                case PaymentRequestStatus.Error:

                    response.PaymentStatus = "PAYMENT_ERROR";

                    if (src.Error.Equals("NOT DETECTED"))
                    {
                        response.PaymentRequest.Error = "TRANSACTION_NOT_DETECTED";
                    }
                    else if (src.Error.Equals("NOT CONFIRMED"))
                    {
                        response.PaymentRequest.Error = "TRANSACTION_NOT_CONFIRMED";
                    }
                    else if (src.Error.Equals("AMOUNT BELOW"))
                    {
                        response.PaymentRequest.Error = "AMOUNT_BELOW";
                    }
                    else if (src.Error.Equals("AMOUNT ABOVE"))
                    {
                        response.PaymentRequest.Error = "AMOUNT_ABOVE";
                    }
                    else if (src.Error.Equals("EXPIRED"))
                    {
                        response.PaymentRequest.Error = "PAYMENT_EXPIRED";
                    } 
                    else if (src.Error.Equals("REFUND NOT CONFIRMED"))
                    {
                        response.RefundRequest = Mapper.Map<RefundRequestResponseModel>(src.Refund);

                        if (response.RefundRequest != null)
                            response.RefundRequest.Error = "TRANSACTION_NOT_CONFIRMED";
                    }
                    else throw new Exception("Unknown payment request error description");

                    break;
                case PaymentRequestStatus.RefundInProgress:

                    response.PaymentStatus = "REFUND_INPROGRESS";

                    response.RefundRequest = Mapper.Map<RefundRequestResponseModel>(src.Refund);

                    break;
                case PaymentRequestStatus.Refunded:

                    response.PaymentStatus = "REFUND_CONFIRMED";

                    response.RefundRequest = Mapper.Map<RefundRequestResponseModel>(src.Refund);

                    break;
                default:
                    throw new Exception("Unknown payment request status");
            }

            return response;
        }
    }
}
