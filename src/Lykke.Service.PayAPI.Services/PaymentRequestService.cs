﻿using System;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Lykke.Service.PayAPI.Core.Domain.PaymentRequest;
using Lykke.Service.PayAPI.Core.Exceptions;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayCallback.Client;
using Lykke.Service.PayCallback.Client.Models;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Exceptions;
using Lykke.Service.PayInternal.Client.Models.Order;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;

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
            OrderModel order;

            try
            {
                payment = await _payInternalClient.CreatePaymentRequestAsync(createPaymentRequest);

                order = await _payInternalClient.ChechoutOrderAsync(new ChechoutRequestModel
                {
                    MerchantId = request.MerchantId,
                    PaymentRequestId = payment.Id
                });
            }
            catch (DefaultErrorResponseException ex)
            {
                throw new ApiRequestException(ex.Error.ErrorMessage, string.Empty, ex.StatusCode);
            }
            catch (CreatePaymentRequestResponseException ex)
            {
                if (ex.Error.Code == CreatePaymentRequestErrorType.SettlementAssetNotAvailable)
                    throw new InvalidSettlementAssetException();

                if (ex.Error.Code == CreatePaymentRequestErrorType.PaymentAssetNotAvailable)
                    throw new InvalidPaymentAssetException();

                throw new ApiRequestException(ex.Message, string.Empty, ex.StatusCode);
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
                catch (ErrorResponseException ex)
                {
                    throw new ApiRequestException(ex.Error.ErrorMessage, string.Empty, ex.StatusCode);
                }
            }

            return new CreatePaymentResponse
            {
                Id = payment.Id,
                PaymentAssetId = payment.PaymentAssetId,
                Amount = order.PaymentAmount,
                OrderId = payment.OrderId,
                Address = payment.WalletAddress,
                Timestamp = requestTime.ToIsoDateTime(),
                ExchangeRate = order.ExchangeRate
            };
        }

        public async Task<PaymentRequestDetailsModel> GetPaymentRequestDetailsAsync(string merchantId, string paymentRequestId)
        {
            try
            {
                return await _payInternalClient.GetPaymentRequestDetailsAsync(merchantId, paymentRequestId);
            }
            catch (DefaultErrorResponseException ex)
            {
                throw new ApiRequestException(ex.Error.ErrorMessage, string.Empty, ex.StatusCode);
            }
        }

        public async Task<PaymentRequestDetailsModel> RefundAsync(RefundRequest request)
        {
            await _payInternalClient.RefundAsync(Mapper.Map<RefundRequestModel>(request));

            return await _payInternalClient.GetPaymentRequestDetailsAsync(request.MerchantId, request.PaymentRequestId);
        }
    }
}
