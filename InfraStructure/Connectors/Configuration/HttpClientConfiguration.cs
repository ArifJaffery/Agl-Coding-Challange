using System;

namespace InfraStructure.Connectors.Configuration
{
    public class HttpClientConfiguration
    {
        public string BaseAddress { get; set; }

        /// <summary>
        /// Number of time to retry for transient errors. Defaults to 3
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Time to wait between retries. Defaults to 600 msec
        /// </summary>
        public TimeSpan RetryInterval { get; set; } = TimeSpan.FromMilliseconds(600);
    }
}
