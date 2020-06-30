using System.Net.Http;
using InfraStructure.Connectors.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Threading;
using InfraStructure.Connectors.Interfaces;
using InfraStructure.Connectors.Models;

namespace InfraStructure.Connectors.ConcreteClasses
{
    public class PetProcessApiClient : AadApimHttpClient, IPetProcessApiClient
    {

        public PetProcessApiClient(HttpClient httpClient,
            IOptions<PetProcessApi> processApiClientConfiguration,
            AuthenticationContext authenticationContext,
            ClientCredential clientCredential,
            ILogger<PetProcessApiClient> logger,
            IHttpContextAccessor httpContextAccessor) : base(httpClient,
            processApiClientConfiguration,
            authenticationContext,
            clientCredential,
            logger,
            httpContextAccessor)
        {

        }

        /// <summary>
        /// GetAsync
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> GetAsync(string url, CancellationToken cancellationToken)
        {
            var response = await Client.GetAsync(url, cancellationToken);
            return response;
        }

        /// <summary>
        /// PostAsync
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
        {
            var response = await Client.PostAsync(url, content);
            return response;
        }

        public async Task<HttpResponseMessage> PatchAsync(string url, HttpContent content)
        {
            var response = await Client.PatchAsync(url, content);
            return response;
        }



    }
}
