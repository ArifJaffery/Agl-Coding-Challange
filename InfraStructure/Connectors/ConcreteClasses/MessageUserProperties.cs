using System;
using System.Collections.Generic;
using System.Text;
using InfraStructure.Connectors.Models;

namespace InfraStructure.Connectors.ConcreteClasses
{
    /// <summary>
    /// Properties specific to describing message on pub/sub or other channels
    /// </summary>
    public class MessageUserProperties : BaseProperties
    {
        public DateTime EnqueuedTimeUtc { get; set; }

        /// <summary>
        /// Time (milliseconds) since the message was first enqueued on channel. This is calculated field, which is difference between CurrentUtc and EnqueuedTimeUtc
        /// </summary>
        public long TimeOnQueue { get; set; }

        public string MessageId { get; set; }
    }
}
