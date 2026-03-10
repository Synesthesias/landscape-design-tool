using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Landscape2.Runtime.LicenseAuth
{
    [Serializable]
    public class UserDto
    {
        //public string Name { get; set; }
        //public string Email { get; set; }

        [JsonProperty("workspaceName")]
        public string workspaceName { get; set; }

        [JsonProperty("licenseKeys")]
        public List<string> LicenseKeys { get; set; } = new();
    }
}