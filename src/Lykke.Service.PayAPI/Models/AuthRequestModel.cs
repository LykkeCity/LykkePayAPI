using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayAPI.Validation;

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
        [EmailAddressAndRowKey]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets password or password hash
        /// </summary>
        [Required]
        public string Password { get; set; }
    }
}
