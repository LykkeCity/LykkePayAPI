using System.Collections.Generic;

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
        /// Gets or sets employee id
        /// </summary>
        public string EmployeeId { get; set; }

        /// <summary>
        /// Gets or sets merchant id
        /// </summary>
        public string MerchantId { get; set; }

        /// <summary>
        /// Gets or sets forcePasswordUpdate flag
        /// </summary>
        public bool ForcePasswordUpdate { get; set; }

        /// <summary>
        /// Gets or sets forcePinUpdateFlag
        /// </summary>
        public bool ForcePinUpdate { get; set; }

        //Tags for push notifications.
        public Dictionary<string, string[]> NotificationIds { get; set; }
    }
}
