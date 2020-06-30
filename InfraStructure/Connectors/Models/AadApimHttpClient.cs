using System;
using System.Net.Http;
using InfraStructure.Connectors.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace InfraStructure.Connectors.Models
{
    public class AadApimHttpClient
    {
        public HttpClient Client => this.ConfiguredHttpClient();

        readonly ILogger _logger;
        readonly HttpClient _httpClient;
        readonly IOptions<AadApimHttpClientConfiguration> _aadApimHttpClientConfiguration;
        readonly AuthenticationContext _authenticationContext;
        readonly ClientCredential _clientCredential;

        public AadApimHttpClient(HttpClient httpClient,
            IOptions<AadApimHttpClientConfiguration> aadApimHttpClientConfiguration,
            AuthenticationContext authenticationContext,
            ClientCredential clientCredential,
            ILogger logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpClient = httpClient;
            _aadApimHttpClientConfiguration = aadApimHttpClientConfiguration;
            _authenticationContext = authenticationContext;
            _clientCredential = clientCredential;

            _httpClient.BaseAddress = new Uri(_aadApimHttpClientConfiguration.Value.BaseAddress);

        }

        HttpClient ConfiguredHttpClient()
        {
            return _httpClient;
        }

    }
}
