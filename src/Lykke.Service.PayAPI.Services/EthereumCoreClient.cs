using Lykke.Service.EthereumCore.Client;
using Lykke.Service.EthereumCore.Client.Models;
using Lykke.Service.PayAPI.Core.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Service.PayAPI.Core.Services;

namespace Lykke.Service.PayAPI.Services
{
    public class EthereumCoreClient : IEthereumCoreClient, IDisposable
    {    
        private readonly IEthereumCoreAPI _service;

        public EthereumCoreClient(string serviceUrl)
        {
            _service = new EthereumCoreAPI(new Uri(serviceUrl));
        }

        public CurrentBlockModel GetBlock()
        {
            var result = _service.ApiBlockPost();
            return Convert<CurrentBlockModel>(result);
        }

        public async Task<CurrentBlockModel> GetBlockAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await _service.ApiBlockPostAsync(cancellationToken);
            return Convert<CurrentBlockModel>(result);
        }

        public TransactionResponse GetTransaction(string transactionHash)
        {
            var result = _service.ApiTransactionsTxHashByTransactionHashPost(transactionHash);
            return Convert<TransactionResponse>(result);
        }

        public async Task<TransactionResponse> GetTransactionAsync(string transactionHash,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var result =
                await _service.ApiTransactionsTxHashByTransactionHashPostAsync(transactionHash, cancellationToken);
            return Convert<TransactionResponse>(result);
        }

        public void Dispose()
        {
            _service?.Dispose();
        }

        private static T Convert<T>(object result) where T : class
        {
            if (result is ErrorResponse errorResponse)
            {
                throw new EthereumCoreApiException(errorResponse);
            }

            return result as T;
        }
    }
}
