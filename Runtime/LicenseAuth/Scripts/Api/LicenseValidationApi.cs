using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Landscape2.Runtime.LicenseAuth
{
    public class LicenseValidationApi : ApiBase<LicenseData>
    {
        protected override string endpoint => $"{EndpointConfig.API_URL}/licenses/validate";

        [Serializable]
        private class ValidateLicenseRequestJson
        {
            [JsonProperty("licenseKey")]
            public string LicenseKey;
        }

        public async Task<(LicenseData, ErrorDto)> ValidateLicenseAsync(string licenseKey)
        {
            var error = await CheckAccessTokenAsync();

            if (error != null)
            {
                return (null, error);
            }

            var requestJson = new ValidateLicenseRequestJson
            {
                LicenseKey = licenseKey
            };

            var body = JsonConvert.SerializeObject(requestJson);

            var request = CreatePostRequest(body, AuthManager.CurrentTokenInfo.AccessToken);

            var response = await SendRequestAsync<LicenseData>(request);

            if (response.error != null)
            {
                if (response.error.Code == "-1")
                {
                    response.error.Code = "3100";
                }
            }

            return response;
        }
    }
}