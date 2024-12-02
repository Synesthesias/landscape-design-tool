using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UIElements;
// セーブ、ロード用
using ToolBox.Serialization;

using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;

namespace Landscape2.Runtime
{
    /// <summary>
    /// 視点場のポイントに関するクラス
    /// </summary>
    public class ViewPoint : LineOfSightModeClass
    {
        private Camera cam;
        private Ray ray;
        private GameObject setPointMarker;
        private GameObject createPointMarker;
        private GameObject viewPointMarkers;
        private VisualElement new_Viewpoint;
        private VisualElement edit_ViewPoint;
        private Sprite viewPointIconSprite;
        private Vector3 editPointMarkerPos;
        private string editPointName;
        private bool isMouseOverUI;
        private LineOfSightDataComponent lineOfSightDataComponent;

        public ViewPoint(LineOfSightDataComponent lineOfSightDataComponentInstance)
        {
            viewPointMarkers = new GameObject("ViewPointMarkers");
            lineOfSightDataComponent = lineOfSightDataComponentInstance;
        }
        public override async void OnEnable(VisualElement element)
        {
            new_Viewpoint = element.Q<VisualElement>("New_Viewpoint");
            edit_ViewPoint = element.Q<VisualElement>("Edit_Viewpoint");
            AsyncOperationHandle<Sprite> viewPointIconHandle = Addressables.LoadAssetAsync<Sprite>("ViewPointIcon");
            viewPointIconSprite = await viewPointIconHandle.Task;
            new_Viewpoint.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            new_Viewpoint.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
            edit_ViewPoint.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            edit_ViewPoint.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            isMouseOverUI = true;
        }

        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            isMouseOverUI = false;
        }

        /// <summary>
        /// クリックされた場所にポイントを配置する
        /// </summary>
        private void SetPoint()
        {
            cam = Camera.main;
            ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                if (isMouseOverUI)
                {
                    Debug.Log($"isMouseOverUI:{isMouseOverUI}");
                    return;
                }
                var setPoint = hit.point;
                var fieldName = "heightValueTextField";
                var heightValue = new_Viewpoint.Q<TextField>(fieldName).text;
                if (!float.TryParse(heightValue, out float height))
                {
                    Debug.LogWarning($"heightValue: {heightValue}");
                    new_Viewpoint.Q<TextField>(fieldName).value = "数値を入力してください";
                    return;
                }
                if (setPointMarker == null)
                {
                    setPointMarker = new GameObject("ViewPoint_Icon");
                    var setPointMarkerSpriteRenderer = setPointMarker.AddComponent<SpriteRenderer>();
                    setPointMarkerSpriteRenderer.sprite = viewPointIconSprite;
                    var boxCollider = setPointMarker.AddComponent<BoxCollider>();
                    boxCollider.size = setPointMarkerSpriteRenderer.sprite.bounds.size;
                }
                // アイコンが埋め込まれないように2.5f高くしている
                setPoint.y += float.Parse(heightValue) * 0.01f + 2.5f;
                setPointMarker.transform.position = setPoint;
            }
            else
            {
                Debug.Log($"raycast not hit ");
            }
        }

        /// <summary>
        /// 視点場を生成する
        /// </summary>
        public string CreatePoint()
        {
            // ポイントがセットされずに作成ボタンが押された場合
            if (setPointMarker == null)
            {
                Debug.Log($"ポイントがセットされていません: setPointMarker is null");
                return "";
            }
            // ボタンの名前が設定されていないときの処理
            var registerName = new_Viewpoint.Q<TextField>("NewViewpointName").text;
            if (registerName == "")
            {
                new_Viewpoint.Q<TextField>("NewViewpointName").value = "名前を入力してください";
                GameObject.Destroy(setPointMarker);
                setPointMarker = null;
                return "";
            }
            // ゲームオブジェクトの追加
            createPointMarker = GameObject.Instantiate(setPointMarker);
            GameObject.Destroy(setPointMarker);
            setPointMarker = null;
            createPointMarker.name = registerName;
            createPointMarker.transform.parent = viewPointMarkers.transform;
            // データの追加
            var isAdded = lineOfSightDataComponent.AddPointDict(LineOfSightType.viewPoint, registerName, createPointMarker.transform.position);
            if (isAdded)
            {
                return registerName;
            }
            else
            {
                new_Viewpoint.Q<TextField>("NewViewpointName").value = "同じ名前のポイントが存在します";
                GameObject.Destroy(createPointMarker);
                createPointMarker = null;
                return "";
            }
        }

        /// <summary>
        /// 視点場を編集する
        /// </summary>
        public (string beforeName, string afterName) EditPoint()
        {
            var registerName = edit_ViewPoint.Q<TextField>("EditViewpointName").text;
            if (registerName == "")
            {
                edit_ViewPoint.Q<TextField>("EditViewpointName").value = "名前を入力してください";
                return ("", "");
            }
            // 編集後のポイントを生成
            createPointMarker = GameObject.Instantiate(setPointMarker);
            createPointMarker.name = registerName;
            createPointMarker.transform.parent = viewPointMarkers.transform;
            // 既存のポイントを削除
            var deleteData = DeletePoint();
            var deleteButtonName = deleteData.deleteButtonName;
            var isAdded = lineOfSightDataComponent.AddPointDict(LineOfSightType.viewPoint, registerName, createPointMarker.transform.position);
            if (isAdded)
            {
                return (deleteButtonName, registerName);
            }
            else
            {
                edit_ViewPoint.Q<TextField>("EditViewpointName").value = "同じ名前のポイントが存在します";
                GameObject.Destroy(createPointMarker);
                createPointMarker = null;
                return ("", "");
            }
        }

        /// <summary>
        /// 視点場を削除する
        /// </summary>
        public (string deleteButtonName, List<string> removedAnalyzeKeyNameList) DeletePoint()
        {
            var pointName = setPointMarker.name;
            // ゲームオブジェクトの削除
            if (setPointMarker != null)
            {
                GameObject.Destroy(setPointMarker);
                setPointMarker = null;
            }
            // データの削除
            var removeData = lineOfSightDataComponent.RemovePointElement(LineOfSightType.viewPoint, pointName);
            var isRemoved = removeData.isRemoved;
            var removedAnalyzeKeyNameList = removeData.removedAnalyzeKeyNameList;
            if (isRemoved)
            {
                return (pointName, removedAnalyzeKeyNameList);
            }
            else
            {
                return ("", removedAnalyzeKeyNameList);
            }
        }

        /// <summary>
        /// 視点場のボタンが押されたときの処理
        /// </summary>
        public void ButtonAction(string pointName)
        {
            foreach (Transform child in viewPointMarkers.transform)
            {
                if (child.gameObject.name == pointName)
                {
                    setPointMarker = child.gameObject;
                    break;
                }
            }
            editPointMarkerPos = setPointMarker.transform.position;
            editPointName = setPointMarker.name;
        }

        /// <summary>
        /// キャンセルボタンが押されたときの処理
        /// </summary>
        public void RestorePoint()
        {
            if (setPointMarker == null)
            {
                new GameObject("ViewPoint_Icon");
            }
            createPointMarker = GameObject.Instantiate(setPointMarker);
            createPointMarker.transform.position = editPointMarkerPos;
            createPointMarker.name = editPointName;
            createPointMarker.transform.parent = viewPointMarkers.transform;
        }

        public override void OnSelect()
        {
            SetPoint();
        }

        public override void OnDisable()
        {
            GameObject.Destroy(setPointMarker);
            setPointMarker = null;
        }
    }
}
