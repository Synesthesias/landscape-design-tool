using Landscape2.Runtime.UiCommon;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Landscape2.Runtime.LicenseAuth
{
    public class LicenseAuthPageRouter : ISubComponent
    {
        private BaseAuthPage currentPage = null;

        public LicenseAuthPageRouter()
        {
            var hasCache = LicenseManager.LoadLoginCache();

            var isValid = LicenseManager.IsLicenseValid();

            AuthManager.IsSkipLogin = hasCache && isValid;

            if (AuthManager.IsSkipLogin)
            {
                // ログインキャッシュが存在し、有効期限内の場合は、メインシーンをロード
                SceneManager.LoadSceneAsync(AuthManager.LaunchSceneName);
            }
            else
            {
                // ログインキャッシュが存在しない場合は、AuthLaunchPageを表示
                ChangePage(new AuthLaunchPage());
            }
        }

        public void ChangePage(BaseAuthPage newPage)
        {
            currentPage?.OnDisable();
            currentPage = newPage;
            currentPage.pageChange += ChangePage;
            currentPage.OnEnable();
        }

        public void Update(float deltaTime)
        {
        }

        public void LateUpdate(float deltaTime)
        {
        }

        public void OnEnable()
        {
        }

        public void OnDisable()
        {
        }

        public void Start()
        {
        }
    }
}