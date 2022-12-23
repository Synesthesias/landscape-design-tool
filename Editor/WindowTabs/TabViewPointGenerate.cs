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
        string _viewpointDescription = "視点場";
        bool _isEdit = false;
        private Vector3 _viewpoint;
        private LandscapeViewPoint viewpointNode;

        public void Draw(GUIStyle labelStyle)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("<size=15>視点場の作成</size>", labelStyle);
            EditorGUILayout.HelpBox("視点場名と視点高と視野角と視点場を設定し視点場の追加をクリックしてください", MessageType.Info);
            _viewpointFOV = EditorGUILayout.FloatField("視野角", _viewpointFOV);
            _viewpointHeight = EditorGUILayout.FloatField("視点高", _viewpointHeight);
            _viewpointDescription = EditorGUILayout.TextField("視点場名", _viewpointDescription);

            if (!_isEdit)
            {
                GUI.color = Color.white;
                if (GUILayout.Button("視点場の指定"))
                {
                    _isEdit = true;
                }
            }
            else
            {

                GUI.color = Color.green;
                if (GUILayout.Button("視点場の追加"))
                {

                    _isEdit = false;
                    GameObject go = new GameObject();
                    go.name = LDTTools.GetNumberWithTag("ViewPoint", "視点場");
                    go.tag = "ViewPoint";

                    LandscapeViewPoint node = go.AddComponent<LandscapeViewPoint>();
                    viewpointNode = node;
                    node.SetDescription( _viewpointDescription);
                    node.SetFOV(  _viewpointFOV);
                    node.SetHeight( _viewpointHeight);
                    go.transform.position = _viewpoint;

                    Selection.activeObject = go;

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
                        GameObject esgo = new GameObject();
                        esgo.name = "EventSystem";
                        EventSystem es = esgo.AddComponent<EventSystem>();
                        StandaloneInputModule im = esgo.AddComponent<StandaloneInputModule>();
                    }
                }
                GUI.color = Color.white;
                if (GUILayout.Button("キャンセル"))
                {
                    _isEdit = true;
                }
            }
        }

        public void OnSceneGUI()
        {
            if (_isEdit)
            {

                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

                var ev = Event.current;
                if (ev.type == EventType.MouseDown)
                {
                    RaycastHit[] hits;
                    int layerMask = 1 << 31;
                    Vector3 mousePosition = Event.current.mousePosition;
                    Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

                    hits = Physics.RaycastAll(ray, Mathf.Infinity, layerMask);
                    if (hits != null)
                    {
                        _viewpoint = hits[0].point;
                    }
                }

                Handles.color = Color.blue;
                Handles.CubeHandleCap(0, _viewpoint, Quaternion.Euler(0, 0, 0), 10.0f, EventType.Repaint);
            }
        }

        
    }
}