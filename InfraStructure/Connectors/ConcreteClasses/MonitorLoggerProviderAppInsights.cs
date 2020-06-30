using InfraStructure.Connectors.Extensions;
using InfraStructure.Connectors.Helpers;
using InfraStructure.Connectors.Interfaces;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
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
    /// Monitor message logger provider for Application Insights 
    /// </summary>
    public class MonitorLoggerProviderAppInsights : IMonitorLoggerProvider
    {
        public const string NAME = "AppInsights";

        public class MonitorLoggerProviderAppInsightsConfiguration
        {
            public string InstrumentationKey { get; set; }
        }

        TelemetryClient _client = null;
        readonly IConfiguration _configuration;
        readonly ILogger _logger;

        public MonitorLoggerProviderAppInsights(ILogger<MonitorLoggerProviderAppInsights> logger, IConfiguration configuration)
        {
            this._configuration = configuration;
            this._logger = logger;
        }

        /// <summary>
        /// Instance initialisation
        /// </summary>
        /// <returns></returns>
        public async Task Init()
        {
            try
            {
                var config = _configuration.GetTypedSection<MonitorLoggerProviderAppInsightsConfiguration>();

                var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
                telemetryConfiguration.InstrumentationKey = config.InstrumentationKey;
                var channel = telemetryConfiguration.TelemetryChannel as InMemoryChannel;
                if (channel != null)
                {
                    channel.DeveloperMode = true;
                }
                _client = new TelemetryClient(telemetryConfiguration);
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
                    await SendMessage(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send a message");
                }
            }
            _client.Flush();
        }

        private async Task SendMessage(MonitorMessage message)
        {

            if (message.MessageType == MonitorMessageTypes.HttpRequest || message.MessageType == MonitorMessageTypes.ServiceBusReceiver)
            {
                var uri = MonitorHelper.GetUri(message.RequestAddress);

                var telemetryItem = new RequestTelemetry();
                telemetryItem.Context.Operation.Id = message.Id;
                telemetryItem.Context.Operation.ParentId = message.CorrelationId;
                telemetryItem.Name = $"{message.RequestMethod}-{uri.Authority}";
                telemetryItem.Timestamp = message.StartTime;
                telemetryItem.Duration = message.Duration;
                telemetryItem.Success = !message.IsError;

                telemetryItem.ResponseCode = message.ResponseCode;
                telemetryItem.Url = uri;
                telemetryItem.Source = message.ProcessName;

                SetTelemetryProperties(telemetryItem.Properties, message);

                _client.TrackRequest(telemetryItem);
            }
            else if (message.MessageType == MonitorMessageTypes.HttpClient || message.MessageType == MonitorMessageTypes.ServiceBusSender || message.MessageType == MonitorMessageTypes.GenericObjectSender)
            {
                var uri = MonitorHelper.GetUri(message.RequestAddress);

                var telemetryItem = new DependencyTelemetry();
                telemetryItem.Context.Operation.Id = message.Id;
                telemetryItem.Context.Operation.ParentId = message.CorrelationId;
                telemetryItem.Name = $"{message.RequestMethod}-{uri.Authority}";
                telemetryItem.Timestamp = message.StartTime;
                telemetryItem.Duration = message.Duration;
                telemetryItem.Success = !message.IsError;

                telemetryItem.ResultCode = message.ResponseCode;
                telemetryItem.Data = message.RequestAddress;
                telemetryItem.Type = message.CallType;
                telemetryItem.Target = uri.Authority;

                SetTelemetryProperties(telemetryItem.Properties, message);

                _client.TrackDependency(telemetryItem);
            }
            else
            {
                var telemetryItem = new TraceTelemetry();
                telemetryItem.Context.Operation.Id = message.Id;
                telemetryItem.Context.Operation.ParentId = message.CorrelationId;

                telemetryItem.Message = $"{message.MessageType}-{message.RequestMethod}-{message.ProcessName}";
                telemetryItem.Timestamp = message.StartTime;
                telemetryItem.SeverityLevel = message.IsError ? SeverityLevel.Error : SeverityLevel.Information;

                SetTelemetryProperties(telemetryItem.Properties, message);

                _client.TrackTrace(telemetryItem);
            }

        }

        private void SetTelemetryProperties(IDictionary<string, string> properties, MonitorMessage message)
        {
            const int LIMIT = 8192;

            properties["CorrelationId"] = message.CorrelationId;
            properties["RequestHeaders"] = MonitorHelper.LimitString(JsonConvert.SerializeObject(message.RequestHeaders, Formatting.Indented), LIMIT);
            properties["ResponseHeaders"] = MonitorHelper.LimitString(JsonConvert.SerializeObject(message.ResponseHeaders, Formatting.Indented), LIMIT);
            properties["RequestArchitectureMetadata"] = $"{message.RequestArchitectureMetadata}";
            properties["ResponseArchitectureMetadata"] = $"{message.ResponseArchitectureMetadata}";
            properties["ProcessName"] = message.ProcessName;
            properties["RequestBody"] = MonitorHelper.LimitString(message.RequestBody, LIMIT);
            properties["RequestUser"] = MonitorHelper.LimitString(message.RequestUser, LIMIT);
            properties["ResponseBody"] = MonitorHelper.LimitString(message.ResponseBody, LIMIT);
            if (!String.IsNullOrEmpty(message.ErrorMessage))
            {
                properties["ErrorMessage"] = MonitorHelper.LimitString(message.ErrorMessage, LIMIT);
            }
        }
    }
}
