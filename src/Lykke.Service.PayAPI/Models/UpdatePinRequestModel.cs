using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayAPI.Models
{
    public class UpdatePinRequestModel
    {
        [Required]
        public string NewPinCode { get; set; }
    }
}
