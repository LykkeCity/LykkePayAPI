using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.PayAPI.Core.Domain.PaymentRequest;
using Lykke.Service.PayAPI.Core.Exceptions;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayCallback.Client;
using Lykke.Service.PayCallback.Client.Models;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;

namespace Lykke.Service.PayAPI.Services
{
    public class PaymentRequestService : IPaymentRequestService
    {
        private readonly IPayInternalClient _payInternalClient;
        private readonly IPayCallbackClient _payCallbackClient;
        private readonly TimeSpan _dueDate;
        private readonly ILog _log;

        public PaymentRequestService(
            IPayInternalClient payInternalClient,
            IPayCallbackClient payCallbackClient,
            TimeSpan dueDate,
            ILog log)
        {
            _payInternalClient = payInternalClient ?? throw new ArgumentNullException(nameof(payInternalClient));
            _payCallbackClient = payCallbackClient ?? throw new ArgumentNullException(nameof(payCallbackClient));
            _dueDate = dueDate;
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task<CreatePaymentResponse> CreatePaymentRequestAsync(CreatePaymentRequest request)
        {
            var paymentDueDate = DateTime.UtcNow.Add(_dueDate);

            var requestTime = DateTime.UtcNow;

            CreatePaymentRequestModel createPaymentRequest = request.ToServiceClientModel(paymentDueDate);

            PaymentRequestModel payment;
            PaymentRequestDetailsModel checkout;

            try
            {
                payment = await _payInternalClient.CreatePaymentRequestAsync(createPaymentRequest);

                checkout = await _payInternalClient.ChechoutAsync(request.MerchantId, payment.Id);
            }
            catch (PayInternal.Client.ErrorResponseException ex)
            {
                throw new ApiRequestException(ex.Error.ErrorMessage, string.Empty, ex.StatusCode);
            }

            if (!string.IsNullOrWhiteSpace(request.CallbackUrl))
            {
                try
                {
                    await _payCallbackClient.AddPaymentCallback(new CreatePaymentCallbackModel
                    {
                        MerchantId = payment.MerchantId,
                        PaymentRequestId = payment.Id,
                        CallbackUrl = request.CallbackUrl
                    });
                }
                catch (PayCallback.Client.ErrorResponseException ex)
                {
                    throw new ApiRequestException(ex.Error.ErrorMessage, string.Empty, ex.StatusCode);
                }
            }

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
            try
            {
                PaymentRequestModel paymentRequest = await _payInternalClient.GetPaymentRequestByAddressAsync(address);

                return await _payInternalClient.GetPaymentRequestDetailsAsync(paymentRequest.MerchantId,
                    paymentRequest.Id);
            }
            catch (PayInternal.Client.ErrorResponseException ex)
            {
                throw new ApiRequestException(ex.Error.ErrorMessage, string.Empty, ex.StatusCode);
            }
        }

        public async Task RefundAsync(string paymentRequestId, string addresss)
        {
            try
            {
                //todo: call payinternalClient to initiate a refund
            }
            catch (PayInternal.Client.ErrorResponseException ex)
            {
                throw new ApiRequestException(ex.Error.ErrorMessage, string.Empty, ex.StatusCode);
            }
        }
    }
}
