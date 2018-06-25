using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lykke.Service.PayAPI.Core.Domain.Invoice;

namespace Lykke.Service.PayAPI.Core.Services
{
    public interface IIataService
    {
        Task<IReadOnlyDictionary<string, string>> GetIataBillingCategoriesAsync();
        IReadOnlyDictionary<string, string> GetIataAssets();
        Task<InvoiceIataSpecificData> GetIataSpecificDataAsync(string invoiceId);
    }
}
