using System;
using System.Collections.Generic;
using System.Text;

namespace InfraStructure.Connectors.Configuration
{
    /// <summary>
    /// A configuratin class for MonitorLogger
    /// </summary>
    public class MonitorLoggerConfiguration
    {
        /// <summary>
        /// Identifies if messages are sent synchronously
        /// </summary>
        public bool IsSynchronous { get; set; } = false;

        /// <summary>
        /// Number of seconds before messages are sent by a MonitorLoggerBackgroundService
        /// </summary>
        public int DelayMilliseconds { get; set; } = 1000;

        /// <summary>
        /// A semicolon separated list of providers configured for monitor message logging
        /// </summary>
        public string Providers { get; set; }
    }
}
