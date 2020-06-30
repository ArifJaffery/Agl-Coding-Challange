
namespace InfraStructure.Connectors.Constants
{
    public static class HttpHeaderConstants
    {
        /// <summary>
        /// Http Header Name for client correlation id for the request
        /// </summary>
        public const string ClientCorrelationIdHeaderName = "correlationId";

        /// <summary>
        /// Http Header Name for client tracking id for the request
        /// </summary>
        public const string MockResponseHeaderName = "x-mock-response";

        /// <summary>
        /// Http Header Name for APIM Subscription Key Header Name
        /// </summary>
        public const string ApimSubscriptionKeyHeaderName = "Ocp-Apim-Subscription-Key";

        /// <summary>
        /// Http Header Name for content type
        /// </summary>
        public const string ContentTypeHeaderName = "Content-Type";

        /// <summary>
        /// Http Header Name for accept
        /// </summary>
        public const string AcceptHeaderName = "Accept";

        /// <summary>
        /// Http Header Name for authorization
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public const string Authorization = "Authorization";

    }
}
