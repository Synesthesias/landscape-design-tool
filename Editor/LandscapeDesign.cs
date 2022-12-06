using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LandscapeDesignTool.Editor
{
    public class LandscapeDesign : EditorWindow
    {

        int _regurationType;
        float _regurationHeight;
        float _screenWidth = 80.0f;
        float _screenHeight = 80.0f;
        float _heightAreaHeight = 30.0f;
        float _heightAreaRadius = 100.0f;

        string _regurationAreaFileName = "";

        // Start is called before the first frame update

        private readonly string[] _tabToggles = { "規制エリア作成", "眺望規制作成", "高さ規制エリア作成", "ShapeFile書き出し" };
        private int _tabIndex;

        [MenuItem("Sandbox/景観まちづくり/景観計画")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(LandscapeDesign), true, "景観計画画面");
        }

        private void OnGUI()
        {

            var style = new GUIStyle(EditorStyles.label);
            style.richText = true;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                _tabIndex = GUILayout.Toolbar(_tabIndex, _tabToggles, new GUIStyle(EditorStyles.toolbarButton), GUI.ToolbarButtonSize.FitToContents);
            }

            if (_tabIndex == 0)
            {
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("<size=15>規制エリア作成</size>", style);
                EditorGUILayout.HelpBox("規制リアの高さを設定しタイプを選択して規制エリア作成をクリックしてください", MessageType.Info);
                string[] options = { "多角形", "円" };
                _regurationHeight = EditorGUILayout.FloatField("高さ", 10);

                _regurationType = EditorGUILayout.Popup(_regurationType, options);
                if (GUILayout.Button("規制エリア作成"))
                {

                    LDTTools.CheckLayers();
                    if (_regurationType == 0)
                    {
                        GameObject grp = GameObject.Find("AnyPolygonRegurationArea");
                        if (!grp)
                        {
                            grp = new GameObject();
                            grp.name = "AnyPolygonRegurationArea";
                            grp.layer = LayerMask.NameToLayer("RegulationArea");

                            AnyPolygonRegurationAreaHandler handler = grp.AddComponent<AnyPolygonRegurationAreaHandler>();
                            handler.areaHeight = _regurationHeight;
                        }
                    }
                    else
                    {
                        GameObject grp = GameObject.Find("AnyCirclnRegurationArea");
                        if (!grp)
                        {
                            grp = new GameObject();
                            grp.name = "AnyCircleRegurationArea";
                            grp.layer = LayerMask.NameToLayer("RegulationArea");

                            AnyCircleRegurationAreaHandler handler = grp.AddComponent<AnyCircleRegurationAreaHandler>();
                            handler.areaHeight = _regurationHeight;



                        }
                    }

                }
            }
            else if (_tabIndex == 1)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("<size=15>眺望対象からの眺望規制作成</size>", style);
                EditorGUILayout.HelpBox("眺望対象地点での幅と高さを設定し眺望規制作成をクリックしてください", MessageType.Info);

                _screenWidth = EditorGUILayout.FloatField("眺望対象地点での幅", _screenWidth);
                _screenHeight = EditorGUILayout.FloatField("眺望対象地点での高さ", _screenHeight);

                if (GUILayout.Button("眺望規制作成"))
                {
                    LDTTools.CheckLayers();

                    GameObject grp = GameObject.Find("RegurationArea");
                    if (!grp)
                    {
                        grp = new GameObject();
                        grp.name = "RegurationArea";
                        grp.layer = LayerMask.NameToLayer("RegulationArea");

                        RegurationAreaHandler handler = grp.AddComponent<RegurationAreaHandler>();
                        handler.screenHeight = _screenHeight;
                        handler.screenWidth = _screenWidth;

                    }

                }

            }
            else if (_tabIndex == 2)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("<size=15>高さ規制エリア作成</size>", style);
                EditorGUILayout.HelpBox("高さ規制リアの高さ半径を設定しタイプを選択して規制エリア作成をクリックしてください", MessageType.Info);
                _heightAreaHeight = EditorGUILayout.FloatField("高さ", _heightAreaHeight);
                _heightAreaRadius = EditorGUILayout.FloatField("半径", _heightAreaRadius);

                if (GUILayout.Button("高さ規制エリア作成"))
                {
                    GameObject grp = GameObject.Find("HeitRegurationAreaGroup");
                    if (!grp)
                    {
                        grp = new GameObject();
                        grp.name = "HeightRegurationArea";
                        grp.layer = LayerMask.NameToLayer("RegulationArea");

                        HeightRegurationAreaHandler handler = grp.AddComponent<HeightRegurationAreaHandler>();
                        handler.areaHeight = _heightAreaHeight;
                        handler.areaRadius = _heightAreaRadius;

                    }
                }
            }
            else if (_tabIndex == 3)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("<size=15>規制エリア出力</size>", style);
                _regurationAreaFileName = EditorGUILayout.TextField("ファイル名", _regurationAreaFileName);
                if (GUILayout.Button("規制エリア出力"))
                {
                    List<List<Vector2>> contours = new List<List<Vector2>>();
                    GameObject grp = GameObject.Find("AnyPolygonRegurationArea");
                    if (grp)
                    {
                        int narea = grp.transform.childCount;
                        for (int i = 0; i < narea; i++)
                        {
                            GameObject go = grp.transform.GetChild(i).gameObject;
                            ShapeItem handler = go.GetComponent<ShapeItem>();
                            if (handler)
                            {
                                List<Vector2> cnt = handler.Contours;
                                contours.Add(cnt);
                            }
                        }
                    }
                    grp = GameObject.Find("AnyCircleRegurationArea");
                    if (grp)
                    {
                        int narea = grp.transform.childCount;
                        for (int i = 0; i < narea; i++)
                        {
                            GameObject go = grp.transform.GetChild(i).gameObject;
                            ShapeItem handler = go.GetComponent<ShapeItem>();
                            if (handler)
                            {
                                List<Vector2> cnt = handler.Contours;
                                contours.Add(cnt);
                            }
                        }
                    }

                    LDTTools.WriteShapeFile(_regurationAreaFileName, "RArea", contours);
                }
            }
        }
    }
}
