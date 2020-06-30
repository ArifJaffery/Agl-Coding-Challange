using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace InfraStructure.Connectors.Interfaces
{
    public interface IPetProcessApiClient
    {
        HttpClient Client { get; }

        /// <summary>
        /// GetAsync
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> GetAsync(string url, CancellationToken cancellationToken);

        /// <summary>
        /// PostAsync
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> PostAsync(string url, HttpContent content);

        /// <summary>
        /// PatchAsync
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> PatchAsync(string url, HttpContent content);

    }
}
