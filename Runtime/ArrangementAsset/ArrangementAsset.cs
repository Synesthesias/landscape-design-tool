using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
// Build時にAssetをimportする
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;  // Taskを使用するために必要
// UI
using UnityEditor;
using UnityEngine.UIElements;
using Landscape2.Runtime.UiCommon;

using RuntimeHandle;

using UnityEngine.InputSystem;
using System.Linq;
using PlateauToolkit.Sandbox;
using ProceduralToolkit;

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
        private Camera cam;
        private Ray ray;
        private VisualElement arrangementAssetUI;
        private ArrangementAssetUI arrangementAssetUIClass;
        private VisualElement editPanel;
        private LandscapeInputActions.ArrangeAssetActions input;
        private GameObject editTarget;
        private ArrangeMode currentMode;
        private CreateMode createMode;
        private EditMode editMode;

        public ArrangementAsset(VisualElement element, SaveSystem saveSystemInstance, LandscapeCamera landscapeCamera)
        {
            new AssetsSubscribeSaveSystem(saveSystemInstance);
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
                landscapeCamera);
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
            }
            arrangementAssetUIClass.DisplayEditPanel(false);
        }
        public void Update(float deltaTime)
        {
            if (currentMode != null)
            {
                currentMode.Update();
            }

            var isEditMode = currentMode == editMode;
            var isEditingAssetTRS = isEditMode &&
                editMode.RuntimeTransformHandleScript != null &&
                editMode.RuntimeTransformHandleScript.isDragging;
            CameraMoveByUserInput.IsCameraMoveActive = !isEditingAssetTRS;
        }

        public void OnSelect(InputAction.CallbackContext context)
        {
            if (context.performed && arrangementAssetUI.style.display == DisplayStyle.Flex)
            {
                if (currentMode == createMode)
                {
                    createMode.OnSelect();
                }
                else
                {
                    SelectAsset();
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
                    if (selectTarget == editTarget)
                    {
                        // 再度同じtargetを選択しない様にする
                        return;
                    }
                    editTarget = selectTarget;
                    arrangementAssetUIClass.SetEditTarget(editTarget);
                    SetMode(ArrangeModeName.Edit);
                    editMode.CreateRuntimeHandle(editTarget, TransformType.Position);

                    return;
                }
            }

            editTarget = null;
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

        public void OnCancel(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (currentMode != null)
                {
                    if (currentMode == editMode)
                    {
                        arrangementAssetUIClass.ResetEditButton();
                    }
                    currentMode.OnCancel();
                }
                SetMode(ArrangeModeName.Normal);
            }
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
    }
}