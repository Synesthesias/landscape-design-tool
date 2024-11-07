using PLATEAU.CityGML;
using PLATEAU.CityInfo;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using Landscape2.Runtime.UiCommon;
using System.ComponentModel;

namespace Landscape2.Runtime
{
    /// <summary>
    /// 高さ可視化機能のUI
    /// </summary>
    public class VisualizeHeightUI : ISubComponent
    {
        private readonly LandscapeCamera landscapeCamera;
        private readonly VisualizeHeight visualizeHeight;
        private VisualElement visualizeHeightPanel;    // 高さ可視化Panel
        private readonly VisualTreeAsset visualizeHeightUXML; // 高さピンのHUD用uxml

        // GlobalNavi_Main用
        private readonly Toggle heightToggle; // 高さ可視化トグル
        private const string UIHeightToggle = "Toggle_HeightDisplay"; // 高さ可視化トグル名前
        private const float headerOffset = 145.0f; // ヘッダーパネルの高さのオフセット
        private SliderInt heightSlider; // 可視化下限スライダー
        private const string UIHeightSlider = "HeightSlider"; // 可視化下限スライダー名前

        // VisualizeHeight用
        private VisualElement heightPin; // 高さピン
        private VisualElement heightPinClone; // 高さピンのクローン
        private const string UIHeightPin = "HeightPin"; // 高さピン名前
        private const float pinOffsetx = 40.0f; // 高さピンのオフセット(width/2)
        private const float pinOffsety = 125.0f; // 高さピンのオフセット(height)

        // 現在表示されている高さピンのリスト
        private List<VisualElement> heightPinList = new List<VisualElement>();
        // 建物と高さピンの対応リスト
        private Dictionary<PLATEAUCityObjectGroup, VisualElement> bldgDir = new Dictionary<PLATEAUCityObjectGroup, VisualElement>();
        // 高さでソートされた建物と高さピンの対応リスト
        private IOrderedEnumerable<KeyValuePair<PLATEAUCityObjectGroup, VisualElement>> sortedBldgDir;
        // 建物のリスト
        private List<PLATEAUCityObjectGroup> buildingList = new List<PLATEAUCityObjectGroup>();

        // 視点が変更されたかどうか
        private bool isCameraMoved = false;
        // 高さ表示するカメラまでの距離
        private const float cameraDistance = 500f;
        // 現在選択している建物
        private PLATEAUCityObjectGroup selectedBuilding;

        public VisualizeHeightUI(VisualizeHeight visualizeHeight,VisualElement uiRoot,LandscapeCamera landscapeCamera)
        {
            this.visualizeHeight = visualizeHeight;
            this.landscapeCamera = landscapeCamera;
            landscapeCamera.OnSetCameraCalled += HandleSetCameraCalled;

            // 高さ可視化用のUXMLを生成
            visualizeHeightUXML = Resources.Load<VisualTreeAsset>("HeightHUD");
            
            visualizeHeightPanel = new UIDocumentFactory().CreateWithUxmlName("HeightHUD");
            visualizeHeightPanel.style.display = DisplayStyle.Flex;
            visualizeHeightPanel.RegisterCallback<ClickEvent>(evt => OnPanelClick());
            heightPin = visualizeHeightPanel.Q<VisualElement>(UIHeightPin);
            heightToggle = uiRoot.Q<Toggle>(UIHeightToggle);
            heightSlider = uiRoot.Q<SliderInt>(UIHeightSlider);

            // uxmlのSortOrderを設定
            GameObject.Find("HeightHUD").GetComponent<UIDocument>().sortingOrder = -1;

            // 可視化下限スライダーの初期設定
            heightSlider.visible = false;
            heightSlider.value = 10; // 初期値は10m

            heightSlider.RegisterValueChangedCallback((evt) => {
                UpdateHeightLimit(evt.newValue);
                UpdateHeightPinDisplay();
            });

            SetHeightPin();
            // 元の高さピンを非表示にする
            heightPin.visible = false;

            // 高さ可視化トグルのイベント登録
            heightToggle.RegisterValueChangedCallback((evt) =>
            {
                if (evt.newValue == true)
                {
                    heightSlider.visible = true;
                    UpdateHeightLimit(heightSlider.value);
                    UpdateHeightPinDisplay();
                }
                else
                {
                    ResetHeightPinDisplay();
                    heightSlider.visible = false;
                }
            });
        }

        // カメラの状態が変更されたら呼び出される関数
        private void HandleSetCameraCalled()
        {
            var cameraState = landscapeCamera.cameraState;
            // 高さピンの表示をリセット
            ResetHeightPinDisplay();
            heightPinList.Clear();

            // 歩行者モードの場合建物選択による高さ表示を有効にする
            if (cameraState != LandscapeCameraState.PointOfView)
            {
                heightToggle.visible = false;
                heightToggle.value = false;
                heightSlider.visible = false;
                visualizeHeightPanel.pickingMode = PickingMode.Position;
            }
            else
            {
                heightToggle.visible = true;
                heightSlider.visible = true;
                visualizeHeightPanel.pickingMode = PickingMode.Ignore;
                selectedBuilding = null;
            }
        }

