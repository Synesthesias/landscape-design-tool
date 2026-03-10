using Newtonsoft.Json;
using System;

namespace Landscape2.Runtime.LicenseAuth
{
    [Serializable]
    public class UpdateDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("releaseDate")]
        public string ReleaseDate { get; set; }

        [JsonProperty("fileUrl")]
        public string FileUrl { get; set; }
    }
}