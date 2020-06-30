
namespace InfraStructure.Connectors.Configuration
{
    public class ApimHttpClientConfiguration : HttpClientConfiguration
    {
        /// <summary>
        /// Azure API Management subscription key
        /// </summary>
        public string ApimSubscriptionKey { get; set; }

        /// <summary>
        /// If set to true, emits the header (x-mock-response) in the outbound API call.
        /// This by convention will cause the target FIP application OR APIM to return mocked response.
        /// Defaults to false.
        /// </summary>
        public bool? IncludeMockResponseHeader { get; set; } = false;
    }
}
