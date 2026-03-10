using Landscape2.Runtime.LicenseAuth;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Landscape2.Runtime
{
    public class ErrorModal : BaseAuthPage
    {
        public override string uxmlName => "ErrorModal";

        public ErrorModal(string title = "エラー", string message = "認証に失敗しました。再度ログインをお願いします。", string subMessage = null, string actionLabel = "閉じる", Action onActionButton = null, Action onCloseButton = null) : base()
        {
            var titleLabel = uiRoot.Q<Label>("TitleLabel");

            titleLabel.text = title;

            var messageLabel = uiRoot.Q<Label>("MessageLabel");

            messageLabel.text = message;

            var subMessageLabel = uiRoot.Q<Label>("SubMessageLabel");

            if (string.IsNullOrEmpty(subMessage))
            {
                subMessageLabel.style.display = DisplayStyle.None;
            }
            else
            {
                subMessageLabel.text = subMessage;
            }

            var closeButton = uiRoot.Q<Button>("CloseButton");

            closeButton.RegisterCallback<ClickEvent>(evt =>
            {
                onCloseButton?.Invoke();

                Close();
            });

            var actionButton = uiRoot.Q<Button>("ActionButton");

            // ログインボタンのクリックイベントを登録
            actionButton.RegisterCallback<ClickEvent>(evt =>
            {
                onActionButton?.Invoke();

                Close();
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
    }
}