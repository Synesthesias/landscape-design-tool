using Landscape2.Runtime.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
// Build時にAssetをimportする
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
// UI
using UnityEngine.UIElements;

using UnityEngine.InputSystem;
using System.Linq;
using PlateauToolkit.Sandbox;

namespace Landscape2.Runtime
{
    // CreateAsset,EditAsset,DeleteAssetクラスの基底クラス
    public abstract class ArrangeMode
    {
        public virtual void OnCancel() { }
        public abstract void Update();
    }

    public enum ArrangeModeName
    {
        Normal,
        Create,
        Edit
    }

    public class ArrangementAsset : ISubComponent, LandscapeInputActions.IArrangeAssetActions
    {
        const float MouseDragMagnitudeThreashold = 10f;
        private Camera cam;
        private Ray ray;
        private VisualElement arrangementAssetUI;
        private ArrangementAssetUI arrangementAssetUIClass;

        private ArrangementAssetSizeUI sizeUI;
        private VisualElement editPanel;
        private LandscapeInputActions.ArrangeAssetActions input;
        private GameObject editTarget;
        private GameObject lastEditTarget;
        private ArrangeMode currentMode;
        private CreateMode createMode;
        private EditMode editMode;

        private GameObject activeTarget; // editTargetはactiveTargetにしてもいいかも知れない

        Vector2? mousePosition;



        public ArrangementAsset(VisualElement element, SaveSystem saveSystemInstance, LandscapeCamera landscapeCamera)
        {
            var subscribeSaveSystem = new AssetsSubscribeSaveSystem(saveSystemInstance);
            createMode = new CreateMode();
            editMode = new EditMode();
            // ボタンの登録
            arrangementAssetUI = element;
            arrangementAssetUI.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            arrangementAssetUIClass = new ArrangementAssetUI(
                arrangementAssetUI,
                this, createMode,
                editMode,
                new AdvertisementRenderer(),
                landscapeCamera,
                subscribeSaveSystem);

            sizeUI = new();
            sizeUI.Show(false);
            
            // プロジェクトからの通知イベント
            subscribeSaveSystem.SaveLoadHandler.OnDeleteAssets.AddListener(OnDeleteAssets);
            subscribeSaveSystem.SaveLoadHandler.OnChangeEditableState.AddListener(OnChangeEditableState);
        }

        public async void OnEnable()
        {
            // 作成するAssetの親オブジェクトの作成
            GameObject createdAssets = new GameObject("CreatedAssets");
            // インプットシステム
            input = new LandscapeInputActions.ArrangeAssetActions(new LandscapeInputActions());
            input.SetCallbacks(this);
            input.Enable();
            // アセットのロード
            SetPlateauAssets(ArrangementAssetType.Plant.GetKeyName(), ArrangementAssetType.Plant.GetButtonName());
            SetPlateauAssets(ArrangementAssetType.Human.GetKeyName(), ArrangementAssetType.Human.GetButtonName());
            SetPlateauAssets(ArrangementAssetType.Building.GetKeyName(), ArrangementAssetType.Building.GetButtonName());
            SetPlateauAssets(ArrangementAssetType.Advertisement.GetKeyName(), ArrangementAssetType.Advertisement.GetButtonName());
            SetPlateauAssets(ArrangementAssetType.Sign.GetKeyName(), ArrangementAssetType.Sign.GetButtonName());
            SetPlateauAssets(ArrangementAssetType.Vehicle.GetKeyName(), ArrangementAssetType.Vehicle.GetButtonName());
            SetPlateauAssets(ArrangementAssetType.StreetFurniture.GetKeyName(), ArrangementAssetType.StreetFurniture.GetButtonName());
            SetPlateauAssets(ArrangementAssetType.Miscellaneous.GetKeyName(), ArrangementAssetType.Miscellaneous.GetButtonName());

            AsyncOperationHandle<GameObject> runtimeHandle = Addressables.LoadAssetAsync<GameObject>("RuntimeTransformHandle_Assets");
            GameObject runtimeTransformHandle = await runtimeHandle.Task;
            AsyncOperationHandle<GameObject> customPassHandle = Addressables.LoadAssetAsync<GameObject>("CustomPass");
            GameObject customPass = await customPassHandle.Task;
            GameObject.Instantiate(customPass);
            // 初期画面
            AsyncOperationHandle<IList<GameObject>> plateauAssetHandle = Addressables.LoadAssetsAsync<GameObject>(ArrangementAssetType.Plant.GetKeyName(), null);
            IList<GameObject> treeAssetsList = await plateauAssetHandle.Task;
            AsyncOperationHandle<IList<Texture2D>> assetsPictureHandle = Addressables.LoadAssetsAsync<Texture2D>("AssetsPicture", null);
            IList<Texture2D> assetsPicture = await assetsPictureHandle.Task;
            arrangementAssetUIClass.CreateButton(treeAssetsList, assetsPicture);

            // インポートボタンの登録
            arrangementAssetUIClass.RegisterImportButtonAction();

            sizeUI.Show(false);
        }

