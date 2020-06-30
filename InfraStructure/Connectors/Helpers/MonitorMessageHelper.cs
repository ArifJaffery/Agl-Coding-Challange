using InfraStructure.Connectors.ConcreteClasses;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using InfraStructure.Connectors.Constants;
using InfraStructure.Connectors.Extensions;
using InfraStructure.Connectors.Models;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using System.IdentityModel.Tokens.Jwt;

namespace InfraStructure.Connectors.Helpers
{
    public class MonitorMessageHelper
    {
        public const string SHOULDSKIP_PREFIX = "testmessage";


        #region HttpContext methods
        /// <summary>
        /// Create a monitor message from HttpContext
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static async Task<MonitorMessage> CreateForHttpRequest(HttpContext httpContext)
        {
            var message = new MonitorMessage();
            message.MessageType = MonitorMessageTypes.HttpRequest;
            message.CallType = message.MessageType.ToString();

            var request = httpContext.Request;
            message.CorrelationId = GetCurrentCorrelationId(httpContext);
            message.RequestAddress = UriHelper.GetDisplayUrl(request);
            message.RequestBody = await GetContent(request);
            message.RequestHeaders = GetHeaders(request.Headers);
            message.RequestArchitectureMetadata = httpContext.GetRequestArchitectureMetadata();
            message.RequestMethod = request.Method;
            message.RequestUser = GetRequestUser(httpContext);
            return message;
        }

        /// <summary>
        /// Update a monitor message from HttpContext
        /// </summary>
        /// <param name="message"></param>
        /// <param name="httpContext"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static async Task UpdateForHttpRequest(MonitorMessage message, HttpContext httpContext, Exception ex = null)
        {
            message.EndTime = DateTimeOffset.UtcNow;
            message.Duration = message.EndTime - message.StartTime;

            var response = httpContext.Response;
            message.ResponseBody = await GetContent(response);
            message.ResponseHeaders = GetHeaders(response.Headers);
            message.ResponseArchitectureMetadata = GetCurrentArchitectureMetadata(httpContext);
            message.ResponseCode = response.StatusCode.ToString();

            if (ex == null)
            {
                message.IsError = response.StatusCode < 200 || response.StatusCode >= 300;
            }
            else
            {
                if (!ProcessShouldSkipException(ex, message))
                {
                    message.IsError = true;
                    message.ErrorMessage = MonitorHelper.GetErrorString(ex);
                    if (response.StatusCode == 200)
                    {
                        message.ResponseCode = "500";
                    }
                }
            }
        }

        #endregion

        #region HttpClient methods
        /// <summary>
        /// Create a monitor message from HttpClient
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task<MonitorMessage> CreateForHttpClient(HttpContext httpContext, HttpRequestMessage request)
        {
            var message = new MonitorMessage();
            message.MessageType = MonitorMessageTypes.HttpClient;
            message.CallType = message.MessageType.ToString();

            message.CorrelationId = GetCurrentCorrelationId(httpContext);

            message.RequestAddress = request.RequestUri.ToString();
            message.RequestBody = await GetContent(request);
            message.RequestArchitectureMetadata = GetCurrentArchitectureMetadata(httpContext);
            message.RequestHeaders = GetHeaders(request.Headers);
            message.RequestMethod = request.Method.ToString();
            message.RequestUser = GetRequestUser(request);

            return message;
        }

        /// <summary>
        /// Update a monitor message from HttpClient
        /// </summary>
        /// <param name="message"></param>
        /// <param name="response"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static async Task UpdateForHttpClient(MonitorMessage message, HttpResponseMessage response, Exception ex = null)
        {
            message.EndTime = DateTimeOffset.UtcNow;
            message.Duration = message.EndTime - message.StartTime;

            if (response != null)
            {
                message.ResponseBody = await GetContent(response);
                message.ResponseHeaders = GetHeaders(response.Headers);
                message.ResponseArchitectureMetadata = GetArchitectureMetadata(response.Headers);
                message.ResponseCode = ((int)response.StatusCode).ToString();
                message.IsError = (int)response.StatusCode < 200 || (int)response.StatusCode >= 300;
            }
            if (ex != null)
            {
                if (!ProcessShouldSkipException(ex, message))
                {
                    message.IsError = true;
                    message.ErrorMessage = MonitorHelper.GetErrorString(ex);
                    if (String.IsNullOrEmpty(message.ResponseCode))
                    {
                        message.ResponseCode = "ERROR";
                    }
                }
            }

        }

