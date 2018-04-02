using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayAPI.Models
{
    public class CreatePaymentRequestModel
    {
        public string SettlementAsset { get; set; }

        public decimal Amount { get; set; }

        public string PaymentAsset { get; set; }

        public string CallbackURL { get; set; }

        public string OrderId { get; set; }

        public double Percent { get; set; }

        public int Pips { get; set; }

        public double FixedFee { get; set; }
    }
}
