using Common;
using System;
using Lykke.Service.PayAPI.Core.Services;

namespace Lykke.Service.PayAPI.Services
{
    public class ExplorerUrlResolver : IExplorerUrlResolver
    {
        private readonly string _transactionUrl;

        public ExplorerUrlResolver(string transactionUrl)
        {
            _transactionUrl = transactionUrl;
        }

        public string GetExplorerUrl(string transactionHash)
        {
            Uri uri = new Uri(new Uri(_transactionUrl.AddLastSymbolIfNotExists('/')), transactionHash);
            return uri?.ToString() ?? string.Empty;
        }
    }
}
