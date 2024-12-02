using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UIElements;

using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
namespace Landscape2.Runtime
{
    /// <summary>
    /// 眺望対象のポイントに関するクラス
    /// </summary>
    public class Landmark : LineOfSightModeClass
    {
        private Camera cam;
        private Ray ray;
        private GameObject setPointMarker;
        private GameObject createPointMarker;
        private GameObject landmarkMarkers;
        private VisualElement new_Landmark;
        private VisualElement edit_Landmark;
        private Sprite landmarkIconSprite;
        private Vector3 editPointMarkerPos;
        private string editPointName;
        private bool isMouseOverUI;
        private LineOfSightDataComponent lineOfSightDataComponent;

        public Landmark(LineOfSightDataComponent lineOfSightDataComponentInstance)
        {
            landmarkMarkers = new GameObject("LandmarkMarkers");
            lineOfSightDataComponent = lineOfSightDataComponentInstance;
        }

        public override async void OnEnable(VisualElement element)
        {
            new_Landmark = element.Q<VisualElement>("New_Landmark");
            edit_Landmark = element.Q<VisualElement>("Edit_Landmark");
            AsyncOperationHandle<Sprite> landmarkIconHandle = Addressables.LoadAssetAsync<Sprite>("LandmarkIcon");
            landmarkIconSprite = await landmarkIconHandle.Task;
            new_Landmark.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            new_Landmark.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
            edit_Landmark.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            edit_Landmark.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
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
                    return;
                }
                var setPoint = hit.point;
                var fieldName = "heightValueTextField";
                var heightValue = new_Landmark.Q<TextField>(fieldName).text;
                if (!float.TryParse(heightValue, out float height))
                {
                    new_Landmark.Q<TextField>(fieldName).value = "数値を入力してください";
                    return;
                }
                if (setPointMarker == null)
                {
                    setPointMarker = new GameObject("Landmark_Icon");
                    var setPointMarkerSpriteRenderer = setPointMarker.AddComponent<SpriteRenderer>();
                    setPointMarkerSpriteRenderer.sprite = landmarkIconSprite;
                    var boxCollider = setPointMarker.AddComponent<BoxCollider>();
                    boxCollider.size = setPointMarkerSpriteRenderer.sprite.bounds.size;
                }
                // アイコンが埋め込まれないように2.5f高くしている
                setPoint.y += float.Parse(heightValue) * 0.01f + 2.5f;
                setPointMarker.transform.position = setPoint;
            }
        }

        /// <summary>
        /// 眺望対象を生成する
        /// </summary>
        public string CreatePoint()
        {
            // ポイントがセットされずに作成ボタンが押された時の処理
            if (setPointMarker == null)
            {
                return "";
            }
            // ボタンの名前が設定されていないときの処理
            var registerName = new_Landmark.Q<TextField>("NewLandmarkName").text;
            if (registerName == "")
            {
                new_Landmark.Q<TextField>("NewLandmarkName").value = "名前を入力してください";
                GameObject.Destroy(setPointMarker);
                setPointMarker = null;
                return "";
            }
            // ゲームオブジェクトの追加
            createPointMarker = GameObject.Instantiate(setPointMarker);
            GameObject.Destroy(setPointMarker);
            setPointMarker = null;
            createPointMarker.name = registerName;
            createPointMarker.transform.parent = landmarkMarkers.transform;
            // データの追加
            var isAdded = lineOfSightDataComponent.AddPointDict(LineOfSightType.landmark, registerName, createPointMarker.transform.position);
            if (isAdded)
            {
                return registerName;
            }
            else
            {
                new_Landmark.Q<TextField>("NewLandmarkName").value = "同じ名前のポイントが存在します";
                GameObject.Destroy(createPointMarker);
                createPointMarker = null;
                return "";
            }
        }

        /// <summary>
        /// 眺望対象を編集する
        /// </summary>
        public (string beforeName, string afterName) EditPoint()
        {
            var registerName = edit_Landmark.Q<TextField>("EditLandmarkName").text;
            if (registerName == "")
            {
                edit_Landmark.Q<TextField>("EditLandmarkName").value = "名前を入力してください";
                return ("", "");
            }
            // 編集後のポイントを追加
            createPointMarker = GameObject.Instantiate(setPointMarker);
            createPointMarker.name = registerName;
            createPointMarker.transform.parent = landmarkMarkers.transform;
            // 既存のボタンの削除
            var deleteData = DeletePoint();
            var deleteButtonName = deleteData.deleteButtonName;
            var isAdded = lineOfSightDataComponent.AddPointDict(LineOfSightType.landmark, registerName, createPointMarker.transform.position);
            if (isAdded)
            {
                return (deleteButtonName, registerName);
            }
            else
            {
                edit_Landmark.Q<TextField>("EditLandmarkName").value = "同じ名前のポイントが存在します";
                GameObject.Destroy(createPointMarker);
                createPointMarker = null;
                return ("", "");
            }
        }

        /// <summary>
        /// 眺望対象を削除する
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
            var removeData = lineOfSightDataComponent.RemovePointElement(LineOfSightType.landmark, pointName);
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
        /// 眺望対象のボタンが押されたときの処理
        /// </summary>
        public void ButtonAction(string pointName)
        {
            foreach (Transform child in landmarkMarkers.transform)
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
                new GameObject("Landmark_Icon");
            }
            createPointMarker = GameObject.Instantiate(setPointMarker);
            createPointMarker.transform.position = editPointMarkerPos;
            createPointMarker.name = editPointName;
            createPointMarker.transform.parent = landmarkMarkers.transform;
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
