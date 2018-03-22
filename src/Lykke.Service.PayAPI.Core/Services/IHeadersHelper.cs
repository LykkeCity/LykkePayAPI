namespace Lykke.Service.PayAPI.Core.Services
{
    public interface IHeadersHelper
    {
        string MerchantId { get; }

        string MerchantSign { get; }
    }
}
