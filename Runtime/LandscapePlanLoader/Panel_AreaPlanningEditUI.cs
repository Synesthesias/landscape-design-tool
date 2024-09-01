using UnityEngine;
using UnityEngine.UIElements;

namespace Landscape2.Runtime.LandscapePlanLoader
{
    /// <summary>
    /// 景観区画のエリア情報編集用UIパネルのプレゼンタークラス
    /// </summary>
    public class Panel_AreaPlanningEditUI
    {
        private readonly VisualElement planning;
        private readonly PlanningUI planningUI;
        private readonly AreaEditManager areaEditManager;

        private readonly TextField areaPlanningName; // エリア名入力欄
        private readonly TextField areaPlanningHeight;   // 制限高さ入力欄
        private readonly VisualElement areaPlanningColor;    // 色彩表示欄

        private readonly VisualTreeAsset colorEditor;    // 色彩編集用のUXMLのテンプレート
        private VisualElement colorEditorClone; // 色彩編集用のUXMLのクローン

        private bool isColorEditing = false;


        public Panel_AreaPlanningEditUI(VisualElement planning, PlanningUI planningUI)
        {
            this.planning = planning;
            this.planningUI = planningUI;

            areaEditManager = new AreaEditManager();

            // 色編集用のUXMLのテンプレートを取得
            colorEditor = Resources.Load<VisualTreeAsset>("UIColorEditor");

            // リスト要素クリック時に編集対象を更新
            planningUI.OnFocusedAreaChanged += RefreshEditor;

            // UIに処理を登録
            VisualElement panel_AreaPlanningEdit = planning.Q<VisualElement>("Panel_AreaPlanningEdit");
            panel_AreaPlanningEdit.Q<Button>("UpButton").clicked += IncrementHeight;
            panel_AreaPlanningEdit.Q<Button>("DownButton").clicked += DecrementHeight;
            panel_AreaPlanningEdit.Q<Button>("CancelButton").RegisterCallback<ClickEvent>(ev => planningUI.InvokeOnFocusedAreaChanged(-1)); // エリアフォーカスを外す
            panel_AreaPlanningEdit.Q<Button>("OKButton").RegisterCallback<ClickEvent>(ev => ConfirmEditData());
            areaPlanningName = panel_AreaPlanningEdit.Q<TextField>("AreaPlanningName");
            areaPlanningHeight = panel_AreaPlanningEdit.Q<TextField>("AreaPlanningHeight");
            areaPlanningColor = panel_AreaPlanningEdit.Q<VisualElement>("AreaPlanningColor");
            areaPlanningHeight.RegisterValueChangedCallback(InputHeight);
            areaPlanningName.RegisterValueChangedCallback(InputAreaName);
            areaPlanningColor.RegisterCallback<ClickEvent>(ev => {
                isColorEditing = !isColorEditing;   // 色彩変更画面の表示切り替え
                EditColor();
            });
        }

        /// <summary>
        /// 編集用UXMLのパラメータ情報を更新
        /// </summary>
        /// <param name="newIndex">新規に表示する地区データのリスト番号</param>
        void RefreshEditor(int newIndex)
        {
            // 編集中の内容を破棄
            areaEditManager.ResetProperty();

            // 色彩変更画面を閉じる
            isColorEditing = false;
            EditColor();

            // 編集対象を更新
            areaEditManager.SetEditTarget(newIndex);

            // 新しい編集対象のデータをUIに反映
            string name = areaEditManager.GetAreaName();
            float? height = areaEditManager.GetLimitHeight();
            areaPlanningName.value = name == null ? "" : name;
            areaPlanningHeight.value = height == null ? "" : height.ToString();
            areaPlanningColor.style.backgroundColor = areaEditManager.GetColor();
        }

        /// <summary>
        /// 制限高さの数値を増やすボタンの処理
        /// </summary>
        void IncrementHeight()
        {
            if(areaEditManager.GetLimitHeight() == null) return;
            areaEditManager.ChangeHeight((float)areaEditManager.GetLimitHeight() + 1);  //インクリメント
            areaPlanningHeight.value = areaEditManager.GetLimitHeight().ToString(); //テキストフィールドに反映
        }

        /// <summary>
        /// 制限高さの数値を減らすボタンの処理
        /// </summary>
        void DecrementHeight()
        {
            if(areaEditManager.GetLimitHeight() == null) return;
            areaEditManager.ChangeHeight((float)areaEditManager.GetLimitHeight() - 1);  //デクリメント
            areaPlanningHeight.value = areaEditManager.GetLimitHeight().ToString(); //テキストフィールドに反映
        }
        
        /// <summary>
        /// 制限高さの数値が直接入力されたときの処理
        /// </summary>
        /// <param name="evt"> 変更内容に関するデータ </param>
        void InputHeight(ChangeEvent<string> evt)
        {
            // 入力値が数値で最大高さ以下の値の場合のみデータを更新
            if(float.TryParse(evt.newValue, out float value) && value <= areaEditManager.GetMaxHeight())
            {
                areaEditManager.ChangeHeight(value);
            }
            else
            {
                // 空欄以外の文字入力があった場合は元の値に戻す
                if(evt.newValue != "")  areaPlanningHeight.value = evt.previousValue;
            }
        }

        /// <summary>
        /// エリア名が入力されたときの処理
        /// </summary>
        /// <param name="evt"> 変更内容に関するデータ </param>
        void InputAreaName(ChangeEvent<string> evt)
        {
            areaEditManager.ChangeAreaName(evt.newValue);
        }

        /// <summary>
        /// エリアの色彩編集を行う処理
        /// </summary>
        void EditColor()
        {
            if(isColorEditing)
            {
                // 色彩変更パネルを画面中央に表示
                colorEditorClone = colorEditor.CloneTree();
                planning.Q<VisualElement>("CenterUpper").Add(colorEditorClone);

                // 色彩の変更を反映
                ColorEditorUI colorEditorUI = new ColorEditorUI(colorEditorClone, areaEditManager.GetColor()) ;
                colorEditorUI.OnColorEdited += (newColor) =>
                {
                    areaPlanningColor.style.backgroundColor = newColor;
                    areaEditManager.ChangeColor(newColor);
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
                if(colorEditorClone != null) colorEditorClone.RemoveFromHierarchy();
            }
        }

        /// <summary>
        /// 変更内容を確定・保持するメソッド
        /// </summary>
        void ConfirmEditData()
        {
            // 色彩変更画面を閉じる
            isColorEditing = false;
            EditColor();

            areaEditManager.ConfirmUpdatedProperty();   //データベースに変更確定を反映
            planningUI.InvokeOnChangeConfirmed();
        }
    }
}
