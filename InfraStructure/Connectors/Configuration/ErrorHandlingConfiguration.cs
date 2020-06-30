using System;
using System.Collections.Generic;
using System.Text;

namespace InfraStructure.Connectors.Configuration
{
    public class ErrorHandlingConfiguration
    {
        /// <summary>
        /// Indicates if the detailed error details are to be included in the output of the application
        /// </summary>
        public bool? IncludeDetailedOutput { get; set; } = false;

        /// <summary>
        /// Telemetry property values that exceed this length are truncated to this length
        /// </summary>
        public int MaxPropertyValueLength { get; set; } = 256;

        /// <summary>
        /// Indicates whether the verbose exception stack trace should be output or not.
        /// Should only be set to true for particular scenarios such as developer testing
        /// or very unusual errors.
        /// </summary>
        public bool Verbose { get; set; } = false;
    }
}