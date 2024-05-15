using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
// 入力処理
using UnityEngine.InputSystem;
// FirstOrDefault()の使用
using System.Linq;

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
        public override void OnEnable()
        {
            
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
                GameObject createdAssets = GameObject.Find("CreatedAssets");
                generatedAsset = GameObject.Instantiate(obj, hit.point, Quaternion.identity, createdAssets.transform) as GameObject;

                int generateLayer = LayerMask.NameToLayer("Ignore Raycast");
                SetLayerRecursively(generatedAsset, generateLayer); 
            }
        }
        public void SetAsset(string assetName,IList<GameObject> assets)
        {
            // 選択されたアセットを取得
            selectedAsset = assets.FirstOrDefault(p => p.name == assetName);
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