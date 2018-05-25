using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayAPI.Models
{
    public class AuthRequestModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
