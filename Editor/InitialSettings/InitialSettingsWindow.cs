using System.Collections.Generic;
using PLATEAU.Util.Async;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Landscape2.Editor
{
    /// <summary>
    /// 景観ツールのInitialSettingsWindowのエントリーポイントです。
    /// </summary>
    public class InitialSettingsWindow : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;
        public InitialSettings initialSettings = new InitialSettings();

        // 初期設定実行ボタン
        public Button runButton;
        // 初期設定実行ボタン名前
        public const string UIRunButton = "RunButton";
        // 初期設定実行可能かの判定用リスト
        List<bool> checkList = new List<bool>();
        // 都市モデルインポート済み判定用の判定用ラベル
        public Label importCheckLabel;
        // 都市モデルインポート済み判定用判定用ラベル名前
        public const string UIImportCheckLabel = "ImportCheck";
        // 都市オブジェクトが配置されているかの判定用ラベル
        public Label cityObjectGroupCheckLabel;
        // 都市オブジェクトが配置されているかの判定用ラベル名前
        public const string UICityObjectGroupCheckLabel = "CityObjectCheck";

        public InitialSettingsWindow()
        {
        }

        [MenuItem("PLATEAU/InitialSettings")]
        public static void Open()
        {
            var window = GetWindow<InitialSettingsWindow>("InitialSettings");
            window.Show();
        }

        public void CreateGUI()
        {
            // Instantiate UXML
            VisualElement uiRoot = rootVisualElement;

            VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
            uiRoot.Add(labelFromUXML);
            runButton = uiRoot.Q<Button>(UIRunButton);
            importCheckLabel = uiRoot.Q<Label>(UIImportCheckLabel);
            cityObjectGroupCheckLabel = uiRoot.Q<Label>(UICityObjectGroupCheckLabel);

            if(initialSettings.CheckInitialSettings() ==true)
            {
                uiRoot.Add(new HelpBox("初期設定が既に行われています", HelpBoxMessageType.Info));
            }

            // 初期設定実行ボタンが押されたとき
            runButton.clicked += () =>
            {
                checkList.Add(initialSettings.CheckInstancedCityModel());
                if(initialSettings.CheckInstancedCityModel() == true)
                {
                    importCheckLabel.text = "〇";
                }
                else
                {
                    importCheckLabel.text = "×";
                    uiRoot.Add(new HelpBox("都市モデルがインポートされていません", HelpBoxMessageType.Error));
                }
                checkList.Add(initialSettings.CheckCityObjectGroup());
                if(initialSettings.CheckCityObjectGroup() == true)
                {
                    cityObjectGroupCheckLabel.text = "〇";
                }
                else
                {
                    cityObjectGroupCheckLabel.text = "×";
                    uiRoot.Add(new HelpBox("属性情報を含む都市オブジェクトが配置されていません", HelpBoxMessageType.Error));
                }

                // チェック項目をすべて満たしている場合初期設定を実行
                if(checkList.All(value => true))
                {
                    initialSettings.CreateSubComponents();
                    initialSettings.ExecuteInitialSettings().ContinueWithErrorCatch();
                }
            };
        }
    }
}
