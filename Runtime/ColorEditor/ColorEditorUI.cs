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
        // 閉じるボタンが押されたときのイベント関数
        public event Action OnCloseButtonClicked = () => { };

        // 色相変更スライダー
        private readonly SliderInt hueSlider;
        // RGB変更スライダー：R
        private readonly SliderInt rSlider;
        // RGB変更スライダー：G
        private readonly SliderInt gSlider;
        // RGB変更スライダー：B
        private readonly SliderInt bSlider;
        // 閉じるボタン
        private readonly Button closeButton;
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
        // 閉じるボタン名前
        private const string UICloseButton = "CloseButton";
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
        // 初期化時の色
        private Color initialColor;

        public ColorEditorUI(VisualElement uiRoot, Color initialColor = default)
        {
            this.initialColor = initialColor;

            // 色彩変更UIの各要素を取得
            hueSlider = uiRoot.Q<SliderInt>(UIHueSlider);
            rSlider = uiRoot.Q<SliderInt>(UIRSlider);
            gSlider = uiRoot.Q<SliderInt>(UIGSlider);
            bSlider = uiRoot.Q<SliderInt>(UIBSlider);
            closeButton = uiRoot.Q<Button>(UICloseButton);
            munsellPanel = uiRoot.Q<VisualElement>(UIMunsellPanel);

            // 色相の初期値の設定
            hueSlider.lowValue = 0;
            hueSlider.highValue = 39;
            hueSlider.value = 0;

            // RGBの初期値の設定
            SetColorSlider(initialColor);

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
                // 色を更新
                OnColorEdited(newColor);
            });

            gSlider.RegisterValueChangedCallback(evt =>
            {
                Color newColor = new Color(rSlider.value / 255f, evt.newValue / 255f, bSlider.value / 255f);
                // 色を更新
                OnColorEdited(newColor);
            });

            bSlider.RegisterValueChangedCallback(evt =>
            {
                Color newColor = new Color(rSlider.value / 255f, gSlider.value / 255f, evt.newValue / 255f);
                // 色を更新
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
                        if (lastFocusButton != null && button != lastFocusButton)
                        {
                            EditBorderColor(lastFocusButton, Color.clear);
                        }

                        // 最後にフォーカスされたボタンを更新
                        lastFocusButton = button;
                        //　色彩スライダーの値を更新
                        SetColorSlider(munselColor);
                    };
                });
            });

            // 閉じるボタンが押されたとき
            closeButton.clicked += () =>
            {
                // 閉じるボタンが押されたときのイベントを発火
                OnCloseButtonClicked();
            };
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

                    // 現在選択されている色のボタンの場合、ボタンのボーダーをフォーカス状態にする
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

        // ボタンのボーダーの色を指定した色に変更する：引数はColor
        private void EditBorderColor(Button button, Color color)
        {
            button.style.borderBottomColor = new StyleColor(color);
            button.style.borderTopColor = new StyleColor(color);
            button.style.borderLeftColor = new StyleColor(color);
            button.style.borderRightColor = new StyleColor(color);
        }

        // 色彩スライダーの値を設定する
        private void SetColorSlider(Color color)
        {
            rSlider.value = (int)(color.r * 255f);
            gSlider.value = (int)(color.g * 255f);
            bSlider.value = (int)(color.b * 255f);
        }

        // UIをリセットする
        public void ResetColorEditorUI(Color color)
        {
            SetColorSlider(color);

            // マンセル表ボタンのフォーカス状態をリセット
            if (lastFocusButton != null)
            {
                EditBorderColor(lastFocusButton, Color.clear);
                lastFocusButton = null;
            }

            // 色相スライダーの値をリセット
            hueSlider.value = 0;
        }

        // 初期化時の色をセットする
        public void SetInitialColor(Color color)
        {
            initialColor = color;
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
    }
}
