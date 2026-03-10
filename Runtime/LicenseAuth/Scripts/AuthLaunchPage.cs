using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Landscape2.Runtime.LicenseAuth
{
    public class AuthLaunchPage : BaseAuthPage
    {
        public override string uxmlName => "AuthLaunch";

        public AuthLaunchPage() : base()
        {
            var loginButton = uiRoot.Q<Button>("LoginButton");

            // ログインボタンのクリックイベントを登録
            loginButton.RegisterCallback<ClickEvent>(evt => HandleLoginButtonClicked());
        }

        private void HandleLoginButtonClicked()
        {
            Debug.Log("Login button clicked");

            // ネットワーク接続がない場合の処理
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                var networkErrorModal = new ErrorModal(
                    message: "通信中に接続に失敗しました。インターネット接続されているか確認してください"
                    );

                return;
            }

            var url = AuthManager.CreateAuthorizeUrl();

            Debug.Log("Opening URL: " + url);

            Application.OpenURL(url);

            EnsureDispatcherAndListener();
        }

        private void EnsureDispatcherAndListener()
        {
            var dispatcher = GameObject.FindAnyObjectByType<NamedPipeDispatcher>();

            if (!dispatcher)
            {
                dispatcher = new GameObject("NamedPipeDispatcher").AddComponent<NamedPipeDispatcher>();
            }

            var listener = GameObject.FindAnyObjectByType<NamedPipeListener>();

            if (!listener)
            {
                listener = new GameObject("NamedPipeAuthListener").AddComponent<NamedPipeListener>();
            }

            // 認証完了イベントの破棄
            listener.OnAuthCompleted -= HandleAuthResult;

            listener.OnAuthCompleted += HandleAuthResult;
        }

        /// <summary>
        /// ログイン画面認証結果の処理
        /// </summary>
        /// <param name="result"></param>
        private void HandleAuthResult(string result)
        {
            if (string.IsNullOrEmpty(result) || !AuthManager.ParseLoginResult(result))
            {
                Debug.LogError("起動パラメータ不正：" + result);

                var authErrorModal = new ErrorModal(
                    subMessage: "サポートが必要な場合は以下のコードをお伝えください。<br>ErrorCode:0100",
                    onActionButton: OnErrorModalClosed,
                    onCloseButton: OnErrorModalClosed
                    );

                return;
            }

            Debug.Log($"Auth completed successfully result: {result}");

            // ログインボタンを無効化
            var loginButton = uiRoot.Q<Button>("LoginButton");

            loginButton.SetEnabled(false);

            // トークン取得、ユーザー情報取得、ライセンス検証、アップデート確認
            FetchAuthToken();
        }

        // トークン取得
        private async void FetchAuthToken()
        {
            //TODO トークン取得API変更
            var errorDto = await ApiBase<TokenApi>.GetAccessTokenAsync();

            if (errorDto != null)
            {
                Debug.LogError("トークン取得失敗: " + errorDto.Message);

                var tokenErrorModal = new ErrorModal(
                    subMessage: $"サポートが必要な場合は以下のコードをお伝えください。<br>ErrorCode:{errorDto.Code}",
                    onActionButton: OnErrorModalClosed,
                    onCloseButton: OnErrorModalClosed
                    );

                return;
            }

            // ユーザー情報取得
            FetchUserInfo();
        }

        // ユーザー情報取得
        private async void FetchUserInfo()
        {
            var api = new UserInfoApi();

            var (userData, errorDto) = await api.GetUserInfoAsync();

            if (errorDto != null)
            {
                Debug.LogError("ユーザー取得失敗: " + errorDto.Message);

                var networkErrorModal = new ErrorModal(
                    subMessage: $"サポートが必要な場合は以下のコードをお伝えください。<br>ErrorCode:{errorDto.Code}",
                    onActionButton: OnErrorModalClosed,
                    onCloseButton: OnErrorModalClosed
                    );

                return;
            }

            Debug.Log("ユーザー取得成功");

            LicenseManager.CurrentUser = userData.User;

            ValidateLicense();
        }

        // ライセンス検証
        private async void ValidateLicense()
        {
            LicenseDto currentLicense = null;

            if (LicenseManager.CurrentUser.LicenseKeys.Count == 0)
            {
                Debug.LogError("有効なライセンスが見つかりません");

                var licenseErrorModal = new ErrorModal(
                    message: "有効なライセンスが見つかりません。",
                    subMessage: $"サポートが必要な場合は以下のコードをお伝えください。<br>ErrorCode:3101",
                    onActionButton: OnErrorModalClosed,
                    onCloseButton: OnErrorModalClosed
                    );

                return;
            }

            // 所持ライセンス分の検証を行う

            foreach (var key in LicenseManager.CurrentUser.LicenseKeys)
            {
                var api = new LicenseValidationApi();

                var (licenseData, errorDto) = await api.ValidateLicenseAsync(key);

                if (errorDto != null)
                {
                    Debug.LogError("ライセンス検証失敗: " + errorDto.Message);

                    var networkErrorModal = new ErrorModal(
                        subMessage: $"サポートが必要な場合は以下のコードをお伝えください。<br>ErrorCode:{errorDto.Code}",
                        onActionButton: OnErrorModalClosed,
                        onCloseButton: OnErrorModalClosed
                        );

                    return;
                }

                currentLicense = licenseData.License;

                break;
            }

            // サーバ側でチェックするのでエラーがなければ通す

            Debug.Log("ライセンス検証成功");

            LicenseManager.CurrentLicense = currentLicense;

            // アップデート確認
            CheckUpdate();
        }

        private async void CheckUpdate()
        {
            /*
            var api = new CheckUpdateApi();

            var (updateData, errorDto) = await api.CheckUpdateAsync();

            if (errorDto != null)
            {
                Debug.LogError("アップデートステータス取得失敗: " + errorDto.Message);

                var networkErrorModal = new ErrorModal(
                    subMessage: $"サポートが必要な場合は以下のコードをお伝えください。<br>ErrorCode:{errorDto.Code}"
                    );

                return;
            }

            Debug.Log("アップデートステータス取得成功");

            LicenseManager.CurrentUpdate = updateData.SoftwareVersion;

            // アップデートが必要な場合、アップデートモーダルを表示
            if (LicenseManager.CurrentUpdate.Version != Application.version)
            {
                var updateModal = new UpdateModal(
                    "アップデート",
                    "アップデートがあります、アップデートをしてください",
                    "アップデート開始"
                    );

                return;
            }
            */

            // ログイン情報をキャッシュに保存
            LicenseManager.SaveLoginCache();

            // ランチャーシーンへ遷移
            SceneManager.LoadSceneAsync(AuthManager.LaunchSceneName);
        }

        // エラーモーダルからの復帰
        public void OnErrorModalClosed()
        {
            // ログインボタンを有効化
            var loginButton = uiRoot.Q<Button>("LoginButton");

            loginButton.SetEnabled(true);
        }
    }
}