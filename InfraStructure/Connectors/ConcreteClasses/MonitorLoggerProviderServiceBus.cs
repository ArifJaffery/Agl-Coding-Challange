using InfraStructure.Connectors.Configuration;
using InfraStructure.Connectors.Extensions;
using InfraStructure.Connectors.Interfaces;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace InfraStructure.Connectors.ConcreteClasses
{
    /// <summary>
    /// Monitor message logger provider for Service Bus
    /// </summary>
    public class MonitorLoggerProviderServiceBus : IMonitorLoggerProvider
    {
        public const string NAME = "ServiceBus";

        public class MonitorLoggerProviderServiceBusConfiguration
        {
            public string ConnectionString { get; set; }

            public string EntityPath { get; set; }

            /// <summary>
            /// Retry (exponential) behaviour for service bus client.
            /// Defaults to MaximumRetryCount = 3, MinimumBackoff = 5 seconds, MaximumBackoff = 30 seconds
            /// </summary>
            public RetryExponentialConfiguration Retry { get; set; } = new RetryExponentialConfiguration
            {
                MaximumRetryCount = 3,
                MinimumBackoff = new TimeSpan(hours: 0, minutes: 0, seconds: 5),
                MaximumBackoff = new TimeSpan(hours: 0, minutes: 0, seconds: 30)
            };

            public TimeSpan SenderTimeout { get; set; } = new TimeSpan(hours: 0, minutes: 0, seconds: 30);
        }


        readonly ILogger _logger;
        readonly IConfiguration _configuration;

        private MessageSender _messageSender;

        public MonitorLoggerProviderServiceBus(ILogger<MonitorLoggerProviderServiceBus> logger, IConfiguration configuration)
        {
            this._logger = logger;
            this._configuration = configuration;

        }


        /// <summary>
        /// Instance initialisation
        /// </summary>
        /// <returns></returns>
        public async Task Init()
        {

            try
            {
                var config = _configuration.GetTypedSection<MonitorLoggerProviderServiceBusConfiguration>();

                var cnnBuilder = new ServiceBusConnectionStringBuilder(config.ConnectionString);
                if (cnnBuilder.EntityPath == null)
                {
                    cnnBuilder.EntityPath = config.EntityPath;
                }

                _messageSender = new MessageSender(
                    cnnBuilder
                    , retryPolicy: config.Retry == null ? null
                    : new RetryExponential(
                        config.Retry.MinimumBackoff,
                        config.Retry.MaximumBackoff,
                        config.Retry.MaximumRetryCount));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialise");
            }
        }

        /// <summary>
        /// Message send method
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public async Task Send(IEnumerable<MonitorMessage> messages)
        {

            foreach (var message in messages)
            {
                try
                {
                    await SaveMessage(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send a message");
                }
            }
        }

        private async Task SaveMessage(MonitorMessage monitorMessage)
        {
            var serializedBody = JsonConvert.SerializeObject(monitorMessage);
            var message = new Message(Encoding.UTF8.GetBytes(serializedBody))
            {
                ContentType = "application/json"
            };

            await _messageSender.SendAsync(message);
        }

    }
}
