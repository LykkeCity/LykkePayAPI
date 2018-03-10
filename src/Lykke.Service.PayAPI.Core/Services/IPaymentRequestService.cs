using System.Threading.Tasks;
using Lykke.Service.PayAPI.Core.Domain.PaymentRequest;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;

namespace Lykke.Service.PayAPI.Core.Services
{
    public interface IPaymentRequestService
    {
        Task<CreatePaymentResponse> CreatePaymentRequestAsync(CreatePaymentRequest request);

        Task<PaymentRequestDetailsModel> GetPaymentRequestDetailsAsync(string address);

        Task<string> RefundAsync(string merchantId, string address, string destinationAddress);
    }
}
