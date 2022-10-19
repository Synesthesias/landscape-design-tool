using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        private float _viewpointFOV = 60.0f;
        private float _viewpointHeight = 1.6f;
        private GameObject _viewpointRoot;

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
            EditorGUILayout.LabelField("<size=15>視点場の作成</size>", style);
            EditorGUILayout.HelpBox("視点場名と視野角を入力し'視点場の追加'ボタンを押下して下さい", MessageType.Info);
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
                    Camera.main.gameObject.AddComponent<WalkThruHandler>();
                }
            }
        }
    }
#endif
}
