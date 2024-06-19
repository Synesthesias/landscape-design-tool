using Landscape2.Runtime.UiCommon;
using UnityEngine;
using UnityEngine.UIElements;

namespace Landscape2.Runtime.WeatherTimeEditor
{
    /// <summary>
    /// 天候・時間帯を変更するためのUI
    /// </summary>
    public class WeatherTimeEditorUI : ISubComponent
    {
        private readonly WeatherTimeEditor weatherTimeEditor;

        // 天候変更ボタン
        private readonly RadioButtonGroup weatherChangeButton;
        // 時間帯変更スライダー
        private readonly Slider timeChangeSlider;
        // 時間帯変更スライダー
        private readonly Label timeLabel;
        // 天候変更ボタン名前
        private const string UIWeatherChangeButton = "WeatherChangeButton";
        // 時間帯変更スライダー名前
        private const string UITimeChangeSlider = "TimeChangeSlider";
        // 時間帯変更スライダー名前
        private const string UITimeLabel = "TimeText";

        public WeatherTimeEditorUI(WeatherTimeEditor weatherTimeEditor, VisualElement uiRoot)
        {
            this.weatherTimeEditor = weatherTimeEditor;

            weatherChangeButton = uiRoot.Q<RadioButtonGroup>(UIWeatherChangeButton);
            timeChangeSlider = uiRoot.Q<Slider>(UITimeChangeSlider);
            timeLabel = uiRoot.Q<Label>(UITimeLabel);

            // 時間帯の初期値の設定
            timeChangeSlider.lowValue = 0.375f;
            timeChangeSlider.highValue = 0.875f;
            timeChangeSlider.value = 0.5f;
            timeLabel.text = this.weatherTimeEditor.GetTimeString(timeChangeSlider.value);

            // 天候変更ボタンの値が変更されたとき
            weatherChangeButton.RegisterValueChangedCallback(evt =>
            {
                // 天候を変更
                this.weatherTimeEditor.SwitchWeather(evt.newValue);
                // 時間帯を更新
                this.weatherTimeEditor.EditTime(timeChangeSlider.value);
            });

            // 時間帯変更スライダーの値が変更されたとき
            timeChangeSlider.RegisterValueChangedCallback(evt =>
            {
                // 時間帯を更新
                this.weatherTimeEditor.EditTime(evt.newValue);
                // 表示される時刻を更新
                timeLabel.text = this.weatherTimeEditor.GetTimeString(timeChangeSlider.value);
            });
         
            UpdateButtonState();
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
        public void UpdateButtonState()
        {
        }
        public void Start()
        {
        }
    }
}
