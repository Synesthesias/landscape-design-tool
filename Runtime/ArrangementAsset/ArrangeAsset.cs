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

using System.Linq;

using RuntimeHandle;

namespace Landscape2.Runtime
{
    public class ArrangeAsset : ISubComponent
    {
        private RuntimeTransformHandle RuntimeTransformHandleScript;
        private GameObject runtimeTransformHandle;
        private GameObject selectedAsset;
        private GameObject generateAsset;
        private Camera cam;
        private Ray ray;
        private string status;
        private Button deleteButton;
        private ScrollView assetListScroll;
        private VisualElement tabs;
        private VisualElement editTable;
        private const string UINameRoot = "AssetArrageContent";
        // ---------------------------------------------------------------------------------------------------------------------------------
        public ArrangeAsset()
        {
            var ArrangementAssetUI = new UIDocumentFactory().CreateWithUxmlName("ArrangementAssetUI");
            // Create Edit Deleteボタンの機能
            var AssetArrageContent = ArrangementAssetUI.Q(UINameRoot);
            var assetTab = new TabUI(AssetArrageContent);
            tabs = ArrangementAssetUI.Q<VisualElement>("tabs");
            foreach(var child in tabs.Children())
            {
                if (child is Button tab)
                {
                    tab.clicked += () => 
                    {
                        InitializeHandle();
                        status = tab.text; 
                    };
                }
            }
            // Transrate Rotate Scaleボタンの機能
            editTable =  ArrangementAssetUI.Q<VisualElement>("EditTable");
            foreach(var child in editTable.Children())
            {
                if (child is Button tab)
                {
                    tab.clicked += () => 
                    {
                        SetTransformType(tab.text);
                    };
                }
            }
            assetListScroll= ArrangementAssetUI.Q<ScrollView>("CreateTable");
            deleteButton = ArrangementAssetUI.Q<Button>("DeleteButton");
        }
        void InitializeHandle()
        {
            // 作成状態の初期化
            if(generateAsset != null)
            {
                GameObject.Destroy(generateAsset);
            }
            // 編集状態の初期化
            RuntimeTransformHandleScript.target = GameObject.Find("Scripts").transform;
            // 削除状態の初期化
            GameObject[] selectedObjects = GameObject.FindGameObjectsWithTag("DeleteAssets");
            foreach(GameObject obj in selectedObjects)
            {
                obj.tag = "PlateauAssets_Props";
                SetLayerRecursively(obj,0);
            }
        }
        void SetTransformType(string name)
        {
            if(RuntimeTransformHandleScript != null)
            {
                if(name == "Transrate")
                {
                    RuntimeTransformHandleScript.type = HandleType.POSITION;
                    RuntimeTransformHandleScript.axes = HandleAxes.XYZ;
                }
                if(name == "Rotate")
                {
                    RuntimeTransformHandleScript.type = HandleType.ROTATION;
                    RuntimeTransformHandleScript.axes = HandleAxes.Y;
                }
                if(name == "Size")
                {
                    RuntimeTransformHandleScript.type = HandleType.SCALE;
                    RuntimeTransformHandleScript.axes = HandleAxes.XYZ;
                }
            }
        }
        // ---------------------------------------------------------------------------------------------------------------------------------
        public async void OnEnable()
        {
            // アセットの取得
            AsyncOperationHandle<IList<GameObject>> assetHandle = Addressables.LoadAssetsAsync<GameObject>("PlateauProps_Assets", null);
            IList<GameObject> assets = await assetHandle.Task;
            // RuntimeTransformHandle.csの参照
            AsyncOperationHandle<GameObject> runtimeHandle = Addressables.LoadAssetAsync<GameObject>("Packages/com.synesthesias.landscape-design-tool-2/Runtime/ArrangementAsset/Prefab/RuntimeTransformHandle.prefab");
            GameObject runtimeTransformHandle = await runtimeHandle.Task;
            GameObject runtimeTransformHandleObject = GameObject.Instantiate(runtimeTransformHandle) as GameObject;
            RuntimeTransformHandleScript = GameObject.Find(runtimeTransformHandle.name + "(Clone)").GetComponent<RuntimeTransformHandle>();

            RuntimeTransformHandleScript.autoScale = true;
            CreateButton(assets);
        }
        private void CreateButton(IList<GameObject> assets)
        {
            // Flexコンテナを作成し、ScrollViewに追加
            VisualElement flexContainer = new VisualElement();
            flexContainer.style.flexDirection = FlexDirection.Row; // 子要素を行方向に配置
            flexContainer.style.flexWrap = Wrap.Wrap; // 子要素がコンテナを超えた場合に折り返し
            flexContainer.style.width = new Length(100, LengthUnit.Percent); // 100%の幅を持つように設定
            flexContainer.style.height = new Length(100, LengthUnit.Percent); // 高さも親の100%
            // アセットをスクロールバーで表示させる
            foreach (GameObject asset in assets)
            {
                Button newButton = new Button()
                {
                    text = asset.name,
                    name = asset.name // ボタンに名前を付ける
                };
                newButton.style.height = 100;
                newButton.style.width = flexContainer.style.width;
                newButton.name = asset.name;
                newButton.clicked += () => 
                {
                    OnSetAsset(asset.name, assets);
                };
                flexContainer.Add(newButton);
            }
            assetListScroll.Add(flexContainer);

            deleteButton.clicked += OnDeleteAsset;
        }
        
