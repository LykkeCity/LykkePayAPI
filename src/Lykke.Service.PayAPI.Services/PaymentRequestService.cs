using System;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Lykke.Service.PayAPI.Core.Domain.PaymentRequest;
using Lykke.Service.PayAPI.Core.Exceptions;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayCallback.Client;
using Lykke.Service.PayCallback.Client.Models;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using RefundResponse = Lykke.Service.PayAPI.Core.Domain.PaymentRequest.RefundResponse;

namespace Lykke.Service.PayAPI.Services
{
    public class PaymentRequestService : IPaymentRequestService
    {
        private readonly IPayInternalClient _payInternalClient;
        private readonly IPayCallbackClient _payCallbackClient;
        private readonly TimeSpan _dueDate;

        public PaymentRequestService(
            IPayInternalClient payInternalClient,
            IPayCallbackClient payCallbackClient,
            TimeSpan dueDate)
        {
            _payInternalClient = payInternalClient ?? throw new ArgumentNullException(nameof(payInternalClient));
            _payCallbackClient = payCallbackClient ?? throw new ArgumentNullException(nameof(payCallbackClient));
            _dueDate = dueDate;
        }

        public async Task<CreatePaymentResponse> CreatePaymentRequestAsync(CreatePaymentRequest request)
        {
            var paymentDueDate = DateTime.UtcNow.Add(_dueDate);

            var requestTime = DateTime.UtcNow;

            var createPaymentRequest =
                Mapper.Map<CreatePaymentRequestModel>(request, opt => opt.Items["DueDate"] = paymentDueDate);

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
                    await _payCallbackClient.SetPaymentCallback(new SetPaymentCallbackModel
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
                Id = payment.Id,
                PaymentAssetId = payment.PaymentAssetId,
                Amount = checkout.Order.PaymentAmount,
                OrderId = payment.OrderId,
                Address = payment.WalletAddress,
                Timestamp = requestTime.ToIsoDateTime(),
                ExchangeRate = checkout.Order.ExchangeRate
            };
        }

        public async Task<PaymentRequestDetailsModel> GetPaymentRequestDetailsAsync(string merchantId, string paymentRequestId)
        {
            try
            {
                return await _payInternalClient.GetPaymentRequestDetailsAsync(merchantId, paymentRequestId);
            }
            catch (PayInternal.Client.ErrorResponseException ex)
            {
                throw new ApiRequestException(ex.Error.ErrorMessage, string.Empty, ex.StatusCode);
            }
        }

        public async Task<RefundResponse> RefundAsync(RefundRequest request)
        {
            try
            {
                var response = await _payInternalClient.RefundAsync(Mapper.Map<RefundRequestModel>(request));

                return Mapper.Map<RefundResponse>(response);
            }
            catch (PayInternal.Client.ErrorResponseException ex)
            {
                throw new ApiRequestException(ex.Error.ErrorMessage, string.Empty, ex.StatusCode);
            }
        }
    }
}