        #endregion

        #region IMessageSender
        /// <summary>
        /// Create a monitor message from a service bus sender message
        /// </summary>
        /// <param name="messageSender"></param>
        /// <param name="serviceBusMessage"></param>
        /// <returns></returns>
        public static async Task<MonitorMessage> CreateForServiceBusMessageSender(ISenderClient messageSender, Message serviceBusMessage)
        {
            var message = new MonitorMessage();
            message.MessageType = MonitorMessageTypes.ServiceBusSender;
            message.CallType = message.MessageType.ToString();

            message.CorrelationId = GetCurrentCorrelationId(serviceBusMessage);

            message.RequestAddress = messageSender.ServiceBusConnection.Endpoint.OriginalString + "/" + messageSender.Path;
            message.RequestBody = await GetContent(serviceBusMessage.Body);
            message.RequestArchitectureMetadata = GetArchitectureMetadata(serviceBusMessage.UserProperties);
            message.RequestHeaders = GetHeaders(serviceBusMessage.UserProperties);
            message.RequestMethod = "SEND";

            return message;
        }

        /// <summary>
        /// Update a monitor message from a service bus sender message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static async Task UpdateForServiceBusMessageSender(MonitorMessage message, Exception ex = null)
        {
            message.EndTime = DateTimeOffset.UtcNow;
            message.Duration = message.EndTime - message.StartTime;

            if (ex != null)
            {
                if (!ProcessShouldSkipException(ex, message))
                {
                    message.IsError = true;
                    message.ErrorMessage = MonitorHelper.GetErrorString(ex);
                    if (String.IsNullOrEmpty(message.ResponseCode))
                    {
                        message.ResponseCode = "ERROR";
                    }
                }
            }
            else
            {
                if (String.IsNullOrEmpty(message.ResponseCode))
                {
                    message.ResponseCode = "SUCCESS";
                }
            }
        }



        #endregion

        #region IMessageReceiver
        /// <summary>
        /// Create a monitor message from a service bus receiver message
        /// </summary>
        /// <param name="messageReceiver"></param>
        /// <param name="serviceBusMessage"></param>
        /// <returns></returns>
        public static async Task<MonitorMessage> CreateForServiceBusMessageReceiver(IReceiverClient messageReceiver, Message serviceBusMessage)
        {
            var message = new MonitorMessage();
            message.MessageType = MonitorMessageTypes.ServiceBusReceiver;
            message.CallType = message.MessageType.ToString();

            message.CorrelationId = GetCurrentCorrelationId(serviceBusMessage);
            if (messageReceiver != null)
            {
                message.RequestAddress = messageReceiver.ServiceBusConnection.Endpoint.OriginalString + "/" + messageReceiver.Path;
            }
            message.RequestBody = await GetContent(serviceBusMessage.Body);
            message.RequestArchitectureMetadata = GetArchitectureMetadata(serviceBusMessage.UserProperties);
            message.RequestHeaders = GetHeaders(serviceBusMessage.UserProperties);
            message.RequestMethod = "RECEIVE";

            return message;
        }

        /// <summary>
        /// Update a monitor message from a service bus receiver message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static async Task UpdateForServiceBusMessageReceiver(MonitorMessage message, Exception ex = null)
        {
            message.EndTime = DateTimeOffset.UtcNow;
            message.Duration = message.EndTime - message.StartTime;

            if (ex != null)
            {
                if (!ProcessShouldSkipException(ex, message))
                {
                    message.IsError = true;
                    message.ErrorMessage = MonitorHelper.GetErrorString(ex);
                    message.ResponseCode = "ERROR";
                }
            }
            else
            {
                message.ResponseCode = "SUCCESS";
            }
        }



        #endregion


        #region Generic object