        private void OnSetAsset(string assetName,IList<GameObject> assets)
        {
            // 選択されたアセットを取得
            selectedAsset = assets.FirstOrDefault(p => p.name == assetName);
            generateAssets();
            status = "Create";
        }
        private void generateAssets()
        {
            cam = Camera.main;
            ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit,Mathf.Infinity))
            {
                // UI要素上でクリックが行われた場合、ここで処理を終了
                if(EventSystem.current.IsPointerOverGameObject())
                {
                    GameObject.Destroy(generateAsset);
                }
                generateAsset = GameObject.Instantiate(selectedAsset, hit.point, Quaternion.identity) as GameObject;
                generateAsset.tag = "PlateauAssets_Props";
                int generateLayer = LayerMask.NameToLayer("generateAsset");

                SetLayerRecursively(generateAsset, generateLayer); 
            }
        }

        private void OnDeleteAsset()
        {
            GameObject[] selectedObjects = GameObject.FindGameObjectsWithTag("DeleteAssets");
            foreach(GameObject obj in selectedObjects)
            {
                GameObject.Destroy(obj);
            }
        }
        // ---------------------------------------------------------------------------------------------------------------------------------

        public void Update(float deltaTime)
        {
            if(status == "Create")
            {
                CreateAsset();
            }
            else if(status == "Edit")
            {
                EditAsset();
            }
            else if(status == "Delete")
            {
                DeleteAsset();
            }
        }

        void CreateAsset()
        {
            
            // 左クリックで位置確定
            if(Input.GetMouseButtonDown(0))
            {
                SetLayerRecursively(generateAsset,0);
                generateAssets();
                return;
            }
            // 右クリックで選択解除
            if(Input.GetMouseButtonDown(1))
            {
                GameObject.Destroy(generateAsset);
                status = "";
                return;
            }

            cam = Camera.main;
            ray = cam.ScreenPointToRay(Input.mousePosition);
            int layerMask = LayerMask.GetMask("generateAsset");
            layerMask = ~layerMask;
            if (Physics.Raycast(ray, out RaycastHit hit,Mathf.Infinity,layerMask))
            {
                generateAsset.transform.position = hit.point;
            }
        }
        void EditAsset()
        {
            if(Input.GetMouseButtonDown(0))
            {
                
                Camera cam = Camera.main;
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit,Mathf.Infinity))
                {
                    // タグが一致した場合、オブジェクトの名前をログに出力
                    if (hit.collider.tag == "PlateauAssets_Props")
                    {
                        RuntimeTransformHandleScript.target = hit.collider.gameObject.transform;                           
                    }
                }
                return;
            }
        }
        void DeleteAsset()
        {
            if(Input.GetMouseButtonDown(0))
            {
                Camera cam = Camera.main;
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit,Mathf.Infinity))
                {
                    if (hit.collider.tag == "PlateauAssets_Props")
                    {
                        hit.collider.tag = "DeleteAssets";
                        int deleteLayer = LayerMask.NameToLayer("deleteAsset");
                        SetLayerRecursively(hit.collider.gameObject, deleteLayer);
                    }
                    else if(hit.collider.tag == "DeleteAssets")
                    {
                        hit.collider.tag = "PlateauAssets_Props";
                        SetLayerRecursively(hit.collider.gameObject,0);
                    }
                }
                return;
            }
            if(Input.GetMouseButtonDown(1))
            {
                GameObject[] selectedObjects = GameObject.FindGameObjectsWithTag("DeleteAssets");
                foreach(GameObject obj in selectedObjects)
                {
                    SetLayerRecursively(obj,0);
                    obj.tag = "PlateauAssets_Props";
                }
                return;
            }
        }
        // ---------------------------------------------------------------------------------------------------------------------------------
        void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (obj == null)
                return;

            obj.layer = newLayer;

            foreach (Transform child in obj.transform)
            {
                if (child == null)
                    continue;

                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
        // ---------------------------------------------------------------------------------------------------------------------------------
        public void OnDisable()
        {
        }
    }
}