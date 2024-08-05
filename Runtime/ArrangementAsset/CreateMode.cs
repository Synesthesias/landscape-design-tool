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

        public void OnEnable(VisualElement element)
        {
            arrangeAssetsUI = element.Q<VisualElement>("CreatePanel");
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
            if(isMouseOverUI && generatedAsset != null)
            {
                GameObject.Destroy(generatedAsset);
            }
            
            if (Physics.Raycast(ray, out RaycastHit hit,Mathf.Infinity))
            {
                GameObject parent = GameObject.Find("CreatedAssets");
                generatedAsset = GameObject.Instantiate(obj, hit.point, Quaternion.identity, parent.transform) as GameObject;    
            }
            else
            {
                GameObject parent = GameObject.Find("CreatedAssets");
                generatedAsset = GameObject.Instantiate(obj, Vector3.zero, Quaternion.identity, parent.transform) as GameObject;
            }
            generatedAsset.name =  obj.name;
            int generateLayer = LayerMask.NameToLayer("Ignore Raycast");
            SetLayerRecursively(generatedAsset, generateLayer); 
        }

        public void SetAsset(string assetName,IList<GameObject> plateauAssets)
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

        public void OnSelect()
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
            generatedAsset = null;
        }
    }
}