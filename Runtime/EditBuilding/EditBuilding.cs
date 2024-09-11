using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Landscape2.Runtime
{
    /// <summary>
    /// 建物編集画面時の機能クラス
    /// </summary>
    public class EditBuilding : ISubComponent
    {
        // 建物が選択されたときのイベント関数
        public event Action<GameObject> OnBuildingSelected = targetObject => { };

        GameObject targetObject;
        GameObject highlightBox = null;
        VisualElement uiRoot;

        private readonly VisualElement materialPanel;
        private const string UIMaterialPanel = "Panel_MaterialEditor";

        public EditBuilding(VisualElement uiRoot)
        {
            this.uiRoot = uiRoot;
            materialPanel = uiRoot.Q<VisualElement>(UIMaterialPanel);

            // 建物編集画面が閉じられたとき
            uiRoot.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                if(uiRoot.style.display == DisplayStyle.None)
                {
                    OnDisable();
                }
            });
        }
        public void Update(float deltaTime)
        {
            // 建物編集画面時の処理
            if (uiRoot.style.display == DisplayStyle.Flex)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    // マウスの座標がパネルの範囲内ある場合は反応しない
                    var panelBound = materialPanel.worldBound;
                    panelBound.position = new Vector2(panelBound.position.x, Screen.height - panelBound.position.y - panelBound.size.y);
                    if (panelBound.Contains(Input.mousePosition))
                    {
                        return;
                    }

                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit = new RaycastHit();

                    if (Physics.Raycast(ray, out hit))
                    {
                        // 建築物をクリックした場合
                        if (hit.collider.gameObject.name.Contains("bldg_"))
                        {
                            targetObject = hit.collider.gameObject;

                            // 建物選択時のハイライトボックスを生成する
                            CreateHighlightBox();

                            OnBuildingSelected(targetObject);
                        }
                    }
                    else
                    {
                        targetObject = null;
                    }
                }
            }
        }

        // 建物選択時のハイライトボックスを生成する
        private void CreateHighlightBox()
        {
            if (highlightBox == null)
            {
                var bbox = Resources.Load("bbox") as GameObject;
                highlightBox = GameObject.Instantiate(bbox);

                MeshFilter mf = highlightBox.GetComponent<MeshFilter>();
                mf.mesh.SetIndices(mf.mesh.GetIndices(0), MeshTopology.LineStrip, 0);
            }

            var meshColider = targetObject.GetComponent<MeshCollider>();
            var bounds = meshColider.bounds;

            highlightBox.transform.localPosition = bounds.center;
            highlightBox.transform.localScale = new Vector3(bounds.size.x, bounds.size.y, bounds.size.z);
        }

        public GameObject GetTargetObject()
        {
            return targetObject;
        }
        public void Start()
        {
        }
        public void OnEnable()
        {
        }
        public void OnDisable()
        {
            targetObject = null;

            if (highlightBox)
            {
                GameObject.Destroy(highlightBox);
                highlightBox = null;
            }
        }
    }
}
