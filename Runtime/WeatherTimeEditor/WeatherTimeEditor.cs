using UnityEngine;
using PlateauToolkit.Rendering;

namespace Landscape2.Runtime.WeatherTimeEditor
{
    /// <summary>
    /// 天気・時間帯を変更
    /// UIは<see cref="WeatherTimeEditorUI"/>が担当
    /// </summary>
    public class WeatherTimeEditor
    {
        public enum Weather
        {
            Sun,
            Rain,
            Cloud,
            Snow
        }
        EnvironmentController environmentController;

        public WeatherTimeEditor()
        {
            environmentController = GameObject.Find("Environment").GetComponent<EnvironmentController>();
        }
        /// <summary>
        /// 天候を変更
        /// </summary>
        public void SwitchWeather(int weatherID)
        {
            Weather weather = (Weather)weatherID;
            switch (weather)
            {
                case Weather.Sun:
                    environmentController.m_Rain = 0.0f;
                    environmentController.m_Snow = 0.0f;
                    environmentController.m_Cloud = 0.33f;
                    break;
                case Weather.Rain:
                    environmentController.m_Rain = 1.0f;
                    environmentController.m_Snow = 0.0f;
                    environmentController.m_Cloud = 1.0f;
                    break;
                case Weather.Cloud:
                    environmentController.m_Rain = 0.0f;
                    environmentController.m_Snow = 0.0f;
                    environmentController.m_Cloud = 1.0f;
                    break;
                case Weather.Snow:
                    environmentController.m_Rain = 0.0f;
                    environmentController.m_Snow = 1.0f;
                    environmentController.m_Cloud = 1.0f;
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 時間帯を変更
        /// </summary>
        public void EditTime(float timeValue)
        {
            environmentController.m_TimeOfDay = timeValue;
        }
    }

}
