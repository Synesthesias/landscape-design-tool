using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Landscape2.Runtime.LicenseAuth
{
    public class TokenApi : ApiBase<TokenResponse>
    {
        protected override string endpoint => $"{EndpointConfig.API_URL}/auth/token";

        [Serializable]
        private class AuthorizationTokenRequestJson
        {
            [JsonProperty("grantType")]
            public string grantType;

            [JsonProperty("code")]
            public string code;

            [JsonProperty("redirectUri")]
            public string redirectUri;

            [JsonProperty("softwareId")]
            public string softwareId;

            [JsonProperty("codeVerifier")]
            public string codeVerifier;
        }

        [Serializable]
        private class RefreshTokenRequestJson
        {
            [JsonProperty("refreshToken")]
            public string refreshToken;

            [JsonProperty("grantType")]
            public string grantType;

            [JsonProperty("softwareId")]
            public string softwareId;
        }

        public async Task<TokenResponse> AuthorizationTokenAsync()
        {
            var dto = new AuthorizationTokenRequestJson
            {
                grantType = "authorization_code",
                code = AuthManager.CurrentTokenInfo.Code,
                redirectUri = AuthManager.URLScheme,
                softwareId = AuthManager.AppID,
                codeVerifier = AuthManager.CurrentTokenInfo.CodeVerifier
            };

            var request = CreatePostRequest(JsonConvert.SerializeObject(dto), null);

            return await SendTokenRequestAsync(request);
        }

        public async Task<TokenResponse> RefreshTokenAsync()
        {
            var dto = new RefreshTokenRequestJson
            {
                refreshToken = AuthManager.CurrentTokenInfo.RefreshToken,
                grantType = "refresh_token",
                softwareId = AuthManager.AppID
            };

            var request = CreatePostRequest(JsonConvert.SerializeObject(dto), null);

            return await SendTokenRequestAsync(request);
        }
    }
}