        private async void SetPlateauAssets(string keyName, string buttonName)
        {
            AsyncOperationHandle<IList<GameObject>> plateauAssetHandle = Addressables.LoadAssetsAsync<GameObject>(keyName, null);
            IList<GameObject> assetsList = await plateauAssetHandle.Task;
            assetsList = assetsList.Where(n => n.GetComponent<PlateauSandboxPlaceableHandler>() != null).ToList();

            AsyncOperationHandle<IList<Texture2D>> assetsPictureHandle = Addressables.LoadAssetsAsync<Texture2D>("AssetsPicture", null);
            IList<Texture2D> assetsPicture = await assetsPictureHandle.Task;
            arrangementAssetUIClass.RegisterCategoryPanelAction(buttonName, assetsList, assetsPicture);
        }

        private bool UpdateTargetDrag()
        {
            bool isMouseDragging = false;
            var mouse = Mouse.current;
            if (editTarget == null)
            {
                mousePosition = null;
                return false;
            }

            // 本来であればInputActionsのselectを使用して押下状態を取ってくるのが適切の筈。
            // しかし、InputActionではDragを取るのにhackeyな事を行わなければならない為直接入力状態を取得する
            if (mouse.leftButton.isPressed)
            {
                if (mousePosition == null)
                {
                    mousePosition = mouse.position.ReadValue();
                }
                ray = cam.ScreenPointToRay(Input.mousePosition);

                if (
                    editMode.RuntimeTransformHandleScript == null ||
                    (editMode.RuntimeTransformHandleScript != null && !editMode.RuntimeTransformHandleScript.isMouseOverHandle)
                )
                {
                    if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ~LayerMask.GetMask(LayerMask.LayerToName(editTarget.layer))))
                    {
                        var mouseDelta = mouse.position.ReadValue() - mousePosition.Value;
                        var targetDelta = editTarget.transform.position - hit.point;

                        // クリックした場所とtargetの位置がずれている
                        if (0 < targetDelta.magnitude)
                        {
                            // 移動したら反映
                            if (MouseDragMagnitudeThreashold < mouseDelta.magnitude)
                            {
                                editTarget.transform.position = hit.point;
                            }
                        }
                        else
                        {
                            editTarget.transform.position = hit.point;
                        }

                    }
                }
                isMouseDragging = true;
            }
            else
            {
                mousePosition = null;
            }

