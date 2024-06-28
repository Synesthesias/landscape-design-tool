using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
// 入力処理
using UnityEngine.InputSystem;

using RuntimeHandle;

namespace Landscape2.Runtime
{
    public class EditMode : ArrangeMode
    {
        private RuntimeTransformHandle runtimeTransformHandleScript;
        private VisualElement arrangeAssetsUI;
        private bool isMouseOverUI;

        public override void OnEnable(VisualElement element)
        {
            arrangeAssetsUI = element;
            arrangeAssetsUI.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            arrangeAssetsUI.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }

        public void SetTransformType(string name)
        {
            if(runtimeTransformHandleScript != null)
            {
                if(name == "Position")
                {
                    runtimeTransformHandleScript.type = HandleType.POSITION;
                    runtimeTransformHandleScript.axes = HandleAxes.XYZ;
                }
                if(name == "Rotation")
                {
                    runtimeTransformHandleScript.type = HandleType.ROTATION;
                    runtimeTransformHandleScript.axes = HandleAxes.Y;
                }
                if(name == "Scale")
                {
                    runtimeTransformHandleScript.type = HandleType.SCALE;
                    runtimeTransformHandleScript.axes = HandleAxes.XYZ;
                }
            }
        }

        public void CreateRuntimeHandle(GameObject obj)
        {
            runtimeTransformHandleScript = RuntimeTransformHandle.Create(null,HandleType.POSITION);
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
                if (CheckParentName(hit.transform))
                {
                    runtimeTransformHandleScript.target = FindAssetComponent(hit.collider.gameObject.transform).transform;
                }
            }
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
        
        private void OnMouseEnter(MouseEnterEvent evt)
        {
            isMouseOverUI = true;
        }
        
        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            isMouseOverUI = false;
        }

        public override void Update()
        {
        }
        public override void OnCancel()
        {
            runtimeTransformHandleScript.target = GameObject.Find("Scripts").transform;
        }
        public override void OnDisable()
        {
            runtimeTransformHandleScript.target = GameObject.Find("Scripts").transform;
        }
    }
}
