using System.Linq;
using AutoMapper;
using Lykke.Service.PayAPI.Models;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;

namespace Lykke.Service.PayAPI
{
    public static class ConvertExtensions
    {
        public static PaymentStatusResponseModel ToStatusApiModel(this PaymentRequestDetailsModel src)
        {
            return new PaymentStatusResponseModel
            {
                PaymentResponse = new PaymentResponseModel
                {
                    Error = src.Error,
                    PaymentRequestId = src.Id,
                    WalletAddress = src.WalletAddress,
                    Transactions = src.Transactions.Select(Mapper.Map<PaymentResponseTransactionModel>).ToList()
                },
                PaymentStatus = src.Status.ToString()
            };
        }
    }
}
