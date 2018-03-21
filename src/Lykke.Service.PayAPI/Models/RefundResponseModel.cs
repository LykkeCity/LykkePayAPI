using System;
using System.Collections.Generic;

namespace Lykke.Service.PayAPI.Models
{
    public class RefundResponseModel
    {
        public string PaymentRequestId { get; set; }

        public decimal Amount { get; set; }

        public string AssetId { get; set; }

        public DateTime DueDate { get; set; }

        public IEnumerable<RefundTransactionResponseModel> Transactions { get; set; }
    }
}
