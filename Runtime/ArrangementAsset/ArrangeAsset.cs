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
        public virtual void OnEnable(VisualElement element) {}
        public virtual void OnDisable() {}
        public abstract void Update();
        public virtual void OnSelect(){}
        public virtual void OnCancel(){}
    }
    public class ArrangeAsset : ISubComponent,LandscapeInputActions.IArrangeAssetActions
    {
        private VisualElement arrangementAssetUI;
        private const string UINameRoot = "AssetArrangeContent";
        private LandscapeInputActions.ArrangeAssetActions input;

        private ArrangeMode currentMode;
        private CreateMode createMode;
        private EditMode editMode;
        private DeleteMode deleteMode;
        public ArrangeAsset()
        {
            createMode = new CreateMode();
            editMode = new EditMode();
            deleteMode = new DeleteMode();

            arrangementAssetUI = new UIDocumentFactory().CreateWithUxmlName("ArrangementAssetUI");
            // Create Edit Deleteボタンの機能
            var assetArrangeContent = arrangementAssetUI.Q(UINameRoot);
            var assetTab = new TabUI(assetArrangeContent);
            var tabs = arrangementAssetUI.Q<VisualElement>("tabs");
            foreach(var child in tabs.Children())
            {
                if (child is Button tab)
                {
                    tab.clicked += () => 
                    {
                        SetMode(tab.name);
                    };
                }
            }
            // Position Rotation Scaleボタンの機能
            var editTable =  arrangementAssetUI.Q<VisualElement>("EditTable");
            foreach(var child in editTable.Children())
            {
                if (child is Button tab)
                {
                    tab.clicked += () => 
                    {
                        editMode.SetTransformType(tab.name);
                    };
                }
            }

            Button deleteButton = arrangementAssetUI.Q<Button>("DeleteButton");
            deleteButton.clicked += () => 
            {
                deleteMode.DeleteAsset();
            };
        }

        public async void OnEnable()
        {
            // 作成するAssetの親オブジェクトの作成
            GameObject createdAsset = new GameObject("CreatedAssets");
            // ユーザーの操作を受け取る準備
            input = new LandscapeInputActions.ArrangeAssetActions(new LandscapeInputActions());
            input.SetCallbacks(this);
            input.Enable();
            // アセットの取得
            AsyncOperationHandle<IList<GameObject>> assetHandle = Addressables.LoadAssetsAsync<GameObject>("PlateauProps_Assets", null);
            IList<GameObject> assets = await assetHandle.Task;
            AsyncOperationHandle<GameObject> runtimeHandle = Addressables.LoadAssetAsync<GameObject>("RuntimeTransformHandle_Assets");
            GameObject runtimeTransformHandle = await runtimeHandle.Task;
            
            SetMode("Create");
            createMode.CreateButton(assets);
            editMode.CreateRuntimeHandle(runtimeTransformHandle);
            
        }
        public void SetMode(string mode)
        {
            if (currentMode != null)
            {
                currentMode.OnDisable();
            }

            if (mode.Contains("Create"))
            {
                currentMode = createMode;
            }
            else if (mode.Contains("Edit"))
            {
                currentMode = editMode;
            }
            else if (mode.Contains("Delete"))
            {
                currentMode = deleteMode;
            }
            currentMode.OnEnable(arrangementAssetUI);
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
            if(context.performed && currentMode != null)
            {
                currentMode.OnSelect();
            }
        }
        public void OnCancel(InputAction.CallbackContext context)
        {
            if(context.performed && currentMode != null)
            {
                currentMode.OnCancel();
            }
        }
        public void OnDisable()
        {
            input.Disable();
        }
    }
}