            return isMouseDragging;
        }

        public void SetMode(ArrangeModeName mode)
        {
            if (currentMode != null)
            {
                currentMode.OnCancel();
            }

            if (mode == ArrangeModeName.Create)
            {
                currentMode = createMode;
                createMode.OnEnable(arrangementAssetUI);
            }
            else if (mode == ArrangeModeName.Edit)
            {
                currentMode = editMode;
                arrangementAssetUIClass.DisplayEditPanel(true);
                return;
            }
            else if (mode == ArrangeModeName.Normal)
            {
                currentMode = null;

                editTarget = null;
                lastEditTarget = null;
            }
            arrangementAssetUIClass.DisplayEditPanel(false);
        }
        public void Update(float deltaTime)
        {
            if (arrangementAssetUI.style.display != DisplayStyle.Flex)
            {
                return;
            }

            bool isMouseDragging = false;


            if (currentMode != null)
            {
                currentMode.Update();
            }

            if (currentMode == editMode)
            {
                isMouseDragging = UpdateTargetDrag();
            }

            var isEditMode = currentMode == editMode;
            var isEditingAssetTRS = isEditMode & isMouseDragging;
            if (!isMouseDragging)
            {
                var handleDragging = editMode.RuntimeTransformHandleScript != null && editMode.RuntimeTransformHandleScript.isDragging;
                isEditingAssetTRS = isEditMode & handleDragging;
            }
            CameraMoveByUserInput.IsCameraMoveActive = !isEditingAssetTRS;

            if (activeTarget != null)
            {
                sizeUI.SetTarget(activeTarget);
            }
            sizeUI.Show(activeTarget != null);
            sizeUI.Update(deltaTime);
        }

        public void OnSelect(InputAction.CallbackContext context)
        {
            if (arrangementAssetUI.style.display != DisplayStyle.Flex)
            {
                return;
            }
            if (context.performed)
            {
                if (currentMode == createMode)
                {
                    createMode.OnSelect();
                    activeTarget = createMode.selectedAsset;
                }
                else
                {
                    SelectAsset();
                    activeTarget = editTarget;
                }

            }
            else if (context.canceled)
            {
                if (editTarget != null)
                {
                    editTarget = null;
                }
                if (activeTarget != null)
                {
                    activeTarget = null;
                }
            }
        }

        private void SelectAsset()
        {
            cam = Camera.main;
            ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                if (CheckParentName(hit.transform, "CreatedAssets"))
                {
                    var selectTarget = FindAssetComponent(hit.transform);

                    if (selectTarget == lastEditTarget)
                    {
                        // 再度同じtargetを選択しない様にする
                        return;
                    }
                    
                    if (!ProjectSaveDataManager.TryCheckData(
                            ProjectSaveDataType.Asset,
                            ProjectSaveDataManager.ProjectSetting.CurrentProject.projectID,
                            selectTarget?.GetInstanceID().ToString()))
                    {
                        // プロジェクト外であれば、抜ける
                        return;
                    }
                    
                    editTarget = selectTarget;
                    lastEditTarget = editTarget;
                    arrangementAssetUIClass.SetEditTarget(editTarget);
                    SetMode(ArrangeModeName.Edit);
                    editMode.CreateRuntimeHandle(editTarget, TransformType.None);

                    editMode.CreateRuntimeHandle(editTarget, TransformType.Position);
                    
                    // ハンドルが表示されたらプロジェクトが編集中として扱う
                    ProjectSaveDataManager.Edit(ProjectSaveDataType.Asset, editTarget.GetInstanceID().ToString());
                    
                    return;
                }
            }

            if (editTarget != null)
            {
                editTarget = null;
            }
        }

        private bool CheckParentName(Transform hitTransform, string parentName)
        {
            Transform current = hitTransform;
            while (current != null)
            {
                if (current.name == parentName)
                {
                    return true;
                }
                current = current.parent;
            }
            return false;
        }

        private GameObject FindAssetComponent(Transform target)
        {
            Transform current = target;
            while (current != null)
            {
                if (current.parent.name == "CreatedAssets")
                {
                    return current.gameObject;
                }
                current = current.parent;
            }
            return null;
        }

        /// <summary>
        /// 右クリック時
        /// </summary>
        /// <param name="context"></param>
        public void OnCancel(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                ResetCurrentMode();
            }
        }

        private void ResetCurrentMode()
        {
            if (currentMode != null)
            {
                if (currentMode == editMode)
                {
                    arrangementAssetUIClass.ResetEditButton();
                }
                currentMode.OnCancel();
                activeTarget = null;
            } 
            SetMode(ArrangeModeName.Normal);
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (arrangementAssetUI.resolvedStyle.display == DisplayStyle.None)
            {
                SetMode(ArrangeModeName.Normal);
            }
        }
        public void OnDisable()
        {
            SetMode(ArrangeModeName.Normal);
            arrangementAssetUI.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            input.Disable();
        }

        public void Start()
        {
        }

        public void LateUpdate(float deltaTime)
        {
        }

        
        private void OnDeleteAssets(List<GameObject> deleteAssets)
        {
            foreach (var asset in deleteAssets)
            {
                GameObject.Destroy(asset);
            }
        }
        
        private void OnChangeEditableState(List<GameObject> editableAssets, List<GameObject> nonEditableAssets)
        {
            ResetCurrentMode();
            
            // 操作可能なアセットと操作不可能なアセットのレイヤーを変更
            foreach (var asset in editableAssets)
            {
                LayerMaskUtil.SetIgnore(asset.gameObject, false);
            }
            foreach (var asset in nonEditableAssets)
            {
                LayerMaskUtil.SetIgnore(asset.gameObject, true);
            }
        }
    }
}