using Newtonsoft.Json;
using System;
using System.Collections;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Networking;

namespace Landscape2.Runtime.LicenseAuth
{
    public class UserInfoApi : ApiBase<UserData>
    {
        protected override string endpoint => $"{EndpointConfig.API_URL}/users/info";

        public async Task<(UserData, ErrorDto)> GetUserInfoAsync()
        {
            var error = await CheckAccessTokenAsync();

            if (error != null)
            {
                return (null, error);
            }

            var request = CreateGetRequest(AuthManager.CurrentTokenInfo.AccessToken);

            var response = await SendRequestAsync<UserData>(request);

            if (response.error != null)
            {
                if (response.error.Code == "-1")
                {
                    response.error.Code = "2100";
                }
            }

            return response;
        }
    }
}