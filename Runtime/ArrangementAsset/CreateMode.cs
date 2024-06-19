using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
// 入力処理
using UnityEngine.InputSystem;
// FirstOrDefault()の使用
using System.Linq;
using UnityEngine.UIElements;
namespace Landscape2.Runtime
{
    // ArrangeModeクラスの派生クラス
    public class CreateMode : ArrangeMode
    {
        private Camera cam;
        private Ray ray;
        
        public GameObject selectedAsset;
        private GameObject generatedAsset;
        private bool isMouseOverUI;
        private ScrollView assetListScroll;

        public override void OnEnable(VisualElement element)
        {
            assetListScroll = element.Q<ScrollView>("CreateTable");
        }

        public override void Update()
        {
            // マウスがUI上にあるかどうか
            if(EventSystem.current.IsPointerOverGameObject())
            {
                isMouseOverUI = true;
            }
            else
            {
                isMouseOverUI = false;
            }


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
                if(isMouseOverUI)
                {
                    GameObject.Destroy(generatedAsset);
                }

                GameObject parent;
                if(obj.name.Contains("Human"))
                {
                    parent = GameObject.Find("HumansAssets");
                }
                else
                {
                    parent = GameObject.Find("PropsAssets");
                }
                generatedAsset = GameObject.Instantiate(obj, hit.point, Quaternion.identity, parent.transform) as GameObject;
                generatedAsset.name =  obj.name;

                int generateLayer = LayerMask.NameToLayer("Ignore Raycast");
                SetLayerRecursively(generatedAsset, generateLayer); 
            }
        }

        public void CreateButton(Dictionary<GameObject, string> assets)
        {
            // Flexコンテナを作成し、ScrollViewに追加
            VisualElement flexContainer = new VisualElement();
            // アセットをスクロールバーで表示させる
            foreach (KeyValuePair<GameObject, string> asset in assets)
            {
                Button newButton = new Button()
                {
                    text = asset.Key.name,
                    name = asset.Key.name // ボタンに名前を付ける
                };
                newButton.AddToClassList("AssetButton");
                newButton.clicked += () => 
                {
                    SetAsset(asset.Key.name, assets);
                };
                flexContainer.Add(newButton);
            }
            assetListScroll.Add(flexContainer);
        }

        private void SetAsset(string assetName,Dictionary<GameObject, string> assets)
        {
            // 選択されたアセットを取得
            selectedAsset = assets.Keys.FirstOrDefault(p => p.name == assetName);
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
            GameObject.Destroy(generatedAsset);   
        }
        
        public override void OnDisable()
        {
            generatedAsset = null;
            selectedAsset = null;
        }
    }
}