using System;
using System.Threading.Tasks;
using InfraStructure.Connectors.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace InfraStructure.Connectors.Extensions
{
    public static class AuthenticationContextExtensions
    {
        /// <summary>
        /// Gets an access token from Azure AD using client credentials.
        /// If the attempt to get a token fails because the server is unavailable, retry twice after 3 seconds each.
        /// </summary>
        /// <returns></returns>
        public static async Task<AuthenticationResult> AuthenticateWithAzureActiveDirectoryAsync(
            this AuthenticationContext authenticationContext,
            ClientCredential clientCredential,
            string resourceId,
            ILogger logger)

        {
            AuthenticationResult result = null;
            var retryCount = 0;
            bool retry;

            do
            {
                retry = false;

                try
                {
                    // ADAL includes an in memory cache, so this call will only send a message to the server if the cached token is expired.
                    result = await authenticationContext.AcquireTokenAsync(resourceId, clientCredential).ConfigureAwait(false);
                }
                catch (AdalException ex)
                {
                    if ("temporarily_unavailable".Equals(ex.ErrorCode, StringComparison.InvariantCultureIgnoreCase))
                    {
                        retry = true;
                        retryCount++;
                        await Task.Delay(3000);
                    }

                    logger?.LogError(ex, $"An error occurred while acquiring a token\nTime: {DateTime.UtcNow}\nRetry: {retry.ToString()}\nError: {ex}\n");
                }

            } while (retry && retryCount < 3);

            return result;
        }


        public static async Task<string> GetAuthenticationTokenWithAzureActiveDirectoryAsync(this
        AuthenticationContext authenticationContext,
        ClientCredential clientCredential,
        string resourceId,
        ILogger logger)

        {
            string accessToken = null;

            if (TestHelper.IsUnitTestContext() == false)
            {
                AuthenticationResult result = await authenticationContext.AuthenticateWithAzureActiveDirectoryAsync(clientCredential, resourceId, logger);
                if (result?.AccessToken != null)
                {
                    accessToken = result.AccessToken;
                }
            }
            else
            {
                accessToken = TestHelper.TEST_OAUTH2_TOKEN;
            }


            return accessToken;
        }

    }
}
