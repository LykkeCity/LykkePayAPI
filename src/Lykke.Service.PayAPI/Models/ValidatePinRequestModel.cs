using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayAPI.Models
{
    public class ValidatePinRequestModel
    {
        [Required]
        public string PinCode { get; set; }
    }
}
