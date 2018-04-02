namespace Lykke.Service.PayAPI.Models
{
    public enum PaymentErrorType
    {
        InvalidPaymentId = 0,

        InvalidDestinationAddress,

        NoPaymentTransactions,

        RefundIsNotAvailable,

        InvalidSettlementAsset,

        InvalidCallbackUrl
    }
}
