using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 入力処理
using UnityEngine.InputSystem;

namespace Landscape2.Runtime
{
    public class DeleteMode : ArrangeMode
    {
        private List<GameObject> selectedAssets;
        public override void OnEnable()
        {
            selectedAssets = new List<GameObject>();
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
        }
        public override void OnSelect()
        {
            Camera cam = Camera.main;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit,Mathf.Infinity))
            {
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