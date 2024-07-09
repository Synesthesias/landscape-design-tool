using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
// 入力処理
using UnityEngine.InputSystem;
// FirstOrDefault()の使用
using System.Linq;
using UnityEngine.UIElements;
// OnSelectの処理待ちに使用
using System.Threading.Tasks;
namespace Landscape2.Runtime
{
    // ArrangeModeクラスの派生クラス
    public class CreateMode : ArrangeMode
    {
        private Camera cam;
        private Ray ray;
        
        public GameObject selectedAsset;
        private GameObject generatedAsset;
        private VisualElement arrangeAssetsUI;
        private bool isMouseOverUI;

        public override void OnEnable(VisualElement element)
        {
            arrangeAssetsUI = element;
            arrangeAssetsUI.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            arrangeAssetsUI.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }

        public override void Update()
        {
            if(generatedAsset != null)
            {
                cam = Camera.main;
                ray = cam.ScreenPointToRay(Input.mousePosition);
                int layerMask = LayerMask.GetMask("Ignore Raycast");
                layerMask = ~layerMask; 
                if (Physics.Raycast(ray, out RaycastHit hit,Mathf.Infinity,layerMask))
                {
                    generatedAsset.transform.position = hit.point;
                }
            }
        }

        public void generateAssets(GameObject obj)
        {
            cam = Camera.main;
            ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit,Mathf.Infinity))
            {
                if(isMouseOverUI && generatedAsset != null)
                {
                    GameObject.Destroy(generatedAsset);
                }

                GameObject parent = GameObject.Find("CreatedAssets");
                generatedAsset = GameObject.Instantiate(obj, hit.point, Quaternion.identity, parent.transform) as GameObject;
                generatedAsset.name =  obj.name;

                int generateLayer = LayerMask.NameToLayer("Ignore Raycast");
                SetLayerRecursively(generatedAsset, generateLayer); 
            }
        }

        public void CreateButton(IList<GameObject> plateauAssets)
        {
            // Flexコンテナを作成し、ScrollViewに追加
            VisualElement flexContainer = new VisualElement();
            // アセットをスクロールバーで表示させる
            foreach (GameObject asset in plateauAssets)
            {
                Button newButton = new Button()
                {
                    text = asset.name,
                    name = asset.name // ボタンに名前を付ける
                };
                newButton.AddToClassList("AssetButton");
                newButton.clicked += () => 
                {
                    SetAsset(asset.name, plateauAssets);
                };
                flexContainer.Add(newButton);
            }
            arrangeAssetsUI.Q<ScrollView>("CreateTable").Add(flexContainer);
        }

        private void SetAsset(string assetName,IList<GameObject> plateauAssets)
        {
            // 選択されたアセットを取得
            selectedAsset = plateauAssets.FirstOrDefault(p => p.name == assetName);
            generateAssets(selectedAsset);
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

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            isMouseOverUI = true;
        }
        
        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            isMouseOverUI = false;
        }

        public override void OnSelect()
        {   
            if(selectedAsset != null)
            {
                SetLayerRecursively(generatedAsset,0);
                generateAssets(selectedAsset);
            }
        }

        public override void OnCancel()
        {
            selectedAsset = null;
            GameObject.Destroy(generatedAsset);
        }
        
        public override void OnDisable()
        {
            generatedAsset = null;
            selectedAsset = null;
        }
    }
}