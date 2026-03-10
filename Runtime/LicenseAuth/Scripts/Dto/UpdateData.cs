using Newtonsoft.Json;

using UnityEngine;

namespace Landscape2.Runtime.LicenseAuth
{
    public class UpdateData : IResponseMessage
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("softwareVersion")]
        public UpdateDto SoftwareVersion { get; set; }
    }
}