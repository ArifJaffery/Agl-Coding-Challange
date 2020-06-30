using InfraStructure.Connectors.Helpers;
using InfraStructure.Connectors.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfraStructure.Connectors.ConcreteClasses
{
    /// <summary>
    /// Monitor message type enumeration
    /// </summary>
    public enum MonitorMessageTypes
    {
        Unknown = 0,
        HttpRequest = 1,
        HttpClient = 2,
        ServiceBusSender = 3,
        ServiceBusReceiver = 4,
        GenericObjectSender = 5,
    }

    /// <summary>
    /// Monitor message class
    /// </summary>
    public class MonitorMessage
    {
        /// <summary>
        /// Message id. This property is made lower case to make it compatible with CosmosDb
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        /// <summary>
        /// Correlation Id. It is used to group all the messages for the same transaction
        /// </summary>
        public string CorrelationId { get; set; }
        /// <summary>
        /// Start time
        /// </summary>
        public DateTimeOffset StartTime { get; set; } = DateTimeOffset.UtcNow;
        /// <summary>
        /// End time
        /// </summary>
        public DateTimeOffset EndTime { get; set; } = DateTimeOffset.UtcNow;
        /// <summary>
        /// A calculated difference between start and end time
        /// </summary>
        public TimeSpan Duration { get; set; }
        /// <summary>
        /// A type of this message
        /// </summary>
        public MonitorMessageTypes MessageType { get; set; }
        /// <summary>
        /// A call time. It is used to distinguish messages if they have the same message type
        /// </summary>
        public string CallType { get; set; }
        /// <summary>
        /// Name of the process that logged this message
        /// </summary>
        public string ProcessName { get; set; }
        /// <summary>
        /// Request address. Usually this is an http request address
        /// </summary>
        public string RequestAddress { get; set; }
        /// <summary>
        /// Request method
        /// </summary>
        public string RequestMethod { get; set; }
        /// <summary>
        /// A user name. Usually it is extracted from an http request auhtorization header
        /// </summary>
        public string RequestUser { get; set; }
        /// <summary>
        /// Response code
        /// </summary>
        public string ResponseCode { get; set; }

        /// <summary>
        /// Request headers
        /// </summary>
        public Dictionary<string, string> RequestHeaders { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// Architecture metadata attached to a caller
        /// </summary>
        public ArchitectureMetadata RequestArchitectureMetadata { get; set; }
        /// <summary>
        /// Request budy
        /// </summary>
        public string RequestBody { get; set; }

        /// <summary>
        /// Response headers
        /// </summary>
        public Dictionary<string, string> ResponseHeaders { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// Architecture metadata attached to a requested web method
        /// </summary>
        public ArchitectureMetadata ResponseArchitectureMetadata { get; set; }
        /// <summary>
        /// Response body
        /// </summary>
        public string ResponseBody { get; set; }

        /// <summary>
        /// Identifies if this message contains errors
        /// </summary>
        public bool IsError { get; set; }
        /// <summary>
        /// Error message
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Start time converted to a unix format
        /// </summary>
        public long UnixStartTime
        {
            get
            {
                return StartTime.ToUnixTimeSeconds();
            }
        }
        /// <summary>
        /// End time converted to a unix format
        /// </summary>
        public long UnixEndTime
        {
            get
            {
                return EndTime.ToUnixTimeSeconds();
            }
        }

        /// <summary>
        /// class constructor
        /// </summary>
        public MonitorMessage()
        {
            ProcessName = MonitorMessageHelper.GetCurrentProcessName();
        }

        /// <summary>
        ///  to string implementation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{MessageType}: ({CorrelationId}) {RequestMethod} {RequestAddress}";
        }
    }
}
