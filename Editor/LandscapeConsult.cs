using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using LandscapeDesignTool;

namespace LandscapeDesignTool.Editor
{
#if UNITY_EDITOR
    public class LandscapeConsult : EditorWindow
    {
        public enum KEY_OPERATION_MODE
        {
            VIEWPOINT,
            None
        }

        private KEY_OPERATION_MODE _keyOperationMode = KEY_OPERATION_MODE.None;
        private GameObject _scriptAttachNode;

        private int _mode = 0;
        int _buildmode = 0;
        private GameObject _rootHeightArea;
        private const string HeightAreaGroupName = "Height Restricted Areas";
        private const string HeightAreaName = "Height Restricted Area";

        /*
         * Viewpoint
         */
        private const string ViewPointGroupName = "ViewPointGroup";
        private const string ViewPointName = "ViewPoint";
        string _viewpointDescription = "視点場";

        /*
         * layer
         */
        string[] layerName = { "RegulationArea" };
        int[] layerId = { 30 };

        private float _viewpointFOV = 60.0f;
        private float _viewpointHeight = 1.6f;
        private GameObject _viewpointRoot;

        float _screenWidth = 80.0f;
        float _screenHeight = 80.0f;

        int _regurationType;
        float _regurationHeight;

        float _heightAreaHeight=30.0f;
        float _heightAreaRadius=100.0f;

        bool _point_edit_in = false;
        string _regurationAreaFileName = "";
        string _heightAreaFileName = "";
        string _viewRegrationAreaFileName = "";

        float _time = 12;

        Color _sunColor = new Color(1,0.9568f, 0.8392f);
        Color _sunColor1 = new Color(0.95294f, 0.71373f, 0.3647f);
        float _sunAngle;
        GameObject _sunLight;
        float _morningTime=5.0f, _nightTime=19.0f;
        float _sunRoll = -20.0f;

        int _weather = 0;
        

        [MenuItem("Sandbox/景観まちづくり/景観策定")]

        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(LandscapeConsult), true, "景観協議画面");
        }

        private void OnGUI()
        {

            EditorGUI.BeginChangeCheck();

            var style = new GUIStyle(EditorStyles.label);
            style.richText = true;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("<size=15>視点場の作成</size>", style);
            EditorGUILayout.HelpBox("視点場名と視点高と視野角を入力し'視点場の追加'ボタンをクリックして下さい", MessageType.Info);
            _viewpointFOV = EditorGUILayout.FloatField("視野角", _viewpointFOV);
            _viewpointHeight = EditorGUILayout.FloatField("視点高", _viewpointHeight);
            _viewpointDescription = EditorGUILayout.TextField("視点場名", _viewpointDescription);

            if (GUILayout.Button("視点場の追加"))
            {
                _viewpointRoot = GameObject.Find(ViewPointGroupName);
                if (!_viewpointRoot)
                {
                    _viewpointRoot = new GameObject(ViewPointGroupName);
                    _viewpointRoot.AddComponent<LandscapeViewPointGroup>();
                }
                GameObject child = new GameObject(ViewPointName);
                child.transform.parent = _viewpointRoot.transform;
                _scriptAttachNode = child;

                _keyOperationMode = KEY_OPERATION_MODE.VIEWPOINT;

                LandscapeViewPoint node = _scriptAttachNode.AddComponent<LandscapeViewPoint>();
                node.ViewpointDescription = _viewpointDescription;
                node.viewpointFOV = _viewpointFOV;
                node.EyeHeight = _viewpointHeight;

                if (!GameObject.Find("UI"))
                {
                    GameObject ui = new GameObject();
                    ui.name = "UI";
                    Canvas canvas = ui.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    CanvasScaler scaler = ui.AddComponent<CanvasScaler>();
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                    GraphicRaycaster raycaster = ui.AddComponent<GraphicRaycaster>();
                }

                if (!Camera.main.gameObject.GetComponent<WalkThruHandler>())
                {
                    Camera.main.gameObject.AddComponent<WalkThruHandler>();
                }

                if (!GameObject.Find("EventSystem"))
                {
                    GameObject go = new GameObject();
                    go.name = "EventSystem";
                    EventSystem es = go.AddComponent<EventSystem>();
                    StandaloneInputModule im = go.AddComponent<StandaloneInputModule>();
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("<size=15>眺望対象からの眺望規制作成</size>", style);
            EditorGUILayout.HelpBox("眺望対象地点での幅と高さを設定し眺望規制作成をクリックしてください", MessageType.Info);

            _screenWidth = EditorGUILayout.FloatField("眺望対象地点での幅", _screenWidth);
            _screenHeight = EditorGUILayout.FloatField("眺望対象地点での高さ", _screenHeight);

            if (GUILayout.Button("眺望規制作成"))
            {
                CheckLayers();

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


            EditorGUILayout.Space();
            EditorGUILayout.LabelField("<size=15>規制エリア作成</size>", style);
            EditorGUILayout.HelpBox("規制リアの高さを設定しタイプを選択して規制エリア作成をクリックしてください", MessageType.Info);
            string[] options = { "多角形", "円" };
            _regurationHeight = EditorGUILayout.FloatField("高さ", 10);

            _regurationType = EditorGUILayout.Popup(_regurationType, options);
            if (GUILayout.Button("規制エリア作成"))
            {

                CheckLayers();
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


            _sunLight = GameObject.Find("SunSource").gameObject;
            EditorGUILayout.Space();
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

        void CheckLayers()
        {

            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

            //layer情報を取得
            var layersProp = tagManager.FindProperty("layers");
            var index = 0;
            foreach (var layerId in layerId)
            {
                if (layersProp.arraySize > layerId)
                {
                    var sp = layersProp.GetArrayElementAtIndex(layerId);
                    if (sp != null && sp.stringValue != layerName[index])
                    {
                        sp.stringValue = layerName[index];
                        Debug.Log("Adding layer " + layerName[index]);
                    }
                }

                index++;
            }

            tagManager.ApplyModifiedProperties();

        }

        private void OnInspectorUpdate()
        {

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
