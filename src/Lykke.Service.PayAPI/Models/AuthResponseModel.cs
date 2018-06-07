namespace Lykke.Service.PayAPI.Models
{
    /// <summary>
    /// Authentication details
    /// </summary>
    public class AuthResponseModel
    {
        /// <summary>
        /// Gets or sets authentication token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets forcePasswordUpdate flag
        /// </summary>
        public bool ForcePasswordUpdate { get; set; }

        /// <summary>
        /// Gets or sets forcePinUpdateFlag
        /// </summary>
        public bool ForcePinUpdate { get; set; }
    }
}
