using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Landscape2.Runtime.UiCommon
{
    public static class ModalUI
    {
        private static VisualElement modalElement;

        /// <summary>
        /// モーダルを表示。
        /// </summary>
        /// <param name="title"></param>
        /// <param name="context"></param>
        /// <param name="isSuccess"></param>
        /// <param name="isFailed"></param>
        /// <param name="onClosed"></param>
        public static void ShowModal(
            string title,
            string context,
            bool isSuccess,
            bool isFailed,
            Action onClosed = null)
        {
            if (modalElement == null)
            {
                modalElement = new UIDocumentFactory().CreateWithUxmlName("Modal");
            }

            modalElement.Q<VisualElement>("Icon_Success").style.display = isSuccess ? DisplayStyle.Flex : DisplayStyle.None;
            modalElement.Q<VisualElement>("Icon_Error").style.display = isFailed ? DisplayStyle.Flex : DisplayStyle.None;

            modalElement.Q<Label>("ModalTitle").text = title;
            modalElement.Q<Label>("ModalText").text = context;
            modalElement.Q<Button>("OKButton").clicked += () =>
            {
                modalElement.style.display = DisplayStyle.None;
                onClosed?.Invoke();
            };
            
            // 表示
            modalElement.style.display = DisplayStyle.Flex;
        }
    }
}