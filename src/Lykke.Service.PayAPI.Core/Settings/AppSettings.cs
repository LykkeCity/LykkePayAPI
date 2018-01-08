using Lykke.Service.PayAPI.Core.Settings.ServiceSettings;
using Lykke.Service.PayAPI.Core.Settings.SlackNotifications;

namespace Lykke.Service.PayAPI.Core.Settings
{
    public class AppSettings
    {
        public PayAPISettings PayAPI { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }
}
