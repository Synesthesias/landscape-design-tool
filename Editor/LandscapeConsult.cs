using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using LandscapeDesignTool;
using LandscapeDesignTool.Editor.WindowTabs;
using UnityEngine.Rendering;

namespace LandscapeDesignTool.Editor
{
#if UNITY_EDITOR
    public class LandscapeConsult : EditorWindow
    {


        private int _mode = 0;
        int _buildmode = 0;
        private GameObject _rootHeightArea;
        private const string HeightAreaGroupName = "Height Restricted Areas";
        private const string HeightAreaName = "Height Restricted Area";


        /*
         * layer
         */
        string[] layerName = { "RegulationArea" };
        int[] layerId = { 30 };

        private TabViewPointGenerate _tabViewPointGenerate = new TabViewPointGenerate();
        

        int _regulationType;
        float _regulationHeight;

        bool _point_edit_in = false;

        float _time = 12;

        Color _sunColor = new Color(1,0.9568f, 0.8392f);
        Color _sunColor1 = new Color(0.95294f, 0.71373f, 0.3647f);
        float _sunAngle;
        GameObject _sunLight=null;
        float _morningTime=5.0f, _nightTime=19.0f;
        float _sunRoll = -20.0f;

        int _weather = 0;


        private readonly string[] _tabToggles = { "視点場作成", "Shapefile読み込み","天候と時間" };
        private int _tabIndex;
        [MenuItem("PLATEAU/景観まちづくり/景観協議")]

        public static void ShowWindow()
        {
            TagAdder.ConfigureTags();
            EditorWindow.GetWindow(typeof(LandscapeConsult), true, "景観協議画面");
        }

        private void OnGUI()
        {

            EditorGUI.BeginChangeCheck(); EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                _tabIndex = GUILayout.Toolbar(_tabIndex, _tabToggles, new GUIStyle(EditorStyles.toolbarButton), GUI.ToolbarButtonSize.FitToContents);
            }

            var style = new GUIStyle(EditorStyles.label);
            style.richText = true;
            if (_tabIndex == 0)
            {
                _tabViewPointGenerate.Draw(style);
            }
            else if (_tabIndex == 2)
            {

                // _sunLight = GameObject.Find("SunSource").gameObject;
                _sunLight = RenderSettings.sun.gameObject;
                EditorGUILayout.Space();
                /*
                if(_sunLight == null)
                {
                    EditorGUILayout.ObjectField("光源指定", _sunLight, typeof(GameObject), true);
                }
                */
                EditorGUILayout.LabelField("<size=15>光源</size>", style);
                EditorGUILayout.ObjectField("光源", _sunLight, typeof(GameObject), true);
                EditorGUILayout.LabelField("<size=15>時間の設定</size>", style);
                _time = EditorGUILayout.Slider("時間", _time, _morningTime, _nightTime);


                EditorGUILayout.Space();
                EditorGUILayout.LabelField("<size=15>天候の設定</size>", style);
                GUIContent[] popupItem = new[]
                {
                    new GUIContent("晴れ"),
                    new GUIContent("薄曇り"),
                    new GUIContent("曇り"),
                    new GUIContent("雨"),
                    new GUIContent("雪")
                };

                _weather = EditorGUILayout.Popup(
                    label: new UnityEngine.GUIContent("天候"),
                    selectedIndex: _weather,
                    displayedOptions: popupItem);
            }

        }

        private void OnInspectorUpdate()
        {

            if (_sunLight == null)
            {
                _sunLight = RenderSettings.sun.gameObject;
            }

            float f = (_time - _morningTime) / (_nightTime - _morningTime);

            float r=1, g=1, b=1;
            Color col = new Color();
            if (_time > (_morningTime + 2) && _time < (_nightTime - 2))
            {
                col = _sunColor;
            }
            else if (_time >= (_nightTime - 2))
            {
                float f1 = 1.0f - (_nightTime - _time) / 2.0f;
                r = _sunColor.r + ((_sunColor1.r - _sunColor.r) * f1);
                g = _sunColor.g + ((_sunColor1.g - _sunColor.g) * f1);
                b = _sunColor.b + ((_sunColor1.b - _sunColor.b) * f1);
                if (r < 0) r = 0;
                if (r > 1) r = 1;
                if (g < 0) g = 0;
                if (g > 1) g = 1;
                if (b < 0) b = 0;
                if (b > 1) b = 1;
                col.r = r;
                col.g = g;
                col.b = b;

                if (_time >= (_nightTime - 1))
                {
                    r = col.r * (((_nightTime-_time) / 1.0f));
                    g = col.g * (((_nightTime - _time) / 1.0f));
                    b = col.b * (((_nightTime - _time) / 1.0f));
                    if (r < 0) r = 0;
                    if (r > 1) r = 1;
                    if (g < 0) g = 0;
                    if (g > 1) g = 1;
                    if (b < 0) b = 0;
                    if (b > 1) b = 1;
                    col.r = r;
                    col.g = g;
                    col.b = b;
                }

            }
            else if( _time <= (_morningTime + 2))
            {
                float f1 = (_time - _morningTime) / 2.0f;
                r = _sunColor1.r + ((_sunColor.r - _sunColor1.r) * f1);
                g = _sunColor1.g +((_sunColor.g - _sunColor1.g) * f1);
                b = _sunColor1.b +((_sunColor.b - _sunColor1.b) * f1);
                if (r < 0) r = 0;
                if (r > 1) r = 1;
                if (g < 0) g = 0;
                if (g > 1) g = 1;
                if (b < 0) b = 0;
                if (b > 1) b = 1;
                col.r = r;
                col.g = g;
                col.b = b;

                if (_time <= (_morningTime + 1))
                {
                    r = col.r * ((_time - _morningTime) / 1.0f);
                    if (r < 0) r = 0;
                    if (r > 1) r = 0;
                    g = col.g * ((_time - _morningTime) / 1.0f);
                    if (g < 0) g = 0;
                    if (g > 1) g = 0;
                    b = col.b * ((_time - _morningTime) / 1.0f);
                    if (b < 0) b = 0;
                    if (g > 1) g = 0;
                    col.r = r;
                    col.g = g;
                    col.b = b;
                }
                
            }

            float strength = 1.0f;
            if(_weather == 1)
            {
                r = col.r * 0.8f;
                g = col.g * 0.8f;
                b = col.b * 0.8f;
                strength = 0.7f;
            }
            else if (_weather == 2)
            {
                r = col.r * 0.6f;
                g = col.g * 0.6f;
                b = col.b * 0.6f;
                strength = 0.4f;
            }
            else if (_weather == 3 || _weather == 4)
            {
                r = col.r * 0.3f;
                g = col.g * 0.3f;
                b = col.b * 0.3f;
                strength = 0.1f;
            }
            else
            {
                r = col.r;
                g = col.g;
                b = col.b;
                strength = 1.0f;
            }


            if (r < 0) r = 0;
            if (r > 1) r = 1;
            if (g < 0) g = 0;
            if (g > 1) g = 1;
            if (b < 0) b = 0;
            if (b > 1) b = 1;
            col.r = r;
            col.g = g;
            col.b = b;

            float angle = 180.0f - 180.0f * f;
            Quaternion r1 = Quaternion.Euler(0, -30, 0);
            Quaternion r2 = Quaternion.Euler(angle, 0, 0);
            _sunLight.transform.rotation = r2*r1;
            _sunLight.GetComponent<Light>().color = col;
            _sunLight.GetComponent<Light>().shadowStrength = strength;



        }
    }


#endif
}
