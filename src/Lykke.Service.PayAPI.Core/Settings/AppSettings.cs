﻿using Lykke.Service.PayAPI.Core.Settings.ServiceSettings;
using Lykke.Service.PayAPI.Core.Settings.SlackNotifications;
using Lykke.Service.PayCallback.Client;
using Lykke.SettingsReader.Attributes;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayAuth.Client;

namespace Lykke.Service.PayAPI.Core.Settings
{
    public class AppSettings
    {
        public PayAPISettings PayAPI { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
        public MarketProfileServiceClientSettings MarketProfileServiceClient { get; set; }
        public AssetsServiceClientSettings AssetsServiceClient { get; set; }
        public PayAuthServiceClientSettings PayAuthServiceClient { get; set; }
        public PayInternalServiceClientSettings PayInternalServiceClient { get; set; }
        public PayCallbackServiceClientSettings PayCallbackServiceClient { get; set; }
    }

    public class MarketProfileServiceClientSettings
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
