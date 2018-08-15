using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayAPI.Models
{
    public class CreatePaymentRequestModel
    {
        [Required]
        public string SettlementAsset { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string PaymentAsset { get; set; }

        public string CallbackURL { get; set; }

        public string OrderId { get; set; }

        public double Percent { get; set; }

        public int Pips { get; set; }

        public double FixedFee { get; set; }
    }
}