        /// <summary>
        /// Create a monitor message for a generic object
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="obj"></param>
        /// <param name="requestAddress"></param>
        /// <param name="requestMethod"></param>
        /// <returns></returns>
        public static async Task<MonitorMessage> CreateForGenericObject(HttpContext httpContext, object obj, string requestAddress, string requestMethod = "SEND")
        {
            var message = new MonitorMessage();
            message.MessageType = MonitorMessageTypes.GenericObjectSender;
            message.CallType = message.MessageType.ToString();

            var request = httpContext.Request;
            message.CorrelationId = GetCurrentCorrelationId(httpContext);
            message.RequestAddress = requestAddress;
            message.RequestBody = await GetContentFromObject(obj);
            //message.RequestHeaders = GetHeaders(request.Headers);
            message.RequestArchitectureMetadata = GetCurrentArchitectureMetadata(httpContext);
            message.RequestMethod = requestMethod;
            return message;
        }

        /// <summary>
        /// Update a monitor message from a generic object
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static async Task UpdateForGenericObject(MonitorMessage message)
        {
            await UpdateForGenericObject(message, null);
        }

        /// <summary>
        /// Update a monitor message from a generic object
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static async Task UpdateForGenericObject(MonitorMessage message, Exception ex)
        {
            message.EndTime = DateTimeOffset.UtcNow;
            message.Duration = message.EndTime - message.StartTime;

            if (ex != null)
            {
                if (!ProcessShouldSkipException(ex, message))
                {
                    message.IsError = true;
                    message.ErrorMessage = MonitorHelper.GetErrorString(ex);
                    message.ResponseCode = "ERROR";
                }
            }
            else
            {
                message.ResponseCode = "SUCCESS";
            }
        }


        public static async Task<object> WrapMonitorMessagingForGenericObject(MonitorLogger _monitorLogger, HttpContext httpContext, object obj, string address, Func<Task<object>> func)
        {
            object result = null;

            var monitorMessage = await MonitorMessageHelper.CreateForGenericObject(httpContext, obj, address);
            try
            {
                try
                {
                    result = await func();

                    await MonitorMessageHelper.UpdateForGenericObject(monitorMessage);
                }
                catch (MonitorShouldSkipException ex)
                {
                    MonitorMessageHelper.UpdateShouldSkipMessage(monitorMessage);
                }
            }
            catch (Exception ex)
            {
                await MonitorMessageHelper.UpdateForGenericObject(monitorMessage, ex);
                throw;
            }
            finally
            {
                await _monitorLogger.WriteMessage(monitorMessage);
            }

            return result;
        }



        public static bool IsShouldSkipCorrelationId(string correlationId)
        {
            if (correlationId != null)
            {
                if (correlationId.ToLower().StartsWith(SHOULDSKIP_PREFIX.ToLower()))
                {
                    return true;
                }
            }

            return false;
        }

        public static void CheckMonitorShouldSkip(string correlationId)
        {
            if (IsShouldSkipCorrelationId(correlationId))
            {
                throw new MonitorShouldSkipException(correlationId);
            }
        }

        public static void UpdateShouldSkipMessage(MonitorMessage message)
        {
            message.ResponseCode = "SKIPPED";
        }

