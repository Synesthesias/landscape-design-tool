using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
// 入力処理
using UnityEngine.InputSystem;

using RuntimeHandle;

namespace Landscape2.Runtime
{
    public enum TransformType
    {
        Position,
        Rotation,
        Scale
    }
    public class EditMode : ArrangeMode
    {
        private RuntimeTransformHandle runtimeTransformHandleScript;

        public void CreateRuntimeHandle(GameObject obj,TransformType transformType)
        {
            ClearHandleObject();
            CreateHandleObject(obj,transformType);
            SetTransformType(transformType);
        } 
        private void ClearHandleObject()
        {
            var obj = GameObject.Find("New Game Object");
            if(obj != null)
            {
                GameObject.Destroy(obj);
            }
        }
        private void CreateHandleObject(GameObject obj,TransformType transformType)
        {
            runtimeTransformHandleScript = RuntimeTransformHandle.Create(null,HandleType.POSITION);
            runtimeTransformHandleScript.target = obj.transform;
        }
        private void SetTransformType(TransformType transformType)
        {
            if(runtimeTransformHandleScript != null)
            {
                if(transformType == TransformType.Position)
                {
                    runtimeTransformHandleScript.type = HandleType.POSITION;
                    runtimeTransformHandleScript.axes = HandleAxes.XYZ;
                }
                if(transformType == TransformType.Rotation)
                {
                    runtimeTransformHandleScript.type = HandleType.ROTATION;
                    runtimeTransformHandleScript.axes = HandleAxes.Y;
                }
                if(transformType == TransformType.Scale)
                {
                    runtimeTransformHandleScript.type = HandleType.SCALE;
                    runtimeTransformHandleScript.axes = HandleAxes.XYZ;
                }
            }
        }

        public void DeleteAsset(GameObject obj)
        {
            ClearHandleObject();
            GameObject.Destroy(obj);
        }
        public override void Update()
        {

        }
        public override void OnCancel()
        {
            ClearHandleObject();
        }
    }
}
