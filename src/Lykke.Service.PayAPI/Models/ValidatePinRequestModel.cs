using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayAPI.Models
{
    /// <summary>
    /// Pin validation request details
    /// </summary>
    public class ValidatePinRequestModel
    {
        /// <summary>
        /// Gets or sets pin code or pin code hash
        /// </summary>
        [Required]
        public string PinCode { get; set; }
    }
}
