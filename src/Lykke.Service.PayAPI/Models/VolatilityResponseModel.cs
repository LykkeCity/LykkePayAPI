using System;

namespace Lykke.Service.PayAPI.Models
{
    public class VolatilityResponseModel
    {
        public string AssetPairId { get; set; }

        public DateTime Date { get; set; }

        public Decimal ClosePriceStdev { get; set; }

        public Decimal HighPriceStdev { get; set; }

        public Decimal MultiplierFactor { get; set; }

        public Decimal ClosePriceVolatilityShield { get; set; }

        public Decimal HighPriceVolatilityShield { get; set; }
    }
}
