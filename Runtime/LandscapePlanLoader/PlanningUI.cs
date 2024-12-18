using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;

namespace Landscape2.Runtime.LandscapePlanLoader
{
    /// <summary>
    /// 景観区画の編集画面UIのプレゼンタークラス
    /// </summary>
    public class PlanningUI : ISubComponent
    {
        private readonly VisualElement title_AreaPlanningInfo;
        private readonly VisualElement title_AreaPlanningList;
        private readonly VisualElement title_AreaPlanningEditMenu;
        private readonly VisualElement panel_AreaPlanningInfo;
        private readonly VisualElement panel_AreaPlanningList;
        private readonly VisualElement panel_AreaPlanningMenu;
        private readonly VisualElement panel_AreaPlanningSubMenu;
        private readonly VisualElement panel_AreaPlanningRegister;
        private readonly VisualElement panel_AreaPlanningEdit;

        private Color? pickedColor = null;


        // 選択中のエリアのindex番号
        public int currentFocusedAreaIndex { get; private set; }    // -1: エリア未選択時

        public event Action<int> OnFocusedAreaChanged = delegate { };   // リストからエリアが選択されたときのイベント
        public event Action OnChangeConfirmed = delegate { };   // エリア編集が確定されたときのイベント


        /// <summary>
        /// パネル表示のステータス
        /// </summary>
        public enum PlanningPanelStatus
        {
            Default,
            ListForcused,
            RegisterAreaMain,
            RegisterAreaColor,
            EditAreaMain,
            EditAreaColor
        }

        public PlanningUI(VisualElement planning)
        {
            currentFocusedAreaIndex = -1;   // エリアが選択されていない状態に設定

            // 各UIパネル制御クラスのインスタンス生成
            new Panel_AreaPlanningListUI(planning, this);
            new Panel_AreaPlanningMenuUI(planning, this);
            new Panel_AreaPlanningSubMenuUI(planning, this);
            new Panel_AreaPlanningInfoUI(planning, this);
            new Panel_AreaPlanningRegister(planning, this);
            new Panel_AreaPlanningEdit(planning, this);

            // 各UIパネルのルートを取得
            title_AreaPlanningInfo = planning.Q<VisualElement>("LeftUpper").Q<VisualElement>("Title_Left");
            title_AreaPlanningList = planning.Q<VisualElement>("LeftLower").Q<VisualElement>("Title_Left");
            title_AreaPlanningEditMenu = planning.Q<VisualElement>("RightUpper").Q<VisualElement>("Title_Right");
            panel_AreaPlanningInfo = planning.Q<VisualElement>("Panel_AreaPlanningInfo");
            panel_AreaPlanningList = planning.Q<VisualElement>("Panel_AreaPlanningList");
            panel_AreaPlanningMenu = planning.Q<VisualElement>("Panel_AreaPlanningMenu");
            panel_AreaPlanningSubMenu = planning.Q<VisualElement>("Panel_AreaPlanningSubMenu");
            panel_AreaPlanningRegister = planning.Q<VisualElement>("Panel_AreaPlanningRegister");
            panel_AreaPlanningEdit = planning.Q<VisualElement>("Panel_AreaPlanningEdit");

            // 選択中のエリアが変更されたときにUIを切り替え
            OnFocusedAreaChanged += index =>
            {
                if (index == -1) ChangePlanningPanelDisplay(PlanningPanelStatus.Default);   //エリア未選択時
                else ChangePlanningPanelDisplay(PlanningPanelStatus.ListForcused);  //エリア選択時
            };
            // エリア編集が確定されたときに、UIをリスト選択時の画面に戻す
            OnChangeConfirmed += () => ChangePlanningPanelDisplay(PlanningUI.PlanningPanelStatus.ListForcused);

            InvokeOnFocusedAreaChanged(-1); // エリア未選択の状態でUIを初期化
        }

