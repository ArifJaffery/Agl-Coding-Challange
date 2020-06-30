using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using InfraStructure.Connectors.Models;
using Microsoft.ApplicationInsights;

namespace InfraStructure.Connectors.AbstractClasses
{
    public abstract class FipApiConnector
    {

        protected abstract string DependencyName { get; }

        protected TelemetryClient TelemetryClient { get; }

        protected FipApiConnector(TelemetryClient telemetryClient)
        {
            TelemetryClient = telemetryClient;
        }

        /// <summary>
        /// Calls an API using the supplied call function
        /// </summary>
        /// <param name="callFunction"></param>
        /// <returns></returns>
        /// <exception cref="CustomException">thrown when the call cannot complete</exception>
        protected async Task<HttpResponseMessage> CallApi(Func<Task<HttpResponseMessage>> callFunction)
        {
            var response = await callFunction().ConfigureAwait(false);
            TelemetryClient.TrackTrace($"API Connector: Response received.",
                new Dictionary<string, string>
                {
                    { nameof(DependencyName), DependencyName },
                    { "RequestUri", response.RequestMessage?.RequestUri?.ToString() },
                    { nameof(response.StatusCode), response.StatusCode.ToString() },
                    { nameof(response.Content), response.Content?.ReadAsStringAsync().Result },
                });
            if (response.IsSuccessStatusCode) return response;

            var exception = await CreateExceptionFromResponse(response);
            throw exception;
        }

        public CustomException FailToCallToExternalServiceException
        {
            get
            {
                return new CustomException(
                    CommonErrors.CommonErrorHeaderMessage.ExternalDependentServiceFailed,
                    CommonErrors.ErrorHeaderCode.ServiceUnavailable.ToString(), new[]
                    {
                        new ErrorDetailModel
                        {
                            Message = CommonErrors.CommonErrorDetailsMessage.FailedToConnectToDependentService,
                            Code = CommonErrors.ErrorDetailCode.ExternalServiceFailure.ToString(),
                            Target = DependencyName
                        }
                    });
            }
        }

        private async Task<CustomException> CreateExceptionFromResponse(HttpResponseMessage response)
        {
            var error = await TryReadErrorResponse(response);

            string headerMessage;
            string detailMessage;
            CommonErrors.ErrorHeaderCode headerCode;
            CommonErrors.ErrorDetailCode detailCode;
            HttpStatusCode statusCode;

            switch (response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    statusCode = HttpStatusCode.InternalServerError;
                    headerCode = CommonErrors.ErrorHeaderCode.InternalError;
                    headerMessage = CommonErrors.CommonErrorHeaderMessage.FailedToUseDependentService;
                    detailCode = CommonErrors.ErrorDetailCode.AuthenticationError;
                    detailMessage = CommonErrors.CommonErrorDetailsMessage.FailedToAuthenticateWithDependentService;
                    break;
                case HttpStatusCode.Forbidden:
                    statusCode = HttpStatusCode.InternalServerError;
                    headerCode = CommonErrors.ErrorHeaderCode.InternalError;
                    headerMessage = CommonErrors.CommonErrorHeaderMessage.FailedToUseDependentService;
                    detailCode = CommonErrors.ErrorDetailCode.AuthorisationError;
                    detailMessage = CommonErrors.CommonErrorDetailsMessage.NotAuthorisedToConnectToDependentService;
                    break;
                case HttpStatusCode.BadRequest:
                    statusCode = HttpStatusCode.InternalServerError;
                    headerCode = CommonErrors.ErrorHeaderCode.InternalError;
                    headerMessage = CommonErrors.CommonErrorHeaderMessage.FailedToUseDependentService;
                    detailCode = CommonErrors.ErrorDetailCode.BadArgumentUsedWithDependentService;
                    detailMessage = error?.Message;
                    break;
                case HttpStatusCode.NotFound:
                    if (error != null)
                    {
                        statusCode = HttpStatusCode.NotFound;
                        headerCode = CommonErrors.ErrorHeaderCode.NotFound;
                        headerMessage = CommonErrors.CommonErrorHeaderMessage.ExternalResourceNotFound;
                        detailCode = CommonErrors.ErrorDetailCode.ExternalResourceNotFound;
                        detailMessage = CommonErrors.CommonErrorDetailsMessage.NoResourceFound;
                    }
                    else
                    {
                        statusCode = HttpStatusCode.ServiceUnavailable;
                        headerCode = CommonErrors.ErrorHeaderCode.ServiceUnavailable;
                        headerMessage = CommonErrors.CommonErrorHeaderMessage.ExternalResourceNotFound;
                        detailCode = CommonErrors.ErrorDetailCode.ExternalResourceNotFound;
                        detailMessage = CommonErrors.CommonErrorDetailsMessage.FailedToConnectToDependentService;
                    }

                    break;
                default:
                    statusCode = HttpStatusCode.ServiceUnavailable;
                    headerCode = CommonErrors.ErrorHeaderCode.ServiceUnavailable;
                    headerMessage = CommonErrors.CommonErrorHeaderMessage.ExternalDependentServiceFailed;
                    detailCode = CommonErrors.ErrorDetailCode.ExternalServiceFailure;
                    detailMessage = CommonErrors.CommonErrorDetailsMessage.FailedToConnectToDependentService;
                    break;
            }

            return new CustomException(headerMessage, headerCode.ToString(), new[]
            {
                new ErrorDetailModel
                {
                    Message = detailMessage,
                    Code = detailCode.ToString(),
                    Target = DependencyName
                }
            }, statusCode);
        }

        /// <summary>
        /// Parse the response body as a standard error object if possible.
        /// Returns null if the parse is unsuccessful
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        async Task<ErrorModel> TryReadErrorResponse(HttpResponseMessage response)
        {
            ErrorModel result = null;
            if (response.Content != null)
            {
                try
                {
                    result = await response.Content.ReadAsAsync<ErrorModel>();
                }
                catch (Exception ex)
                {
                    TelemetryClient.TrackException(ex);
                }
            }

            return result;
        }

        /// <summary>
        /// Parses a HttpResponseMessage content into the specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        /// <exception cref="CustomException">thrown when response content cannot be parsed</exception>
        protected async Task<T> ParseResponse<T>(HttpResponseMessage response)
        {
            try
            {
                return await response.Content.ReadAsAsync<T>().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                TelemetryClient.TrackException(ex);
                throw FailedToPerformTranslationLogicException;
            }
        }

        private CustomException FailedToPerformTranslationLogicException
        {
            get
            {
                return new CustomException(
                    CommonErrors.CommonErrorHeaderMessage.InternalErrorWithinApi,
                    CommonErrors.ErrorHeaderCode.InternalError.ToString(), new[]
                    {
                        new ErrorDetailModel
                        {
                            Message = CommonErrors.CommonErrorDetailsMessage.FailedToPerformTranslationLogic,
                            Code = CommonErrors.ErrorDetailCode.InternalError.ToString(),
                            Target = DependencyName,
                        }
                    }, HttpStatusCode.InternalServerError);
            }
        }

        protected T ParseText<T>(string text)
        {
            T result = default;
            if (string.IsNullOrEmpty(text)) return result;
            try
            {
                result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(text);
            }
            catch (Exception ex)
            {
                TelemetryClient.TrackException(ex);
                throw FailedToPerformTranslationLogicException;
            }

            return result;
        }

    }
}
