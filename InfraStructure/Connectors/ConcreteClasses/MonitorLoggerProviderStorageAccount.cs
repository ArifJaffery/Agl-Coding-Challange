using InfraStructure.Connectors.Extensions;
using InfraStructure.Connectors.Interfaces;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace InfraStructure.Connectors.ConcreteClasses
{
    /// <summary>
    /// Monitor message logger provider for Storage Account
    /// </summary>
    public class MonitorLoggerProviderStorageAccount : IMonitorLoggerProvider
    {
        public const string NAME = "StorageAccount";

        public class MonitorLoggerStorageAccountConfiguration
        {
            public string ConnectionString { get; set; }
            public string ContainerName { get; set; } = "monitorlogger";
        }


        readonly ILogger _logger;
        readonly IConfiguration _configuration;

        private CloudBlobContainer cloudBlobContainer;

        public MonitorLoggerProviderStorageAccount(ILogger<MonitorLoggerProviderStorageAccount> logger, IConfiguration configuration)
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
                var config = _configuration.GetTypedSection<MonitorLoggerStorageAccountConfiguration>();

                var storageAccount = CloudStorageAccount.Parse(config.ConnectionString);
                var storageClient = storageAccount.CreateCloudBlobClient();
                this.cloudBlobContainer = storageClient.GetContainerReference(config.ContainerName);
                if (!this.cloudBlobContainer.Exists())
                {
                    this.cloudBlobContainer.Create();
                }
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
            var now = DateTime.UtcNow;

            foreach (var message in messages)
            {
                try
                {
                    await SaveMessage(message, now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send a message");
                }
            }
        }

        private async Task SaveMessage(MonitorMessage message, DateTime now)
        {
            var folderName = $"{message.CorrelationId}";
            var fileName = $"{message.StartTime.ToString("yyyy-MM-dd-HH.mm.ss.ffff")}-{message.MessageType}-{message.RequestMethod}-{message.ProcessName}.json";
            var fullFileName = folderName + "/" + fileName;

            var blockBlob = this.cloudBlobContainer.GetBlockBlobReference(fullFileName);

            var payload = Newtonsoft.Json.JsonConvert.SerializeObject(message, Newtonsoft.Json.Formatting.Indented);

            await blockBlob.UploadTextAsync(payload);
        }

    }
}
