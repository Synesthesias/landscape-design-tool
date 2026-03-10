using System.Threading.Tasks;

namespace Landscape2.Runtime.LicenseAuth
{
    public class LogoutApi : ApiBase<LogoutData>
    {
        protected override string endpoint => $"{EndpointConfig.API_URL}/logout";

        public async Task<(LogoutData, ErrorDto)> LogoutAsync()
        {
            // トークンの有効性を確認
            var error = await CheckAccessTokenAsync();

            if (error != null)
            {
                return (null, error);
            }

            var request = CreatePostRequest(null, AuthManager.CurrentTokenInfo.AccessToken);

            return await SendRequestAsync<LogoutData>(request);
        }
    }
}