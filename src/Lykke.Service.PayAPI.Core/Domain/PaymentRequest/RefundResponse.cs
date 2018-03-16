using System;
using System.Collections.Generic;

namespace Lykke.Service.PayAPI.Core.Domain.PaymentRequest
{
    public class RefundResponse
    {
        public string PaymentRequestId { get; set; }

        public decimal Amount { get; set; }

        public string AssetId { get; set; }

        public DateTime DueDate { get; set; }

        public IEnumerable<RefundTransactionResponse> Transactions { get; set; }
    }
}
