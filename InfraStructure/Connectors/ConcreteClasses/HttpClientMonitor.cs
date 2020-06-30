using InfraStructure.Connectors.Constants;
using InfraStructure.Connectors.Helpers;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace InfraStructure.Connectors.ConcreteClasses
{
    public class HttpClientMonitor : DelegatingHandler
    {
        readonly MonitorLogger _monitorLogger;
        readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// A constructor
        /// </summary>
        /// <param name="monitorLogger"></param>
        /// <param name="httpContextAccessor"></param>
        public HttpClientMonitor(MonitorLogger monitorLogger, IHttpContextAccessor httpContextAccessor)
        {
            this._monitorLogger = monitorLogger;
            this._httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// SendAsync method implementation
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            // set required headers
            AddHeaderValue(request, HttpHeaderConstants.ClientCorrelationIdHeaderName, $"{MonitorMessageHelper.GetCurrentCorrelationId(httpContext)}");
            AddHeaderValue(request, HttpConstants.ArchitectureMetadataName, $"{MonitorMessageHelper.GetCurrentArchitectureMetadata(httpContext)}");

            // log request
            var monitorMessage = await MonitorMessageHelper.CreateForHttpClient(httpContext, request);

            if (!String.IsNullOrWhiteSpace(monitorMessage.CorrelationId))
            {
                await _monitorLogger.WriteMessage(monitorMessage);
            }


            try
            {
                var response = await InnerSendAsync(request, cancellationToken);

                await MonitorMessageHelper.UpdateForHttpClient(monitorMessage, response);

                return response;
            }
            catch (Exception ex)
            {
                await MonitorMessageHelper.UpdateForHttpClient(monitorMessage, null, ex);

                throw;
            }
            finally
            {
                // if this call is done from async process then flush monitor logger messages
                if (httpContext == null || String.IsNullOrWhiteSpace(httpContext.Request.Method))
                {
                    await _monitorLogger.Flush(false);
                }
            }
        }

        protected virtual async Task<HttpResponseMessage> InnerSendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        private void AddHeaderValue(HttpRequestMessage request, string name, string value)
        {
            if (!request.Headers.Contains(name))
            {
                request.Headers.Add(name, value);
            }
        }


    }
}
