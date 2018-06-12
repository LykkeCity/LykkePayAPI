using System.Linq;
using System.Security.Claims;
using Lykke.Service.PayAPI.Core.Domain.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayAPI
{
    public static class ControllerExtensions
    {
        public static string GetUserEmail(this Controller controller)
        {
            return controller?.User?.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
        }

        public static string GetUserEmployeeId(this Controller controller)
        {
            return controller?.User?.Claims.SingleOrDefault(x => x.Type == PrivateClaimTypes.EmployeeId)?.Value;
        }

        public static string GetUserMerchantId(this Controller controller)
        {
            return controller?.User?.Claims.SingleOrDefault(x => x.Type == PrivateClaimTypes.MerchantId)?.Value;
        }
    }
}
