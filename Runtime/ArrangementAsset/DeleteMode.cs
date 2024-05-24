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
        private bool isMouseOverUI;
        public override void OnEnable(VisualElement element)
        {
            selectedAssets = new List<GameObject>();
            selectedAssetsScroll = element.Q<ScrollView>("SelectedAssetScrollView");
        }
        public override void Update()
        {
            if(EventSystem.current.IsPointerOverGameObject())
            {
                isMouseOverUI = true;
            }
            else
            {
                isMouseOverUI = false;
            }
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
                if(isMouseOverUI)
                {
                    return;
                }
                GameObject createdAssets = GameObject.Find("CreatedAssets");
                if(selectedAssets.Contains(hit.collider.gameObject))
                {
                    SetLayerRecursively(hit.collider.gameObject,0);
                    selectedAssets.Remove(hit.collider.gameObject);
                }
                else if (hit.transform.parent == createdAssets.transform)
                {
                    selectedAssets.Add(hit.collider.gameObject);
                    int deleteLayer = LayerMask.NameToLayer("UI");
                    SetLayerRecursively(hit.collider.gameObject, deleteLayer);
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
        }
    }
}