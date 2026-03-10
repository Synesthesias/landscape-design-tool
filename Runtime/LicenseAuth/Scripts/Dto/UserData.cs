using Newtonsoft.Json;

namespace Landscape2.Runtime.LicenseAuth
{
    public class UserData : IResponseMessage
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("user")]
        public UserDto User { get; set; }
    }
}