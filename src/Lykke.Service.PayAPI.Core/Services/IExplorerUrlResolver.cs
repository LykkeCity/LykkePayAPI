namespace Lykke.Service.PayAPI.Core.Services
{
    public interface IExplorerUrlResolver
    {
        string GetExplorerUrl(string transactionHash);
    }
}
