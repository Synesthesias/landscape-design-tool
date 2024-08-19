using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Landscape2.Runtime.LandscapePlanLoader
{
    public class Panel_AreaPlanningListUI
    {
        private readonly PlanningUI planningUI;

        // UXMLのエリアデータリストのルート
        private readonly VisualElement areaPlanningListRoot;

        // UXMLのエリアデータリスト要素のテンプレート
        private readonly VisualTreeAsset list_AreaPlanningTemplate;

        // UXMLのエリアデータリスト要素のインスタンスを管理するリスト
        private readonly List<VisualElement> list_AreaPlanningList = new List<VisualElement>();


        public Panel_AreaPlanningListUI(VisualElement planning, PlanningUI planningUI)
        {
            this.planningUI = planningUI;

            // リスト要素のルートとテンプレートの取得
            areaPlanningListRoot = planning.Q<VisualElement>("Panel_AreaPlanningList").Q<VisualElement>("unity-content-container");
            list_AreaPlanningTemplate = Resources.Load<VisualTreeAsset>("List_AreaPlanning");

            RefreshAreaPlanningList();  // エリアデータリストUIの初期化
            AreasDataComponent.AreaCountChangedEvent += RefreshAreaPlanningList;    // エリアデータ数が変更されたときにエリア一覧リストを更新

            planningUI.OnChangeConfirmed += RefreshAreaPlanningList;  // エリアの編集が確定されたときにエリア一覧リスト情報を更新
        }

        /// <summary>
        /// 景観区画リストの更新処理
        /// </summary>
        void RefreshAreaPlanningList()
        {
            // 既存の表示リストを削除
            while (list_AreaPlanningList.Count > 0)
            {
                areaPlanningListRoot.Remove(list_AreaPlanningList[0]);
                list_AreaPlanningList.RemoveAt(0);
            }

            // 読み込み済のエリアデータの総数を取得
            int areaDataCount = AreasDataComponent.GetPropertyCount();
            if (areaDataCount == 0)
            {
                // エリアデータがない場合はダイアログ「データが登録されていません」を表示
                areaPlanningListRoot.Q<Label>("Dialogue").style.display = DisplayStyle.Flex;
                return;
            }
            else
            {
                areaPlanningListRoot.Q<Label>("Dialogue").style.display = DisplayStyle.None;
            }

            // 最新のエリアデータのリスト要素インスタンスを生成して表示
            for (int index = 0; index < areaDataCount; index++)
            {
                VisualElement newAreaListInstance = list_AreaPlanningTemplate.CloneTree();
                areaPlanningListRoot.Add(newAreaListInstance);  // ListUXMLのRootに追加
                list_AreaPlanningList.Add(newAreaListInstance); // インスタンス管理用リストに追加

                // インスタンスにエリアの各属性情報を反映
                AreaProperty areaProperty = AreasDataComponent.GetProperty(index);
                newAreaListInstance.Q<VisualElement>("ColorSample").style.backgroundColor = areaProperty.color;
                newAreaListInstance.Q<Label>("AreaPlanningName").text = areaProperty.name;

                int currentIndex = index;
                newAreaListInstance.Q<VisualElement>("DeleteButton").AddManipulator(new Clickable(() => OnClickAreaDataDelete(currentIndex)));
                newAreaListInstance.Q<Button>("List").RegisterCallback<ClickEvent>(ev => planningUI.InvokeOnFocusedAreaChanged(currentIndex));
            }
        }

        /// <summary>
        /// リスト要素の削除ボタンの処理
        /// </summary>
        /// <param name="index">リストの要素番号</param>
        void OnClickAreaDataDelete(int index)
        {
            AreasDataComponent.TryRemoveProperty(index);    // データベースのエリアデータを削除
            planningUI.InvokeOnFocusedAreaChanged(-1);      // エリアの選択状態をリセット
        }
    }
}