        public static bool ProcessShouldSkipException(Exception ex, MonitorMessage message)
        {
            if (ex is MonitorShouldSkipException)
            {
                UpdateShouldSkipMessage(message);
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion


        #region Misc methods

        #region GetContent
        /// <summary>
        /// Get HttpRequest body content
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task<string> GetContent(HttpRequest request)
        {
            return await request.GetRequestBodyAsync();
        }

        /// <summary>
        /// Get HttpResponse body content
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static async Task<string> GetContent(HttpResponse response)
        {
            try
            {
                response.Body.Seek(0, SeekOrigin.Begin);

                string text = await new StreamReader(response.Body).ReadToEndAsync();

                response.Body.Seek(0, SeekOrigin.Begin);

                return text;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get a HttpRequestMessage body content
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task<string> GetContent(HttpRequestMessage request)
        {
            if (request.Content != null)
            {
                var content = await request.Content.ReadAsStringAsync();
                return content;
            }
            return "";
        }

        /// <summary>
        /// Get a HttpResponseMessage body content
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static async Task<string> GetContent(HttpResponseMessage response)
        {
            if (response.Content != null)
            {
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            return "";
        }

        /// <summary>
        /// Get a string content from a byte array
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static async Task<string> GetContent(byte[] bytes)
        {
            var bodyString = Encoding.UTF8.GetString(bytes);
            return await Task.FromResult(bodyString);
        }

        /// <summary>
        /// Get a string from an object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static async Task<string> GetContentFromObject(object obj)
        {
            if (obj == null)
            {
                return null;
            }
            if (obj is string)
            {
                return (string)obj;
            }
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        #endregion

        #region GetHeaders

        /// <summary>
        /// Get headers dictionary from a IHeaderDictionary object
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetHeaders(IHeaderDictionary headers)
        {
            var dict = new Dictionary<string, string>();
            foreach (KeyValuePair<string, StringValues> pair in headers)
            {
                var value = $"{pair.Value}";
                dict[pair.Key] = FilterHeaderValue(pair.Key, value);
            }
            return dict;
        }

        /// <summary>
        /// Get headers dictionary from a HttpHeaders object
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetHeaders(HttpHeaders headers)
        {
            var dict = new Dictionary<string, string>();
            foreach (var pair in headers)
            {
                var value = $"{pair.Value?.FirstOrDefault()}";
                dict[pair.Key] = FilterHeaderValue(pair.Key, value);
            }
            return dict;
        }

        /// <summary>
        /// Get headers dictionary from another dictionary object
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetHeaders(IDictionary<string, object> headers)
        {
            var dict = new Dictionary<string, string>();
            foreach (var pair in headers)
            {
                var value = $"{pair.Value}";
                dict[pair.Key] = FilterHeaderValue(pair.Key, value);
            }
            return dict;
        }

        /// <summary>
        /// Hide/mask sencitive header values
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string FilterHeaderValue(string name, string value)
        {
            if (value != null && name != null)
            {
                name = name.ToLower();

                if (name == "authorization")
                {
                    var parts = value.Split(' ');
                    if (parts.Length > 1)
                    {
                        value = parts[0] + " " + MonitorHelper.MaskSecretString(parts[1], 10);
                    }
                }
                if (name.EndsWith("key"))
                {
                    value = MonitorHelper.MaskSecretString(value, 10);
                }
            }
            return value;
        }



        #endregion

        #region GetCurrentCorrelationId

        /// <summary>
        /// Get correlation id from a HttpContext object
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static string GetCurrentCorrelationId(HttpContext httpContext)
        {
            if (httpContext != null)
            {
                return httpContext.GetClientCorrelationId();
            }
            return null;
        }

        /// <summary>
        /// Get correlation id from a service bus message object
        /// </summary>
        /// <param name="serviceBusMessage"></param>
        /// <returns></returns>
        public static string GetCurrentCorrelationId(Message serviceBusMessage)
        {
            if (serviceBusMessage != null)
            {
                if (serviceBusMessage.UserProperties.ContainsKey(nameof(MessageUserProperties.CorrelationId)))
                {
                    return $"{serviceBusMessage.UserProperties[nameof(MessageUserProperties.CorrelationId)]}";
                }
            }
            return null;
        }

        #endregion

        #region GetCurrentArchitectureMetadata

        /// <summary>
        /// Get architecture metadata from a HttpContext object
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ArchitectureMetadata GetCurrentArchitectureMetadata(HttpContext context)
        {
            if (context != null)
            {
                return context.GetFromContext<ArchitectureMetadata>(nameof(ArchitectureMetadata));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get architecture metadata from a HttpRequest object
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static ArchitectureMetadata GetArchitectureMetadata(HttpRequest request)
        {
            var value = request.Headers[HttpConstants.ArchitectureMetadataName];
            if (!string.IsNullOrEmpty(value))
            {
                return ArchitectureMetadata.FromString(value);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get architecture metadata from a HttpResponse object
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static ArchitectureMetadata GetArchitectureMetadata(HttpResponse response)
        {
            var value = response.Headers[HttpConstants.ArchitectureMetadataName];
            if (!string.IsNullOrEmpty(value))
            {
                return ArchitectureMetadata.FromString(value);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get architecture metadata from a HttpHeaders object
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static ArchitectureMetadata GetArchitectureMetadata(HttpHeaders headers)
        {
            if (headers.Contains(HttpConstants.ArchitectureMetadataName))
            {
                var values = headers.GetValues(HttpConstants.ArchitectureMetadataName);
                return ArchitectureMetadata.FromString(values.FirstOrDefault());
            }
            return null;
        }

        /// <summary>
        /// Get architecture metadata from a dictionary object
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static ArchitectureMetadata GetArchitectureMetadata(IDictionary<string, object> dict)
        {
            return ArchitectureMetadata.FromObjectDictionary(dict);
        }

        #endregion

        #region GetRequestUser

        /// <summary>
        /// Get a user name from an authorization header value
        /// </summary>
        /// <param name="authorization"></param>
        /// <returns></returns>
        public static string GetAuthorizationUser(string authorization)
        {
            string name = null;
            try
            {
                if (!String.IsNullOrWhiteSpace(authorization))
                {
                    var parts = authorization.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 1)
                    {
                        var format = parts[0].ToLower();
                        var encoded = parts[1];
                        if (format == "basic")
                        {
                            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
                            name = decoded.Split(':')[0];
                        }
                        else if (format == "bearer")
                        {
                            if (encoded != TestHelper.TEST_OAUTH2_TOKEN)
                            {
                                var tokenHandler = new JwtSecurityTokenHandler();
                                var token = tokenHandler.ReadJwtToken(encoded);

                                name = token.Claims.FirstOrDefault(v => v.Type == "appid")?.Value;
                                if (String.IsNullOrEmpty(name))
                                {
                                    name = token.Claims.FirstOrDefault(v => v.Type == "unique_name")?.Value;
                                }
                                if (String.IsNullOrEmpty(name))
                                {
                                    name = token.Claims.FirstOrDefault(v => v.Type == "email")?.Value;
                                }
                                if (String.IsNullOrEmpty(name))
                                {
                                    name = token.Claims.FirstOrDefault(v => v.Type == "nameid")?.Value;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // ignore errors that could happen from some unsupported encoding.
            }
            return name;
        }

        /// <summary>
        /// Get a user name from an authorization a HttpContext object
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static string GetRequestUser(HttpContext httpContext)
        {
            try
            {
                var authorization = $"{httpContext.Request.Headers["Authorization"]}";
                return GetAuthorizationUser(authorization);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get a user name from an authorization a HttpRequestMessage object
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetRequestUser(HttpRequestMessage request)
        {
            try
            {
                var authorization = $"{request.Headers.GetValues("Authorization").FirstOrDefault()}";
                return GetAuthorizationUser(authorization);
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region CorrelationId
        /// <summary>
        /// Generate Guid value from any string. Uses an MD5 hashing algorithm
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Guid GenerateGuid(string s)
        {
            if (String.IsNullOrWhiteSpace(s))
            {
                return Guid.Empty;
            }

            s = s.Trim();
            var guid = Guid.Empty;
            if (Guid.TryParse(s, out guid))
            {
                return guid;
            }

            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(s);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                guid = new Guid(hashBytes);
            }

            return guid;
        }

        #endregion

        /// <summary>
        /// Get a process name for the current execution context
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentProcessName()
        {
            string processName = null;
            try
            {
                processName = Assembly.GetEntryAssembly().GetName().Name;
                if (processName == "func")
                {
                    var stackTrace = new StackTrace();
                    foreach (var frame in stackTrace.GetFrames())
                    {
                        var method = frame.GetMethod();
                        var attrs = method.GetCustomAttributes();
                        if (attrs != null)
                        {
                            foreach (var attr in attrs)
                            {
                                var type = attr.GetType();
                                if (type.Name == "FunctionNameAttribute")
                                {
                                    processName = method.DeclaringType?.Assembly.GetName().Name;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // do nothing
            }

            if (string.IsNullOrEmpty(processName))
            {
                processName = Assembly.GetEntryAssembly().GetName().Name;
            }

            return processName;
        }

        /// <summary>
        /// Get a string from an object as JSON
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetContentFromObjectAsJson(object obj)
        {
            return GetContentFromObject(obj).Result;
        }

        /// <summary>
        /// Get a string from an object as XML
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetContentFromObjectAsXml(object obj)
        {
            if (obj == null)
            {
                return null;
            }
            if (obj is string)
            {
                return (string)obj;
            }

            var ser = new System.Xml.Serialization.XmlSerializer(obj.GetType());
            var sw = new StringWriter();
            ser.Serialize(sw, obj);
            var res = sw.ToString();
            return res;
        }

        #endregion


    }
}
