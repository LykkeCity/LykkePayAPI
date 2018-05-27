using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayAPI.Models
{
    /// <summary>
    /// Update password request details
    /// </summary>
    public class UpdatePasswordRequestModel
    {
        /// <summary>
        /// Gets or sets current password or password hash
        /// </summary>
        [Required]
        public string CurrentPasssword { get; set; }

        /// <summary>
        /// Gets or sets new password hash
        /// </summary>
        [Required]
        public string NewPasswordHash { get; set; }
    }
}
