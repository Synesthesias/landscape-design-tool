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
        private GameObject a;

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
            // GameObject runtimeTransformHandleObject = GameObject.Instantiate(obj,Vector3.zero,Quaternion.identity) as GameObject;
            // runtimeTransformHandleObject.name = "runtimeTransformHandleObject";
            // GameObject test = GameObject.Find("runtimeTransformHandleObject");
            // runtimeTransformHandleScript = test.GetComponent<RuntimeTransformHandle>();

            // runtimeTransformHandleScript = RuntimeTransformHandle.Create(null,HandleType.POSITION);
            a = GameObject.Find("Scripts");
            runtimeTransformHandleScript = a.GetComponent<RuntimeTransformHandle>();
            if(a == null)
                    {
                        GameObject cubePrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);

                        // プレハブをインスタンス化
                        GameObject cube = GameObject.Instantiate(cubePrefab);

                        // インスタンス化後のキューブの大きさを100に設定
                        cube.transform.localScale = new Vector3(100, 100, 100);
                        cube.transform.position = new Vector3(0, 0,137.6f);
                        
                        // キューブの色を赤に設定
                        Renderer cubeRenderer = cube.GetComponent<Renderer>();
                        cubeRenderer.material.color = Color.red;
                    }
                    else if(runtimeTransformHandleScript == null)
                    {
                        GameObject cubePrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);

                        // プレハブをインスタンス化
                        GameObject cube = GameObject.Instantiate(cubePrefab);

                        // インスタンス化後のキューブの大きさを100に設定
                        cube.transform.localScale = new Vector3(100, 100, 100);
                        
                        // キューブの位置を設定 (任意で変更可能)
                        cube.transform.position = new Vector3(0, 0,137.6f);
                        
                        // キューブの色を赤に設定
                        Renderer cubeRenderer = cube.GetComponent<Renderer>();
                        cubeRenderer.material.color = Color.blue;
                        
                    }
                    else
                    {
                        GameObject cubePrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);

                        // プレハブをインスタンス化
                        GameObject cube = GameObject.Instantiate(cubePrefab);

                        // インスタンス化後のキューブの大きさを100に設定
                        cube.transform.localScale = new Vector3(100, 100, 100);
                        
                        // キューブの位置を設定 (任意で変更可能)
                        cube.transform.position = new Vector3(0, 0,137.6f);
                        
                        // キューブの色を赤に設定
                        Renderer cubeRenderer = cube.GetComponent<Renderer>();
                        cubeRenderer.material.color = Color.green;
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
