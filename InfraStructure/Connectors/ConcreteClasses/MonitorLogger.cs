using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using InfraStructure.Connectors.Extensions;

namespace InfraStructure.Connectors.ConcreteClasses
{
    /// <summary>
    /// The main class to log messages. It uses a MonitorLoggerSener or a BackgroundService to send messages via message logger providers
    /// </summary>
    public class MonitorLogger
    {
        protected readonly List<MonitorMessage> _offlineMessages = new List<MonitorMessage>();

        public MonitorLoggerBackgroundService BackgroundService { get; set; }

        readonly IHttpContextAccessor httpContextAccessor;
        readonly MonitorLoggerSender monitorLoggerSender;
        const string CONTEXT_ITEM_NAME = "MonitorLoggerMessageItems";

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="monitorLoggerSender"></param>
        public MonitorLogger(IHttpContextAccessor httpContextAccessor, MonitorLoggerSender monitorLoggerSender)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.monitorLoggerSender = monitorLoggerSender;
        }

        /// <summary>
        /// Extract message collection from HttpContext
        /// </summary>
        /// <returns></returns>
        protected List<MonitorMessage> GetContextMessages()
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return null;
            }
            if (String.IsNullOrWhiteSpace(httpContext.Request.Method))
            {
                return null;
            }
            var contextMessages = httpContext.GetFromContext<List<MonitorMessage>>(CONTEXT_ITEM_NAME);
            if (contextMessages == null)
            {
                contextMessages = new List<MonitorMessage>();
                httpContext.SaveToContext(CONTEXT_ITEM_NAME, contextMessages);
            }
            return contextMessages;
        }

        /// <summary>
        /// Write a message into message buffer
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public virtual async Task WriteMessage(MonitorMessage message)
        {
            if (message != null)
            {
                var contextMessages = GetContextMessages();
                if (contextMessages != null)
                {
                    contextMessages.Add(message);
                }
                else
                {
                    lock (_offlineMessages)
                    {
                        _offlineMessages.Add(message);
                    }
                }

            }
        }

        protected List<MonitorMessage> GetMessages(bool flushContext)
        {
            List<MonitorMessage> messagesCopy = new List<MonitorMessage>();

            // flush messages from http context 
            if (flushContext)
            {
                var contextMessages = GetContextMessages();
                if (contextMessages != null)
                {
                    messagesCopy.AddRange(contextMessages);
                    contextMessages.Clear();
                }
            }

            // flush local messages
            lock (_offlineMessages)
            {
                messagesCopy.AddRange(_offlineMessages);
                _offlineMessages.Clear();
            }

            // check if logging should be skipped
            var httpContext = this.httpContextAccessor.HttpContext;
            if (httpContext != null && httpContext.GetSkipMonitorLogging())
            {
                messagesCopy.Clear(); //skipping because is caled from HealthCheckController
            }

            return messagesCopy;
        }

        /// <summary>
        /// Sends all the messages from a buffer via a message sender (synchronous path) or a background service (asynchronous path)
        /// </summary>
        /// <param name="flushContext"></param>
        /// <returns></returns>
        public virtual async Task Flush(bool flushContext)
        {
            var messages = GetMessages(flushContext);

            if (messages.Count > 0)
            {
                if (this.BackgroundService != null)
                {
                    await BackgroundService.EnqueueMessages(messages);
                }
                else
                {
                    await this.monitorLoggerSender.ProcessMessages(messages);
                }
            }
        }
    }
}
