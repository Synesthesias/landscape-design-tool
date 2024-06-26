using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 入力処理
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;

namespace Landscape2.Runtime
{
    public class DeleteMode : ArrangeMode
    {
        private List<GameObject> selectedAssets;
        private ScrollView selectedAssetsScroll;
        public override void OnEnable(VisualElement element)
        {
            selectedAssets = new List<GameObject>();
            selectedAssetsScroll = element.Q<ScrollView>("SelectedAssetScrollView");
        }

        public override void Update()
        {
        }
        
        public void DeleteAsset()
        {
            foreach(GameObject obj in selectedAssets)
            {
                GameObject.Destroy(obj);
            }
            selectedAssets.Clear();
            SetScrollContents();
        }
        public override void OnSelect()
        {
            Camera cam = Camera.main;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit,Mathf.Infinity))
            {
                if(selectedAssets.Contains(hit.collider.gameObject))
                {
                    SetLayerRecursively(hit.collider.gameObject,0);
                    selectedAssets.Remove(hit.collider.gameObject);
                }
                else if (CheckParentName(hit.transform))
                {
                    selectedAssets.Add(FindAssetComponent(hit.collider.gameObject.transform));
                    int deleteLayer = LayerMask.NameToLayer("UI");
                    SetLayerRecursively(FindAssetComponent(hit.collider.gameObject.transform), deleteLayer);
                }
            }
            SetScrollContents();
        }
        public override void OnCancel()
        {
            foreach(GameObject obj in selectedAssets)
            {
                SetLayerRecursively(obj,0);
            }
            selectedAssets.Clear();
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
        private bool CheckParentName(Transform hitTransform)
        {
            Transform current = hitTransform;
            while (current != null)
            {
                if (current.name == "CreatedAssets")
                {
                    return true;
                }
                current = current.parent;
            }
            return false;
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
        void SetScrollContents()
        {
            selectedAssetsScroll.Clear(); 
            VisualElement flexContainer = new VisualElement();
            foreach(GameObject asset in selectedAssets)
            {
                Button newButton = new Button()
                {
                    text = asset.name,
                    name = asset.name // ボタンに名前を付ける
                };
                newButton.AddToClassList("AssetButton");
                newButton.clicked += () => 
                {
                    FocusAsset(asset);
                };
                flexContainer.Add(newButton);
            }
            selectedAssetsScroll.Add(flexContainer);
        }
        void FocusAsset(GameObject obj)
        {
            Camera cam = Camera.main;
            Vector3 targetPosition = obj.transform.position;
            targetPosition.y += 1f;
            targetPosition.z -= 3f;
            cam.transform.position = targetPosition;
        }
        public override void OnDisable()
        {
            foreach(GameObject obj in selectedAssets)
            {
                SetLayerRecursively(obj,0);
            }
            selectedAssets.Clear();
            selectedAssetsScroll.Clear();
        }
    }
}