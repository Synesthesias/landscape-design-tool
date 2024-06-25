using Landscape2.Runtime.UiCommon;
using UnityEngine;
using UnityEngine.UIElements;

namespace Landscape2.Runtime.CameraPositionMemory
{
    /// <summary>
    /// カメラ位置の記憶と復元に関するUIと機能を管理します。
    /// ボタン押下時のメイン処理は <see cref="CameraPositionMemory"/>に委譲します。
    /// </summary>
    public class CameraPositionMemoryUI : ISubComponent
    {
        private readonly CameraPositionMemory cameraPositionMemory;
        private readonly ButtonWithClickMessage[] saveButtons; // 添字はスロットID
        private readonly ButtonWithClickMessage[] restoreButtons; // 添字はスロットID
        private readonly Button saveResetButton;
        private float timeToResetSaveButton;
        private float timeToResetRestoreButton;
        private readonly RenameCameraSlotUI renameCameraSlotUI;

        private const string UINameCameraRoot = "CameraPosition";
        private const string UINameCameraSaveButton = "CameraSaveSlot";
        private const string UINameCameraRestoreButton = "CameraRestoreSlot";
        private const string UINameSlotRenameButton = "CameraRenameSlot";
        private const string UINameResetSaveButton = "ResetCameraSaveButton";
        
        public CameraPositionMemoryUI(CameraPositionMemory cameraPositionMemory)
        {
            this.cameraPositionMemory = cameraPositionMemory;
            this.renameCameraSlotUI = new RenameCameraSlotUI(cameraPositionMemory, this);

            var uiRoot = new UIDocumentFactory().CreateWithUxmlName("UICameraPositionMemory");

            int slotCount = CameraPositionMemory.SlotCount;
            saveButtons = new ButtonWithClickMessage[slotCount];
            restoreButtons = new ButtonWithClickMessage[slotCount];
            var cameraUiRoot = uiRoot.Q(UINameCameraRoot);
            var cameraTab = new TabUI(cameraUiRoot);

            // 保存ボタンの機能を構築
            for (int i = 0; i < slotCount; i++)
            {
                string saveButtonName = UINameCameraSaveButton + (i+1);
                int slotIndex = i;
                var buttonUi = cameraUiRoot.Q<Button>(saveButtonName);
                buttonUi.focusable = false;
                saveButtons[i] = new ButtonWithClickMessage(
                    buttonUi,
                    "保存しました！",
                    () => OnClickedSaveButton(slotIndex) // ここに i を渡してはいけないことに注意
                );
                
            }

            // 復元ボタンの機能を構築
            for (int i = 0; i < slotCount; i++)
            {
                string restoreButtonName = UINameCameraRestoreButton + (i+1);
                int slotIndex = i;
                var buttonUi = cameraUiRoot.Q<Button>(restoreButtonName);
                buttonUi.focusable = false;
                restoreButtons[i] = new ButtonWithClickMessage(
                    buttonUi,
                    "復元しました！",
                    () => OnClickedRestoreButton(slotIndex)
                );
            }
            
            // 名前変更ボタンの機能を構築
            for (int i = 0; i < slotCount; i++)
            {
                string renameButtonName = UINameSlotRenameButton + (i + 1);
                int slotIndex = i;
                var buttonUis = cameraUiRoot.Query<Button>(renameButtonName);
                buttonUis.ForEach(button =>
                {
                    button.focusable = false;
                    button.clicked += () => OnClickedRenameButton(slotIndex);
                });
            }

            //セーブデータリセットボタンの機能を構築
            saveResetButton = uiRoot.Q<Button>(UINameResetSaveButton);
            saveResetButton.focusable = false;
            saveResetButton.clicked += OnClickedResetSaveButton; 
            
            UpdateButtonState();
        }

        public void Start()
        {

        }

        public void Update(float deltaTime)
        {
            foreach (var b in saveButtons)
            {
                b.Update(deltaTime);
            }

            foreach (var b in restoreButtons)
            {
                b.Update(deltaTime);
            }
        }

        public void OnEnable()
        {
        }

        public void OnDisable()
        {
        }

        /// <summary>
        /// 「カメラ位置を記憶」ボタンが押された時、記憶してボタンのテキストを変える
        /// </summary>
        private void OnClickedSaveButton(int slotId)
        {
            cameraPositionMemory.Save(slotId);
            UpdateButtonState();
        }

        /// <summary>
        /// 「カメラ位置を復元」ボタンが押された時、復元してボタンのテキストを変える
        /// </summary>
        private void OnClickedRestoreButton(int slotId)
        {
            cameraPositionMemory.Restore(slotId);
            UpdateButtonState();
        }

        private void OnClickedRenameButton(int slotId)
        {
            renameCameraSlotUI.Open(slotId, cameraPositionMemory.GetName(slotId));
        }

        /// <summary>
        /// 保存データがあるかどうかによってボタンのテキストとスタイルを切り替えます
        /// </summary>
        public void UpdateButtonState()
        {
            int slotCount = CameraPositionMemory.SlotCount;
            
            // 保存ボタンのテキスト変更
            for (int i = 0; i < slotCount; i++)
            {
                string text = cameraPositionMemory.GetName(i);
                if (cameraPositionMemory.IsSaved(i))
                {
                    text += "(上書き)";
                }
                else
                {
                    text += "(新規)";
                }

                saveButtons[i].NormalButtonText = text;
            }
            
            // 復元ボタンのテキスト変更
            for (int i = 0; i < slotCount; i++)
            {
                string text = cameraPositionMemory.GetName(i);
                var button = restoreButtons[i];
                var buttonUi = button.Button;
                if (cameraPositionMemory.IsSaved(i))
                {
                    buttonUi.SetEnabled(true);
                }
                else
                {
                    text += "(未保存)";
                    buttonUi.SetEnabled(false);
                }
                button.NormalButtonText = text;
            }
        }

        /// <summary>
        /// 「カメラ保存をリセット」ボタンが押された時
        /// </summary>
        private void OnClickedResetSaveButton()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            cameraPositionMemory.LoadPersistenceDataOrDefault();
            UpdateButtonState();
        }
    }
}
