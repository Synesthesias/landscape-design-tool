using UnityEngine.UIElements;

namespace Landscape2.Runtime.LandscapePlanLoader
{
    /// <summary>
    /// 景観区画編集画面のメニューパネルUIのプレゼンタークラス
    /// </summary>
    public class Panel_AreaPlanningMenuUI
    {
        private readonly LandscapePlanLoadManager landscapePlanLoadManager;

        public Panel_AreaPlanningMenuUI(VisualElement planning, PlanningUI planningUI)
        {
            landscapePlanLoadManager = new LandscapePlanLoadManager();

            // ボタンへ処理を登録
            VisualElement panel_AreaPlanningMenu = planning.Q<VisualElement>("Panel_AreaPlanningMenu");
            panel_AreaPlanningMenu.Q<Button>("ImportButton").RegisterCallback<ClickEvent>(ev => LoadSHPFile());
            panel_AreaPlanningMenu.Q<Button>("NewAreaButton")
                .RegisterCallback<ClickEvent>(ev => planningUI.ChangePlanningPanelDisplay(PlanningUI.PlanningPanelStatus.RegisterAreaMain));
        }

        /// <summary>
        /// SHPファイルから景観区画データを読み込むメソッド
        /// </summary>
        void LoadSHPFile()
        {
            string path = landscapePlanLoadManager.BrowseFolder();  // ファイル選択ダイアログを表示
            if (path != null)
            {
                landscapePlanLoadManager.LoadShapefile(path);
            }
        }
    }
}
