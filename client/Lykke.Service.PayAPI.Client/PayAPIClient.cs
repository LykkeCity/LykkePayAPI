using System;
using Common.Log;

namespace Lykke.Service.PayAPI.Client
{
    public class PayAPIClient : IPayAPIClient, IDisposable
    {
        private readonly ILog _log;

        public PayAPIClient(string serviceUrl, ILog log)
        {
            _log = log;
        }

        public void Dispose()
        {
            //if (_service == null)
            //    return;
            //_service.Dispose();
            //_service = null;
        }
    }
}
