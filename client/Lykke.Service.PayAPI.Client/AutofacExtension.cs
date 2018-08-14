using System;
using Autofac;
using Common.Log;

namespace Lykke.Service.PayAPI.Client
{
    public static class AutofacExtension
    {
        public static void RegisterPayAPIClient(this ContainerBuilder builder, string serviceUrl)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (serviceUrl == null) throw new ArgumentNullException(nameof(serviceUrl));
            if (string.IsNullOrWhiteSpace(serviceUrl))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(serviceUrl));

            builder.RegisterType<PayAPIClient>()
                .WithParameter("serviceUrl", serviceUrl)
                .As<IPayAPIClient>()
                .SingleInstance();
        }

        public static void RegisterPayAPIClient(this ContainerBuilder builder, PayAPIServiceClientSettings settings)
        {
            builder.RegisterPayAPIClient(settings?.ServiceUrl);
        }
    }
}
