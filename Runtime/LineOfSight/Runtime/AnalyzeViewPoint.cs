using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Landscape2.Runtime
{
    /// <summary>
    /// 視点場解析モードにおいてクリックの動作を制御する列挙型
    /// </summary>
    public enum ViewPointSetMode
    {
        None,
        landmark,
        viewPoint
    }

    /// <summary>
    /// 視点場解析に必要なデータの構造体
    /// </summary>
    public struct AnalyzeViewPointElements
    {
        // 0 ~ 180(度数)
        public int rangeWidth;
        // 0 ~ 50(m)
        public int rangeHeight;
        public string startPosName;
        public string endPosName;
        public Vector3 startPos;
        public Vector3 endPos;
        public Color lineColorValid;
        public Color lineColorInvalid;
        public static readonly AnalyzeViewPointElements Empty = new AnalyzeViewPointElements
        {
            rangeWidth = 0,
            rangeHeight = 0,
            startPosName = "",
            endPosName = "",
            startPos = Vector3.zero,
            endPos = Vector3.zero,
            lineColorValid = Color.clear,
            lineColorInvalid = Color.clear
        };
    }

    /// <summary>
    /// 視点場の解析に関するクラス
    /// </summary>
    public class AnalyzeViewPoint : LineOfSightModeClass
    {
        private Camera cam;
        private Ray ray;
        private ViewPointSetMode viewPointSetMode;
        private VisualElement viewPointListPanel;
        private VisualElement viewPointListPanelTitle;
        private VisualElement landMarkListPanel;
        private VisualElement landMarkListPanelTitle;
        private VisualElement analyzeSettingPanel;
        private VisualElement analyzeSettingPanelTitle;

        private LineOfSightUI.ViewStateControl viewPointList_View;
        private LineOfSightUI.ViewStateControl landmarkList_View;
        private LineOfSightUI.ViewStateControl analyzeSettingPanel_View;


        private GameObject targetViewPoint;
        private GameObject targetLandmark;
        private AnalyzeViewPointElements analyzeViewPointData;
        private AnalyzeViewPointElements editViewPointData;
        private LineOfSightDataComponent lineOfSightDataComponent;
        private const string ObjNameLineOfSight = "LineOfSight";

        private readonly Color lineColorValid = new(0, 191f / 255f, 1f, 0f);
        private readonly Color lineColorInvalid = new(1f, 140f / 255f, 0f, 0.2f);

        public AnalyzeViewPoint(LineOfSightDataComponent lineOfSightDataComponentInstance)
        {
            analyzeViewPointData = new AnalyzeViewPointElements();
            lineOfSightDataComponent = lineOfSightDataComponentInstance;
        }

        public override void OnEnable(VisualElement element)
        {
            viewPointListPanel = element.Q<VisualElement>("ViewPointList");
            viewPointListPanelTitle = element.Q<VisualElement>("Title_ViewList");
            landMarkListPanel = element.Q<VisualElement>("LandMarkList");
            landMarkListPanelTitle = element.Q<VisualElement>("Title_LandmarkList");
            analyzeSettingPanel = element.Q<VisualElement>("AnalyzeSetting");
            analyzeSettingPanelTitle = element.Q<VisualElement>("Title_AnalyzeSetting");

            viewPointList_View = new(viewPointListPanelTitle, viewPointListPanel);
            landmarkList_View = new(landMarkListPanelTitle, landMarkListPanel);
            analyzeSettingPanel_View = new(analyzeSettingPanelTitle, analyzeSettingPanel);


            targetViewPoint = null;
            targetLandmark = null;
            analyzeViewPointData.rangeWidth = 80;
            analyzeViewPointData.rangeHeight = 50;
            analyzeViewPointData.lineColorValid = lineColorValid;
            analyzeViewPointData.lineColorInvalid = lineColorInvalid;
        }

        public void ClearSetMode()
        {
            viewPointSetMode = ViewPointSetMode.None;
        }

        public void SetLandMark()
        {
            viewPointSetMode = ViewPointSetMode.landmark;
        }

        public void SetViewPoint()
        {
            viewPointSetMode = ViewPointSetMode.viewPoint;
        }

        /// <summary>
        /// クリックされたポイントを登録する
        /// </summary>
        private void SetTarget()
        {
            cam = Camera.main;
            ray = cam.ScreenPointToRay(Input.mousePosition);

            Debug.Log($"SetTarget");

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                var new_Analyze_Viewpoint = analyzeSettingPanel.Q<VisualElement>("New_Analyze_Viewpoint");
                var hitObject = hit.collider.gameObject;
                Debug.Log(hitObject.name);
                if (viewPointSetMode == ViewPointSetMode.viewPoint)
                {
                    var viewPointMarkers = GameObject.Find("ViewPointMarkers");
                    if (hitObject.transform.IsChildOf(viewPointMarkers.transform))
                    {
                        targetViewPoint = hitObject;
                        new_Analyze_Viewpoint.Q<Label>("ViewpointName").text = hitObject.name;
                        viewPointList_View.Show(false);
                        analyzeSettingPanel_View.Show(true);
                        // viewPointListPanel.style.display = DisplayStyle.None;
                        // analyzeSettingPanel.style.display = DisplayStyle.Flex;
                        ClearSetMode();
                    }
                }
                else if (viewPointSetMode == ViewPointSetMode.landmark)
                {
                    var landmarkMarkers = GameObject.Find("LandmarkMarkers");
                    if (hitObject.transform.IsChildOf(landmarkMarkers.transform))
                    {
                        targetLandmark = hitObject;
                        new_Analyze_Viewpoint.Q<Label>("LandmarkName").text = hitObject.name;
                        landmarkList_View.Show(false);
                        analyzeSettingPanel_View.Show(true);
                        // landMarkListPanel.style.display = DisplayStyle.None;
                        // analyzeSettingPanel.style.display = DisplayStyle.Flex;
                        ClearSetMode();
                    }
                }
            }
            if (targetLandmark != null && targetViewPoint != null)
            {
                CreateLineOfSight();
            }
        }

        public void UpdateTargets()
        {
            var new_Analyze_Viewpoint = analyzeSettingPanel.Q<VisualElement>("New_Analyze_Viewpoint");
            var viewpointName = new_Analyze_Viewpoint.Q<Label>("ViewpointName").text;
            var landmarkName = new_Analyze_Viewpoint.Q<Label>("LandmarkName").text;

            var viewPointMarkers = GameObject.Find("ViewPointMarkers");
            var landmarkMarkers = GameObject.Find("LandmarkMarkers");

            foreach (Transform t in viewPointMarkers.transform)
            {
                if (t.name == viewpointName)
                {
                    targetViewPoint = t.gameObject;
                    break;
                }
            }

            foreach (Transform t in landmarkMarkers.transform)
            {
                if (t.name == landmarkName)
                {
                    targetLandmark = t.gameObject;
                    break;
                }
            }

        }

        /// <summary>
        /// 見通し解析のラインを生成する(解析を行う)
        /// </summary>
        public float CreateLineOfSight()
        {
            // 見通し解析の初期化
            ClearLineOfSight();
            if (targetViewPoint == null)
            {
                UpdateTargets();
            }
            analyzeViewPointData.startPos = targetViewPoint.transform.position;
            analyzeViewPointData.startPosName = targetViewPoint.name;
            analyzeViewPointData.endPos = targetLandmark.transform.position;
            analyzeViewPointData.endPosName = targetLandmark.name;

            var obj = new GameObject(ObjNameLineOfSight);
            obj.transform.parent = targetViewPoint.transform;

            float rangeWidth = analyzeViewPointData.rangeWidth * Mathf.Deg2Rad;
            int rangeHeight = analyzeViewPointData.rangeHeight;
            Vector3 startPointPos = analyzeViewPointData.startPos;
            Vector3 endPointPos = analyzeViewPointData.endPos;

            float result = -1;
            int circumferenceInterval = Mathf.FloorToInt(rangeWidth * Mathf.Rad2Deg);
            var diff = endPointPos - startPointPos;
            float radius = Vector2.Distance(new Vector2(startPointPos.x, startPointPos.z), new Vector2(endPointPos.x, endPointPos.z));

            Vector3 targetPoint = new Vector3(0, 0, 0);
            for (int i = 0; i < circumferenceInterval; i++)
            {
                for (int j = 0; j < rangeHeight; j++)
                {
                    targetPoint.x = radius * Mathf.Sin(i * rangeWidth / circumferenceInterval - rangeWidth / 2);
                    targetPoint.y = endPointPos.y - startPointPos.y + j - rangeHeight / 2;
                    targetPoint.z = radius * Mathf.Cos(i * rangeWidth / circumferenceInterval - rangeWidth / 2);
                    // 始点から終点のベクトルの角度を求めます。
                    float angle = Vector2.SignedAngle(Vector2.up, new Vector2(diff.x, diff.z));
                    var rotator = Quaternion.AngleAxis(-angle, Vector3.up);
                    targetPoint = rotator * targetPoint + startPointPos;

                    RaycastHit hit;
                    if (RaycastBuildings(targetViewPoint, targetPoint, out hit))
                    {
                        DrawLine(targetViewPoint.transform.position, hit.point, obj, analyzeViewPointData.lineColorValid);
                        DrawLine(hit.point, targetPoint, obj, analyzeViewPointData.lineColorInvalid);
                    }
                    else
                    {
                        DrawLine(targetViewPoint.transform.position, targetPoint, obj, analyzeViewPointData.lineColorValid);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 見通し解析のラインを削除する
        /// </summary>
        public void ClearLineOfSight()
        {
            if (targetViewPoint == null)
            {
                return;
            }
            var root = targetViewPoint.transform;
            for (int i = 0; i < root.childCount; ++i)
            {
                var trans = root.GetChild(i);
                string childName = trans.name;
                if (childName == ObjNameLineOfSight)
                {
                    Object.DestroyImmediate(trans.gameObject);
                }
            }
        }

        /// <summary>
        /// ラインを生成する
        /// </summary>
        void DrawLine(Vector3 origin, Vector3 distination, GameObject parent, Color col)
        {
            Vector3[] point = new Vector3[2];
            point[0] = origin;
            point[1] = distination;

            GameObject go = new GameObject("ViewRegurationAreaByLine");

            LineRenderer lineRenderer = go.AddComponent<LineRenderer>();

            lineRenderer.SetPositions(point);
            lineRenderer.positionCount = point.Length;
            lineRenderer.startWidth = 1.0f;
            lineRenderer.endWidth = 1.0f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

            lineRenderer.startColor = col;
            lineRenderer.endColor = col;

            go.transform.parent = parent.transform;
        }

        /// <summary>
        /// ラインが建物にあたった時の処理
        /// </summary>
        bool RaycastBuildings(GameObject startPoint, Vector3 destination, out RaycastHit hitInfo)
        {
            bool result = false;

            hitInfo = new RaycastHit();

            Vector3 direction = (destination - startPoint.transform.position).normalized;
            float distance = Vector3.Distance(startPoint.transform.position, destination);

            RaycastHit[] hits;
            hits = Physics.RaycastAll(startPoint.transform.position, direction, distance);

            float minDistance = float.MaxValue;
            if (hits.Length <= 0)
                return result;

            foreach (var hit in hits)
            {
                if (hit.collider.gameObject.name == startPoint.name)
                    continue;

                int layerIgnoreRaycast = LayerMask.NameToLayer("RegulationArea");

                if (hit.collider.gameObject.layer == layerIgnoreRaycast)
                    continue;

                result = true;

                if (hit.distance >= minDistance)
                    continue;

                hitInfo = hit;
                minDistance = hit.distance;
            }
            return result;
        }

        /// <summary>
        /// 視点場の解析結果を登録する
        /// </summary>
        public AnalyzeViewPointElements RegisterAnalyzeData()
        {
            var keyName = CreateAnalyzeDataDictKey(analyzeViewPointData);
            Debug.Log($"keyName : {keyName}");
            var isAdded = lineOfSightDataComponent.AddAnalyzeViewPoinDatatDict(keyName, analyzeViewPointData);
            if (isAdded)
            {
                return analyzeViewPointData;
            }
            else
            {
                return AnalyzeViewPointElements.Empty;
            }
        }

        /// <summary>
        /// 視点場の解析結果を編集する
        /// </summary>
        public (string deleteButtonName, AnalyzeViewPointElements analyzeViewPointData) EditAnalyzeData()
        {
            var deleteButtonName = DeleteAnalyzeData();
            var keyName = CreateAnalyzeDataDictKey(analyzeViewPointData);
            var isAdded = lineOfSightDataComponent.AddAnalyzeViewPoinDatatDict(keyName, analyzeViewPointData);

            if (isAdded)
            {
                return (deleteButtonName, analyzeViewPointData);
            }
            else
            {
                return ("", AnalyzeViewPointElements.Empty);
            }
        }

        /// <summary>
        /// 視点場の解析結果を削除する
        /// </summary>
        public string DeleteAnalyzeData()
        {
            // ゲームオブジェクトの削除
            ClearLineOfSight();
            // データの削除
            if (editViewPointData.Equals(AnalyzeViewPointElements.Empty))
            {
                return "";
            }
            var keyName = CreateAnalyzeDataDictKey(editViewPointData);
            var isRemoved = lineOfSightDataComponent.RemoveAnalyzeViewPointElement(keyName);
            editViewPointData = AnalyzeViewPointElements.Empty;
            if (isRemoved)
            {
                return keyName;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 眺望対象解析のスライダーの登録
        /// </summary>
        public void SetAnalyzeRange()
        {
            var slider_Viewpoint = analyzeSettingPanel.Q<VisualElement>("Slider_Viewpoint");
            Debug.Log(slider_Viewpoint);
            var horizontalSlider = slider_Viewpoint.Q<SliderInt>("HorizontalSlider");
            Debug.Log(horizontalSlider);
            horizontalSlider.lowValue = 10;
            horizontalSlider.highValue = 180;
            horizontalSlider.value = analyzeViewPointData.rangeWidth;
            horizontalSlider.RegisterValueChangedCallback(evt =>
            {
                analyzeViewPointData.rangeWidth = evt.newValue;
                CreateLineOfSight();
            });

            var verticalSlider = slider_Viewpoint.Q<SliderInt>("VerticalSlider");
            verticalSlider.lowValue = 0;
            verticalSlider.highValue = 150;
            verticalSlider.value = analyzeViewPointData.rangeHeight;
            verticalSlider.RegisterValueChangedCallback(evt =>
            {
                analyzeViewPointData.rangeHeight = evt.newValue;
                CreateLineOfSight();
            });
        }

        /// <summary>
        /// 登録キーの生成
        /// </summary>
        private string CreateAnalyzeDataDictKey(AnalyzeViewPointElements analyzeViewPointElements)
        {
            var key = analyzeViewPointElements.startPosName + analyzeViewPointElements.endPosName + analyzeViewPointElements.rangeWidth + analyzeViewPointElements.rangeHeight;
            return key;
        }

        /// <summary>
        /// 解析結果のボタンが押されたときの処理
        /// </summary>
        public void ButtonAction(AnalyzeViewPointElements analyzeViewPointElements)
        {
            editViewPointData = analyzeViewPointElements;
            analyzeViewPointData = analyzeViewPointElements;
            var viewPointMarkers = GameObject.Find("ViewPointMarkers");
            foreach (Transform child in viewPointMarkers.transform)
            {
                if (child.gameObject.name == analyzeViewPointData.startPosName)
                {
                    targetViewPoint = child.gameObject;
                }
            }
            var landmarkMarkers = GameObject.Find("LandmarkMarkers");
            foreach (Transform child in landmarkMarkers.transform)
            {
                if (child.gameObject.name == analyzeViewPointData.endPosName)
                {
                    targetLandmark = child.gameObject;
                }
            }
            if (targetViewPoint == null || targetLandmark == null)
            {
                return;
            }
            CreateLineOfSight();
        }

        public override void OnSelect()
        {
            Debug.Log($"OnSelect");
            SetTarget();
        }

        public override void OnDisable()
        {
        }
    }
}
