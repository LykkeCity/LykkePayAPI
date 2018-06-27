using System.Threading;
using System.Threading.Tasks;
using Lykke.Service.EthereumCore.Client.Models;

namespace Lykke.Service.PayAPI.Core.Services
{
    public interface IEthereumCoreClient
    {
        CurrentBlockModel GetBlock();

        Task<CurrentBlockModel> GetBlockAsync(
            CancellationToken cancellationToken = default(CancellationToken));

        TransactionResponse GetTransaction(string transactionHash);

        Task<TransactionResponse> GetTransactionAsync(string transactionHash,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
