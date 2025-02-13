using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Landscape2.Runtime
{
    public class SnackBar
    {
        private VisualElement center;

        protected VisualElement snackBarClone; // Snackbarのクローン

        protected Button closeButton;



        public SnackBar(VisualElement rootElement, string target = "CenterUpper")
        {
            if (rootElement.Q<VisualElement>("Snackbar") == null)
            {
                var snackBar = Resources.Load<VisualTreeAsset>("Snackbar");
                snackBarClone = snackBar.CloneTree();
                rootElement.Q<VisualElement>(target).Add(snackBarClone);
            }

            snackBarClone = rootElement.Q<VisualElement>("Snackbar");
            closeButton = snackBarClone.Q<Button>("CloseButton");

            closeButton.clicked += () =>
            {
                Hide();
            };

            Hide();
        }

        public void ShowMessage(string message)
        {
            snackBarClone.Q<Label>("SnackbarText").text = message;
            snackBarClone.visible = true;
            closeButton.visible = true;
        }

        public void Hide()
        {
            HideCore();
        }

        private void HideCore()
        {
            snackBarClone.visible = false;
            closeButton.visible = false;
            // snackBarClone.Q<Button>("CloseButton").visible = false;
        }
    }
}
