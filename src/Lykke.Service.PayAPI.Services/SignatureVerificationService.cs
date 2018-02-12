using System;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
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
        private readonly ILog _log;
        private readonly IPayAuthClient _payAuthClient;

        public SignatureVerificationService(ILog log, IPayAuthClient payAuthClient)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
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
                    return SecurityErrorType.SignIncorrect;
            }
        }

        public async Task<SecurityErrorType> VerifyPostRequest(HttpRequest httpRequest)
        {
            var verifyRequest = new VerifyRequest
            {
                ClientId = httpRequest.GetMerchantId(),
                SystemId = LykkePayConstants.SystemId,
                Signature = httpRequest.GetMerchantSign(),
                Text = httpRequest.ReadBody()
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
                Text = httpRequest.Path
            };

            return await DoVerifyRequest(verifyRequest);
        }

        private async Task<SecurityErrorType> DoVerifyRequest(VerifyRequest verifyRequest)
        {
            string verificationResultStr = await _payAuthClient.VerifyAsync(verifyRequest);

            if (!Enum.TryParse(verificationResultStr, out SecurityErrorType verificationResult))
                throw new UnrecognizedSignatureVerificationException(verificationResultStr);

            return verificationResult;
        }
    }
}
