using Microsoft.AspNetCore.Mvc;
using Common.Log;
using Lykke.Service.PayAPI.Core;

namespace Lykke.Service.PayAPI.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ILog Log;

        protected string MerchantId => HttpContext.Request.GetMerchantId();

        public BaseController(ILog log)
        {
            Log = log;
        }
    }
}
