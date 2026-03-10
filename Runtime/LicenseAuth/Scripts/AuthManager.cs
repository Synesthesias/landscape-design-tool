using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Landscape2.Runtime.LicenseAuth
{
    /// <summary>
    ///
    /// </summary>
    public static class AuthManager
    {
        public static bool IsSkipLogin = false;

        public static event Action<string> OnAuthCompleted;

        public static void TriggerLoginCompleted(string token)
        {
            OnAuthCompleted?.Invoke(token);
        }

        // コールバックURL（スキーム）
        public static readonly string URLScheme = "keikantoolyokotemachi://launch";

        // クライアントID
        public static readonly string AppID = "59cbf858-058e-4f03-900b-27638b7ca8dd";

        public static readonly string LaunchSceneName = "Maebashi_Yokotemachi";

        public static readonly string LoginSceneName = "LoginScene";

        public static string State { get; private set; }// CSRF対策用の state、検証目的なのでオンメモリで保持

        public static TokenInfo CurrentTokenInfo { get; private set; } = new TokenInfo();

        // 既定スコープ
        private static readonly string[] DefaultScopes = { "openid", "profile", "email" };

        public static string GetScopesString() => string.Join("+", DefaultScopes);

        /// <summary>
        /// 認可エンドポイントのURLを返す
        /// </summary>
        public static string CreateAuthorizeUrl()
        {
            var query = CreateLaunchRequest();

            return $"{EndpointConfig.LOGIN_URL}?{query}";
        }

        /// <summary>
        /// 起動時にCMS(認可サーバ)へ渡すクエリを作成
        /// </summary>
        public static string CreateLaunchRequest()
        {
            // PKCE: code_verifier を生成（43〜128文字のbase64url）
            CurrentTokenInfo.CodeVerifier = GenerateCodeVerifier();

            // PKCE: code_challenge = BASE64URL( SHA256(code_verifier) )
            string codeChallenge = MakeCodeChallenge(CurrentTokenInfo.CodeVerifier);

            // CSRF対策: state 生成
            State = GenerateState();

            // クエリを組み立て
            var sb = new StringBuilder();
            sb.Append("responseType=code").Append("&"); ;
            sb.Append("softwareId=").Append(AppID).Append("&");
            sb.Append("redirectUri=").Append(Uri.EscapeDataString(URLScheme)).Append("&");
            sb.Append("codeChallenge=").Append(codeChallenge).Append("&");
            sb.Append("codeChallengeMethod=S256").Append("&"); ;
            sb.Append("state=").Append(State).Append("&");
            sb.Append("scope=").Append(GetScopesString()).Append("&"); ;
            sb.Append("loginType=app");

            return sb.ToString();
        }

        public static bool ParseLoginResult(string result)
        {
            try
            {
                string code = null, state = null;

                var authData = result.Split('&');

                foreach (var item in authData)
                {
                    var keyValue = item.Split('=');

                    if (keyValue.Length == 2)
                    {
                        if (keyValue[0] == "code")
                        {
                            code = keyValue[1];
                        }
                        else if (keyValue[0] == "state")
                        {
                            state = keyValue[1];
                        }
                    }
                }

                if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
                {
                    Debug.LogError("Login result does not contain valid code or state.");

                    return false;
                }

                CurrentTokenInfo.Code = code;

                if (!ValidateState(state))
                {
                    Debug.LogError("State does not match. Possible CSRF attack.");

                    return false;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error parsing login result: {ex.Message}");

                return false;
            }

            return true;
        }

        /// <summary>
        /// コールバックで返ってきた state の検証（合っていれば true）
        /// </summary>
        public static bool ValidateState(string returnedState) =>
            !string.IsNullOrEmpty(returnedState) && returnedState == State;

        // 32バイト→base64url(=43文字)でRFC準拠の verifier を作成
        private static string GenerateCodeVerifier()
        {
            Span<byte> buf = stackalloc byte[32];
            RandomNumberGenerator.Fill(buf);
            return Base64UrlEncode(buf);
        }

        private static string MakeCodeChallenge(string verifier)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.ASCII.GetBytes(verifier);
            var hash = sha.ComputeHash(bytes);
            return Base64UrlEncode(hash);
        }

        private static string GenerateState()
        {
            Span<byte> buf = stackalloc byte[16];
            RandomNumberGenerator.Fill(buf);
            return Base64UrlEncode(buf);
        }

        private static string Base64UrlEncode(ReadOnlySpan<byte> bytes)
        {
            string s = Convert.ToBase64String(bytes);
            // base64url: '+'→'-', '/'→'_', '='除去
            return s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

        public static bool IsAccessTokenValid()
        {
            return !string.IsNullOrEmpty(CurrentTokenInfo.AccessToken) && DateTimeOffset.UtcNow < CurrentTokenInfo.AccessTokenExpiresAtUtc;
        }

        public static bool IsRefreshTokenValid()
        {
            return !string.IsNullOrEmpty(CurrentTokenInfo.RefreshToken);
        }

        public static void SetTokenInfo(TokenInfo tokenInfo)
        {
            CurrentTokenInfo = tokenInfo;
        }

        // TODO 保存の仕方によってはDTOをLisenceManagerから移譲、あくまでTokenはAuthManagerで管理
        public static void SetTokenInfo(TokenResponse tokenResponse)
        {
            // IdTokenが空の場合はトークン更新、そうでなければ新規取得とみなす
            if (string.IsNullOrEmpty(tokenResponse.IdToken))
            {
                CurrentTokenInfo.AccessToken = tokenResponse.AccessToken;
                CurrentTokenInfo.AccessTokenExpiresAtUtc = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                CurrentTokenInfo.RefreshToken = tokenResponse.RefreshToken;
            }
            else
            {
                CurrentTokenInfo.AccessToken = tokenResponse.AccessToken;
                CurrentTokenInfo.TokenType = tokenResponse.TokenType;
                CurrentTokenInfo.AccessTokenExpiresAtUtc = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                CurrentTokenInfo.RefreshToken = tokenResponse.RefreshToken;
                CurrentTokenInfo.IdToken = tokenResponse.IdToken;
                CurrentTokenInfo.Scope = tokenResponse.Scope;

                // JWTの中身をデコードしてログに出す
                DumpJwt(tokenResponse.IdToken);

                void DumpJwt(string jwt)
                {
                    var parts = jwt.Split('.');

                    if (parts.Length != 3)
                    {
                        UnityEngine.Debug.LogError("Not a JWT");

                        return;
                    }

                    UnityEngine.Debug.Log("Header:  " + B64UrlToString(parts[0]));
                    UnityEngine.Debug.Log("Payload: " + B64UrlToString(parts[1]));
                }
            }
        }

        /// <summary>
        /// 表示用のユーザー名を返す
        /// </summary>
        /// <returns></returns>
        public static string GetUserDisplayNameFromIdToken()
        {
            return TryGetIdTokenPayload(out var p) ? p?.name : "User Name";
        }

        /// <summary>
        /// IDトークンから payload を取り出す
        /// </summary>
        private static bool TryGetIdTokenPayload(out IdTokenPayload payload)
        {
            payload = null;

            var jwt = CurrentTokenInfo?.IdToken;

            if (string.IsNullOrEmpty(jwt))
            {
                return false;
            }

            var parts = jwt.Split('.');

            if (parts.Length != 3)
            {
                return false;
            }

            try
            {
                string json = B64UrlToString(parts[1]);

                payload = JsonConvert.DeserializeObject<IdTokenPayload>(json);

                return payload != null;
            }
            catch
            {
                return false;
            }
        }

        private static string B64UrlToString(string s)
        {
            s = s.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4) { case 2: s += "=="; break; case 3: s += "="; break; }
            return Encoding.UTF8.GetString(Convert.FromBase64String(s));
        }
    }

    public class TokenInfo
    {
        public string Code { get; set; }

        public string CodeVerifier { get; set; }

        public string AccessToken { get; set; }

        public string TokenType { get; set; }

        public DateTimeOffset AccessTokenExpiresAtUtc { get; set; }

        public string RefreshToken { get; set; }

        public string IdToken { get; set; }

        public string Scope { get; set; }
    }

    // JWT payload
    public class IdTokenPayload
    {
        // 標準クレーム
        public string sub { get; set; }

        public string iss { get; set; }

        public string aud { get; set; }

        public long exp { get; set; }

        public long iat { get; set; }

        // オプショナルなユーザー情報
        public string name { get; set; }

        public string email { get; set; }
    }
}