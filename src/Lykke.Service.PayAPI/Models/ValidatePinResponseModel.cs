using System.Collections.Generic;

namespace Lykke.Service.PayAPI.Models
{
    public class ValidatePinResponseModel
    {
        public bool Passed { get; set; }

        /// <summary>
        ///Tags for push notifications.
        /// </summary>
        public Dictionary<string, string[]> NotificationIds { get; set; }
    }
}
