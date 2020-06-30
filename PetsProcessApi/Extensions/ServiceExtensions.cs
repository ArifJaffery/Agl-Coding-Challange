using InfraStructure.Connectors.ConcreteClasses;
using InfraStructure.Connectors.Configuration;
using InfraStructure.Connectors.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Polly;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.ApplicationInsights.Extensibility;
using PetsProcessApi.Orchestrator;

namespace PetsProcessApi.Extensions
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// Get Configuration object from a service collection
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <returns></returns>
        public static IConfiguration GetConfiguration(this IServiceCollection serviceCollection)
        {
            var sp = serviceCollection.BuildServiceProvider();
            var configuration = sp.GetRequiredService<IConfiguration>();
            return configuration;
        }

        /// <summary>
        /// Adds IOption configuration from a configuration section object into a service collection and returns this object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceCollection"></param>
        /// <param name="configSection"></param>
        /// <returns></returns>
        public static IOptions<T> AddOptionsAndRetrieve<T>(this IServiceCollection serviceCollection, IConfigurationSection configSection) where T : class, new()
        {
            if (configSection == null)
            {
                throw new Exception($"'{typeof(T).Name}' configuration section is not found");
            }
            serviceCollection.Configure<T>(configSection);
            var sb = serviceCollection.BuildServiceProvider();
            var options = sb.GetService<IOptions<T>>();
            return options;
        }

        /// <summary>
        /// Adds IOption configuration from a configuration section object into a service collection and returns this object
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <typeparam name="TType"></typeparam>
        /// <param name="serviceCollection"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IOptions<TInterface> AddOptionsAndRetrieve<TInterface, TType>(this IServiceCollection serviceCollection, IConfiguration configuration) where TInterface : class, new() where TType : class
        {
            var configSection = configuration.GetSection(typeof(TType).Name);
            return serviceCollection.AddOptionsAndRetrieve<TInterface>(configSection);
        }

        /// <summary>
        /// Adds IOption configuration from a configuration section object into a service collection and returns this object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceCollection"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IOptions<T> AddOptionsAndRetrieve<T>(this IServiceCollection serviceCollection, IConfiguration configuration) where T : class, new()
        {
            return serviceCollection.AddOptionsAndRetrieve<T, T>(configuration);
        }

        public static IServiceCollection AddOrchestrations(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped(typeof(IPetProcessOrchestrator), typeof(PetProcessOrchestrator));
            return serviceCollection;
        }
        public static IServiceCollection AddConfiguredHttpClient<TClientInterface, TClientImplementation, TClientOption>(this IServiceCollection serviceCollection)
            where TClientInterface : class
            where TClientImplementation : class, TClientInterface
            where TClientOption : HttpClientConfiguration, new()
        {
            var configuration = serviceCollection.GetConfiguration();

            var opts = serviceCollection.AddOptionsAndRetrieve<TClientOption>(configuration);

            var maxRetries = opts?.Value?.MaxRetries ?? 3;
            var retryInterval = opts?.Value?.RetryInterval ?? TimeSpan.FromMilliseconds(600);

            var clientBuilder = serviceCollection.AddHttpClient<TClientInterface, TClientImplementation>()
                .AddHttpMessageHandler<HttpClientMonitor>()
                .AddTransientHttpErrorPolicy(p =>
                    p.WaitAndRetryAsync(maxRetries, _ => retryInterval));

            var handler = serviceCollection.BuildServiceProvider().GetService<HttpClientLoggerHandler>();
            if (handler != null)
            {
                clientBuilder.AddHttpMessageHandler<HttpClientLoggerHandler>();
            }

            serviceCollection.TryAddTransient<HttpClientMonitor>();

            return serviceCollection;
        }

        /// <summary>
        /// To init the service connector and the retry policy
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="configuration"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        public static IServiceCollection AddConnectors(this IServiceCollection serviceCollection,
            IConfiguration configuration, IHostingEnvironment env)
        {

            serviceCollection
                .AddConfiguredHttpClient<IPetProcessApiClient, PetProcessApiClient, PetProcessApi>();

            serviceCollection.AddSingleton(typeof(IPetProcessApiConnector), typeof(PetProcessApiConnector));

            return serviceCollection;
        }

        /// <summary>
        /// Adds a default configuration for an outbound call
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public static IServiceCollection AddDefaultOutboundClientCredentialsServices(this IServiceCollection serviceCollection, string sectionName)
        {
            var configuration = serviceCollection.GetConfiguration();

            string aadInstance = configuration[$"{sectionName}:AADInstance"];
            string tenant = configuration[$"{sectionName}:Tenant"];
            string clientId = configuration[$"{sectionName}:ClientId"];
            string appKey = configuration[$"{sectionName}:ApiCodeKey"];
            if (appKey == null)
            {
                appKey = configuration[$"{sectionName}:ClientSecret"];
            }
            string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);

            AuthenticationContext authContext = new AuthenticationContext(authority);
            ClientCredential clientCredential = new ClientCredential(clientId, appKey);

            serviceCollection.AddSingleton<AuthenticationContext>(authContext);
            serviceCollection.AddSingleton<ClientCredential>(clientCredential);

            return serviceCollection;
        }

        /// <summary>
        /// Get HostingEnfironment object from a service collection
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <returns></returns>
        public static IHostingEnvironment GetHostingEnvironment(this IServiceCollection serviceCollection)
        {
            var sp = serviceCollection.BuildServiceProvider();
            var hostingEnvironment = sp.GetService<IHostingEnvironment>();
            return hostingEnvironment;
        }

        /// <summary>
        /// Adds an applicatin insights custom telemetry processor
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <returns></returns>
        public static IServiceCollection AddAppInsightsCustomTelemetryProcessor(this IServiceCollection serviceCollection)
        {
            var configDescriptor = serviceCollection.SingleOrDefault(tc => tc.ServiceType == typeof(TelemetryConfiguration));
            if (configDescriptor?.ImplementationFactory != null)
            {
                var implFactory = configDescriptor.ImplementationFactory;

                serviceCollection.Remove(configDescriptor);
                serviceCollection.AddSingleton(provider =>
                {
                    if (!(implFactory.Invoke(provider) is TelemetryConfiguration config))
                        return null;

                    config.TelemetryProcessorChainBuilder.Use(next => new CustomTelemetryProcessor(next, provider.GetRequiredService<IHttpContextAccessor>()));
                    config.TelemetryProcessorChainBuilder.Build();

                    return config;
                });
            }

            return serviceCollection;
        }

        /// <summary>
        /// Adds IOption configuration from a configuration section object into a service collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceCollection"></param>
        /// <param name="configSection"></param>
        /// <returns></returns>
        public static IServiceCollection AddOptions<T>(this IServiceCollection serviceCollection, IConfigurationSection configSection) where T : class
        {
            if (configSection == null)
            {
                throw new Exception($"'{typeof(T).Name}' configuration section is not found");
            }
            serviceCollection.Configure<T>(configSection);
            return serviceCollection;
        }

        /// <summary>
        /// Adds IOption configuration from a configuration section object into a service collection
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <typeparam name="TType"></typeparam>
        /// <param name="serviceCollection"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddOptions<TInterface, TType>(this IServiceCollection serviceCollection, IConfiguration configuration) where TInterface : class where TType : class
        {
            var configSection = configuration.GetSection(typeof(TType).Name);
            return serviceCollection.AddOptions<TInterface>(configSection);
        }

        /// <summary>
        /// Adds IOption configuration from a configuration section object into a service collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceCollection"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddOptions<T>(this IServiceCollection serviceCollection, IConfiguration configuration) where T : class
        {
            return serviceCollection.AddOptions<T, T>(configuration);
        }

        /// <summary>
        /// Adds common services used in transaction visibility and telemetry
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <returns></returns>
        public static IServiceCollection AddCommonTelemetry(this IServiceCollection serviceCollection)
        {
            var configuration = serviceCollection.GetConfiguration();
            var hostingEnvironment = serviceCollection.GetHostingEnvironment();

            serviceCollection.AddApplicationInsightsTelemetryProcessor<CustomTelemetryProcessor>();
            serviceCollection.AddOptions<ErrorHandlingConfiguration>(configuration);

            // find key ending with "LogStorage:PayloadContainerName"
            string loggingContainerKey = null;
            foreach (var pair in configuration.AsEnumerable())
            {
                if (pair.Key != null && pair.Key.ToLower().EndsWith("LogStorage:PayloadContainerName".ToLower()))
                {
                    loggingContainerKey = pair.Key;
                    break;
                }
            }

            // configure PayloadLogger
            // if loggingContainerKey exists then PayloadLogger configuration exists for this api project
            // if we are in a Development environment then do not instanciate PayloadLogger because
            //   it is most probably running from a unit test and PayloadLogger constructor tries to connect to an Azure Storage Account 
            //   which might not be available at the time of testing.
            if (loggingContainerKey != null && hostingEnvironment.IsDevelopment() == false)
            {
                var sectionName = loggingContainerKey.Split(':')[0];

                var loggingConnectionString = configuration[$"{sectionName}:ConnectionString"];
                var loggingContainerName = configuration[$"{sectionName}:PayloadContainerName"];

                var payloadLogger = new PayloadLogger(loggingConnectionString, loggingContainerName, true);
                serviceCollection.AddSingleton<IPayloadLogger>(payloadLogger);

                serviceCollection.AddTransient<HttpClientLoggerHandler>();
            }
            else
            {
                // if for some reason PayloadLogger is not injected into services then use PayloadMonitorLogger class
                //   this will protect the code in case of some object expects IPayloadLogger injected.
                serviceCollection.AddTransient<IPayloadLogger, PayloadMonitorLogger>();
            }

            serviceCollection.AddMonitorLogging(configuration, true);
            return serviceCollection;
        }

        /// <summary>
        /// Gets a typed config object from a configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static T GetTypedSection<T>(this IConfiguration configuration) where T : class, new()
        {
            return configuration.GetTypedSection<T>(null);
        }

        /// <summary>
        /// Adds a monitor logging service
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="configuration"></param>
        /// <param name="useAsyncService"></param>
        /// <returns></returns>
        public static IServiceCollection AddMonitorLogging(this IServiceCollection serviceCollection, IConfiguration configuration, bool useAsyncService)
        {
            var monitorLoggerConfig = configuration.GetTypedSection<MonitorLoggerConfiguration>();
            var providers = monitorLoggerConfig.Providers;
            const string allProviders = "all";

            if (String.IsNullOrWhiteSpace(providers))
            {
                providers = allProviders;
            }
            providers = providers.ToLower();

            if (providers == allProviders || providers.Contains(MonitorLoggerProviderServiceBus.NAME.ToLower()))
            {
                serviceCollection.AddSingleton(typeof(IMonitorLoggerProvider), typeof(MonitorLoggerProviderServiceBus));
            }
            if (providers == allProviders || providers.Contains(MonitorLoggerProviderStorageAccount.NAME.ToLower()))
            {
                serviceCollection.AddSingleton(typeof(IMonitorLoggerProvider), typeof(MonitorLoggerProviderStorageAccount));
            }
            if (providers == allProviders || providers.Contains(MonitorLoggerProviderAppInsights.NAME.ToLower()))
            {
                serviceCollection.AddSingleton(typeof(IMonitorLoggerProvider), typeof(MonitorLoggerProviderAppInsights));
            }


            var sp = serviceCollection.BuildServiceProvider();

            var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
            var monitorLoggerSender = new MonitorLoggerSender(sp);
            serviceCollection.AddSingleton<MonitorLoggerSender>(monitorLoggerSender);

            var monitorLogger = new MonitorLogger(httpContextAccessor, monitorLoggerSender);
            serviceCollection.TryAddSingleton<MonitorLogger>(monitorLogger);

            if (!monitorLoggerConfig.IsSynchronous && useAsyncService)
            {
                serviceCollection.AddHostedService<MonitorLoggerBackgroundService>();
            }
            return serviceCollection;
        }
        /// <summary>
        /// Gets a typed config object from a configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configuration"></param>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public static T GetTypedSection<T>(this IConfiguration configuration, string sectionName) where T : class, new()
        {
            if (sectionName == null)
            {
                sectionName = typeof(T).Name;
            }
            var options = new T();
            configuration.GetSection(sectionName).Bind(options);
            return options;
        }
    }
}
