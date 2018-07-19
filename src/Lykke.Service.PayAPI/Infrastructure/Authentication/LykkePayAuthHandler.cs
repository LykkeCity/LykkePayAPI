using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Contracts.Security;
using Lykke.Service.PayAPI.Core;
using Lykke.Service.PayAPI.Core.Exceptions;
using Lykke.Service.PayAPI.Core.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lykke.Service.PayAPI.Infrastructure.Authentication
{
    public class LykkePayAuthHandler : AuthenticationHandler<LykkePayAuthOptions>
    {
        private readonly HttpContext _httpContext;
        private readonly ISignatureVerificationService _signatureVerificationService;
        private readonly ILog _log;

        public LykkePayAuthHandler(
            IOptionsMonitor<LykkePayAuthOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IHttpContextAccessor httpContextAccessor,
            ISignatureVerificationService signatureVerificationService,
            ILogFactory logFactory) : base(options, logger, encoder, clock)
        {
            _httpContext = httpContextAccessor.HttpContext;
            _signatureVerificationService = signatureVerificationService ??
                                            throw new ArgumentNullException(nameof(signatureVerificationService));
            _log = logFactory?.CreateLog(this) ?? throw new ArgumentNullException(nameof(logFactory));
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string merchantId = _httpContext.Request.GetMerchantId();
            string merchantSign = _httpContext.Request.GetMerchantSign();

            if (string.IsNullOrWhiteSpace(merchantId) || string.IsNullOrWhiteSpace(merchantSign))
                return AuthenticateResult.NoResult();

#if DEBUG
            return CreateSuccessResult();
#endif 

            try
            {
                SecurityErrorType verificationResult =
                    await _signatureVerificationService.VerifyRequest(_httpContext.Request);

                switch (verificationResult)
                {
                    case SecurityErrorType.Ok:
                        return CreateSuccessResult();
                    case SecurityErrorType.SignIncorrect:
                        return AuthenticateResult.Fail("Invalid signature");
                    default:
                        return AuthenticateResult.Fail("Unexpected signature verification result");
                }
            }
            catch (UnrecognizedSignatureVerificationException ex)
            {
                _log.Error(ex);

                return AuthenticateResult.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                _log.Error(ex);

                return AuthenticateResult.Fail(ex);
            }
            
        }

        private AuthenticateResult CreateSuccessResult()
        {
            var identities = new List<ClaimsIdentity> {new ClaimsIdentity("Header")};

            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identities), Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}
