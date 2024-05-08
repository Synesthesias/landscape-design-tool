using Landscape2.Runtime.UiCommon;
using UnityEngine;
using UnityEngine.UIElements;

namespace Landscape2.Runtime.WeatherTimeEditor
{
    /// <summary>
    /// 天候・時間帯を変更するためのUIです。
    /// </summary>
    public class WeatherTimeEditorUI : ISubComponent
    {
        private readonly WeatherTimeEditor weatherTimeEditor;

        //天候変更ボタン
        private readonly RadioButtonGroup weatherChangeButton;
        //時間帯変更スライダー
        private readonly Slider timeChangeSlider;
        //天候変更ボタン名前
        private const string UIWeatherChangeButton = "WeatherChangeButton";
        //時間帯変更スライダー名前
        private const string UITimeChangeSlider = "TimeChangeSlider";

        public WeatherTimeEditorUI(WeatherTimeEditor WeatherTimeEditor)
        {
            this.weatherTimeEditor = WeatherTimeEditor;
            var uiRoot = new UIDocumentFactory().CreateWithUxmlName("UIWeatherTimeEditor");

            //天候変更ボタンの値が変更されたとき
            weatherChangeButton = uiRoot.Q<RadioButtonGroup>(UIWeatherChangeButton);
            weatherChangeButton.RegisterValueChangedCallback(evt =>
            {
                //Debug.Log(evt.newValue);
                weatherTimeEditor.SwitchWeather(evt.newValue);
            });

            //時間帯変更スライダーの値が変更されたとき
            timeChangeSlider = uiRoot.Q<Slider>(UITimeChangeSlider);
            timeChangeSlider.value = 0.5f;
            timeChangeSlider.RegisterValueChangedCallback(evt =>
            {
                //Debug.Log(evt.newValue);
                weatherTimeEditor.EditTime(evt.newValue);
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
    }
}
