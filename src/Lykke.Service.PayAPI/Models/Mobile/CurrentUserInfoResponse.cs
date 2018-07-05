using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayAPI.Models.Mobile
{
    public class CurrentUserInfoResponse
    {
        public string MerchantName { get; set; }
        public string MerchantLogoUrl { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool IsInternalSupervisor { get; set; }
    }
}
