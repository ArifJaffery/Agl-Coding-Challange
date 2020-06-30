using System.Web;

namespace InfraStructure.Connectors.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Converts a string from PascalCase to camelCase
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToCamelCase(this string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Length > 1)
            {
                return char.ToLowerInvariant(str[0]) + str.Substring(1);
            }

            return str;
        }

        /// <summary>
        /// URL-encodes a string, for instance safely encodes "+" characters
        /// in ISO8601 date-time strings.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToUrlEncoded(this string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                return HttpUtility.UrlEncode(str);
            }

            return str;
        }

    }
}
