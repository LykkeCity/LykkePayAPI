using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Newtonsoft.Json;
using Common.Log;
using Lykke.Contracts.Security;
using Lykke.Service.PayAuth.Client;
using Lykke.Service.PayAuth.Client.Models;
using Lykke.SettingsReader;
using Lykke.Service.PayAPI.Core.Settings;

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
