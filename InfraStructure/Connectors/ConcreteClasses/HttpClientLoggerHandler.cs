using InfraStructure.Connectors.Constants;
using InfraStructure.Connectors.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InfraStructure.Connectors.ConcreteClasses
{
    public class HttpClientLoggerHandler : DelegatingHandler
    {
        readonly ILogger<HttpClientLoggerHandler> _logger;
        readonly IPayloadLogger _payloadLogger;

        public HttpClientLoggerHandler(ILogger<HttpClientLoggerHandler> logger, IPayloadLogger payloadLogger)
        {
            _logger = logger;
            _payloadLogger = payloadLogger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.Method != HttpMethod.Get) // Dont log GET request bodies
                {
                    var correlationId = request.Headers.Contains(HttpHeaderConstants.ClientCorrelationIdHeaderName) ? request.Headers.GetValues(HttpHeaderConstants.ClientCorrelationIdHeaderName).FirstOrDefault() : nameof(HttpClientLoggerHandler);

                    var requestPayload = await request.Content.ReadAsStringAsync();
                    var blobUrl = await _payloadLogger.LogPayloadAsync(correlationId, requestPayload);
                    _logger?.LogInformation($"Logged Http Request Payload to: {blobUrl}");
                }
            }
            catch (Exception ex)
            {
                // Swallow this exception as it should not block the rest of the pipeline
                _logger?.LogError("Error logging payload {@exception}", ex);
            }

            var response = await base.SendAsync(request, cancellationToken);
            return response;
        }
    }
}
