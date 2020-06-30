using InfraStructure.Connectors.ConcreteClasses;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfraStructure.Connectors.Models
{
    /// <summary>
    /// Properties that are common for messages, events, log entries, etc.
    /// </summary>
    public class BaseProperties : ArchitectureMetadata
    {
        /// <summary>
        /// Value for header "correlationId". Source: HTTP Request Header
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Entity Identifier for the HTTP request (if present). Source: HTTP Request Path
        /// </summary>
        public string EntityId { get; set; }

        /// <summary>
        /// HTTP method and request uri.
        /// E.g. PUT /api/order/123
        /// </summary>
        public string TrackedEventType { get; set; } = string.Empty;

        /// <summary>
        /// UTC timestamp generated at the time of exception getting logged
        /// </summary>
        public string Timestamp { get; set; } = string.Empty;

        /// <summary>
        /// Name of the user/identity. Source: Primary claims principal identity
        /// </summary>
        public string User { get; set; } = string.Empty;

        /// <summary>
        /// Request payload. Source: HTTP request
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public string Payload { get; set; } = string.Empty;

        /// <summary>
        /// Client app making the HTTP request
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// Message for the log event
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The number of times the request was retried.
        /// </summary>
        public int RetryCount { get; set; }
    }
}
