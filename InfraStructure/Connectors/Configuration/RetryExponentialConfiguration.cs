using System;
using System.Collections.Generic;
using System.Text;

namespace InfraStructure.Connectors.Configuration
{
    public class RetryExponentialConfiguration
    {
        /// <summary>
        /// The minimum duration between consecutive retries
        /// </summary>
        public TimeSpan MinimumBackoff { get; set; }

        /// <summary>
        /// The maximum duration between consecutive retries
        /// </summary>
        public TimeSpan MaximumBackoff { get; set; }

        /// <summary>
        /// The maximum amount of time the system will retry the operation
        /// </summary>
        public int MaximumRetryCount { get; set; }
    }
}
