using Lykke.Service.PayAPI.Core.Settings.ServiceSettings;
using Lykke.Service.PayAPI.Core.Settings.SlackNotifications;
using Lykke.SettingsReader.Attributes;
using Lykke.Service.PayInternal.Client;

namespace Lykke.Service.PayAPI.Core.Settings
{
    public class AppSettings
    {
        public PayAPISettings PayAPI { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
        public MarketProfileServiceClientSettings MarketProfileServiceClient { get; set; }
        public AssetsServiceClientSettings AssetsServiceClient { get; set; }
        public PayAuthClientSettings PayAuthClient { get; set; }
        public PayInternalServiceClientSettings PayInternalServiceClient { get; set; }
    }

    public class MarketProfileServiceClientSettings
    {
        [HttpCheck("api/isalive")]
        public string ServiceUrl { get; set; }
    }
    public class PayAuthClientSettings
    {
        [HttpCheck("api/isalive")]
        public string ServiceUrl { get; set; }
    }
    public class AssetsServiceClientSettings
    {
        [HttpCheck("api/isalive")]
        public string ServiceUrl { get; set; }
    }
}
