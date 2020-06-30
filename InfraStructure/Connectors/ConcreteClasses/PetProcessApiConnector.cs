using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using System.Threading;
using InfraStructure.Connectors.AbstractClasses;
using InfraStructure.Connectors.Interfaces;
using InfraStructure.Connectors.Models;

namespace InfraStructure.Connectors.ConcreteClasses
{
    public class PetProcessApiConnector : FipApiConnector, IPetProcessApiConnector
    {
        protected override string DependencyName => "Pet Process API";

        readonly IPetProcessApiClient _petProcessApiClient;

        public PetProcessApiConnector(IPetProcessApiClient processApiClient, TelemetryClient telemetryClient) : base(telemetryClient)
        {
            _petProcessApiClient = processApiClient;
        }
        public async Task IsAlive()
        {
            var response =
                await CallApi(() => _petProcessApiClient.Client.GetAsync("isAlive"))
                    .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new CustomException(
                    CommonErrors.CommonErrorHeaderMessage.ExternalDependentServiceFailed,
                    CommonErrors.ErrorHeaderCode.ServiceUnavailable.ToString(), new[]
                    {
                        new ErrorDetailModel
                        {
                            Message = CommonErrors.CommonErrorDetailsMessage.FailedToConnectToDependentService,
                            Code = CommonErrors.ErrorDetailCode.ExternalServiceFailure.ToString(),
                            Target = DependencyName
                        }
                    });
            }
        }
        private async Task<T> CallGetAsync<T>(CancellationToken cancellationToken, string operationName, Dictionary<string, string> logData,
            string uri)
        {
            T result;
            TelemetryClient.TrackTrace($"Calling {operationName}", logData);
            using (var operation = TelemetryClient.StartOperation<DependencyTelemetry>(operationName))
            {
                operation.Telemetry.Data = uri;
                operation.Telemetry.Success = false;
                var response =
                    await CallApi(() => _petProcessApiClient.GetAsync(uri, cancellationToken))
                        .ConfigureAwait(false);
                var responseText = response.Content != null ? await response.Content.ReadAsStringAsync() : string.Empty;
                var output = logData.ToDictionary(item => item.Key, item => item.Value);
                output.Add(nameof(response.StatusCode), response.StatusCode.ToString());
                output.Add(nameof(response.Content), responseText);
                TelemetryClient.TrackTrace($"Received response from {operationName}", output);
                result = ParseText<T>(responseText);
                operation.Telemetry.Success = true;
            }
            return result;
        }
        public async Task<T> GetAllPersonPets<T>(CancellationToken cancellationToken)
        {
            var operationName = $"{DependencyName} - {nameof(GetAllPersonPets)}";
            var uri = string.Empty;
            var logData = new Dictionary<string, string>()
            {
                { nameof(T), "GetAll" },
            };
            return await CallGetAsync<T>(cancellationToken, operationName, logData, uri);
        }
    }
}
