using System;
using System.Collections.Generic;
using System.Text;
using InfraStructure.Connectors.Constants;
using InfraStructure.Connectors.Extensions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.AspNetCore.Http;

namespace InfraStructure.Connectors.ConcreteClasses
{
    // ReSharper disable once UnusedMember.Global
    public class CustomTelemetryProcessor : ITelemetryProcessor
    {
        readonly ITelemetryProcessor _next;
        readonly IHttpContextAccessor _httpContextAccessor;

        public CustomTelemetryProcessor(ITelemetryProcessor next, IHttpContextAccessor httpContextAccessor)
        {
            _next = next;
            _httpContextAccessor = httpContextAccessor;
        }

        public void Process(ITelemetry item)
        {
            if (item is OperationTelemetry operationTelemetry)
            {
                // get correlationId. If the id has not already been loaded into HttpContext (by HttpContextEnricherMiddleware),
                // get it directly from the request header
                var correlationIdValue = _httpContextAccessor?.HttpContext?.GetClientCorrelationId() ?? GetCorrelationIdHeaderValue(_httpContextAccessor?.HttpContext?.Request);

                UpsertTelemetryProperty(operationTelemetry, HttpConstants.ClientCorrelationIdKeyName, correlationIdValue);

                // get the user ID
                UpsertTelemetryProperty(operationTelemetry, HttpConstants.UsernameIdName, _httpContextAccessor?.HttpContext?.GetUser());

                // load properties from action metadata (if it has been loaded into HttpContext by HttpContextEnricherMiddleware)                
                var actionArchitectureMetadata = _httpContextAccessor?.HttpContext?.GetActionArchitectureMetadata();

                if (actionArchitectureMetadata != null)
                {
                    UpsertTelemetryProperty(operationTelemetry, nameof(ArchitectureMetadata.Component), actionArchitectureMetadata.Component);
                    UpsertTelemetryProperty(operationTelemetry, nameof(ArchitectureMetadata.Service), actionArchitectureMetadata.Service);
                    UpsertTelemetryProperty(operationTelemetry, nameof(ArchitectureMetadata.ServiceInterface), actionArchitectureMetadata.ServiceInterface);
                    UpsertTelemetryProperty(operationTelemetry, nameof(ArchitectureMetadata.OrgUnit), actionArchitectureMetadata.OrgUnit);
                    UpsertTelemetryProperty(operationTelemetry, nameof(ArchitectureMetadata.Domain), actionArchitectureMetadata.Domain);
                    UpsertTelemetryProperty(operationTelemetry, nameof(ArchitectureMetadata.Target), actionArchitectureMetadata.Target);
                    UpsertTelemetryProperty(operationTelemetry, nameof(ArchitectureMetadata.EntityType), actionArchitectureMetadata.EntityType);
                }
            }

            // Send the item to the next TelemetryProcessor
            _next.Process(item);
        }

        static void UpsertTelemetryProperty(OperationTelemetry operationTelemetry, string propertyName, string propertyValue)
        {
            if (operationTelemetry.Properties.ContainsKey(propertyName))
            {
                operationTelemetry.Properties[propertyName] = propertyValue;
            }
            else
            {
                operationTelemetry.Properties.Add(propertyName, propertyValue);
            }
        }

        /// <summary>
        /// Gets correlationId value from HTTP request header, if present. Null otherwise.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        static string GetCorrelationIdHeaderValue(HttpRequest request)
        {
            if (request == null)
                return null;

            if (request.Headers.TryGetValue(HttpHeaderConstants.ClientCorrelationIdHeaderName, out var headerValues))
            {
                return headerValues;
            }

            return null;
        }
    }
}
