using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Landscape2.Runtime
{
    /// <summary>
    /// グローバルナビの機能
    /// </summary>
    public class GlobalNaviFunctionsUI
    {
        private Button menuButton;
        private VisualElement functionsElement;

        private Toggle walkModeToggle;

        private Button screenCaptureButton;

        public bool IsOnWalkMode => walkModeToggle.value;
        public UnityEvent<bool> OnToggleWalkMode = new();

        public UnityEvent OnClickScreenCapture = new();

        public GlobalNaviFunctionsUI(VisualElement element)
        {
            menuButton = element.Q<Button>("Btn_Humberger");
            functionsElement = element.Q<VisualElement>("FunctionContainer");

            walkModeToggle = functionsElement.Q<Toggle>("Toggle_WalkMode");
            screenCaptureButton = functionsElement.Q<Button>($"Button_Capture");

            RegisterEvents();
            ShowFunctions(false);
        }

        private void RegisterEvents()
        {
            menuButton.clicked += () =>
            {
                if (functionsElement.style.display == DisplayStyle.None)
                {
                    ShowFunctions(true);
                }
                else
                {
                    ShowFunctions(false);
                }
            };

            // ScreenShot
            screenCaptureButton.clicked += () =>
            {
                OnClickScreenCapture.Invoke();
            };


            // 歩行モード
            walkModeToggle.RegisterValueChangedCallback((value) =>
            {
                OnToggleWalkMode.Invoke(value.newValue);
            });
        }

        public void ShowFunctions(bool isShow)
        {
            functionsElement.style.display = isShow ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}