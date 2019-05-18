using Confifu.Abstractions;
using Confifu.Abstractions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using ServiceStack.Confifu;
using ServiceStack.WebHost.Endpoints;
using System;

namespace ServiceStack.LocalRequest.Confifu
{
    public static class AppConfigExts
    {
        public class Config
        {
            readonly IAppConfig appConfig;

            public bool LogRequests { get; set; }

            internal Config(IAppConfig appConfig)
            {
                this.appConfig = appConfig;
            }

            internal void InitDefaults()
            {
                var vars = appConfig.GetConfigVariables().WithPrefix("ServiceStack:LocalRequest:");
                LogRequests = ParseBool(vars["LogRequests"], false);

                appConfig
                    .UseServiceStack(c =>
                    {
                        c.LocalAppHost();
                    })
                    .RegisterServices(sc =>
                    {
                        sc.Replace(ServiceDescriptor.Transient<SimpleRequestExecutor>(sp => 
                            new SimpleRequestExecutor(
                                EndpointHost.AppHost as LocalServiceStackHost, 
                                LogRequests, 
                                sp.GetService<ILogger>())
                                )
                            );

                        sc.Replace(ServiceDescriptor.Transient<LocalClientFactory, LocalClientFactory>());
                    })
                    .AddAppRunnerAfter(() => {
                        if(LogRequests)
                        {
                            var logger = appConfig.GetServiceProvider().GetService<ILogger>();
                            logger.LogDebug("Request logging is turned on!");
                        }
                    });
            }

            bool ParseBool(string str, bool defaultValue)
            {
                if (str == null)
                    return defaultValue;
                bool res;
                if (bool.TryParse(str, out res))
                    return res;
                return defaultValue;
            }
        }

        public static void LocalAppHost(this ServiceStackConfig config)
        {
            config.AppHost = () => new LocalServiceStackHost(config.ServiceHostName, config.ServiceHostAssemblies.ToArray());
        }

        public static IAppConfig UseServiceStackLocalRequest(this IAppConfig appConfig, 
            Action<Config> configurator = null)
        {
            var config = appConfig.EnsureConfig("ServiceStack:LocalRequest", () => new Config(appConfig), c =>
            {
                c.InitDefaults();
            });
            configurator?.Invoke(config);
            return appConfig;
        }
    }
}
