using System;
using System.Linq;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common;
using Common.Log;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.MarketProfile.Client;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayAPI.Core.Settings;
using Lykke.Service.PayAPI.Core.Settings.ServiceSettings;
using Lykke.Service.PayAPI.Services;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;
using Lykke.Service.PayAuth.Client;

namespace Lykke.Service.PayAPI.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;
        private readonly ILog _log;
        // NOTE: you can remove it if you don't need to use IServiceCollection extensions to register service specific dependencies
        private readonly IServiceCollection _services;

        public ServiceModule(IReloadingManager<AppSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            // TODO: Do not register entire settings in container, pass necessary settings to services which requires them
            // ex:
            //  builder.RegisterType<QuotesPublisher>()
            //      .As<IQuotesPublisher>()
            //      .WithParameter(TypedParameter.From(_settings.CurrentValue.QuotesPublication))

            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<PayAuthClient>()
                .As<IPayAuthClient>()
                .WithParameter("settings", new PayAuthServiceClientSettings() { ServiceUrl = _settings.CurrentValue.PayAuthClient.ServiceUrl })
                .SingleInstance();
            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();

            builder.RegisterType<LykkeMarketProfile>()
                .As<ILykkeMarketProfile>()
                .WithParameter("baseUri", new Uri(_settings.CurrentValue.MarketProfileServiceClient.ServiceUrl));

            builder.RegisterInstance<IAssetsService>(
                new AssetsService(new Uri(_settings.CurrentValue.AssetsServiceClient.ServiceUrl)));

            builder.Register(x =>
            {
                var assetsService = x.Resolve<IComponentContext>().Resolve<IAssetsService>();

                return new CachedDataDictionary<string, AssetPair>(
                    async () => (await assetsService.AssetPairGetAllAsync()).ToDictionary(itm => itm.Id)
                );
            });

            builder.Populate(_services);
        }
    }
}
