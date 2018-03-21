using System.Threading.Tasks;
using Lykke.Service.PayAPI.Core.Domain.PaymentRequest;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using RefundResponse = Lykke.Service.PayAPI.Core.Domain.PaymentRequest.RefundResponse;

namespace Lykke.Service.PayAPI.Core.Services
{
    public interface IPaymentRequestService
    {
        Task<CreatePaymentResponse> CreatePaymentRequestAsync(CreatePaymentRequest request);

        Task<PaymentRequestDetailsModel> GetPaymentRequestDetailsAsync(string merchantId, string paymentRequestId);

        Task<RefundResponse> RefundAsync(RefundRequest request);
    }
}
