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
    public class Test : ISubComponent
    {
        private RuntimeTransformHandle RuntimeTransformHandleScript;
        private GameObject runtimeTransformHandle;
        private GameObject selectedPrefab;
        private GameObject generatePrefab;
        private Camera cam;
        private Ray ray;
        private string status;
        private Button deleteButton;
        private ScrollView prefabListScroll;
        private VisualElement tabs;
        private VisualElement editTable;
        private const string UINameRoot = "PrefabMenu";
        
        public Test()
        {
            var ArrangePrefabUI = new UIDocumentFactory().CreateWithUxmlName("PrefabList");
            // Create Edit Deleteタブの初期化
            var prefabUiRoot = ArrangePrefabUI.Q(UINameRoot);
            var prefabTab = new TabUI(prefabUiRoot);
            tabs = ArrangePrefabUI.Q<VisualElement>("tabs");
            foreach(var child in tabs.Children())
            {
                if (child is Button tab)
                {
                    tab.clicked += () => 
                    {
                        status = tab.text; 
                    };
                }
            }

            editTable =  ArrangePrefabUI.Q<VisualElement>("EditTable");
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
            prefabListScroll= ArrangePrefabUI.Q<ScrollView>("AssetScroll");
            deleteButton = ArrangePrefabUI.Q<Button>("DeleteButton");
            

            status = "Create";
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

        public async void OnEnable()
        {
            // Addressables.LoadAssetAsyncで読み込む
            AsyncOperationHandle<IList<GameObject>> prefabHandle = Addressables.LoadAssetsAsync<GameObject>("Plateau_Prefabs", null);
            AsyncOperationHandle<GameObject> runtimeHandle = Addressables.LoadAssetAsync<GameObject>("Assets/ArragementAsset/Asset/RuntimeTransformHandle.prefab");

            // .Taskで読み込み完了までawaitできる
            GameObject runtimeTransformHandle = await runtimeHandle.Task;
            IList<GameObject> prefabs = await prefabHandle.Task;

            GameObject runtimeTransformHandleObject = GameObject.Instantiate(runtimeTransformHandle) as GameObject;
            RuntimeTransformHandleScript = GameObject.Find(runtimeTransformHandle.name + "(Clone)").GetComponent<RuntimeTransformHandle>();
            RuntimeTransformHandleScript.autoScale = true;
            CreateButton(prefabs);
        }
        // ボタンに関する関数
        // ---------------------------------------------------------------------------------------------------------------------------------
        private void CreateButton(IList<GameObject> prefabs)
        {
            // Flexコンテナを作成し、ScrollViewに追加
            VisualElement flexContainer = new VisualElement();
            flexContainer.style.flexDirection = FlexDirection.Row; // 子要素を行方向に配置
            flexContainer.style.flexWrap = Wrap.Wrap; // 子要素がコンテナを超えた場合に折り返し
            flexContainer.style.width = new Length(100, LengthUnit.Percent); // 100%の幅を持つように設定
            flexContainer.style.height = new Length(100, LengthUnit.Percent); // 高さも親の100%

            foreach (GameObject prefab in prefabs)
            {
                Button newButton = new Button()
                {
                    text = prefab.name,
                    name = prefab.name // ボタンに名前を付ける
                };
                newButton.style.height = 100;
                newButton.style.width = flexContainer.style.width;
                newButton.name = prefab.name;
                newButton.clicked += () => 
                {
                    OnSetPrefab(prefab.name, prefabs);
                };
                flexContainer.Add(newButton);
            }
            prefabListScroll.Add(flexContainer);

            deleteButton.clicked += OndeletePrefab;
        }
        // ---------------------------------------------------------------------------------------------------------------------------------
        private void OnSetPrefab(string prefabName,IList<GameObject> prefabs)
        {
            // 選択されたオブジェクトの生成
            selectedPrefab = prefabs.FirstOrDefault(p => p.name == prefabName);
            generatePrefabs();
            status = "Set";
        }
        private void generatePrefabs()
        {
            cam = Camera.main;
            ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit,Mathf.Infinity))
            {
                // UI要素上でクリックが行われた場合、ここで処理を終了
                if(EventSystem.current.IsPointerOverGameObject())
                {
                    GameObject.Destroy(generatePrefab);
                }
                generatePrefab = GameObject.Instantiate(selectedPrefab, hit.point, Quaternion.identity) as GameObject;
                generatePrefab.tag = "PlateauAssets_Props";
                int generateLayer = LayerMask.NameToLayer("generatePrefab");
                SetLayerRecursively(generatePrefab, generateLayer); 
            }
        }

        private void OndeletePrefab()
        {
            GameObject[] selectedObjects = GameObject.FindGameObjectsWithTag("DeletePrefabs");
            foreach(GameObject obj in selectedObjects)
            {
                GameObject.Destroy(obj);
            }
        }

        public void Update(float deltaTime)
        {
            if(status == "Set")
            {
                SetPrefab();
            }
            else if(status == "Edit")
            {
                EditPrefab();
            }
            else if(status == "Delete")
            {
                DeletePrefab();
            }
        }

        void SetPrefab()
        {
            
            // 左クリックで位置確定
            if(Input.GetMouseButtonDown(0))
            {
                SetLayerRecursively(generatePrefab,0);
                generatePrefabs();
                return;
            }
            // 右クリックで選択解除
            if(Input.GetMouseButtonDown(1))
            {
                GameObject.Destroy(generatePrefab);
                // SetLayerRecursively(generatePrefab,0);
                status = "";
                return;
            }

            cam = Camera.main;
            ray = cam.ScreenPointToRay(Input.mousePosition);
            int layerMask = LayerMask.GetMask("generatePrefab");
            layerMask = ~layerMask;
            if (Physics.Raycast(ray, out RaycastHit hit,Mathf.Infinity,layerMask))
            {
                generatePrefab.transform.position = hit.point;
            }
        }
        void EditPrefab()
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
        void DeletePrefab()
        {
            if(Input.GetMouseButtonDown(0))
            {
                Camera cam = Camera.main;
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit,Mathf.Infinity))
                {
                    if (hit.collider.tag == "PlateauAssets_Props")
                    {
                        int deleteLayer = LayerMask.NameToLayer("deletePrefab");
                        SetLayerRecursively(hit.collider.gameObject, deleteLayer);
                        hit.collider.tag = "DeletePrefabs";
                    }
                    else if(hit.collider.tag == "DeletePrefabs")
                    {
                        hit.collider.tag = "PlateauAssets_Props";
                        int deleteLayer = LayerMask.NameToLayer("deletePrefab");
                        SetLayerRecursively(hit.collider.gameObject,0);
                    }
                }
                return;
            }
            if(Input.GetMouseButtonDown(1))
            {
                GameObject[] selectedObjects = GameObject.FindGameObjectsWithTag("DeletePrefabs");
                foreach(GameObject obj in selectedObjects)
                {
                    SetLayerRecursively(obj,0);
                    obj.tag = "PlateauAssets_Props";
                }
                return;
            }
        }
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

        public void OnDisable()
        {
        }
    }
}