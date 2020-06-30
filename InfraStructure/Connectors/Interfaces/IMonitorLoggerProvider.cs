using InfraStructure.Connectors.ConcreteClasses;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace InfraStructure.Connectors.Interfaces
{
    /// <summary>
    /// A common interface for all the monitor logger providers
    /// </summary>
    public interface IMonitorLoggerProvider
    {
        /// <summary>
        /// Provider initialisation
        /// </summary>
        /// <returns></returns>
        Task Init();

        /// <summary>
        /// Sending of the message
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        Task Send(IEnumerable<MonitorMessage> messages);
    }
}
