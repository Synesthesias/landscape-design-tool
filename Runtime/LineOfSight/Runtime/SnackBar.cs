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



        public SnackBar(VisualElement rootElement, string target = "CenterUpper")
        {
            if (rootElement.Q<VisualElement>("Snackbar") == null)
            {
                var snackBar = Resources.Load<VisualTreeAsset>("Snackbar");
                snackBarClone = snackBar.CloneTree();
                rootElement.Q<VisualElement>(target).Add(snackBarClone);
            }
            snackBarClone = rootElement.Q<VisualElement>("Snackbar");
            snackBarClone.visible = false;
            snackBarClone.Q<Button>("CloseButton").visible = false;
        }

        public void ShowMessage(string message)
        {
            snackBarClone.Q<Label>("SnackbarText").text = message;
            snackBarClone.visible = true;
            snackBarClone.Q<Button>("CloseButton").visible = true;
        }

        public void Hide()
        {
            HideCore();
        }

        private void HideCore()
        {
            snackBarClone.visible = false;
            snackBarClone.Q<Button>("CloseButton").visible = false;
        }
    }
}
