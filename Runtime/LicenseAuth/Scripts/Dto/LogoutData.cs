using Newtonsoft.Json;

namespace Landscape2.Runtime.LicenseAuth
{
    public class LogoutData : IResponseMessage
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}