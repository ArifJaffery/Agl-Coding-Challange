using System.Text;
using Microsoft.AspNetCore.Http.Internal;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Http;
using InfraStructure.Connectors.Constants;
using InfraStructure.Connectors.Models;
using InfraStructure.Connectors.ConcreteClasses;

namespace InfraStructure.Connectors.Extensions
{
    public static class HttpExtensions
    {
        #region HttpContext Extensions

        /// <summary>
        /// Gets the user name for the HTTP request.
        /// In case of user identity based access scenario, returns the name of the primary claims identity
        /// In case of application identity based access scenarion, returns the value of the x_app_name claim
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetUser(this HttpContext context)
        {
            if (context == null)
                return null;

            var userName = string.Empty;

            if (string.IsNullOrWhiteSpace(context.User?.Identity?.Name) == false)
            {
                userName = context.User?.Identity?.Name;
            }
            else
            {
                var appNameClaim = context.User?.Claims.FirstOrDefault(c => c.Type == AuthorizationClaimsTypes.AppName);

                if (appNameClaim != null)
                {
                    userName = appNameClaim.Value;
                }
            }

            return userName;
        }

        /// <summary>
        /// Gets the app name for the HTTP request.
        /// This is extracted from the x_app_name claim, if present
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetClientAppName(this HttpContext context)
        {
            if (context == null)
                return null;

            var appName = string.Empty;
            var appNameClaim = context.User?.Claims.FirstOrDefault(c => c.Type == AuthorizationClaimsTypes.AppName);

            if (appNameClaim != null)
            {
                appName = appNameClaim.Value;
            }
            return appName;
        }


        public static void SaveToContext(this HttpContext context, string name, object value)
        {
            context.Items[name] = value;
        }

        public static object GetFromContext(this HttpContext context, string name)
        {
            return context.Items[name];
        }

        public static T GetFromContext<T>(this HttpContext context, string name, object defValue = null)
        {
            var value = context.Items[name] ?? defValue;
            return (T)value;
        }

        public static ArchitectureMetadata GetRequestArchitectureMetadata(this HttpContext httpContext)
        {
            var value = httpContext.Request.Headers[HttpConstants.ArchitectureMetadataName];
            var stringValue = $"{value}";
            return !string.IsNullOrEmpty(stringValue) ? ArchitectureMetadata.FromString(stringValue) : null;
        }

        public static void SetResponseArchitectureMetadata(this HttpContext httpContext, ArchitectureMetadata metadata)
        {
            if (metadata != null)
            {
                httpContext.Response.Headers.Add(HttpConstants.ArchitectureMetadataName, metadata.ToString());
            }
        }



        #endregion

        #region HttpRequest Extensions

        /// <summary>
        /// Gets a text representation of the request body
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        public static async Task<string> GetRequestBodyAsync(this HttpRequest httpRequest)
        {
            httpRequest.EnableRewind();

            var bodyAsText = await new StreamReader(httpRequest.Body).ReadToEndAsync();
            httpRequest.Body.Position = 0;
            //var injectedRequestStream = new MemoryStream();
            //var bytesToWrite = Encoding.UTF8.GetBytes(bodyAsText);

            //injectedRequestStream.Write(bytesToWrite, 0, bytesToWrite.Length);
            //injectedRequestStream.Seek(0, SeekOrigin.Begin);

            //httpRequest.Body = injectedRequestStream;

            return bodyAsText;
        }


        /// <summary>
        /// Gets an object representation of the request body deserialized from XML
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        public static async Task<T> GetRequestXmlObjectAsync<T>(this HttpRequest httpRequest)
        {
            var bodyAsText = await new StreamReader(httpRequest.Body).ReadToEndAsync();
            var injectedRequestStream = new MemoryStream();
            var bytesToWrite = Encoding.UTF8.GetBytes(bodyAsText);

            injectedRequestStream.Write(bytesToWrite, 0, bytesToWrite.Length);
            injectedRequestStream.Seek(0, SeekOrigin.Begin);

            httpRequest.Body = injectedRequestStream;

            T result;
            using (var stream = new MemoryStream(bytesToWrite))
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(T));
                    var xmlObject = (T)serializer.Deserialize(stream);
                    result = xmlObject;
                }
                catch
                {
                    // if a request body cannot be transalted into an XML object for some reason then return NULL following the same approach as MVC does with input parameters.
                    // ReSharper disable once RedundantTypeSpecificationInDefaultExpression
                    result = default(T);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets an object representation of the request body deserialized from JSON
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        public static async Task<T> GetRequestJsonlObjectAsync<T>(this HttpRequest httpRequest)
        {
            var bodyAsText = await new StreamReader(httpRequest.Body).ReadToEndAsync();
            var injectedRequestStream = new MemoryStream();
            var bytesToWrite = Encoding.UTF8.GetBytes(bodyAsText);

            injectedRequestStream.Write(bytesToWrite, 0, bytesToWrite.Length);
            injectedRequestStream.Seek(0, SeekOrigin.Begin);

            httpRequest.Body = injectedRequestStream;

            T result;
            try
            {
                var jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(bodyAsText);
                result = jsonObject;
            }
            catch
            {
                // if a request body cannot be transalted into a JSON object for some reason then return NULL following the same approach as MVC does with input parameters.
                // ReSharper disable once RedundantTypeSpecificationInDefaultExpression
                result = default(T);
            }
            return result;
        }

        /// <summary>
        /// Gets correlationId value from HTTP request context items collection, if present. Null otherwise.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static string GetClientCorrelationId(this HttpContext httpContext)
        {
            if (httpContext?.Items == null)
                return null;

            if (httpContext.Items.TryGetValue(HttpConstants.ClientCorrelationIdKeyName, out var correlationIdValue))
            {
                return (string)correlationIdValue;
            }

            return null;
        }

        /// <summary>
        /// Gets correlationId value from HTTP request header, if present. Null otherwise.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static string GetCorrelationIdHeaderValue(this HttpContext httpContext)
        {
            if (httpContext == null)
                return null;

            if (httpContext.Request.Headers.TryGetValue(HttpHeaderConstants.ClientCorrelationIdHeaderName, out var headerValues))
            {
                return headerValues;
            }

            return null;
        }

        /// <summary>
        /// Gets payload organization id value from HTTP request context items collection, if present. Null otherwise.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static string GetPayloadOrganizationId(this HttpContext httpContext)
        {
            if (httpContext?.Items == null)
                return null;

            if (httpContext.Items.TryGetValue(DataModelAuthorization.PayloadOrganizationIdKeyName, out var payloadOrganizationIdValue))
            {
                return (string)payloadOrganizationIdValue;
            }

            return null;
        }


        public static void SetSkipMonitorLogging(this HttpContext httpContext, bool flag)
        {
            httpContext.SaveToContext("SkipMonitorLogging", flag);
        }

        public static bool GetSkipMonitorLogging(this HttpContext httpContext)
        {
            return httpContext.GetFromContext<bool>("SkipMonitorLogging", false);
        }


        #endregion

    }
}
