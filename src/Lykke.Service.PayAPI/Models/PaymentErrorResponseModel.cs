namespace Lykke.Service.PayAPI.Models
{
    public class PaymentErrorResponseModel
    {
        public PaymentErrorDetailsModel Error { get; set; }

        public static PaymentErrorResponseModel Create(PaymentErrorType errorCode)
        {
            return new PaymentErrorResponseModel {Error = new PaymentErrorDetailsModel {Code = errorCode}};
        }
    }
}
