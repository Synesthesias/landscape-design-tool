using Newtonsoft.Json;

namespace Landscape2.Runtime.LicenseAuth
{
    /// <summary>
    /// APIのレスポンスを受け取るための汎用クラス
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResponse<T>
    {
        [JsonProperty("status")]
        public bool Status { get; set; }

        [JsonProperty("data")]
        public T Data { get; set; }

        [JsonProperty("error")]
        public ErrorDto Error { get; set; }
    }
}