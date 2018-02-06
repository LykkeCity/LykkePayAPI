using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.PayAPI.Core.Domain.PaymentRequest;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;

namespace Lykke.Service.PayAPI.Services
{
    public class PaymentRequestService : IPaymentRequestService
    {
        private readonly IPayInternalClient _payInternalClient;
        private readonly TimeSpan _dueDate;
        private readonly ILog _log;

        public PaymentRequestService(
            IPayInternalClient payInternalClient,
            TimeSpan dueDate,
            ILog log)
        {
            _payInternalClient = payInternalClient ?? throw new ArgumentNullException(nameof(payInternalClient));
            _dueDate = dueDate;
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task<CreatePaymentResponse> CreatePaymentRequestAsync(CreatePaymentRequest request)
        {
            var paymentDueDate = DateTime.UtcNow.Add(_dueDate);

            var requestTime = DateTime.UtcNow;

            CreatePaymentRequestModel createPaymentRequest = request.ToServiceClientModel(paymentDueDate);

            PaymentRequestModel payment = await _payInternalClient.CreatePaymentRequestAsync(createPaymentRequest);

            PaymentRequestDetailsModel
                checkout = await _payInternalClient.ChechoutAsync(request.MerchantId, payment.Id);

            // todo: create callback url record

            return new CreatePaymentResponse
            {
                PaymentAssetId = payment.PaymentAssetId,
                Amount = checkout.Order.PaymentAmount,
                OrderId = payment.OrderId,
                Address = payment.WalletAddress,
                Timestamp = requestTime.ToIsoDateTime(),
                ExchangeRate = checkout.Order.ExchangeRate
            };
        }

        public async Task<PaymentRequestDetailsModel> GetPaymentRequestDetailsAsync(string address)
        {
            PaymentRequestModel paymentRequest = await _payInternalClient.GetPaymentRequestByAddressAsync(address);

            return await _payInternalClient.GetPaymentRequestDetailsAsync(paymentRequest.MerchantId, paymentRequest.Id);
        }
    }
}
