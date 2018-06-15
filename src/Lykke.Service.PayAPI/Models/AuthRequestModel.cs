using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayAPI.Models
{
    /// <summary>
    /// Authorization request details
    /// </summary>
    public class AuthRequestModel
    {
        /// <summary>
        /// Gets or sets email
        /// </summary>
        [Required]
        [Validation.EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets password or password hash
        /// </summary>
        [Required]
        public string Password { get; set; }
    }
}
