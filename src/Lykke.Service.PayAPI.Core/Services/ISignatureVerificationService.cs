using System.Threading.Tasks;
using Lykke.Contracts.Security;
using Microsoft.AspNetCore.Http;

namespace Lykke.Service.PayAPI.Core.Services
{
    public interface ISignatureVerificationService
    {
        Task<SecurityErrorType> VerifyRequest(HttpRequest httpRequest);
        Task<SecurityErrorType> VerifyPostRequest(HttpRequest httpRequest);
        Task<SecurityErrorType> VerifyGetRequest(HttpRequest httpRequest);
    }
}
