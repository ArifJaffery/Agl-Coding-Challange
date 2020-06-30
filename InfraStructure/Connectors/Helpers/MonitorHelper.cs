using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace InfraStructure.Connectors.Helpers
{
    /// <summary>
    /// A static class with common reusable methods
    /// </summary>
    public static class MonitorHelper
    {
        /// <summary>
        /// Replaces some number of characters in an input value with asteriscs. 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="openLength"></param>
        /// <returns></returns>
        public static string MaskSecretString(string value, int openLength)
        {
            var len = value.Length;
            var len1 = Math.Min(value.Length / 2, openLength);
            var len2 = len - len1;
            string value2 = value.Substring(0, len1) + new string('*', len2);
            return value2;
        }

        /// <summary>
        /// Retreave a value from an xml string using XPath
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetValueFromXml(string xml, string path)
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(xml);
                var node = doc.SelectSingleNode(path);

                return node.InnerText;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieve a value from JSON string using a Json Path
        /// </summary>
        /// <param name="json"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetValueFromJson(string json, string path)
        {
            try
            {
                var obj = JObject.Parse(json);
                var value = obj.SelectToken(path);

                return $"{value}";
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Limit a length of a string to some value of characters
        /// </summary>
        /// <param name="s"></param>
        /// <param name="limitSize"></param>
        /// <returns></returns>
        public static string LimitString(string s, int limitSize)
        {
            if (s != null && s.Length > limitSize)
            {
                s = s.Substring(limitSize);
            }
            return s;
        }

        /// <summary>
        /// Generate a string from any exception object.
        /// It can include inner exceptions and stack traces 
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="withInnerExceptions"></param>
        /// <param name="includeStackTrace"></param>
        /// <returns></returns>
        public static string GetErrorString(Exception ex, bool withInnerExceptions = true, bool includeStackTrace = true)
        {
            var sb = new StringBuilder();
            try
            {
                CompileExceptionText(sb, ex, withInnerExceptions, includeStackTrace);
            }
            catch
            {
                // ignore errors
            }

            return sb.ToString();
        }

        /// <summary>
        /// Inner implementation of a GetErrorString method. It cam be called recursively for each of an inner exception
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="ex"></param>
        /// <param name="withInnerExceptions"></param>
        /// <param name="includeStackTrace"></param>
        private static void CompileExceptionText(StringBuilder sb, Exception ex, bool withInnerExceptions, bool includeStackTrace)
        {
            if (ex == null)
            {
                return;
            }

            var s = $"Error: {ex.Message}";

            if (includeStackTrace)
            {
                try
                {
                    s += $" \nStackTrace: {ex.StackTrace} \n";
                }
                catch
                {
                    // ignore any exception raised from ex.StackTrace
                }

            }

            if (sb.Length > 0)
            {
                sb.Append(" \n");
            }
            sb.Append(s);

            if (withInnerExceptions)
            {
                CompileExceptionText(sb, ex.InnerException, withInnerExceptions, includeStackTrace);

                if (ex is AggregateException)
                {
                    AggregateException aggEx = (AggregateException)ex;

                    var innerExs = aggEx.InnerExceptions;
                    try
                    {
                        innerExs = aggEx.Flatten().InnerExceptions;
                    }
                    catch
                    {
                        // ignore any exception raised from InnerException
                    }

                    foreach (var e in innerExs)
                    {
                        CompileExceptionText(sb, e, withInnerExceptions, includeStackTrace);
                    }
                }
            }
        }

        /// <summary>
        /// Make a string look like Uri.
        /// It is used in palces where a uri formatted string is expected.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Uri GetUri(string url)
        {
            if (url == null)
            {
                url = "";
            }
            url = url.Trim();
            var parts = url.Split("://", 2, StringSplitOptions.None);
            if (parts.Length == 1)
            {
                url = "unknown://" + url;
            }
            else if (parts[1] == "")
            {
                url = url + "unknown";
            }

            try
            {
                var uri = new Uri(url);
                return uri;
            }
            catch
            {
                return new Uri("unknown://unknown");
            }
        }
    }
}
