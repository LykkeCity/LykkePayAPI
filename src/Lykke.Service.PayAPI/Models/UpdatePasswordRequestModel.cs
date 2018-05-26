using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayAPI.Models
{
    public class UpdatePasswordRequestModel
    {
        [Required]
        public string CurrentPasssword { get; set; }

        [Required]
        public string NewPassword { get; set; }
    }
}
