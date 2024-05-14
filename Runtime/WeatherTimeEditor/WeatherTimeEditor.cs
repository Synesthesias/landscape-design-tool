using UnityEngine;
using PlateauToolkit.Rendering;
using System;
using Codice.Client.Common;
using UnityEngine.UIElements;

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
        Weather currentWeather = Weather.Sun; // 現在の天候

        public WeatherTimeEditor()
        {
            environmentController = GameObject.Find("Environment").GetComponent<EnvironmentController>();
        }
        /// <summary>
        /// 天候を変更
        /// </summary>
        public void SwitchWeather(int weatherID)
        {
            currentWeather = (Weather)weatherID;
            switch (currentWeather)
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
            // 晴れのときのみtimeValueをEnvironmentControllerのTimeOfDay値と同期させる
            if (currentWeather == Weather.Sun)
            {
                environmentController.m_SunIntensity = 100000f;
                environmentController.m_TimeOfDay = timeValue;  
            }
            else
            {
                // 空の色を固定する
                environmentController.m_TimeOfDay = 0.5f;
                // 雨・曇り・雪のときは太陽光の強さを変更することで時間帯を表現
                // timeValueが0.5f(12:00)のとき太陽光の強さが最大, 0.0f(0:00)と1.0f(24:00)のとき最小
                environmentController.m_SunIntensity = Mathf.Sin(2 * Mathf.PI * 0.5f * timeValue) * 100000f;
            }
        }
        /// <summary>
        /// timeValue値から時刻を計算
        /// </summary>
        DateTime CalculateTime(float timeValue)
        {
            int year = (int)environmentController.m_Date.x;
            int month = (int)environmentController.m_Date.y;
            int day = (int)environmentController.m_Date.z;
            double totalHours = timeValue * 24;
            int hour = (int)totalHours;
            int minute = (int)((totalHours - hour) * 60);
            int second = (int)((((totalHours - hour) * 60) - minute) * 60);
            DateTime combinedDateTime;

            if (timeValue >= 0.9999)
            {
                hour = hour % 24;
            }

            try
            {
                combinedDateTime = new DateTime(year, month, day, hour, minute, second, environmentController.m_TimeZone);
            }
            catch
            {

                combinedDateTime = DateTime.Now;
            }

            return combinedDateTime;
        }
        /// <summary>
        /// timeValue値から(HH:mm)のフォーマットで時刻を取得
        /// </summary>
        public string GetTimeString(float timeValue)
        {      
            DateTime time = CalculateTime(timeValue);
            return time.ToString("HH:mm");
        }
    }
}
