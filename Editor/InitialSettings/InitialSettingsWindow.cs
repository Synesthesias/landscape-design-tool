using System.Collections.Generic;
using PLATEAU.Util.Async;
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
        [SerializeField]private VisualTreeAsset m_VisualTreeAsset = default;
        [SerializeField]private Texture checkTexture;
        [SerializeField]private Texture errorTexture;

        private InitialSettings initialSettings = new InitialSettings();
        private const string UIRunButton = "RunButton"; // 初期設定実行ボタン名前
        private const string UIImportCheckColumn = "ImportCheckColumn"; // 都市モデルインポート済み判定欄名前
        private const string UICityObjectCheckColumn = "CityObjectCheckColumn"; // 都市オブジェクトが配置されているかの判定欄名前
        private const string UIImportHelpboxColumn = "ImportHelpboxColumn"; // 都市モデルインポート済み判定Helpbox欄名前
        private const string UICityObjectHelpboxColumn = "CityObjectHelpboxColumn"; // 都市オブジェクトが配置されているかの判定Helpbox欄名前

        private List<bool> checkList = new List<bool>(); // 初期設定実行可能かの判定用リスト

        [MenuItem("PLATEAU/InitialSettings")]
        public static void Open()
        {
            var window = GetWindow<InitialSettingsWindow>("InitialSettings");
            window.Show();
        }

        public void CreateGUI()
        {
            VisualElement uiRoot = rootVisualElement;
            VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
            uiRoot.Add(labelFromUXML);

            var runButton = uiRoot.Q<Button>(UIRunButton);
            var importCheckImage = new Image();
            var cityObjectGroupCheckImage = new Image();
            var importCheckColumn = uiRoot.Q<VisualElement>(UIImportCheckColumn);
            var cityObjectCheckColumn = uiRoot.Q<VisualElement>(UICityObjectCheckColumn);
            var importHelpboxColumn = uiRoot.Q<VisualElement>(UIImportHelpboxColumn);
            var cityObjectHelpboxColumn = uiRoot.Q<VisualElement>(UICityObjectHelpboxColumn);

            var initialSettingsHelpBox = new HelpBox("初期設定が既に行われています", HelpBoxMessageType.Info);
            var importCheckHelpBox = new HelpBox("都市モデルがインポートされているか確認してください", HelpBoxMessageType.Error);
            var cityObjectCheckHelpBox = new HelpBox("都市オブジェクトが配置されているか確認してください", HelpBoxMessageType.Error);

            importCheckColumn.Add(importCheckImage);
            cityObjectCheckColumn.Add(cityObjectGroupCheckImage);

            if (initialSettings.CheckSubComponents() == true)
            {
                uiRoot.Add(initialSettingsHelpBox);
            }
            else
            {
                uiRoot.Remove(initialSettingsHelpBox);
            }

            // 初期設定実行ボタンが押されたとき
            runButton.clicked += () =>
            {
                // 都市モデルインポート済み判定
                checkList.Add(initialSettings.CheckInstancedCityModel());
                if(initialSettings.CheckInstancedCityModel() == true)
                {
                    importCheckImage.image = checkTexture;
                    importHelpboxColumn.Remove(importCheckHelpBox);
                }
                else
                {
                    importCheckImage.image = errorTexture;
                    importHelpboxColumn.Add(importCheckHelpBox);
                }

                // 都市オブジェクトが配置されているかの判定
                checkList.Add(initialSettings.CheckCityObjectGroup());
                if(initialSettings.CheckCityObjectGroup() == true)
                {
                    cityObjectGroupCheckImage.image = checkTexture;
                    cityObjectHelpboxColumn.Remove(cityObjectCheckHelpBox);
                }
                else
                {
                    cityObjectGroupCheckImage.image = errorTexture;
                    cityObjectHelpboxColumn.Add(cityObjectCheckHelpBox);                    
                }

                // チェック項目をすべて満たしている場合初期設定を実行
                if(checkList.Contains(false) == false)
                {
                    initialSettings.CreateSubComponents();
                    initialSettings.ExecuteInitialSettings().ContinueWithErrorCatch();
                }
            };
        }
    }
}
