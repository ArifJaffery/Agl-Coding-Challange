using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using InfraStructure.Connectors.Configuration;

namespace InfraStructure.Connectors.ConcreteClasses
{
    /// <summary>
    /// A background service used to send monitor messages asynchronously
    /// </summary>
    public class MonitorLoggerBackgroundService : BackgroundService
    {

        private ILogger<MonitorLoggerBackgroundService> _logger;
        private readonly ConcurrentQueue<List<MonitorMessage>> _queue = new ConcurrentQueue<List<MonitorMessage>>();
        private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);
        private readonly MonitorLoggerConfiguration _config;
        private readonly MonitorLoggerSender _monitorLoggerSender;
        private readonly IServiceProvider _services;

        /// <summary>
        /// class constructor
        /// </summary>
        /// <param name="services"></param>
        /// <param name="monitorLoggerSender"></param>
        /// <param name="monitorLogger"></param>
        /// <param name="configuration"></param>
        public MonitorLoggerBackgroundService(IServiceProvider services, MonitorLoggerSender monitorLoggerSender, MonitorLogger monitorLogger, IConfiguration configuration)
        {
            _services = services;
            var  sectionName = typeof(MonitorLoggerConfiguration).Name;
            var options = new MonitorLoggerConfiguration();
            configuration.GetSection(sectionName).Bind(options);
            _config = options;

           _monitorLoggerSender = monitorLoggerSender;
            monitorLogger.BackgroundService = this;
        }

        /// <summary>
        /// Starts service handler
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            if (_logger == null)
            {
                _logger = _services.GetService<ILogger<MonitorLoggerBackgroundService>>();
            }
            return base.StartAsync(cancellationToken);
        }

        /// <summary>
        /// Stop service handler
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }

        /// <summary>
        /// Execure service handler
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (false == stoppingToken.IsCancellationRequested)
                {

                    if (_queue.TryDequeue(out var messages))
                    {
                        await _monitorLoggerSender.ProcessMessages(messages);
                    }
                    else
                    {
                        //await _signal.WaitAsync(cancellationToken);
                        await Task.Delay(_config.DelayMilliseconds, stoppingToken);
                    }

                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error occurred executing {nameof(ExecuteAsync)}. {ex.Message}");
            }
        }

        /// <summary>
        /// Enqueues monitor messages for further sending
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public async Task EnqueueMessages(List<MonitorMessage> messages)
        {
            try
            {
                for (int i = 0; i < messages.Count; i++)
                {
                    System.Diagnostics.Debug.WriteLine($"*** {i}: {messages[i]}");
                }

                _queue.Enqueue(messages);
                _signal.Release();
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error occurred executing {nameof(EnqueueMessages)}. {ex.Message}");
            }
        }

        /// <summary>
        /// Class dispose handler
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
