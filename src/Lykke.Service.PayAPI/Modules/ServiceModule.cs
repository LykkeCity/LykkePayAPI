using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.IataApi.Client;
using Lykke.Service.MarketProfile.Client;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayAPI.Core.Settings;
using Lykke.Service.PayAPI.Services;
using Lykke.Service.PayAuth.Client;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayCallback.Client;
using Lykke.Service.PayHistory.Client;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayPushNotifications.Client;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Lykke.Service.PayVolatility.Client;

namespace Lykke.Service.PayAPI.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;
        private readonly IServiceCollection _services;

        public ServiceModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_settings)
                .As<IReloadingManager<AppSettings>>()
                .SingleInstance();
            builder.RegisterInstance(_settings)
                .As<IReloadingManager<AppSettings>>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<PayAuthClient>()
                .As<IPayAuthClient>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayAuthServiceClient))
                .SingleInstance();

            builder.RegisterType<PayInvoiceClient>()
                .As<IPayInvoiceClient>()
                .WithParameter("settings",
                    new PayInvoiceServiceClientSettings()
                        {ServiceUrl = _settings.CurrentValue.PayInvoiceServiceClient.ServiceUrl})
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();

            builder.RegisterType<SignatureVerificationService>()
                .As<ISignatureVerificationService>();

            builder.RegisterType<LykkeMarketProfile>()
                .As<ILykkeMarketProfile>()
                .WithParameter("baseUri", new Uri(_settings.CurrentValue.MarketProfileServiceClient.ServiceUrl));

            builder.RegisterInstance<IAssetsService>(
                new AssetsService(new Uri(_settings.CurrentValue.AssetsServiceClient.ServiceUrl)));

            builder.RegisterType<PayInternalClient>()
                .As<IPayInternalClient>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayInternalServiceClient))
                .SingleInstance();

            builder.RegisterType<PayCallbackClient>()
                .As<IPayCallbackClient>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayCallbackServiceClient))
                .SingleInstance();

            builder.RegisterType<IataApiClient>()
                .As<IIataApiClient>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.IataApiServiceClient))
                .SingleInstance();

            builder.RegisterType<PaymentRequestService>()
                .As<IPaymentRequestService>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayAPI.PaymentRequestDueDate));

            builder.RegisterType<RatesService>()
                .As<IRatesService>()
                .SingleInstance();

            builder.RegisterType<HeadersHelper>()
                .As<IHeadersHelper>()
                .SingleInstance();

            builder.RegisterType<AuthService>()
                .As<IAuthService>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayAPI.JwtSecurity));

            builder.RegisterType<MerchantService>()
                .As<IMerchantService>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayAPI.Merchant))
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayAPI.CacheExpirationPeriods));

            builder.Register(x =>
            {
                var assetsService = x.Resolve<IComponentContext>().Resolve<IAssetsService>();

                return new CachedDataDictionary<string, AssetPair>(
                    async () => (await assetsService.AssetPairGetAllAsync()).ToDictionary(itm => itm.Id)
                );
            });
            builder.RegisterType<MerchantWalletsService>()
                .As<IMerchantWalletsService>();

            builder.RegisterType<IataService>()
                .As<IIataService>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayAPI.Iata))
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayAPI.CacheExpirationPeriods));

            builder.RegisterType<AssetSettingsService>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayAPI.Iata.CashoutAssets.Assets))
                .As<IAssetSettingsService>();

            RegisterHistory(builder);

            builder.RegisterPayPushNotificationsClient(_settings.CurrentValue.PayPushNotificationsServiceClient,
                n => n);

            builder.RegisterCachedPayVolatilityClient(_settings.CurrentValue.PayVolatilityServiceClient, null);

            builder.Populate(_services);
        }

        private void RegisterHistory(ContainerBuilder builder)
        {
            builder.RegisterPayHistoryClient(_settings.CurrentValue.PayHistoryServiceClient.ServiceUrl);

            builder.RegisterType<ExplorerUrlResolver>()
                .As<IExplorerUrlResolver>()
                .WithParameter("transactionUrl", _settings.CurrentValue.PayAPI.TransactionUrl)
                .SingleInstance();

            builder.RegisterType<EthereumCoreClient>()
                .As<IEthereumCoreClient>()
                .WithParameter("serviceUrl", _settings.CurrentValue.EthereumServiceClient.ServiceUrl)
                .SingleInstance();

            builder.RegisterType<PayHistoryService>()
                .As<IPayHistoryService>()
                .WithParameter("merchantDefaultLogoUrl", _settings.CurrentValue.PayAPI.Merchant.MerchantDefaultLogoUrl)
                .SingleInstance();

            builder.RegisterType<HistoryOperationTitleProvider>()
                .As<IHistoryOperationTitleProvider>()
                .SingleInstance();

            builder.Register(x =>
            {
                var assetsService = x.Resolve<IComponentContext>().Resolve<IAssetsService>();

                return new CachedDataDictionary<string, Asset>(
                    async () => (await assetsService.AssetGetAllAsync()).ToDictionary(itm => itm.Id)
                );
            });
        }
    }
}
