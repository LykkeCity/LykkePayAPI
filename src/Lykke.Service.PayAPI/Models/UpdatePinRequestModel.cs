using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayAPI.Models
{
    /// <summary>
    /// Pin code update details
    /// </summary>
    public class UpdatePinRequestModel
    {
        /// <summary>
        /// Gets or sets pin code hash
        /// </summary>
        [Required]
        public string NewPinCodeHash { get; set; }
    }
}
