using Newtonsoft.Json;

namespace Landscape2.Runtime.LicenseAuth
{
    /// <summary>
    /// トークン取得のレスポンス
    /// ApiResponse と形式が異なるため別クラスとして定義
    /// パラメータがほぼ共通のため AuthorizationToken/RefreshToken で共通化
    /// </summary>
    public class TokenResponse
    {
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }

        [JsonProperty("tokenType")]
        public string TokenType { get; set; }

        [JsonProperty("expiredIn")]
        public int ExpiresIn { get; set; }

        [JsonProperty("refreshToken")]
        public string RefreshToken { get; set; }

        [JsonProperty("idToken")]
        public string IdToken { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("error")]
        public ErrorDto Error { get; set; }
    }
}