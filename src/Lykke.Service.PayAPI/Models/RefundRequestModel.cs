using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayAPI.Models
{
    public class RefundRequestModel
    {
        [Required]
        public string PaymentRequestId { get; set; }

        public string Address { get; set; }
    }
}
