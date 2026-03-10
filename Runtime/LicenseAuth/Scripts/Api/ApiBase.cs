using Newtonsoft.Json;

using System;
using System.Linq;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Networking;

namespace Landscape2.Runtime.LicenseAuth
{
    /// <summary>
    /// 接続先の設定
    /// </summary>
    public static class EndpointConfig
    {
        // 接続先のベースURL
        private static readonly string BASE_URL = $"https://vyn-tech.online";

        // ログインページのURL
        public static readonly string LOGIN_URL = $"{BASE_URL}/web/login";

        // APIのエンドポイントURL
        public static readonly string API_URL = $"{BASE_URL}/api";
    }

    public abstract class ApiBase<T>
    {
        protected abstract string endpoint
        {
            get;
        }

        protected async Task<(TData data, ErrorDto error)> SendRequestAsync<TData>(UnityWebRequest request) where TData : IResponseMessage
        {
            Debug.Log($"[UnityWebRequest REQUEST] URL: {request.url}\nMethod: {request.method}\nBody: {(request.uploadHandler is UploadHandlerRaw u ? System.Text.Encoding.UTF8.GetString(u.data) : "(no body)")}\nHeader: Authorization: {request.GetRequestHeader("Authorization")}");

            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            Debug.Log($"[UnityWebRequest RESULT]\nURL: {request.url}\nResult: {request.result}\nError: {request.error}\nResponse Code: {request.responseCode}\nDownload Text: {request.downloadHandler?.text ?? string.Empty}");

            var code = request.responseCode;
            var result = request.result;
            var text = request.downloadHandler?.text ?? string.Empty;

            ApiResponse<TData> resp = null;
            Exception jsonEx = null;

            try
            {
                if (!string.IsNullOrWhiteSpace(text) && text.TrimStart().StartsWith("{"))
                {
                    resp = JsonConvert.DeserializeObject<ApiResponse<TData>>(text);
                }
            }
            catch (Exception ex)
            {
                jsonEx = ex; // ログ用に保持（後で汎用エラーに落とす）
            }

            bool httpOk = (code >= 200 && code <= 299) && (result == UnityWebRequest.Result.Success);

            if (httpOk)
            {
                // 2xx の正常応答
                if (resp?.Status == true)
                {
                    Debug.Log($"[API SUCCESS] {resp.Data?.Message}");

                    return (resp.Data, null);
                }

                // 2xx だが API の error がある場合
                if (resp?.Error != null)
                {
                    Debug.Log($"[API ERROR] {resp.Error} {resp.Data?.Message}");

                    return (default, resp.Error);
                }

                // 汎用エラーフォールバック（jsonパース失敗ならメッセージに例外メッセージが入る）
                return (default, new ErrorDto { Code = "-1", Message = jsonEx?.Message });
            }
            else
            {
                // 非 2xx（例: 400 Bad Request）の際も API の error を優先
                if (resp?.Error != null)
                {
                    Debug.Log($"[API ERROR] {resp.Error} {resp.Data?.Message}");

                    return (default, resp.Error);
                }

                // 汎用エラーフォールバック（jsonパース失敗ならメッセージに例外メッセージが入る）
                return (default, new ErrorDto { Code = "-1", Message = jsonEx?.Message });
            }
        }

        protected async Task<TokenResponse> SendTokenRequestAsync(UnityWebRequest request)
        {
            Debug.Log($"[UnityWebRequest REQUEST] URL: {request.url}\nMethod: {request.method}\nBody: {(request.uploadHandler is UploadHandlerRaw u ? System.Text.Encoding.UTF8.GetString(u.data) : "(no body)")}\nHeader: Authorization: {request.GetRequestHeader("Authorization")}");

            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            Debug.Log($"[UnityWebRequest RESULT]\nURL: {request.url}\nResult: {request.result}\nError: {request.error}\nResponse Code: {request.responseCode}\nDownload Text: {request.downloadHandler?.text}");

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string json = request.downloadHandler.text;

                    var response = JsonConvert.DeserializeObject<TokenResponse>(json);

                    //　API側でエラーがある場合は Error プロパティに情報が入る

                    return response;
                }
                catch (Exception e)
                {
                    var response = Activator.CreateInstance<TokenResponse>();

                    response.Error = new ErrorDto { Code = "-1", Message = "レスポンス解析失敗: " + e.Message };

                    return response;
                }
            }
            else
            {
                var response = Activator.CreateInstance<TokenResponse>();

                response.Error = new ErrorDto { Code = "-1", Message = "通信失敗: " + request.error };

                return response;
            }
        }

        // 共通POST用ヘルパー
        protected UnityWebRequest CreatePostRequest(string jsonBody, string token = null)
        {
            var request = new UnityWebRequest(endpoint, "POST");

            if (!string.IsNullOrEmpty(jsonBody))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

                request.uploadHandler = new UploadHandlerRaw(bodyRaw);

                request.downloadHandler = new DownloadHandlerBuffer();

                request.SetRequestHeader("Content-Type", "application/json");

                // フォーム送信？
                //request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
                //request.SetRequestHeader("Accept", "application/json");
            }
            else
            {
                // ボディ無しPOST
                // downloadHandlerを設定しないとレスポンスが取得できない
                request.downloadHandler = new DownloadHandlerBuffer();
            }

            if (!string.IsNullOrEmpty(token))
            {
                request.SetRequestHeader("Authorization", $"Bearer {token}");
            }

            return request;
        }

        // 共通GET用ヘルパー
        protected UnityWebRequest CreateGetRequest(string token = null)
        {
            var request = UnityWebRequest.Get(endpoint);

            request.downloadHandler = new DownloadHandlerBuffer();

            if (!string.IsNullOrEmpty(token))
            {
                request.SetRequestHeader("Authorization", $"Bearer {token}");
            }

            return request;
        }

        /// <summary>
        /// アクセストークンの取得を行う。
        /// </summary>
        /// <returns></returns>
        public static async Task<ErrorDto> GetAccessTokenAsync()
        {
            var tokenapi = new TokenApi();

            var authorizationResponse = await tokenapi.AuthorizationTokenAsync();

            if (authorizationResponse.Error == null)
            {
                AuthManager.SetTokenInfo(authorizationResponse);

                return null;
            }

            if (authorizationResponse.Error.Code == "-1")
            {
                authorizationResponse.Error.Code = "1100"; // トークン認証失敗汎用エラー
            }

            return authorizationResponse.Error;
        }

        /// <summary>
        /// アクセストークンの有効性を確認し、必要に応じてリフレッシュを行う
        /// </summary>
        /// <returns></returns>
        public static async Task<ErrorDto> CheckAccessTokenAsync()
        {
            if (AuthManager.IsAccessTokenValid())
            {
                return null;
            }
            else
            {
                if (AuthManager.IsRefreshTokenValid())
                {
                    var tokenapi = new TokenApi();

                    var refreshResponse = await tokenapi.RefreshTokenAsync();

                    if (refreshResponse.Error == null)
                    {
                        AuthManager.SetTokenInfo(refreshResponse);

                        return null;
                    }

                    if (refreshResponse.Error.Code == "-1")
                    {
                        refreshResponse.Error.Code = "1200"; // トークンリフレッシュ失敗汎用エラー
                    }

                    return refreshResponse.Error;
                }

                return new ErrorDto { Code = "1300", Message = "Invalid Token & RefresToken." }; // トークンもリフレッシュトークンも無効
            }
        }
    }
}