using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayAPI
{
    public static class ControllerExtensions
    {
        public static string GetUserEmail(this Controller controller)
        {
            return controller?.User?.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
        }
    }
}
