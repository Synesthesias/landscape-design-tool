
using Landscape2.Runtime.UiCommon;
using Landscape2.Runtime.UiCommon;
using UnityEngine.UIElements;

namespace Landscape2.Runtime.CameraPositionMemory
{
    /// <summary>
    /// カメラ位置保存スロットをリネームするためのUIです。
    /// </summary>
    public class RenameCameraSlotUI
    {
        private int slotId;
        private string prevSlotName;

        private VisualElement uiRoot;
        private Label prevSlotNameLabel;
        private TextField nextSlotNameTextField;
        private Button okButton;
        private Button cancelButton;
        private Label labelWarningNameLength;

        private CameraPositionMemory cameraPositionMemory;
        private CameraPositionMemoryUI cameraPositionMemoryUi;

        private const int MaxNameLength = 10;
        
        public RenameCameraSlotUI(CameraPositionMemory cameraPositionMemoryArg, CameraPositionMemoryUI cameraPositionMemoryUiArg)
        {
            cameraPositionMemory = cameraPositionMemoryArg;

            uiRoot = new UIDocumentFactory().CreateWithUxmlName("UIRenameCameraSlot");
            
            prevSlotNameLabel = uiRoot.Q<Label>("PrevSlotNameLabel");
            nextSlotNameTextField = uiRoot.Q<TextField>("TextFieldSlotName");
            okButton = uiRoot.Q<Button>("OkButton");
            cancelButton = uiRoot.Q<Button>("CancelButton");
            labelWarningNameLength = uiRoot.Q<Label>("LabelWarningNameLength");

            okButton.clicked += OnClickedOkButton;
            cancelButton.clicked += OnClickedCancelButton;
            nextSlotNameTextField.RegisterValueChangedCallback(OnChangedNameField);

            cameraPositionMemoryUi = cameraPositionMemoryUiArg;
            
            HideWindow();
        }

        public void Open(int slotIdArg, string prevSlotNameArg)
        {
            slotId = slotIdArg;
            prevSlotName = prevSlotNameArg;
            prevSlotNameLabel.text = prevSlotNameArg;
            nextSlotNameTextField.value = prevSlotNameArg;
            ShowWindow();
        }

        private void OnClickedOkButton()
        {
            var prevSlot = cameraPositionMemory.GetSlotData(slotId);
            string nextSlotName = nextSlotNameTextField.value;
            cameraPositionMemory.SetSlotData(slotId, new SlotData(prevSlot.Position, prevSlot.Rotation, prevSlot.IsSaved, nextSlotName));
            
            cameraPositionMemoryUi.UpdateButtonState();
            HideWindow();
        }

        private void OnClickedCancelButton()
        {
            HideWindow();
        }

        private void OnChangedNameField(ChangeEvent<string> e)
        {
            // 文字数制限の警告を表示
            int length = e.newValue.Length;
            if (length > MaxNameLength)
            {
                labelWarningNameLength.style.display = DisplayStyle.Flex;
                labelWarningNameLength.text = $"{MaxNameLength}文字以内で入力してください(現在{length}文字)";
                okButton.SetEnabled(false);
            }
            else
            {
                labelWarningNameLength.style.display = DisplayStyle.None;
                okButton.SetEnabled(true);
            }
        }

        private void ShowWindow()
        {
            uiRoot.style.display = DisplayStyle.Flex;
            // 名前入力でカメラが動いてしまうのを防ぎます
            CameraMoveByUserInput.IsKeyboardActive = false;
        }

        private void HideWindow()
        {
            uiRoot.style.display = DisplayStyle.None;
            CameraMoveByUserInput.IsKeyboardActive = true;
        }
    }
    
}