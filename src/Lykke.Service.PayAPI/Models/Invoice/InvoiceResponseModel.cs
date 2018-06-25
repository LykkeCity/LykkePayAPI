using System;
using System.Collections.Generic;
using System.Text;
using Lykke.Service.PayInvoice.Client.Models.Invoice;

namespace Lykke.Service.PayAPI.Models
{
    public class InvoiceResponseModel : InvoiceModel
    {
        /// <summary>
        /// Display name of the merchant
        /// </summary>
        public string MerchantName { get; set; }

        /// <summary>
        /// Status name
        /// </summary>
        public new string Status { get; set; }

        /// <summary>
        /// IATA invoice date
        /// </summary>
        public string IataInvoiceDate { get; set; }

        /// <summary>
        /// Settlement month period
        /// </summary>
        public string SettlementMonthPeriod { get; set; }

        /// <summary>
        /// Logo blob url of the merchant
        /// </summary>
        public string LogoUrl { get; set; }

        public decimal SettlementAmountInBaseAsset { get; set; }
    }
}
