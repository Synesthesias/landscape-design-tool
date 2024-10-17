using iShape.Geometry.Polygon;
using Landscape2.Runtime.UiCommon;
using System.Collections;
using System.Collections.Generic;
using ToolBox.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Landscape2.Runtime.LandscapePlanLoader
{
    /// <summary>
    /// 景観区画作成画面のメニューパネルUIのプレゼンタークラス
    /// </summary>
    public class Panel_AreaPlanningRegisterUI
    {
        private readonly LandscapePlanLoadManager landscapePlanLoadManager;
        private readonly VisualElement planning;

        private readonly TextField areaPlanningName; // エリア名入力欄
        private readonly TextField areaPlanningHeight;   // 制限高さ入力欄
        private readonly VisualElement areaPlanningColor;    // 色彩表示欄
        private readonly VisualTreeAsset colorEditor;    // 色彩編集用のUXMLのテンプレート
        private VisualElement colorEditorClone; // 色彩編集用のUXMLのクローン
        private VisualElement regulationAreaPanel; // 景観区域作成Panel

        private bool isColorEditing = false;
        private List<Vector3> vertices = new List<Vector3>();
        private List<PlanAreaSaveData> listOfSaveData;
        private List<GameObject> pinList = new List<GameObject>();
        private PlanAreaSaveData newSaveData;

        private float limitHeight;
        private const float wallMaxHeight = 300f;
        private GameObject pin; // ピンのプレハブ

        public Panel_AreaPlanningRegisterUI(VisualElement planning, PlanningUI planningUI)
        {
            landscapePlanLoadManager = new LandscapePlanLoadManager();

            this.planning = planning;

            // 色編集用のUXMLのテンプレートを取得
            colorEditor = Resources.Load<VisualTreeAsset>("UIColorEditor");

            // UIに処理を登録
            VisualElement panel_AreaPlanningEdit = planning.Q<VisualElement>("Panel_AreaPlanningRegister");
            panel_AreaPlanningEdit.Q<Button>("UpButton").clicked += IncrementHeight;
            panel_AreaPlanningEdit.Q<Button>("DownButton").clicked += DecrementHeight;
            panel_AreaPlanningEdit.Q<Button>("CancelButton").RegisterCallback<ClickEvent>(ev => planningUI.InvokeOnFocusedAreaChanged(-1)); // エリアフォーカスを外す
            panel_AreaPlanningEdit.Q<Button>("OKButton").RegisterCallback<ClickEvent>(ev => CreateAreaData());
            areaPlanningName = panel_AreaPlanningEdit.Q<TextField>("AreaPlanningName");
            areaPlanningHeight = panel_AreaPlanningEdit.Q<TextField>("AreaPlanningHeight");
            areaPlanningColor = panel_AreaPlanningEdit.Q<VisualElement>("AreaPlanningColor");
            areaPlanningHeight.RegisterValueChangedCallback(InputHeight);
            areaPlanningColor.RegisterCallback<ClickEvent>(ev => {
                isColorEditing = !isColorEditing;   // 色彩変更画面の表示切り替え
                EditColor();
            });

            // 頂点データ作成用Panelを生成
            regulationAreaPanel = new UIDocumentFactory().CreateWithUxmlName("Panel_RegulationArea");
            regulationAreaPanel.RegisterCallback<MouseDownEvent>(ev => AddVertexIfClicked());
            GameObject.Find("Panel_RegulationArea").GetComponent<UIDocument>().sortingOrder = -1;
            regulationAreaPanel.style.display = DisplayStyle.None;

            limitHeight = 1.0f;
            areaPlanningHeight.value = limitHeight.ToString();
            areaPlanningColor.style.backgroundColor = Color.red;

            // 景観区画登録画面のメニューパネルが表示されたときに、頂点データ作成用Panelを表示
            panel_AreaPlanningEdit.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                if (panel_AreaPlanningEdit.style.display == DisplayStyle.Flex)
                {
                    regulationAreaPanel.style.display = DisplayStyle.Flex;
                }
                else 
                {
                    OnDisable();
                }
            });

            pin = Resources.Load("Pin") as GameObject;

        }

        /// <summary>
        /// クリック時に頂点生成を行うメソッド
        /// </summary>
        private void AddVertexIfClicked()
        {
            RaycastHit[] hits;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            hits = Physics.RaycastAll(ray, Mathf.Infinity);
            if (hits == null || hits.Length == 0)
                return;
            // 地面をクリックした場合
            if (hits[0].collider.gameObject.name.Contains("dem_"))
            {
                AddPin(hits[0].point);
                vertices.Add(hits[0].point);
            }
        }

        public void AddPin(Vector3 pos)
        {
            // クリックした位置にピンを生成
            var pinObj = GameObject.Instantiate(pin);
            pinObj.transform.position = pos;
            pinList.Add(pinObj);
        }

        public void ClearPin()
        {
            foreach (var pin in pinList)
            {
                GameObject.Destroy(pin);
            }
            pinList.Clear();
        }

        /// <summary>
        /// 制限高さの数値を増やすボタンの処理
        /// </summary>
        void IncrementHeight()
        {
            // 入力値が数値で最大高さ以下の値の場合のみデータを更新
            if (float.TryParse(areaPlanningHeight.value, out float value) && value <= wallMaxHeight)
            {
                limitHeight ++;
                areaPlanningHeight.value = limitHeight.ToString(); //テキストフィールドに反映
            }
        }

        /// <summary>
        /// 制限高さの数値を減らすボタンの処理
        /// </summary>
        void DecrementHeight()
        {
            // 入力値が数値で1以上の値の場合のみデータを更新
            if (float.TryParse(areaPlanningHeight.value, out float value) && value >= 1.0f)
            {
                limitHeight --;
                areaPlanningHeight.value = limitHeight.ToString(); //テキストフィールドに反映
            }
        }

        /// <summary>
        /// 制限高さの数値が直接入力されたときの処理
        /// </summary>
        /// <param name="evt"> 変更内容に関するデータ </param>
        void InputHeight(ChangeEvent<string> evt)
        {
            // 入力値が数値で最大高さ以下かつ0以上の値の場合のみデータを更新
            if (float.TryParse(evt.newValue, out float value) && value <= wallMaxHeight && value >= 1.0f)
            {
                limitHeight = value;
            }
            else
            {
                // 空欄以外の文字入力があった場合は元の値に戻す
                if (evt.newValue != "") areaPlanningHeight.value = evt.previousValue;
            }
        }

        /// <summary>
        /// エリアの色彩編集を行う処理
        /// </summary>
        void EditColor()
        {
            if (isColorEditing)
            {
                // 色彩変更パネルを画面中央に表示
                colorEditorClone = colorEditor.CloneTree();
                planning.Q<VisualElement>("CenterUpper").Add(colorEditorClone);

                // 色彩の変更を反映
                ColorEditorUI colorEditorUI = new ColorEditorUI(colorEditorClone, Color.red);
                colorEditorUI.OnColorEdited += (newColor) =>
                {
                    areaPlanningColor.style.backgroundColor = newColor;
                };
                colorEditorUI.OnCloseButtonClicked += () =>
                {
                    isColorEditing = false;
                    EditColor();
                };
            }
            else
            {
                // 色彩変更画面を閉じる
                if (colorEditorClone != null) colorEditorClone.RemoveFromHierarchy();
            }
        }

        /// <summary>
        /// 新規景観区画データを作成するメソッド
        /// </summary>
        void CreateAreaData()
        {
            if (vertices.Count < 3) return;

            string name = areaPlanningName.value;
            float height = float.Parse(areaPlanningHeight.value);
            Color color = areaPlanningColor.style.backgroundColor.value;
            List<List<Vector3>> listOfVertices = new List<List<Vector3>>();
            listOfVertices.Add(new List<Vector3>(vertices));

            // 新規景観区画データを作成
            newSaveData = new PlanAreaSaveData(
                0,
                name,
                height,
                10.0f,
                color,
                wallMaxHeight,
                listOfVertices
                );
            listOfSaveData = new List<PlanAreaSaveData>
            {
                newSaveData
            };

            landscapePlanLoadManager.LoadFromSaveData(listOfSaveData);

            OnDisable();
        }

        private void OnDisable()
        {
            vertices.Clear();
            ClearPin();
            regulationAreaPanel.style.display = DisplayStyle.None;
        }
    }
}
