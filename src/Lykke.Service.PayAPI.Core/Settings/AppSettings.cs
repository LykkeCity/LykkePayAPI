﻿using Lykke.Service.PayAPI.Core.Settings.ServiceSettings;
using Lykke.Service.PayAPI.Core.Settings.SlackNotifications;
using Lykke.Service.PayCallback.Client;
using Lykke.Service.PayAuth.Client;
using Lykke.Service.PayInvoice.Client;
using Lykke.SettingsReader.Attributes;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.IataApi.Client;
using Lykke.Service.PayHistory.Client;
using Lykke.Service.EthereumCore.Client;
using Lykke.Service.PayMerchant.Client;
using Lykke.Service.PayPushNotifications.Client;
using Lykke.Service.PayVolatility.Client;

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
        public PayInvoiceServiceClientSettings PayInvoiceServiceClient { get; set; }
        [Optional]
        public IataApiServiceClientSettings IataApiServiceClient { get; set; }
        public PayHistoryServiceClientSettings PayHistoryServiceClient { get; set; }
        public EthereumServiceClientSettings EthereumServiceClient { get; set; }
        [Optional]
        public PayPushNotificationsServiceClientSettings PayPushNotificationsServiceClient { get; set; }
        public PayVolatilityServiceClientSettings PayVolatilityServiceClient { get; set; }
        public PayMerchantServiceClientSettings PayMerchantServiceClient { get; set; }
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
