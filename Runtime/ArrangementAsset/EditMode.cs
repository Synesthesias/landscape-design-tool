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

        public override void OnEnable(VisualElement element)
        {
        }
        public override void Update()
        {
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
                GameObject createdAssets = GameObject.Find("CreatedAssets");
                Debug.Log(hit.transform.parent);
                if (hit.transform.parent == createdAssets.transform)
                {
                    runtimeTransformHandleScript.target = hit.collider.gameObject.transform;    
                         
                }
            }
        }
        public override void OnCancel()
        {

        }
        public override void OnDisable()
        {
            runtimeTransformHandleScript.target = GameObject.Find("Scripts").transform;
        }
    }
}
