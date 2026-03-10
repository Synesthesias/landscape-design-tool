using Landscape2.Runtime.LicenseAuth;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Landscape2.Runtime
{
    public class UpdateModal : BaseAuthPage
    {
        public override string uxmlName => "UpdateModal";

        public UpdateModal(string title, string message, string actionLabel, Action onActionButton = null, Action onCloseButton = null) : base()
        {
            var titleLabel = uiRoot.Q<Label>("TitleLabel");

            titleLabel.text = title;

            var messageLabel = uiRoot.Q<Label>("MessageLabel");

            messageLabel.text = message;

            var actionButtonLabel = uiRoot.Q<Label>("ActionButtonLabel");

            actionButtonLabel.text = actionLabel;

            var closeButton = uiRoot.Q<Button>("CloseButton");

            closeButton.RegisterCallback<ClickEvent>(evt =>
            {
                // 強制アップデート

                //onCloseButton?.Invoke();

                //Close();
            });

            var actionButton = uiRoot.Q<Button>("ActionButton");

            // アップデートボタンのクリックイベントを登録
            actionButton.RegisterCallback<ClickEvent>(evt =>
            {
                actionButton.SetEnabled(false);

                ExecuteUpdate();
            });
        }

        public void Close()
        {
            var uiDocument = GameObject.FindObjectsOfType<UIDocument>();

            foreach (var doc in uiDocument)
            {
                if (doc.rootVisualElement == uiRoot)
                {
                    GameObject.DestroyImmediate(doc.gameObject);
                }
            }
        }

        // アップデート開始
        private async void ExecuteUpdate()
        {
            // ローカルサーバでホスト python -m http.server 8080
            //var updater = new UpdaterService("http://localhost:8080/KeikanSetup.exe");

            var updater = new UpdaterService(LicenseManager.CurrentUpdate.FileUrl);

            updater.OnProgressChanged += progress =>
            {
                Debug.Log($"Update progress: {progress * 100}%");

                var messageLabel = uiRoot.Q<Label>("MessageLabel");

                messageLabel.text = $"アップデート中: {progress * 100:F2}%";
            };

            updater.OnDownloadCompleted += () =>
            {
                Debug.Log("Update download completed");

                updater.LaunchInstaller();
            };

            updater.OnError += error =>
            {
                Debug.LogError($"Update error: {error}");

                var messageLabel = uiRoot.Q<Label>("MessageLabel");

                messageLabel.text = "リトライします。";

                var actionButtonLabel = uiRoot.Q<Label>("ActionButtonLabel");

                actionButtonLabel.text = "再試行";

                var actionButton = uiRoot.Q<Button>("ActionButton");

                actionButton.SetEnabled(true);
            };

            await updater.StartUpdateAsync();
        }
    }
}