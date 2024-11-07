using Landscape2.Runtime.UiCommon;
using SFB;
using UnityEngine;
using UnityEngine.UIElements;

namespace Landscape2.Runtime.GisDataLoader
{
    /// <summary>
    /// GISデータをロードするプレゼンター
    /// </summary>
    public class GisLoadUI
    {
        private VisualElement panel;
        private DropdownField dropdownField;
        private Button deleteAllButton;
        private TextField nameTextField;
        
        private GisLoader gisLoader;
        private GisPointInfos gisPointInfos;

        // 選択中の属性Indexを保持
        private int selectAttributeIndex = -1;
        
        // 入力したデータ名保持
        private string dataName;
        
        public GisLoadUI(GisLoader gisLoader, GisPointInfos gisPointInfos, VisualElement root)
        {
            panel = root.Q<VisualElement>("Panel_GisImport");
            this.gisLoader = gisLoader;
            this.gisPointInfos = gisPointInfos;
            
            RegisterEvents();
            RegisterButtons();
        }

        private void RegisterEvents()
        {
            gisLoader.OnLoad.AddListener(UpdateAttributeList);
            gisPointInfos.OnDelete.AddListener((index) =>
            {
                deleteAllButton.SetEnabled(gisPointInfos.Points.Count > 0);
            });
            gisPointInfos.OnCreate.AddListener((index) =>
            {
                deleteAllButton.SetEnabled(gisPointInfos.Points.Count > 0);
            });
        }

        private void RegisterButtons()
        {
            // データ選択ボタンクリック
            var selectButton = panel.Q<Button>("ImportButton");
            selectButton.clicked += OpenFile;

            // 属性情報リスト選択
            dropdownField = panel.Q<DropdownField>("GisDropDownList");
            dropdownField.RegisterValueChangedCallback((evt) =>
            {
                SelectAttribute(dropdownField.index);
            });

            // データ名入力
            nameTextField = panel.Q<TextField>("GisDataName");
            nameTextField.RegisterValueChangedCallback((evt) =>
            {
                SetDataName(evt.newValue);
            });

            // 一括削除ボタン
            deleteAllButton = panel.Q<Button>("AllDeleteButton");
            deleteAllButton.clicked += DeleteAll;
            deleteAllButton.SetEnabled(false); // デフォルト非アクティブ

            // キャンセルボタン
            var cancelButton = panel.Q<Button>("CancelButton");
            cancelButton.clicked += Reset;

            // 登録ボタン
            var registerButton = panel.Q<Button>("OKButton");
            registerButton.clicked += Regist;
            
            // 登録ボタンは初期状態で非活性
            registerButton.SetEnabled(false);
        }

        private void OpenFile()
        {
            var extensions = new[] {
                new ExtensionFilter("GeoJson", "geojson"),
                new ExtensionFilter("ShapeFile", "shp")
            };
            
            string[] filePaths;
#if UNITY_EDITOR && UNITY_STANDALONE_OSX 
            // NOTE: macで開発時用にEditorのファイル選択ダイアログを表示
            var openFile = UnityEditor.EditorUtility.OpenFilePanel("Select File", "", "shp, geojson");
            filePaths = new string[] { openFile };
#else
            filePaths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);
#endif
            
            if (filePaths.Length <= 0)
            {
                Debug.LogWarning("ファイルが選択されていません");
                return;
            }
            
            // GISデータロード
            gisLoader.Load(filePaths[0]);
        }
        
        private void UpdateAttributeList()
        {
            dropdownField.choices.Clear();
            
            // 属性リストを更新
            foreach (var attribute in gisLoader.GetAttributeList())
            {
                dropdownField.choices.Add(attribute); 
            }

            // ドロップダウンの初期値を設定
            dropdownField.index = 0;
            selectAttributeIndex = 0;
        }
        
        private void SelectAttribute(int index)
        {
            // 選択中のIDを保持
            selectAttributeIndex = index;
            TryEnableRegistButton();
        }

        private void SetDataName(string name)
        {
            dataName = name;
            TryEnableRegistButton();
        }
        
        private void TryEnableRegistButton()
        {
            var registerButton = panel.Q<Button>("OKButton");
            var isShow = !string.IsNullOrEmpty(dataName) && selectAttributeIndex >= 0;
            registerButton.SetEnabled(isShow);
        }

        private void DeleteAll()
        {
            // 確認ポップアップ表示
            ModalUI.ShowSelectModal("データ削除します", "本当によろしいですか？", ModalUI.SelectModalType.Info, null, () =>
            {
                // データ削除
                gisPointInfos.DeleteAll();
                
                // ポップアップ表示
                ModalUI.ShowModal("削除しました", "全てのデータを削除しました", true, false);
            });
        }
        
        private void Reset()
        {
            selectAttributeIndex = -1;
            gisLoader.Clear();
            dropdownField.choices.Clear();
            dropdownField.index = -1;
            nameTextField.value = "";
        }
        
        private void Regist()
        {
            // データ登録
            gisPointInfos.Regist(dataName, gisLoader.GisDataList, selectAttributeIndex);
        }
    }
}