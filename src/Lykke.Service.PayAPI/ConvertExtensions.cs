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
            PaymentStatusResponseModel apiResponse;

            switch (src.Status)
            {
                    case PaymentRequestStatus.New:

                        apiResponse = new PaymentStatusResponseModel
                        {
                            PaymentStatus = "PAYMENT_REQUEST_CREATED",
                            PaymentResponse = new PaymentResponseModel
                            {
                                Id = src.Id,
                                Timestamp = src.Timestamp.ToIsoDateTime(), 
                                Address = src.WalletAddress,
                                OrderId = src.Order?.Id,
                                PaymentAsset = src.PaymentAssetId,
                                Amount = src.Order?.PaymentAmount ?? 0,
                                ExchangeRate = src.Order?.ExchangeRate ?? 0,
                                Transactions = src.Transactions.Select(Mapper.Map<PaymentResponseTransactionModel>).ToList()
                            }
                        };

                        break;
                    case PaymentRequestStatus.Confirmed:

                        apiResponse = new PaymentStatusResponseModel
                        {
                            PaymentStatus = "PAYMENT_CONFIRMED",
                            PaymentResponse = new PaymentResponseModel
                            {
                                PaymentAsset = src.PaymentAssetId,
                                Timestamp = src.Timestamp.ToIsoDateTime(),
                                Transactions = src.Transactions.Select(Mapper.Map<PaymentResponseTransactionModel>)
                                    .ToList()
                            }
                        };

                        break;
                    case PaymentRequestStatus.InProcess:

                        apiResponse = new PaymentStatusResponseModel
                        {
                            PaymentStatus = "PAYMENT_INPROGRESS",
                            PaymentResponse = new PaymentResponseModel
                            {
                                PaymentAsset = src.PaymentAssetId,
                                Timestamp = src.Timestamp.ToIsoDateTime(),
                                Settlement = "TRANSACTION_DETECTED",
                                Transactions = src.Transactions.Select(Mapper.Map<PaymentResponseTransactionModel>)
                                    .ToList()
                            }
                        };

                    break;
                    case PaymentRequestStatus.Error:
                        var apiStatus = "PAYMENT_ERROR";

                        if (src.Error.Equals("NOT DETECTED"))
                        {
                            apiResponse = new PaymentStatusResponseModel
                            {
                                PaymentStatus = apiStatus,
                                PaymentResponse = new PaymentResponseModel
                                {
                                    Error = "TRANSACTION_NOT_DETECTED"
                                }
                            };
                        } else if (src.Error.Equals("NOT CONFIRMED"))
                        {
                            apiResponse = new PaymentStatusResponseModel
                            {
                                PaymentStatus = apiStatus,
                                PaymentResponse = new PaymentResponseModel
                                {
                                    Error = "TRANSACTION_NOT_CONFIRMED"
                                }
                            };
                        } else if (src.Error.Equals("AMOUNT BELOW"))
                        {
                            apiResponse = new PaymentStatusResponseModel
                            {
                                PaymentStatus = apiStatus,
                                PaymentResponse = new PaymentResponseModel
                                {
                                    Error = "AMOUNT_BELOW",
                                    PaymentAsset = src.PaymentAssetId,
                                    Timestamp = src.Timestamp.ToIsoDateTime(),
                                    RefundLink =
                                        src.Transactions.OrderByDescending(x => x.FirstSeen).FirstOrDefault()
                                            ?.RefundLink,
                                    Transactions = src.Transactions.Select(Mapper.Map<PaymentResponseTransactionModel>)
                                        .ToList()
                                }
                            };
                        } else if (src.Error.Equals("AMOUNT ABOVE"))
                        {
                            apiResponse = new PaymentStatusResponseModel
                            {
                                PaymentStatus = apiStatus,
                                PaymentResponse = new PaymentResponseModel
                                {
                                    Error = "AMOUNT_ABOVE",
                                    PaymentAsset = src.PaymentAssetId,
                                    Timestamp = src.Timestamp.ToIsoDateTime(),
                                    RefundLink =
                                        src.Transactions.OrderByDescending(x => x.FirstSeen).FirstOrDefault()
                                            ?.RefundLink,
                                    Transactions = src.Transactions.Select(Mapper.Map<PaymentResponseTransactionModel>)
                                        .ToList()
                                }
                            };
                        } else if (src.Error.Equals("EXPIRED"))
                        {
                            apiResponse = new PaymentStatusResponseModel
                            {
                                PaymentStatus = apiStatus,
                                PaymentResponse = new PaymentResponseModel
                                {
                                    Error = "PAYMENT_EXPIRED",
                                    PaymentAsset = src.PaymentAssetId,
                                    Timestamp = src.Timestamp.ToIsoDateTime(),
                                    RefundLink =
                                        src.Transactions.OrderByDescending(x => x.FirstSeen).FirstOrDefault()
                                            ?.RefundLink,
                                    Transactions = src.Transactions.Select(Mapper.Map<PaymentResponseTransactionModel>)
                                        .ToList()
                                }
                            };
                        }
                        else throw new Exception("Unknown payment request error description");

                        break;
                    default:
                        throw new Exception("Unknown payment request status");
            }

            return apiResponse;
        }
    }
}
