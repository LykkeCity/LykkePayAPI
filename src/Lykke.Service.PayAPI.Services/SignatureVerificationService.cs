using System;
using System.Net;
using System.Threading.Tasks;
using Lykke.Contracts.Security;
using Lykke.Service.PayAPI.Core;
using Lykke.Service.PayAPI.Core.Exceptions;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayAuth.Client;
using Lykke.Service.PayAuth.Client.Models;
using Microsoft.AspNetCore.Http;

namespace Lykke.Service.PayAPI.Services
{
    public class SignatureVerificationService : ISignatureVerificationService
    {
        private readonly IPayAuthClient _payAuthClient;

        public SignatureVerificationService(IPayAuthClient payAuthClient)
        {
            _payAuthClient = payAuthClient ?? throw new ArgumentNullException(nameof(payAuthClient));
        }

        public async Task<SecurityErrorType> VerifyRequest(HttpRequest httpRequest)
        {
            switch (httpRequest?.Method)
            {
                case WebRequestMethods.Http.Post:
                    return await VerifyPostRequest(httpRequest);
                case WebRequestMethods.Http.Get:
                    return await VerifyGetRequest(httpRequest);
                default:
                    // todo: requires rethinking
                    return SecurityErrorType.SignIncorrect;
            }
        }

        public async Task<SecurityErrorType> VerifyPostRequest(HttpRequest httpRequest)
        {
            string body = httpRequest.ReadBody();

            var verifyRequest = new VerifyRequest
            {
                ClientId = httpRequest.GetMerchantId(),
                SystemId = LykkePayConstants.SystemId,
                Signature = httpRequest.GetMerchantSign(),
                Text = string.IsNullOrEmpty(body) ? httpRequest.Path.Value : body
            };

            return await DoVerifyRequest(verifyRequest);
        }

        public async Task<SecurityErrorType> VerifyGetRequest(HttpRequest httpRequest)
        {
            var verifyRequest = new VerifyRequest
            {
                ClientId = httpRequest.GetMerchantId(),
                SystemId = LykkePayConstants.SystemId,
                Signature = httpRequest.GetMerchantSign(),
                Text = httpRequest.Path.Value
            };

            return await DoVerifyRequest(verifyRequest);
        }

        private async Task<SecurityErrorType> DoVerifyRequest(VerifyRequest verifyRequest)
        {
            SignatureValidationResponse validationResponse = await _payAuthClient.VerifyAsync(verifyRequest);

            if (!Enum.TryParse(validationResponse.ErrorType, out SecurityErrorType verificationResult))
                throw new UnrecognizedSignatureVerificationException(validationResponse.Description);

            return verificationResult;
        }
    }
}
