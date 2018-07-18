using System;
using Common.Log;
using Lykke.Common.Log;

namespace Lykke.Service.PayAPI.Client
{
    public class PayAPIClient : IPayAPIClient, IDisposable
    {
        private readonly ILog _log;

        public PayAPIClient(string serviceUrl, ILogFactory logFactory)
        {
            _log = logFactory.CreateLog(this);
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
