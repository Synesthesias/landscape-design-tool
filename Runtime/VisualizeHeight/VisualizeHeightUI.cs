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


        public VisualizeHeightUI(VisualizeHeight visualizeHeight,VisualElement uiRoot)
        {
            this.visualizeHeight = visualizeHeight;

            // 高さ可視化用のPanelを生成
            visualizeHeightUXML = Resources.Load<VisualTreeAsset>("HeightHUD");
            
            visualizeHeightPanel = new UIDocumentFactory().CreateWithUxmlName("HeightHUD");
            visualizeHeightPanel.style.display = DisplayStyle.None;
            heightPin = visualizeHeightPanel.Q<VisualElement>(UIHeightPin);
            heightToggle = uiRoot.Q<Toggle>(UIHeightToggle);
            heightSlider = uiRoot.Q<SliderInt>(UIHeightSlider);

            // uxmlのSortOrderを設定
            GameObject.Find("HeightHUD").GetComponent<UIDocument>().sortingOrder = -1;

            // 可視化下限スライダーの初期設定
            heightSlider.visible = false;
            heightSlider.value = 10; // 初期値は10m


            SetHeightPin();
            // 元の高さピンを非表示にする
            heightPin.visible = false;

            // 高さ可視化トグルのイベント登録
            heightToggle.RegisterValueChangedCallback((evt) =>
            {
                if (evt.newValue == true)
                {
                    visualizeHeightPanel.style.display = DisplayStyle.Flex;
                    heightSlider.visible = true;
                    // 高さ可視化がONの場合は高さピンを表示
                    UpdateHeightPin(heightSlider.value);
                }
                else
                {
                    // 非表示に切り替え
                    visualizeHeightPanel.style.display = DisplayStyle.None;
                    heightSlider.visible = false;
                }
            });

            // 可視化下限スライダーのイベント登録
            heightSlider.RegisterValueChangedCallback((evt) =>
            {
                UpdateHeightPin(evt.newValue);
            });
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
            }
            // 建物の高さでソート
            sortedBldgDir = bldgDir.OrderByDescending(pin => float.Parse(pin.Value.Q<Label>().text));
        }

        // 高さピンを更新
        private void UpdateHeightPin(float heightLimit)
        {
            // 現在表示されている高さピンを非表示にしてリストを初期化
            foreach (var bldgItem in bldgDir)
            {
                bldgItem.Value.style.display = DisplayStyle.None;
            }
            heightPinList.Clear();

            foreach (var bldgItem in sortedBldgDir)
            {
                var height = float.Parse(bldgItem.Value.Q<Label>().text);
                // 高さ上限を下回った場合は終了
                if (height < heightLimit)
                {
                    break;
                }

                // 建物のTransform.positionをScreen座標に変換
                var bldgBounds = bldgItem.Key.transform.GetComponent<MeshCollider>().bounds;
                var topPos = new Vector3(bldgBounds.center.x, bldgBounds.max.y, bldgBounds.center.z);
                var screenPos = RuntimePanelUtils.CameraTransformWorldToPanel(visualizeHeightPanel.panel, topPos, Camera.main);

                var vp = Camera.main.WorldToViewportPoint(topPos);
                bool isActive = vp.x >= 0.0f && vp.x <= 1.0f && vp.y >= 0.0f && vp.y <= 1.0f && vp.z >= 0.0f;
                bool isInScreen = screenPos.x > pinOffsetx && screenPos.x < Screen.width - pinOffsetx && screenPos.y > headerOffset + pinOffsety;
                bool isNearCamera = Vector2.Distance(new Vector2(topPos.x,topPos.z), new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.z)) < cameraDistance;

                // ヘッダーパネルをのぞくScreenに映る範囲内の場合は表示
                if (isActive && isInScreen && isNearCamera)
                {
                    bldgItem.Value.style.display = DisplayStyle.Flex;
                    heightPinList.Add(bldgItem.Value);

                    //HeightPinの位置を設定
                    var xPos = screenPos.x - pinOffsetx;
                    var yPos = screenPos.y - pinOffsety;
                    bldgItem.Value.style.translate = new Translate() { x = xPos, y = yPos };
                }
            }
        }
        public void Start()
        {
        }
        public void Update(float deltaTime)
        {
            if (heightToggle.value == true)
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
                    // 視点が変更された場合は高さピンを更新
                    if (isCameraMoved == true)
                    {
                        visualizeHeightPanel.style.display = DisplayStyle.Flex;
                        UpdateHeightPin(heightSlider.value);
                        isCameraMoved = false;
                    }
                }
            }
        }
        public void OnEnable()
        {
        }
        public void OnDisable()
        {
        }
    }
}
