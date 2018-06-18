using System;

namespace Lykke.Service.PayAPI.Models.Mobile.History
{
    public class HistoryOperationModel
    {
        public string Id { get; set; }

        public string Logo { get; set; }

        public string Title { get; set; }

        public DateTime TimeStamp { get; set; }

        public decimal Amount { get; set; }

        public string AssetId { get; set; }

        public string SoldBy { get; set; }

        public int BlockHeight { get; set; }

        public int BlockConfirmations { get; set; }

        public string TxHash { get; set; }
    }
}
