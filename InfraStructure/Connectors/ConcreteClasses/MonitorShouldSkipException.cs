using System;
using System.Collections.Generic;
using System.Text;

namespace InfraStructure.Connectors.ConcreteClasses
{
    public class MonitorShouldSkipException : Exception
    {
        public string CorrelationId { get; private set; }

        public MonitorShouldSkipException(string correlationId)
        {
            CorrelationId = correlationId;
        }
    }
}
