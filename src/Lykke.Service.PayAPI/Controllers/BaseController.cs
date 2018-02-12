using Microsoft.AspNetCore.Mvc;
using Common.Log;
using Lykke.Service.PayAPI.Core;

namespace Lykke.Service.PayAPI.Controllers
{
    public class BaseController : Controller
    {
        protected string MerchantId => HttpContext.Request.GetMerchantId();

        protected readonly ILog _log;

        public BaseController(ILog log)
        {
            _log = log;
        }
    }
}
