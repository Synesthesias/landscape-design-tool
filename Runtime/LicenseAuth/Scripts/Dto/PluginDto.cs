using Newtonsoft.Json;
using System;

namespace Landscape2.Runtime.LicenseAuth
{
    [Serializable]
    public class PluginDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }
}