        /// <summary>
        /// 景観区画編集画面のパネル表示を切り替えるメソッド
        /// </summary>
        /// <param name="status"> 表示するパネルステータス </param>
        public void ChangePlanningPanelDisplay(PlanningPanelStatus status)
        {
            switch (status)
            {
                // 初期状態
                case PlanningPanelStatus.Default:
                    title_AreaPlanningInfo.style.display = DisplayStyle.Flex;
                    title_AreaPlanningList.style.display = DisplayStyle.Flex;
                    title_AreaPlanningEditMenu.style.display = DisplayStyle.Flex;
                    panel_AreaPlanningInfo.style.display = DisplayStyle.Flex;
                    panel_AreaPlanningList.style.display = DisplayStyle.Flex;
                    panel_AreaPlanningMenu.style.display = DisplayStyle.Flex;
                    panel_AreaPlanningSubMenu.style.display = DisplayStyle.None;
                    panel_AreaPlanningRegister.style.display = DisplayStyle.None;
                    panel_AreaPlanningEdit.style.display = DisplayStyle.None;
                    break;

                // リストからエリアが選択された状態
                case PlanningPanelStatus.ListForcused:
                    title_AreaPlanningInfo.style.display = DisplayStyle.Flex;
                    title_AreaPlanningList.style.display = DisplayStyle.Flex;
                    title_AreaPlanningEditMenu.style.display = DisplayStyle.Flex;
                    panel_AreaPlanningInfo.style.display = DisplayStyle.Flex;
                    panel_AreaPlanningList.style.display = DisplayStyle.Flex;
                    panel_AreaPlanningMenu.style.display = DisplayStyle.Flex;
                    panel_AreaPlanningSubMenu.style.display = DisplayStyle.Flex;
                    panel_AreaPlanningRegister.style.display = DisplayStyle.None;
                    panel_AreaPlanningEdit.style.display = DisplayStyle.None;
                    break;

                // エリア作成時の状態
                case PlanningPanelStatus.RegisterAreaMain:
                    title_AreaPlanningInfo.style.display = DisplayStyle.Flex;
                    title_AreaPlanningList.style.display = DisplayStyle.Flex;
                    title_AreaPlanningEditMenu.style.display = DisplayStyle.Flex;
                    panel_AreaPlanningInfo.style.display = DisplayStyle.Flex;
                    panel_AreaPlanningList.style.display = DisplayStyle.Flex;
                    panel_AreaPlanningMenu.style.display = DisplayStyle.None;
                    panel_AreaPlanningSubMenu.style.display = DisplayStyle.None;
                    panel_AreaPlanningRegister.style.display = DisplayStyle.Flex;
                    panel_AreaPlanningEdit.style.display = DisplayStyle.None;
                    break;

                // エリア作成時の色彩変更時の状態
                case PlanningPanelStatus.RegisterAreaColor:
                    title_AreaPlanningInfo.style.display = DisplayStyle.None;
                    title_AreaPlanningList.style.display = DisplayStyle.None;
                    title_AreaPlanningEditMenu.style.display = DisplayStyle.Flex;
                    panel_AreaPlanningInfo.style.display = DisplayStyle.None;
                    panel_AreaPlanningList.style.display = DisplayStyle.None;
                    panel_AreaPlanningMenu.style.display = DisplayStyle.None;
                    panel_AreaPlanningSubMenu.style.display = DisplayStyle.None;
                    panel_AreaPlanningRegister.style.display = DisplayStyle.Flex;
                    panel_AreaPlanningEdit.style.display = DisplayStyle.None;
                    break;
                // エリア編集時の状態
                case PlanningPanelStatus.EditAreaMain:
                    title_AreaPlanningInfo.style.display = DisplayStyle.Flex;
                    title_AreaPlanningList.style.display = DisplayStyle.Flex;
                    title_AreaPlanningEditMenu.style.display = DisplayStyle.Flex;
                    panel_AreaPlanningInfo.style.display = DisplayStyle.Flex;
                    panel_AreaPlanningList.style.display = DisplayStyle.Flex;
                    panel_AreaPlanningMenu.style.display = DisplayStyle.None;
                    panel_AreaPlanningSubMenu.style.display = DisplayStyle.None;
                    panel_AreaPlanningRegister.style.display = DisplayStyle.None;
                    panel_AreaPlanningEdit.style.display = DisplayStyle.Flex;
                    break;

                // エリア編集時の色彩変更時の状態
                case PlanningPanelStatus.EditAreaColor:
                    title_AreaPlanningInfo.style.display = DisplayStyle.None;
                    title_AreaPlanningList.style.display = DisplayStyle.None;
                    title_AreaPlanningEditMenu.style.display = DisplayStyle.Flex;
                    panel_AreaPlanningInfo.style.display = DisplayStyle.None;
                    panel_AreaPlanningList.style.display = DisplayStyle.None;
                    panel_AreaPlanningMenu.style.display = DisplayStyle.None;
                    panel_AreaPlanningSubMenu.style.display = DisplayStyle.None;
                    panel_AreaPlanningRegister.style.display = DisplayStyle.None;
                    panel_AreaPlanningEdit.style.display = DisplayStyle.Flex;
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// colorを保持する
        /// </summary>
        /// <param name="col"></param>
        public void PushColorStack(Color col)
        {
            pickedColor = col;
        }

        /// <summary>
        /// 保持したcolorの値を返す
        /// </summary>
        /// <returns></returns>
        public Color? PopColorStack()
        {
            return pickedColor;
        }

        /// <summary>
        /// エリアが選択されたときの処理を発火するメソッド
        /// </summary>
        /// <param name="index">選択されたエリアのindex番号</param>
        public void InvokeOnFocusedAreaChanged(int index)
        {
            currentFocusedAreaIndex = index;
            OnFocusedAreaChanged(index);
        }

        /// <summary>
        /// エリア編集が確定されたときの処理を発火するメソッド
        /// </summary>
        public void InvokeOnChangeConfirmed()
        {
            OnChangeConfirmed();
        }

        public void Start()
        {
        }
        public void Update(float deltaTime)
        {
        }
        public void OnEnable()
        {
        }
        public void OnDisable()
        {
        }

        public void LateUpdate(float deltaTime)
        {
        }

    }
}
