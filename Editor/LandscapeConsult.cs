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
                    GameObject grp = GameObject.Find("AnyCirclenRegurationArea");
                    if (!grp)
                    {
                        grp = new GameObject();
                        grp.name = "AnyCirclenRegurationArea";
                        grp.layer = LayerMask.NameToLayer("RegulationArea");

                        AnyCircleRegurationAreaHandler handler = grp.AddComponent<AnyCircleRegurationAreaHandler>();
                        handler.areaHeight = _regurationHeight;



                    }
                }

            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("<size=15>規制エリア作成</size>", style);
            EditorGUILayout.HelpBox("高さ規制リアの高さ半径を設定しタイプを選択して規制エリア作成をクリックしてください", MessageType.Info);
            _heightAreaHeight = EditorGUILayout.FloatField("高さ", _heightAreaHeight);
            _heightAreaRadius = EditorGUILayout.FloatField("半径", _heightAreaRadius);

            if (GUILayout.Button("規制エリア作成"))
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
    }
#endif
}
