using Landscape2.Runtime.UiCommon;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace Landscape2.Runtime
{
    /// <summary>
    /// 色彩変更用のUI
    /// </summary>
    public class ColorEditorUI : ISubComponent
    {
        // 色が変更されたときのイベント関数
        public event Action<Color> OnColorEdited = color => { };

        // 色相変更スライダー
        private readonly SliderInt hueSlider;
        // RGB変更スライダー：R
        private readonly SliderInt rSlider;
        // RGB変更スライダー：G
        private readonly SliderInt gSlider;
        // RGB変更スライダー：B
        private readonly SliderInt bSlider;
        // 見本色パネル
        private readonly VisualElement colorPanel;
        // リセットボタン
        private readonly Button resetButton;
        // 閉じるボタン
        //private readonly Button closeButton;
        // マンセル表パネル
        private readonly VisualElement munsellPanel;

        // 色相変更スライダー名前
        private const string UIHueSlider = "HueSlider";
        // RGB変更スライダー名前：R
        private const string UIRSlider = "RSlider";
        // RGB変更スライダー名前：G
        private const string UIGSlider = "GSlider";
        // RGB変更スライダー名前：B
        private const string UIBSlider = "BSlider";
        // 見本色パネル名前
        private const string UIColorPanel = "ColorPanel";
        // リセットボタン名前
        private const string UIResetButton = "ResetButton";
        // マンセル表パネル名前
        private const string UIMunsellPanel = "MunsellPanel";

        // マンセル表の色セット
        private List<List<string>> codemap = new List<List<string>>();
        MunsellData munsellData = new MunsellData();

        // ボタンフォーカス時のボーダー色
        private Color focusColor = new Color(1f, 1f, 0f, 1f);

        // 最後にフォーカスされたボタン
        private Button lastFocusButton;
        // 現在選択されているマンセル表の色
        private Color munselColor;

        public ColorEditorUI(VisualElement uiRoot)
        {
            hueSlider = uiRoot.Q<SliderInt>(UIHueSlider);
            rSlider = uiRoot.Q<SliderInt>(UIRSlider);
            gSlider = uiRoot.Q<SliderInt>(UIGSlider);
            bSlider = uiRoot.Q<SliderInt>(UIBSlider);
            colorPanel = uiRoot.Q<VisualElement>(UIColorPanel);
            resetButton = uiRoot.Q<Button>(UIResetButton);
            munsellPanel = uiRoot.Q<VisualElement>(UIMunsellPanel);

            // 色相の初期値の設定
            hueSlider.lowValue = 0;
            hueSlider.highValue = 39;
            hueSlider.value = 0;

            // RGBの初期値の設定
            rSlider.value = 255;
            gSlider.value = 255;
            bSlider.value = 255;

            // 色相を初期化
            EditHue(hueSlider.value);

            // 色相変更スライダーの値が変更されたとき
            hueSlider.RegisterValueChangedCallback(evt =>
            {
                // 色相を変更
                EditHue(hueSlider.value);
            });

            // RGB変更スライダーの値が変更されたとき
            rSlider.RegisterValueChangedCallback(evt =>
            {
                Color newColor = new Color(evt.newValue / 255f, gSlider.value / 255f, bSlider.value / 255f);
                // 色見本を更新
                colorPanel.style.backgroundColor = new StyleColor(newColor);
                // 建築物の色を更新
                OnColorEdited(newColor);
            });

            gSlider.RegisterValueChangedCallback(evt =>
            {
                Color newColor = new Color(rSlider.value / 255f, evt.newValue / 255f, bSlider.value / 255f);
                // 色見本を更新
                colorPanel.style.backgroundColor = new StyleColor(newColor);
                // 建築物の色を更新
                OnColorEdited(newColor);
            });

            bSlider.RegisterValueChangedCallback(evt =>
            {
                Color newColor = new Color(rSlider.value / 255f, gSlider.value / 255f, evt.newValue / 255f);
                // 色見本を更新
                colorPanel.style.backgroundColor = new StyleColor(newColor);
                // 建築物の色を更新
                OnColorEdited(newColor);
            });

            // マンセル表のボタンがクリックされたとき
            munsellPanel.Children().ToList().ForEach(v =>
            {
                var panel = v as VisualElement;
                panel.Children().ToList().ForEach(b =>
                {
                    var button = b as Button;
                    button.clicked += () =>
                    {
                        // マンセル表のボタンの色を取得
                        munselColor = button.style.backgroundColor.value;
                        // ボタンのボーダーをフォーカス用の色にする
                        EditBorderColor(button, focusColor);

                        // 最後にフォーカスされたボタンのボーダー色を非フォーカス用の色にする
                        if (lastFocusButton != null)
                        {
                            EditBorderColor(lastFocusButton, Color.clear);
                        }
                        // 色見本を更新
                        colorPanel.style.backgroundColor = new StyleColor(munselColor);
                        // 最後にフォーカスされたボタンを更新
                        lastFocusButton = button;

                        // 建築物の色を更新
                        OnColorEdited(munselColor);
                    };
                });
            });

            // リセットボタンが押されたとき
            resetButton.clicked += () =>
            {
                // RGBをリセット
                rSlider.value = 255;
                gSlider.value = 255;
                bSlider.value = 255;

                // 色見本をリセット
                colorPanel.style.backgroundColor = new StyleColor(new Color(1f, 1f, 1f));

                // 建築物の色を更新
                OnColorEdited(Color.white);
                //this.colorEditor.EditBuildingColor(Color.white);

            };
        }

        public void Start()
        {
        }

        // 色相を変更する
        private void EditHue(int val)
        {
            // 色相セットを変更
            HueValue(val);
            int i = 0;
            // マンセル表のボタンの色を変更する
            munsellPanel.Children().ToList().ForEach(v =>
            {
                int j = 0;
                var panel = v as VisualElement;
                panel.Children().ToList().ForEach(b =>
                {
                    var button = b as Button;

                    Color newColor = Color.white;
                    if (j < codemap[i].Count)
                    {

                        if (ColorUtility.TryParseHtmlString("#" + codemap[i][j], out newColor))
                        {
                            button.style.backgroundColor = newColor;
                        }

                        button.style.backgroundColor = newColor;
                        // ボタンをアクティブ化
                        button.SetEnabled(true);
                    }
                    else
                    {
                        // ボタンを非アクティブ化
                        button.SetEnabled(false);
                        // ボタンの色を無色にする
                        button.style.backgroundColor = new StyleColor(Color.clear);
                    }

                    // 現在選択されている色のボタンの場合、ボタンのボーダーをフォーカス用の色にする
                    if (newColor == munselColor)
                    {
                        EditBorderColor(button, focusColor);
                    }
                    else
                    {
                        EditBorderColor(button, Color.clear);
                    }
                    j++;
                });
                i++;
            });
        }
        // 色相の値によってマンセル表の色セットを変更
        public void HueValue(int val)
        {
            switch (val)
            {
                case 0:
                    codemap = munsellData.codemap25R; break;
                case 1:
                    codemap = munsellData.codemap5R; break;
                case 2:
                    codemap = munsellData.codemap75R; break;
                case 3:
                    codemap = munsellData.codemap10R; break;
                case 4:
                    codemap = munsellData.codemap25YR; break;
                case 5:
                    codemap = munsellData.codemap5YR; break;
                case 6:
                    codemap = munsellData.codemap75YR; break;
                case 7:
                    codemap = munsellData.codemap10YR; break;
                case 8:
                    codemap = munsellData.codemap25Y; break;
                case 9:
                    codemap = munsellData.codemap5Y; break;
                case 10:
                    codemap = munsellData.codemap75Y; break;
                case 11:
                    codemap = munsellData.codemap10Y; break;
                case 12:
                    codemap = munsellData.codemap25GY; break;
                case 13:
                    codemap = munsellData.codemap5GY; break;
                case 14:
                    codemap = munsellData.codemap75GY; break;
                case 15:
                    codemap = munsellData.codemap10GY; break;
                case 16:
                    codemap = munsellData.codemap25G; break;
                case 17:
                    codemap = munsellData.codemap5G; break;
                case 18:
                    codemap = munsellData.codemap75G; break;
                case 19:
                    codemap = munsellData.codemap10G; break;
                case 20:
                    codemap = munsellData.codemap25BG; break;
                case 21:
                    codemap = munsellData.codemap5BG; break;
                case 22:
                    codemap = munsellData.codemap75BG; break;
                case 23:
                    codemap = munsellData.codemap10BG; break;
                case 24:
                    codemap = munsellData.codemap25B; break;
                case 25:
                    codemap = munsellData.codemap5B; break;
                case 26:
                    codemap = munsellData.codemap75B; break;
                case 27:
                    codemap = munsellData.codemap10B; break;
                case 28:
                    codemap = munsellData.codemap25PB; break;
                case 29:
                    codemap = munsellData.codemap5PB; break;
                case 30:
                    codemap = munsellData.codemap75PB; break;
                case 31:
                    codemap = munsellData.codemap10PB; break;
                case 32:
                    codemap = munsellData.codemap25P; break;
                case 33:
                    codemap = munsellData.codemap5P; break;
                case 34:
                    codemap = munsellData.codemap75P; break;
                case 35:
                    codemap = munsellData.codemap10P; break;
                case 36:
                    codemap = munsellData.codemap25RP; break;
                case 37:
                    codemap = munsellData.codemap5RP; break;
                case 38:
                    codemap = munsellData.codemap75RP; break;
                case 39:
                    codemap = munsellData.codemap10RP; break;
            }
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

        // ボタンのボーダーの色を指定した色に変更する：引数はColor
        private void EditBorderColor(Button button, Color color)
        {
            button.style.borderBottomColor = new StyleColor(color);
            button.style.borderTopColor = new StyleColor(color);
            button.style.borderLeftColor = new StyleColor(color);
            button.style.borderRightColor = new StyleColor(color);
        }
    }

    // 建物編集UI用のサブクラス
    public class BuildingColorEditorUI : ColorEditorUI
    {
        // 地物型が変更されたときのイベント関数
        public event Action<int> OnFieldChanged = id => { };
        // 地物型選択リスト
        private readonly DropdownField buildingField;
        // 地物型選択リスト名前
        private const string UIBuildingField = "BuildingField";
        // 地物型選択リストの文字列を管理する配列
        private  string[] uiBuildingFields = { "要素全体", "壁" , "屋根/屋上" };

        public BuildingColorEditorUI(VisualElement uiRoot) : base(uiRoot)
        {
            buildingField = uiRoot.Q<DropdownField>(UIBuildingField);
            // 地物型選択リストの初期値の設定
            buildingField.choices.Clear();
            // 地物型選択リストの値が変更されたとき
            buildingField.RegisterValueChangedCallback(evt =>
            {
                // 色を変更するマテリアルを切り替える
                OnFieldChanged(Array.IndexOf(uiBuildingFields, evt.newValue));
            });
        }

        public void SetFieldList(int count)
        {
            buildingField.choices.Clear();
            // ドロップダウンリストの要素数を更新
            for (int i = 0;i < count;i ++)
            {
                buildingField.choices.Add(uiBuildingFields[i]);
            }
            // ドロップダウンリストの初期値を設定
            buildingField.value = uiBuildingFields[0];
        }
    }   
    
}
