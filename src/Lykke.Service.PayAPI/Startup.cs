using System;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Logs;
using Lykke.Service.PayAPI.Core;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayAPI.Core.Settings;
using Lykke.Service.PayAPI.Infrastructure.Authentication;
using Lykke.Service.PayAPI.Models;
using Lykke.Service.PayAPI.Modules;
using Lykke.SettingsReader;
using Lykke.SlackNotification.AzureQueue;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;

namespace Lykke.Service.PayAPI
{
    public class Startup
    {
        public IHostingEnvironment Environment { get; }
        public IContainer ApplicationContainer { get; private set; }
        public IConfigurationRoot Configuration { get; }
        public ILog Log { get; private set; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            Environment = env;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            try
            {
                var appSettings = Configuration.LoadSettings<AppSettings>();

                services.AddMvcCore().AddVersionedApiExplorer(opt =>
                {
                    opt.GroupNameFormat = "'v'VVV";
                    opt.SubstituteApiVersionInUrl = true;
                });

                services.AddMvc()
                    .AddJsonOptions(options =>
                    {
                        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    });

                services.AddApiVersioning(opt =>
                {
                    opt.ReportApiVersions = true;
                    opt.AssumeDefaultVersionWhenUnspecified = true;
                    opt.DefaultApiVersion = new ApiVersion(1, 0);
                });

                services.AddSwaggerGen(options =>
                {
                    var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        options.SwaggerDoc(
                            $"{description.GroupName}",
                            new Info
                            {
                                Version = description.GroupName,
                                Title = "PayAPI"
                            });

                        options.DescribeAllEnumsAsStrings();
                        options.EnableXmsEnumExtension();
                        options.EnableXmlDocumentation();
                        options.MakeResponseValueTypesRequired();
                        // this filter produced null exception: options.OperationFilter<FormFileUploadOperationFilter>();
                    }

                    options.OperationFilter<SwaggerDefaultValues>();
                    options.OperationFilter<HeaderAccessOperationFilter>();
                });

                services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = LykkePayConstants.AuthenticationScheme;
                        options.DefaultChallengeScheme = LykkePayConstants.AuthenticationScheme;
                    })
                    .AddScheme<LykkePayAuthOptions, LykkePayAuthHandler>(LykkePayConstants.AuthenticationScheme,
                        LykkePayConstants.AuthenticationScheme, options => { })
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer = appSettings.CurrentValue.PayAPI.JwtSecurity.Issuer,
                            ValidateAudience = true,
                            ValidAudience = appSettings.CurrentValue.PayAPI.JwtSecurity.Audience,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(appSettings.CurrentValue.PayAPI.JwtSecurity.Key))
                        };
                    });

                var builder = new ContainerBuilder();

                Log = CreateLogWithSlack(services, appSettings);

                builder.RegisterModule(new ServiceModule(appSettings, Log));
                builder.Populate(services);
                ApplicationContainer = builder.Build();

                Mapper.Initialize(cfg =>
                {
                    cfg.AddProfiles(typeof(AutoMapperProfile));
                    cfg.AddProfiles(typeof(Services.AutoMapperProfile));
                });

                Mapper.AssertConfigurationIsValid();

                return new AutofacServiceProvider(ApplicationContainer);
            }
            catch (Exception ex)
            {
                Log?.WriteFatalErrorAsync(nameof(Startup), nameof(ConfigureServices), "", ex).GetAwaiter().GetResult();
                throw;
            }
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime, IApiVersionDescriptionProvider provider)
        {
            try
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }

                app.UseLykkeMiddleware("PayAPI", ex => new { Message = "Technical problem" });

                app.UseAuthentication();

                app.UseMvc();

                app.UseSwagger(c =>
                {
                    c.PreSerializeFilters.Add((swagger, httpReq) => swagger.Host = httpReq.Host.Value);
                });

                app.UseSwaggerUI(
                    options =>
                    {
                        foreach (var description in provider.ApiVersionDescriptions)
                        {
                            options.RoutePrefix = "swagger/ui";
                            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                                description.GroupName.ToUpperInvariant());
                        }
                    });

                app.UseStaticFiles();

                appLifetime.ApplicationStarted.Register(() => StartApplication().GetAwaiter().GetResult());
                appLifetime.ApplicationStopping.Register(() => StopApplication().GetAwaiter().GetResult());
                appLifetime.ApplicationStopped.Register(() => CleanUp().GetAwaiter().GetResult());
            }
            catch (Exception ex)
            {
                Log?.WriteFatalErrorAsync(nameof(Startup), nameof(Configure), "", ex).GetAwaiter().GetResult();
                throw;
            }
        }

        private async Task StartApplication()
        {
            try
            {
                // NOTE: Service not yet recieve and process requests here

                await ApplicationContainer.Resolve<IStartupManager>().StartAsync();

                await Log.WriteMonitorAsync("", $"Env: {Program.EnvInfo}", "Started");
            }
            catch (Exception ex)
            {
                await Log.WriteFatalErrorAsync(nameof(Startup), nameof(StartApplication), "", ex);
                throw;
            }
        }

        private async Task StopApplication()
        {
            try
            {
                // NOTE: Service still can recieve and process requests here, so take care about it if you add logic here.

                await ApplicationContainer.Resolve<IShutdownManager>().StopAsync();
            }
            catch (Exception ex)
            {
                if (Log != null)
                {
                    await Log.WriteFatalErrorAsync(nameof(Startup), nameof(StopApplication), "", ex);
                }
                throw;
            }
        }

        private async Task CleanUp()
        {
            try
            {
                // NOTE: Service can't recieve and process requests here, so you can destroy all resources

                if (Log != null)
                {
                    await Log.WriteMonitorAsync("", $"Env: {Program.EnvInfo}", "Terminating");
                }

                ApplicationContainer.Dispose();
            }
            catch (Exception ex)
            {
                if (Log != null)
                {
                    await Log.WriteFatalErrorAsync(nameof(Startup), nameof(CleanUp), "", ex);
                    (Log as IDisposable)?.Dispose();
                }
                throw;
            }
        }

        private static ILog CreateLogWithSlack(IServiceCollection services, IReloadingManager<AppSettings> settings)
        {
            var consoleLogger = new LogToConsole();
            var aggregateLogger = new AggregateLogger();

            aggregateLogger.AddLog(consoleLogger);

            var dbLogConnectionStringManager = settings.Nested(x => x.PayAPI.Db.LogsConnString);
            var dbLogConnectionString = dbLogConnectionStringManager.CurrentValue;

            if (string.IsNullOrEmpty(dbLogConnectionString))
            {
                consoleLogger.WriteWarningAsync(nameof(Startup), nameof(CreateLogWithSlack), "Table loggger is not inited").Wait();
                return aggregateLogger;
            }

            if (dbLogConnectionString.StartsWith("${") && dbLogConnectionString.EndsWith("}"))
                throw new InvalidOperationException($"LogsConnString {dbLogConnectionString} is not filled in settings");

            var persistenceManager = new LykkeLogToAzureStoragePersistenceManager(
                AzureTableStorage<LogEntity>.Create(dbLogConnectionStringManager, "PayAPILog", consoleLogger),
                consoleLogger);

            // Creating slack notification service, which logs own azure queue processing messages to aggregate log
            var slackService = services.UseSlackNotificationsSenderViaAzureQueue(new AzureQueueIntegration.AzureQueueSettings
            {
                ConnectionString = settings.CurrentValue.SlackNotifications.AzureQueue.ConnectionString,
                QueueName = settings.CurrentValue.SlackNotifications.AzureQueue.QueueName
            }, aggregateLogger);

            var slackNotificationsManager = new LykkeLogToAzureSlackNotificationsManager(slackService, consoleLogger);

            // Creating azure storage logger, which logs own messages to concole log
            var azureStorageLogger = new LykkeLogToAzureStorage(
                persistenceManager,
                slackNotificationsManager,
                consoleLogger);

            azureStorageLogger.Start();

            aggregateLogger.AddLog(azureStorageLogger);

            return aggregateLogger;
        }
    }
}
