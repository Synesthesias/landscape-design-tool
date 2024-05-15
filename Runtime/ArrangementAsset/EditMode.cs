using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 入力処理
using UnityEngine.InputSystem;

using RuntimeHandle;

namespace Landscape2.Runtime
{
    public class EditMode : ArrangeMode
    {
        private RuntimeTransformHandle runtimeTransformHandleScript;
        public override void OnEnable()
        {
            runtimeTransformHandleScript = GameObject.Find("runtimeTransformHandleObject").GetComponent<RuntimeTransformHandle>();
            runtimeTransformHandleScript.autoScale = true;
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
        public override void OnSelect()
        {
            Camera cam = Camera.main;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit,Mathf.Infinity))
            {
                GameObject createdAssets = GameObject.Find("CreatedAssets");
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
