﻿using System;
using Lykke.Service.PayInvoice.Client.Models.Invoice;

namespace Lykke.Service.PayAPI.Models.Mobile.History
{
    public class HistoryOperationModel: HistoryOperationViewModel
    {
        public string MerchantName { get; set; }

        public string EmployeeEmail { get; set; }

        public string TxHash { get; set; }

        public string ExplorerUrl { get; set; }

        public int BlockHeight { get; set; }

        public int BlockConfirmations { get; set; }

        public string InvoiceNumber { get; set; }

        public string BillingCategory { get; set; }

        public InvoiceStatus? InvoiceStatus { get; set; }
    }
}
