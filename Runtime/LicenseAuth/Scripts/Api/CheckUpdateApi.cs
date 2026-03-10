using Newtonsoft.Json;
using System.Threading.Tasks;

using UnityEngine;

namespace Landscape2.Runtime.LicenseAuth
{
    public class CheckUpdateApi : ApiBase<UpdateData>
    {
        protected override string endpoint => $"{EndpointConfig.API_URL}/softwares/latest";

        public async Task<(UpdateData, ErrorDto)> CheckUpdateAsync()
        {
            // トークンの有効性を確認
            var error = await CheckAccessTokenAsync();

            if (error != null)
            {
                return (null, error);
            }

            var request = CreateGetRequest(AuthManager.CurrentTokenInfo.AccessToken);

            var response = await SendRequestAsync<UpdateData>(request);

            if (response.error != null)
            {
                if (response.error.Code == "-1")
                {
                    response.error.Code = "4100";
                }
            }

            return response;
        }
    }
}