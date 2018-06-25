namespace Lykke.Service.PayAPI.Models.Mobile.History
{
    public enum HistoryOperationType
    {
        None = 0,
        Recharge = 1,
        OutgoingInvoicePayment = 2,
        IncomingInvoicePayment = 3,
        OutgoingExchange = 4,
        IncomingExchange = 5,
        Withdrawal = 6
    }
}
