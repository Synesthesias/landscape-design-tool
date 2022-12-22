using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LandscapeDesignTool.Editor.WindowTabs
{
    /// <summary>
    /// 視点場作成のGUIを描画します。
    /// </summary>
    public class TabViewPointGenerate
    {
        private float _viewpointFOV = 60.0f;
        private float _viewpointHeight = 1.6f;
        private GameObject _viewpointRoot;
        string _viewpointDescription = "視点場";
        private const string ViewPointGroupName = "ViewPointGroup";
        private const string ViewPointName = "ViewPoint";
        private GameObject _scriptAttachNode;
        private KEY_OPERATION_MODE _keyOperationMode = KEY_OPERATION_MODE.None;
        
        private enum KEY_OPERATION_MODE
        {
            VIEWPOINT,
            None
        }

        public void Draw(GUIStyle labelStyle)
        {
            EditorGUILayout.Space();
                EditorGUILayout.LabelField("<size=15>視点場の作成</size>", labelStyle);
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
        }
    }
}