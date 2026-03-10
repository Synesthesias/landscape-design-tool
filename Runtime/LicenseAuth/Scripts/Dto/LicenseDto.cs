using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Landscape2.Runtime.LicenseAuth
{
    [Serializable]
    public class LicenseDto
    {
        // getでローカルの情報を返すようにする
        [JsonProperty("licenseCheckedAt")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTimeOffset LicenseCheckedAt { get; set; }

        [JsonProperty("expiryDate")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTimeOffset ExpiryDate { get; set; }

        [JsonProperty("software")]
        public PluginDto Software { get; set; }
    }
}