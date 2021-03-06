﻿using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using Common.Log;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Common.Log;
using Lykke.Logs;
using Lykke.Service.PayAPI.Core;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayAPI.Core.Settings;
using Lykke.Service.PayAPI.Infrastructure.Authentication;
using Lykke.Service.PayAPI.Models;
using Lykke.Service.PayAPI.Modules;
using Lykke.Service.PayAPI.SwaggerFilters;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.PayAPI
{
    public class Startup
    {
        public IHostingEnvironment Environment { get; }
        public IContainer ApplicationContainer { get; private set; }
        public IConfigurationRoot Configuration { get; }
        private ILog _log;
        private IHealthNotifier _healthNotifier;

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
                        options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
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
                                Title = "PayAPI",
                                Description =
@"**Lykke PayAPI** is the set of API methods aimed to provide customers the out-of-the-box functionality for their 
clients to make payments in BTC, ETH and other assets depending on customer needs.

#### Allowed HTTPs requests:
- **POST** : To create resource 
- **PUT** : To update resource
- **GET** : To get a resource or list of resources
- **DELETE** : To delete resource

#### Description Of Usual Server Responses:
- 200 `OK` - the request was successful (some API calls may return 201 instead).
- 201 `Created` - the request was successful and a resource was created.
- 204 `No Content` - the request was successful but there is no representation to return (i.e. the response is empty).
- 400 `Bad Request` - the request could not be understood or was missing required parameters.
- 401 `Unauthorized` - authentication failed or user doesn't have permissions for requested operation.
- 403 `Forbidden` - access denied.
- 404 `Not Found` - resource was not found."
                            });

                        options.DescribeAllEnumsAsStrings();
                        options.EnableXmsEnumExtension();

                        #region EnableXmlDocumentation
                        var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                        var entryAssembly = Assembly.GetEntryAssembly();

                        //Set the comments path for the swagger json and ui.
                        var xmlPath = Path.Combine(basePath, $"{entryAssembly.GetName().Name}.xml");

                        if (File.Exists(xmlPath))
                        {
                            options.IncludeXmlComments(xmlPath);
                        }
                        #endregion

                        options.MakeResponseValueTypesRequired();
                        // this filter produced null exception: options.OperationFilter<FormFileUploadOperationFilter>();
                    }

                    options.OperationFilter<SwaggerDefaultValues>();
                    options.OperationFilter<HeaderAccessOperationFilter>();
                    options.OperationFilter<SwaggerExtensionsFilter>();
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

                services.AddLykkeLogging
                (
                    appSettings.ConnectionString(x => x.PayAPI.Db.LogsConnString),
                    "PayAPILog",
                    appSettings.CurrentValue.SlackNotifications.AzureQueue.ConnectionString,
                    appSettings.CurrentValue.SlackNotifications.AzureQueue.QueueName
                );

                builder.RegisterModule(new ServiceModule(appSettings));
                builder.Populate(services);
                ApplicationContainer = builder.Build();

                _log = ApplicationContainer.Resolve<ILogFactory>().CreateLog(this);
                _healthNotifier = ApplicationContainer.Resolve<IHealthNotifier>();

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
                _log?.Critical(ex);
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

                app.UseLykkeMiddleware(ex => new { Message = "Technical problem" });

                app.UseCors(builder =>
                {
                    builder.AllowAnyOrigin();
                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();
                });

                app.UseAuthentication();

                app.UseMvc();

                app.UseSwagger(c =>
                {
                    c.PreSerializeFilters.Add((swagger, httpReq) =>
                    {
                        swagger.Host = httpReq.Host.Value;
                        swagger.Schemes = new List<string> { "https", "http" };
                    });
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
                _log?.Critical(ex);
                throw;
            }
        }

        private async Task StartApplication()
        {
            try
            {
                // NOTE: Service not yet recieve and process requests here

                await ApplicationContainer.Resolve<IStartupManager>().StartAsync();

                _healthNotifier?.Notify("Started");
            }
            catch (Exception ex)
            {
                _log?.Critical(ex);
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
                _log?.Critical(ex);
                throw;
            }
        }

        private Task CleanUp()
        {
            try
            {
                // NOTE: Service can't recieve and process requests here, so you can destroy all resources

                _healthNotifier?.Notify("Terminating");

                ApplicationContainer.Dispose();

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                if (_log != null)
                {
                    _log?.Critical(ex);
                    (_log as IDisposable)?.Dispose();
                }
                throw;
            }
        }
    }
}
