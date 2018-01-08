using System.Threading.Tasks;

namespace Lykke.Service.PayAPI.Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}