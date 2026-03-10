using Newtonsoft.Json;

namespace Landscape2.Runtime.LicenseAuth
{
    public class LicenseData : IResponseMessage
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("license")]
        public LicenseDto License { get; set; }
    }
}