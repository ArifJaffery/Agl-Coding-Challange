using InfraStructure.Connectors.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace InfraStructure.Connectors.ConcreteClasses
{
    /// <summary>
    /// Monitor message sender
    /// </summary>
    public class MonitorLoggerSender
    {

        private IEnumerable<IMonitorLoggerProvider> _providers;
        private ILogger<MonitorLoggerSender> _logger;
        private readonly IServiceProvider _services;

        /// <summary>
        /// class constructor
        /// </summary>
        /// <param name="services"></param>
        public MonitorLoggerSender(IServiceProvider services)
        {
            this._services = services;
        }

        /// <summary>
        /// Sends all the messages via monitor message logger providers
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public async Task ProcessMessages(List<MonitorMessage> messages)
        {
            try
            {
                if (_logger == null)
                {
                    _logger = _services.GetService<ILogger<MonitorLoggerSender>>();
                }

                if (_providers == null)
                {
                    _providers = _services.GetServices<IMonitorLoggerProvider>();

                    foreach (var provider in _providers)
                    {
                        if (provider != null)
                        {
                            await provider.Init();
                        }
                    }
                }

                foreach (var provider in _providers)
                {
                    string providerName = "Unknown";
                    try
                    {
                        if (provider != null)
                        {
                            providerName = provider.GetType().Name;
                            await provider.Send(messages);
                        }
                    }
                    catch (Exception ex)
                    {
                        this._logger.LogError(ex, $"Error sending messages to provider {providerName}. {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error occurred executing {nameof(ProcessMessages)}. {ex.Message}");
            }
        }

    }
}
