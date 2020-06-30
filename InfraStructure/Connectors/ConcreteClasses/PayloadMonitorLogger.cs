using InfraStructure.Connectors.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace InfraStructure.Connectors.ConcreteClasses
{
    public class PayloadMonitorLogger : IPayloadLogger
    {
        private MonitorLogger _monitorLogger;

        public PayloadMonitorLogger(MonitorLogger monitorLogger)
        {
            _monitorLogger = monitorLogger;
        }

        public async Task<string> LogPayloadAsync(string correlationId, string payload, string extension = "txt")
        {
            var monitorMessage = new MonitorMessage()
            {
                CorrelationId = correlationId,
                RequestBody = payload,
                RequestAddress = "payload logger",
                MessageType = MonitorMessageTypes.GenericObjectSender,
                RequestMethod = "PAYLOADLOGGER"
            };

            await _monitorLogger.WriteMessage(monitorMessage);

            return "";
        }
    }
}