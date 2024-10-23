using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Landscape2.Runtime.UiCommon;

namespace Landscape2.Runtime.LandscapePlanLoader
{
    /// <summary>
    /// 景観区画のエリア情報編集用UIパネルのプレゼンタークラス
    /// </summary>
    public class Panel_AreaPlanningEdit : Panel_AreaPlanningEditBaseUI
    {
        private readonly AreaEditManager areaEditManager;
        private AreaPlanningEdit areaPlanningEdit;
        private VisualElement panel_ViewFix; // 視点固定用パネル

        public Panel_AreaPlanningEdit(VisualElement planning, PlanningUI planningUI) : base(planning, planningUI)
        {
            // 親のパネルを取得
            panel_AreaPlanningEdit = planning.Q<VisualElement>("Panel_AreaPlanningEdit");

            areaEditManager = new AreaEditManager();
            areaPlanningEdit = new AreaPlanningEdit(areaEditManager,displayPinLine);

            // リスト要素クリック時に編集対象を更新
            planningUI.OnFocusedAreaChanged += RefreshEditor;

            // 頂点データ編集用Panelを生成
            panel_PointEditor = new UIDocumentFactory().CreateWithUxmlName("Panel_EditVertices");
            GameObject.Find("Panel_EditVertices").GetComponent<UIDocument>().sortingOrder = -1;
            panel_PointEditor.style.display = DisplayStyle.None;

            // 頂点編集中に視点が動かないようにするためのパネル
            panel_ViewFix = planning.Q<VisualElement>("Panel_EditPoint");
            panel_ViewFix.RegisterCallback<MouseDownEvent>(ev => OnClickPanel());
            panel_ViewFix.RegisterCallback<MouseMoveEvent>(ev => OnDragPanel());
            panel_ViewFix.RegisterCallback<MouseUpEvent>(ev => OnReleasePanel());
            panel_ViewFix.style.display = DisplayStyle.None;

            panel_PointEditor.RegisterCallback<MouseUpEvent>(ev => panel_ViewFix.style.display = DisplayStyle.Flex);

            base.InitializeUI();
            base.RegisterCommonCallbacks();

        }

        /// <summary>
        /// 景観計画区域編集パネルが開かれたときの処理
        /// </summary>
        protected override void OnDisplayPanel()
        {
            if (panel_AreaPlanningEdit.style.display == DisplayStyle.Flex)
            {
                panel_PointEditor.style.display = DisplayStyle.Flex;
                panel_ViewFix.style.display = DisplayStyle.Flex;
                areaPlanningEdit.CreatePinline();
                base.CreateSnackbar("頂点ピンをドラッグすると形状を編集できます");
            }
            else
            {
                areaPlanningEdit.ClearVertexEdit();
                panel_PointEditor.style.display = DisplayStyle.None;
                panel_ViewFix.style.display = DisplayStyle.None;
                base.RemoveSnackbar();
            }
        }

        /// <summary>
        /// 制限高さの数値を増やすボタンの処理
        /// </summary>
        protected override void IncrementHeight()
        {
            if(areaEditManager.GetLimitHeight() == null) return;
            areaEditManager.ChangeHeight((float)areaEditManager.GetLimitHeight() + 1);  //インクリメント
            areaPlanningHeight.value = areaEditManager.GetLimitHeight().ToString(); //テキストフィールドに反映
        }

        /// <summary>
        /// 制限高さの数値を減らすボタンの処理
        /// </summary>
        protected override void DecrementHeight()
        {
            if(areaEditManager.GetLimitHeight() == null) return;
            areaEditManager.ChangeHeight((float)areaEditManager.GetLimitHeight() - 1);  //デクリメント
            areaPlanningHeight.value = areaEditManager.GetLimitHeight().ToString(); //テキストフィールドに反映
        }

        /// <summary>
        /// 制限高さの数値が直接入力されたときの処理
        /// </summary>
        /// <param name="evt"> 変更内容に関するデータ </param>
        protected override void InputHeight(ChangeEvent<string> evt)
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
        protected override void InputAreaName(ChangeEvent<string> evt)
        {
            areaEditManager.ChangeAreaName(evt.newValue);
        }

        /// <summary>
        /// エリアの色彩編集を行う処理
        /// </summary>
        protected override void EditColor()
        {
            if(isColorEditing)
            {
                base.EditColor();

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
        /// キャンセルボタンを押したときの処理
        /// </summary>
        protected override void OnCancelButtonClicked()
        {
            areaPlanningEdit.ClearVertexEdit();
            //areaPlanningEdit.CreatePinline();
        }

        /// <summary>
        /// OKボタンを押したときの処理
        /// </summary>
        protected override void OnOKButtonClicked()
        {
           // 色彩変更画面を閉じる
            isColorEditing = false;
            EditColor();

           // 頂点の編集を適用
            areaPlanningEdit.ConfirmEditData();
            //データベースに変更確定を反映
            areaEditManager.ConfirmUpdatedProperty();   
           planningUI.InvokeOnChangeConfirmed();

        }

        /// <summary>
        /// 頂点編集パネルをクリックしたときの処理
        /// </summary>
        private void OnClickPanel()
        {
            if (areaPlanningEdit.IsClickPin()) // ピンをクリック 
            { 
                panel_ViewFix.style.display = DisplayStyle.Flex;
            }
            else if(areaPlanningEdit.IsClickLine()) // ラインをクリック
            {
                panel_ViewFix.style.display = DisplayStyle.Flex;
                // 中点に頂点を追加
                areaPlanningEdit.AddVertexToLine();
            }
            else
            {
                panel_ViewFix.style.display = DisplayStyle.None;
            }
        }

        /// <summary>
        /// 頂点編集パネルをドラッグしたときの処理
        /// </summary>
        private void OnDragPanel()
        {
            areaPlanningEdit.OnDragPin();
        }

        /// <summary>
        /// 頂点編集パネルのクリックを解除したときの処理
        /// </summary>
        private void OnReleasePanel()
        {
            areaPlanningEdit.OnReleasePin();
            panel_ViewFix.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// 編集用UXMLのパラメータ情報を更新
        /// </summary>
        /// <param name = "newIndex" > 新規に表示する地区データのリスト番号 </ param >
        void RefreshEditor(int newIndex)
        {
            // 編集中の内容を破棄
            areaEditManager.ResetProperty();

            // 色彩変更画面を閉じる
           isColorEditing = false;
            EditColor();
            //isVertexEditing = false;
            // 頂点編集の内容を破棄
            areaPlanningEdit.ClearVertexEdit();

            // 編集対象を更新
            areaEditManager.SetEditTarget(newIndex);

            // 新しい編集対象のデータをUIに反映
            string name = areaEditManager.GetAreaName();
            float? height = areaEditManager.GetLimitHeight();
            areaPlanningName.value = name == null ? "" : name;
            areaPlanningHeight.value = height == null ? "" : height.ToString();
            areaPlanningColor.style.backgroundColor = areaEditManager.GetColor();
        }
    }
}
