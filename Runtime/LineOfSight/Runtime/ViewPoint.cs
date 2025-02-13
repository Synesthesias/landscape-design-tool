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
using System.Linq.Expressions;
using UnityEngine.PlayerLoop;

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

        private float offsetYValue;
        private Vector3 markerPosition;

        private Dictionary<string, float> heightValueIndex = new();


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

            var fieldName = "heightValueTextField";

            foreach (var elem in new List<VisualElement>() { new_Viewpoint, edit_ViewPoint })
            {
                var heightInputField = elem.Q<TextField>(fieldName);
                heightInputField.RegisterCallback<ChangeEvent<string>>(input =>
                {
                    if (input.newValue != input.previousValue)
                    {
                        if (float.TryParse(input.newValue, out var heightValue))
                        {
                            offsetYValue = heightValue;
                            UpdateMarkerPos();
                        }
                    }
                });

            }
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            isMouseOverUI = true;
        }

        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            isMouseOverUI = false;
        }

        private void UpdateMarkerPos()
        {
            if (setPointMarker == null)
            {
                return;
            }

            setPointMarker.transform.position = markerPosition + new Vector3(0f, offsetYValue, 0f);
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
                markerPosition = setPoint;

                UpdateMarkerPos();
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

            new_Viewpoint.Q<TextField>();


            // ゲームオブジェクトの追加
            createPointMarker = GameObject.Instantiate(setPointMarker);
            GameObject.Destroy(setPointMarker);
            setPointMarker = null;
            createPointMarker.name = registerName;

            heightValueIndex[registerName] = offsetYValue;
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
        /// uiに値を入れる。
        /// ButtonAction()の後に呼び出されるのを想定
        /// </summary>
        public void InitializeEditPoint()
        {
            if (setPointMarker == null)
            {
                Debug.LogWarning($"setPointMarker is null");
                return;
            }
            edit_ViewPoint.Q<TextField>("EditViewpointName").value = setPointMarker.name;

            edit_ViewPoint.Q<TextField>("heightValueTextField").value = offsetYValue.ToString();

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
            heightValueIndex[registerName] = offsetYValue;
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

            // キャンセルした際の為編集前座標を保持しておく
            editPointMarkerPos = setPointMarker.transform.position;
            editPointName = setPointMarker.name;

            // 編集座標を修正
            markerPosition = editPointMarkerPos;
            offsetYValue = 0f;
            if (heightValueIndex.TryGetValue(setPointMarker.name, out var offsetY))
            {
                markerPosition -= new Vector3(0f, offsetY, 0f);
                offsetYValue = offsetY;
            }
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
