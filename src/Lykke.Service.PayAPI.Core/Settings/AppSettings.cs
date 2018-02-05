using Lykke.Service.PayAPI.Core.Settings.ServiceSettings;
using Lykke.Service.PayAPI.Core.Settings.SlackNotifications;

namespace Lykke.Service.PayAPI.Core.Settings
{
    public class AppSettings
    {
        public PayAPISettings PayAPI { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
        public MarketProfileServiceClientSettings MarketProfileServiceClient { get; set; }
        public AssetsServiceClientSettings AssetsServiceClient { get; set; }
        public PayAuthClientSettings PayAuthClient { get; set; }
    }

    public class MarketProfileServiceClientSettings
    {
        public string ServiceUrl { get; set; }
    }
    public class PayAuthClientSettings
    {
        public string ServiceUrl { get; set; }
    }
    public class AssetsServiceClientSettings
    {
        public string ServiceUrl { get; set; }
    }
}