        // 高さピンの初期化
        private void SetHeightPin()
        {
            // すべての建物を取得
            buildingList = visualizeHeight.GetBuildingList();
            
            foreach (var building in buildingList)
            {
                heightPinClone = visualizeHeightUXML.CloneTree().Q<VisualElement>(UIHeightPin);
                bldgDir.Add(building, heightPinClone);
                // 高さピンを複製
                visualizeHeightPanel.Add(heightPinClone);

                // 建物の高さを設定
                string height = visualizeHeight.GetBuildingHeight(building);
                bldgDir[building].Q<Label>().text = height;
                // 高さピンを非表示にする
                bldgDir[building].style.display = DisplayStyle.None;                
            }
            // 建物の高さでソート
            sortedBldgDir = bldgDir.OrderByDescending(pin => float.Parse(pin.Value.Q<Label>().text));
        }

        // 高さ上限の更新
        private void UpdateHeightLimit(float heightLimit)
        {
            // 高さピンのリストを初期化
            //ResetHeightPinDisplay();
            heightPinList.Clear();

            foreach (var bldgItem in sortedBldgDir)
            {
                var height = float.Parse(bldgItem.Value.Q<Label>().text);
                // 高さ上限を下回った場合は終了
                if (height < heightLimit)
                {
                    break;
                }
                heightPinList.Add(bldgItem.Value);
            }       
        }

        // 高さピンの位置を更新
        private void UpdateHeightPinDisplay()
        {
            // 現在表示されている高さピンを非表示
            ResetHeightPinDisplay();

            foreach (var heightPin in heightPinList)
            {
                // 建物のTransform.positionをScreen座標に変換
                var bldgBounds = bldgDir.FirstOrDefault(kvp => kvp.Value == heightPin).Key.transform.GetComponent<MeshCollider>().bounds;
                var topPos = new Vector3(bldgBounds.center.x, bldgBounds.max.y, bldgBounds.center.z);
                var screenPos = RuntimePanelUtils.CameraTransformWorldToPanel(visualizeHeightPanel.panel, topPos, Camera.main);

                var vp = Camera.main.WorldToViewportPoint(topPos);
                bool isActive = vp.x >= 0.0f && vp.x <= 1.0f && vp.y >= 0.0f && vp.y <= 1.0f && vp.z >= 0.0f;
                bool isInScreen = screenPos.x > pinOffsetx && screenPos.x < Screen.width - pinOffsetx && screenPos.y > headerOffset + pinOffsety;
                bool isNearCamera = Vector2.Distance(new Vector2(topPos.x,topPos.z), new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.z)) < cameraDistance;

                // ヘッダーパネルをのぞくScreenに映る範囲内の場合は表示
                if (isActive && isInScreen && isNearCamera)
                {
                    DisplayHeightPin(heightPin, screenPos);
                }
            }
        }

        // 高さピンを表示する関数
        private void DisplayHeightPin(VisualElement pin , Vector2 displayPos)
        {
            pin.style.display = DisplayStyle.Flex;

            //HeightPinの位置を設定
            var xPos = displayPos.x - pinOffsetx;
            var yPos = displayPos.y - pinOffsety;
            pin.style.translate = new Translate() { x = xPos, y = yPos };
        }

        // 高さピンを全て非表示にする関数
        private void ResetHeightPinDisplay()
        {
            foreach (var bldg in bldgDir)
            {
                bldg.Value.style.display = DisplayStyle.None;
            }
        }

        // 歩行者モードにおいて高さ可視化パネルがクリックされた場合の処理
        private void OnPanelClick()
        {
            if (landscapeCamera.cameraState != LandscapeCameraState.Walker) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();

            if (Physics.Raycast(ray, out hit))
            {
                // 建築物をクリックした場合
                if (hit.collider.gameObject.name.Contains("bldg_"))
                {
                    var targetObject = hit.collider.gameObject;
                    // 選択した建物の高さを表示
                    OnBuildingSelected(targetObject);
                }
            }
        }

        // 歩行者モードにおいて建物が選択された場合の処理
        public void OnBuildingSelected(GameObject targetObject)
        {
            //ResetHeightPinDisplay();
            heightPinList.Clear();

            var building = targetObject.GetComponent<PLATEAUCityObjectGroup>();
            if (building == null) return;

            if (building == selectedBuilding)
            {
                selectedBuilding = null;
            }
            else
            {
                selectedBuilding = building;
                heightPinList.Add(bldgDir[building]);
            }
            UpdateHeightPinDisplay();
        }

        // 視点が変更された場合の処理
        private void UpdateHeightView()
        {
            // 視点が変更された場合は高さピンを更新
            if (Camera.main.transform.hasChanged)
            {
                visualizeHeightPanel.style.display = DisplayStyle.None;
                // 視点が変更されたフラグを立てる
                isCameraMoved = true;
                Camera.main.transform.hasChanged = false;
            }
            else
            {
                if (isCameraMoved == true)
                {
                    visualizeHeightPanel.style.display = DisplayStyle.Flex;
                    if (landscapeCamera.cameraState == LandscapeCameraState.Walker || heightToggle.value == true)
                    {
                        UpdateHeightPinDisplay();
                    }
                    isCameraMoved = false;
                }
            }          
        }
        public void Start()
        {
        }
        public void Update(float deltaTime)
        {
            // 高さピンの表示を更新
            UpdateHeightView();
        }
        public void OnEnable()
        {
        }
        public void OnDisable()
        {
        }
    }
}
