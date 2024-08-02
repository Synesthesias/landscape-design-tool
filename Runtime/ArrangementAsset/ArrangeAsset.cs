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

namespace Landscape2.Runtime
{
    // CreateAsset,EditAsset,DeleteAssetクラスの基底クラス
    public abstract class ArrangeMode
    {
        public virtual void OnCancel() {}
        public abstract void Update();
    }

    public enum ArrangeModeName
    {
        Normal,
        Create,
        Edit
    }

    public class ArrangeAsset : ISubComponent,LandscapeInputActions.IArrangeAssetActions
    {
        private Camera cam;
        private Ray ray;
        private VisualElement arrangementAssetUI;
        private VisualElement editPanel;
        private LandscapeInputActions.ArrangeAssetActions input;
        private GameObject editTarget;

        private ArrangeMode currentMode;
        private CreateMode createMode;
        private EditMode editMode;

        public ArrangeAsset(VisualElement element)
        {
            createMode = new CreateMode();
            editMode = new EditMode();
            // ボタンの登録
            arrangementAssetUI = element;
            editPanel = arrangementAssetUI.Q<VisualElement>("EditPanel");
            var moveButton = editPanel.Q<RadioButton>("MoveButton");
            moveButton.RegisterCallback<ClickEvent>(evt =>
            {    
                editMode.CreateRuntimeHandle(editTarget,TransformType.Position);
            });
            var rotateButton = editPanel.Q<RadioButton>("RotateButton");
            rotateButton.RegisterCallback<ClickEvent>(evt =>
            {    
                editMode.CreateRuntimeHandle(editTarget,TransformType.Rotation);
            });
            var scaleButton = editPanel.Q<RadioButton>("ScaleButton");
            scaleButton.RegisterCallback<ClickEvent>(evt =>
            {    
                editMode.CreateRuntimeHandle(editTarget,TransformType.Scale);
            });
            var deleteButton = editPanel.Q<Button>("ContextButton");
            deleteButton.clicked += () =>
            {
                editMode.DeleteAsset(editTarget);
                editPanel.style.display = DisplayStyle.None;
            };
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
            SetPlateauAssets("Tree_Assets","AssetCategory_Tree");
            SetPlateauAssets("Humans_Assets","AssetCategory_Human");
            SetPlateauAssets("Information_Assets","AssetCategory_Info");
            SetPlateauAssets("Advertisement_Assets","AssetCategory_Ad");
            SetPlateauAssets("PublicFacilities_Assets","AssetCategory_Public");
            SetPlateauAssets("RoadSign_Assets","AssetCategory_Sign");
            SetPlateauAssets("Vehicle_Assets","AssetCategory_Vehicle");
            SetPlateauAssets("StreetLight_Assets","AssetCategory_Light");
            SetPlateauAssets("Other_Assets","AssetCategory_Other");

            AsyncOperationHandle<GameObject> runtimeHandle = Addressables.LoadAssetAsync<GameObject>("RuntimeTransformHandle_Assets");
            GameObject runtimeTransformHandle = await runtimeHandle.Task;
            AsyncOperationHandle<GameObject> customPassHandle = Addressables.LoadAssetAsync<GameObject>("CustomPass");
            GameObject customPass = await customPassHandle.Task;
            GameObject.Instantiate(customPass);
            AsyncOperationHandle<IList<GameObject>> plateauAssetHandle = Addressables.LoadAssetsAsync<GameObject>("Tree_Assets", null);
            IList<GameObject> treeAssetsList = await plateauAssetHandle.Task;
            CreateButton(treeAssetsList);
        }

        private async void SetPlateauAssets(string keyName,string buttonName)
        {
            AsyncOperationHandle<IList<GameObject>> plateauAssetHandle = Addressables.LoadAssetsAsync<GameObject>(keyName, null);
            IList<GameObject> assetsList = await plateauAssetHandle.Task;
            var assetCategory = arrangementAssetUI.Q<RadioButton>(buttonName);
            assetCategory.RegisterCallback<ClickEvent>(evt =>
            {    
                CreateButton(assetsList);
            });
        }

        private void CreateButton(IList<GameObject> assetList)
        {
            var assetListScrollView = arrangementAssetUI.Q<ScrollView>("AssetListScrollView");
            assetListScrollView.Clear();
            VisualElement flexContainer = new VisualElement();
            foreach(GameObject asset in assetList)
            {
                Button newButton = new Button()
                {
                    text = asset.name,
                    name = asset.name // ボタンに名前を付ける
                };
                newButton.AddToClassList("AssetButton");
                newButton.clicked += () => 
                {
                    SetMode(ArrangeModeName.Create);
                    createMode.SetAsset(asset.name, assetList);
                };
                flexContainer.Add(newButton);
            }
            assetListScrollView.Add(flexContainer);
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
            else if(mode == ArrangeModeName.Edit)
            {
                currentMode = editMode;
                editPanel.style.display = DisplayStyle.Flex;
                return;
            }
            else if(mode == ArrangeModeName.Normal)
            {
                currentMode = null;
            }
            editPanel.style.display = DisplayStyle.None;
        }

        public void Update(float deltaTime)
        {
            if(currentMode != null)
            {
                currentMode.Update();
            }
        }

        public void OnSelect(InputAction.CallbackContext context)
        {
            if(context.performed)
            {
                if(currentMode == createMode)
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
            if (Physics.Raycast(ray, out RaycastHit hit,Mathf.Infinity))
            {
                if (CheckParentName(hit.transform,"CreatedAssets"))
                {
                    SetMode(ArrangeModeName.Edit);
                    editTarget = FindAssetComponent(hit.transform);
                    editMode.CreateRuntimeHandle(editTarget,TransformType.Position);
                }
            }
        }

        private bool CheckParentName(Transform hitTransform,string parentName)
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
        // 右クリック
        public void OnCancel(InputAction.CallbackContext context)
        {
            if(context.performed)
            {
                if(currentMode != null)
                {
                    currentMode.OnCancel();
                }
                SetMode(ArrangeModeName.Normal);
            }
        }
        
        public void OnDisable()
        {
            input.Disable();
        }

        public void Start()
        {
        }
    }
}