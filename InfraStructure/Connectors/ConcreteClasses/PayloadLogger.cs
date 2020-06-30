using InfraStructure.Connectors.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace InfraStructure.Connectors.ConcreteClasses
{
    public class PayloadLogger : IPayloadLogger
    {
        const string NoCorrelationPrefix = "NO-CORRELATION";

        readonly bool _createHourlyFolders;
        readonly CloudBlobContainer _cloudBlobContainer;

        public PayloadLogger(string blobStorageConnectionString, string containerName, bool createHourlyFolders = false)
        {
            _createHourlyFolders = createHourlyFolders;

            var storageAccount = CloudStorageAccount.Parse(blobStorageConnectionString);
            var storageClient = storageAccount.CreateCloudBlobClient();

            _cloudBlobContainer = storageClient.GetContainerReference(containerName);
            if (!_cloudBlobContainer.Exists())
            {
                _cloudBlobContainer.Create();
            }
        }

        public async Task<string> LogPayloadAsync(string correlationId, string payload, string extension = "txt")
        {
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = $"{NoCorrelationPrefix}-{Guid.NewGuid().ToString()}";
            }

            using (var payloadStream = new MemoryStream(Encoding.UTF8.GetBytes(payload)))
            {
                return await UploadToBlob(correlationId, payloadStream, extension);
            }
        }

        async Task<string> UploadToBlob(string correlationId, Stream payloadStream, string extension)
        {
            var now = DateTime.UtcNow;
            var fileName = _createHourlyFolders
                ? $"{now.Year}{now.Month:D2}{now.Day:D2}{now.Hour:D2}\\{correlationId}-{now.Hour:D2}{now.Minute:D2}{now.Second:D2}{now.Millisecond:D4}.{extension}"
                : $"{now.Year}{now.Month:D2}{now.Day:D2}\\{correlationId}-{now.Hour:D2}{now.Minute:D2}{now.Second:D2}{now.Millisecond:D4}.{extension}";

            var blockBlob = _cloudBlobContainer.GetBlockBlobReference(fileName);

            await blockBlob.UploadFromStreamAsync(payloadStream);
            return blockBlob.Uri.ToString();
        }
    }
}
