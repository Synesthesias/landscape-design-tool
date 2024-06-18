using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static PlasticPipe.PlasticProtocol.Messages.Serialization.ItemHandlerMessagesSerialization;

namespace Landscape2.Editor
{
    /// <summary>
    /// 景観ツールのInitialSettingsWindowのエントリーポイントです。
    /// </summary>
    public class InitialSettingsWindow : EditorWindow
    {
        [SerializeField]private VisualTreeAsset visualTreeAsset = default;
        [SerializeField]private Texture checkTexture;
        [SerializeField]private Texture errorTexture;
        private InitialSettings initialSettings = new InitialSettings();
        private VisualElement uiRoot;
        private const string UIRunButton = "RunButton"; // 初期設定実行ボタン名前

        private const string UIImportCheck = "ImportCheckColumn"; // 都市モデルインポート済み判定欄名前
        private const string UIImportHelpbox = "ImportHelpboxColumn"; // 都市モデルインポート済み判定Helpbox欄名前
        private const string UICityObjectCheck = "CityObjectCheckColumn"; // 都市オブジェクトが配置されているかの判定欄名前
        private const string UICityObjectHelpbox = "CityObjectHelpboxColumn"; // 都市オブジェクトが配置されているかの判定Helpbox欄名前
        private const string UISubComponentsCheck = "SubComponentsCheckColumn"; // SubCompornentsが生成されたかの判定欄名前
        private const string UISubComponentsHelpbox = "SubComponentsHelpboxColumn"; // SubCompornentsが生成されたかの判定Helpbox欄名前

        private List<bool> checkList = new List<bool>(); // 初期設定実行可能かの判定用リスト

        [MenuItem("PLATEAU/InitialSettings")]
        public static void Open()
        {
            var window = GetWindow<InitialSettingsWindow>("InitialSettings");
            window.Show();
        }

        public void CreateGUI()
        {
            HelpBox initialSettingsHelpBox = new HelpBox("初期設定が既に行われています", HelpBoxMessageType.Info);
            HelpBox importCheckHelpBox = new HelpBox("都市モデルがインポートされているか確認してください", HelpBoxMessageType.Error);
            HelpBox cityObjectCheckHelpBox = new HelpBox("都市オブジェクトが配置されているか確認してください", HelpBoxMessageType.Error);
            HelpBox subCompornentsCheckHelpBox = new HelpBox("SubCompornentsの生成に失敗しました", HelpBoxMessageType.Error);

            uiRoot = rootVisualElement;
            VisualElement labelFromUXML = visualTreeAsset.Instantiate();
            uiRoot.Add(labelFromUXML);
            var runButton = uiRoot.Q<Button>(UIRunButton);
            runButton.SetEnabled(false);

            // 初期設定が既に実行されているかの判定
            checkList.Add(!initialSettings.CheckSubComponents());
            if(initialSettings.CheckSubComponents() == true)
            {
                uiRoot.Add(initialSettingsHelpBox);
            }
            else
            {
                if(uiRoot.Contains(initialSettingsHelpBox))
                {
                    uiRoot.Remove(initialSettingsHelpBox);
                }
            }

            // 都市モデルインポート済みかの判定
            var isImport = initialSettings.CheckImportCityModel();
            AddCheckListUI(isImport, UIImportCheck, UIImportHelpbox, importCheckHelpBox);

            // 都市オブジェクトが配置されているかの判定
            var isCityObject = initialSettings.CheckCityObjectGroup();
            AddCheckListUI(isCityObject, UICityObjectCheck, UICityObjectHelpbox, cityObjectCheckHelpBox);

            // チェック項目をすべて満たしている場合初期設定を実行できるようにする
            if (checkList.Contains(false) == false)
            {
                runButton.SetEnabled(true);
            }

            // 初期設定実行ボタンが押されたとき
            runButton.clicked += () =>
            {
                // SubComponentsを生成
                var isCreate = initialSettings.CreateSubComponents();
                AddCheckListUI(isCreate, UISubComponentsCheck, UISubComponentsHelpbox, subCompornentsCheckHelpBox);

                // 初期設定後は再び実行できないようにする
                uiRoot.Add(initialSettingsHelpBox);
                runButton.SetEnabled(false);
            };
        }
        // チェックリストのUI処理
        void AddCheckListUI(bool isCheck,string checkUI,string helpBoxUI,HelpBox helpbox)
        {
            var chackImage = new Image();
            var checkColumn = uiRoot.Q<VisualElement>(checkUI);
            var helpboxColumn = uiRoot.Q<VisualElement>(helpBoxUI);
            checkColumn.Add(chackImage);
            
            checkList.Add(isCheck);
            if (isCheck == true)
            {
                chackImage.image = checkTexture;
                if (helpboxColumn.Contains(helpbox))
                {
                    helpboxColumn.Remove(helpbox);
                }
            }
            else
            {
                chackImage.image = errorTexture;
                helpboxColumn.Add(helpbox);
            }
        }
    }
}
