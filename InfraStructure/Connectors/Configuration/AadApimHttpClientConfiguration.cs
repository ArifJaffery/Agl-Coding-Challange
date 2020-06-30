
namespace InfraStructure.Connectors.Configuration
{
    public class AadApimHttpClientConfiguration : ApimHttpClientConfiguration
    {
        /// <summary>
        /// Url of the resource for which the client should obtain access token.
        /// E.g. if outbound call is to http://google.com.au, that is the resource url, which will be
        /// included as the audience claim in the access token received back from Azure Active Directory
        /// </summary>
        public string AadResourceUrl { get; set; }
    }
}
