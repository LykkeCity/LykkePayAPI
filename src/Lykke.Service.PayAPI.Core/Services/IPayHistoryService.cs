using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayAPI.Core.Domain.PayHistory;

namespace Lykke.Service.PayAPI.Core.Services
{
    public interface IPayHistoryService
    {
        Task<IReadOnlyList<HistoryOperationView>> GetHistoryAsync(string merchantId);
        Task<IReadOnlyList<HistoryOperationView>> GetHistoryByInvoiceAsync(string invoiceId);
        Task<HistoryOperation> GetDetailsAsync(string merchantId, string id);
    }
}
