using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayAPI.Models
{
    public class UpdatePasswordRequestModel
    {
        [Required]
        public string OldPasssword { get; set; }

        [Required]
        public string NewPassword { get; set; }
    }
}
