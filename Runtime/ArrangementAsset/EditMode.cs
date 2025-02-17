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
        Scale,
        None
    }
    public class EditMode : ArrangeMode
    {
        private RuntimeTransformHandle runtimeTransformHandleScript;
        private GameObject editAsset;

        public RuntimeTransformHandle RuntimeTransformHandleScript => runtimeTransformHandleScript;

        public void CreateRuntimeHandle(GameObject obj, TransformType transformType)
        {
            ClearHandleObject();
            CreateHandleObject(obj, transformType);
            SetTransformType(transformType);
            editAsset = obj;
            ChangeEditAssetLayer(editAsset, LayerMask.NameToLayer("UI"));
        }
        private void ClearHandleObject()
        {
            ChangeEditAssetLayer(editAsset, LayerMask.NameToLayer("Default"));
            var obj = GameObject.Find("RuntimeTransformHandle");
            if (obj != null)
            {
                GameObject.Destroy(obj);
            }
        }

        private void ChangeEditAssetLayer(GameObject parent, int layerIndex)
        {
            if (parent == null)
            {
                return;
            }
            parent.layer = layerIndex;
            foreach (Transform child in parent.transform)
            {
                ChangeEditAssetLayer(child.gameObject, layerIndex);
            }
        }

        private void CreateHandleObject(GameObject obj, TransformType transformType)
        {
            if (transformType == TransformType.None)
            {
                return;
            }
            runtimeTransformHandleScript = RuntimeTransformHandle.Create(null, HandleType.POSITION);
            runtimeTransformHandleScript.autoScale = true;
            runtimeTransformHandleScript.target = obj.transform;
        }

        private void SetTransformType(TransformType transformType)
        {
            if (runtimeTransformHandleScript != null)
            {
                if (transformType == TransformType.Position)
                {
                    runtimeTransformHandleScript.type = HandleType.POSITION;
                    runtimeTransformHandleScript.axes = HandleAxes.XYZ;
                }
                if (transformType == TransformType.Rotation)
                {
                    runtimeTransformHandleScript.type = HandleType.ROTATION;
                    runtimeTransformHandleScript.axes = HandleAxes.Y;
                }
                if (transformType == TransformType.Scale)
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
