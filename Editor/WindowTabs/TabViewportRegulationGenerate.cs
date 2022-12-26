using UnityEditor;
using UnityEngine;

namespace LandscapeDesignTool.Editor.WindowTabs
{
    public class TabViewportRegulationGenerate
    {
        float _screenWidth = 80.0f;
        float _screenHeight = 80.0f;
        int _pointid = 0;
        bool _isEdit = false;
        Vector3 _viewpoint;
        Vector3 _originPoint;
        Color _areaColor = new Color(0, 1, 0, 0.2f);
        Color _areaInvalidColor = new Color(1, 0, 0, 0.2f);
        RegurationAreaHandler _viewRegurationArea;
        GameObject _targetGameObject;

        public void Draw(GUIStyle labelStyle)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("<size=15>眺望対象からの眺望規制作成</size>", labelStyle);
            EditorGUILayout.HelpBox("眺望対象地点での幅と高さを設定し眺望規制作成をクリックしてください", MessageType.Info);

            _screenWidth = EditorGUILayout.FloatField("眺望対象地点での幅", _screenWidth);
            _screenHeight = EditorGUILayout.FloatField("眺望対象地点での高さ", _screenHeight);
            _areaColor = EditorGUILayout.ColorField("色の設定", _areaColor);
            _areaInvalidColor = EditorGUILayout.ColorField("規制色の設定", _areaInvalidColor);

            GameObject[] points =  GameObject.FindGameObjectsWithTag("ViewPoint");
            string[] pointname = new string[points.Length];
            for( int i=0; i<points.Length; i++)
            {
                pointname[i] = points[i].GetComponent<LandscapeViewPoint>().GetDescription();
            }

            _pointid = EditorGUILayout.Popup(_pointid, pointname);

            _originPoint = points[_pointid].transform.position;

            float length = Vector3.Distance(_originPoint, _viewpoint);

            if (_isEdit)
            {
                GUI.color = Color.green;
                if (GUILayout.Button("眺望規制作成"))
                {
                    _isEdit = false;
                    Vector3[] vertex = new Vector3[6];
                    vertex[0] = new Vector3(0, 0, 0);
                    vertex[1] = new Vector3(-_screenWidth / 2.0f, -_screenHeight / 2.0f, length);
                    vertex[2] = new Vector3(-_screenWidth / 2.0f, _screenHeight / 2.0f, length);
                    vertex[3] = new Vector3(_screenWidth / 2.0f, _screenHeight / 2.0f, length);
                    vertex[4] = new Vector3(_screenWidth / 2.0f, -_screenHeight / 2.0f, length);
                    vertex[5] = new Vector3(0, 0, length);

                    int[] idx = {
                        0, 1, 2,
                        0, 2, 3,
                        0, 3, 4,
                        0, 4, 1,
                        5, 2, 1,
                        5, 3, 2,
                        5, 4, 3,
                        5, 1, 4 };

                    GameObject go = new GameObject();
                    go.layer = LayerMask.NameToLayer("RegulationArea");
                    go.name = LDTTools.GetNumberWithTag("ViewRegurationArea", "眺望規制エリア");
                    go.tag = "ViewRegurationArea";
                    _viewRegurationArea = go.AddComponent<RegurationAreaHandler>();

                    var mf = go.AddComponent<MeshFilter>();
                    var mesh = new Mesh();
                    mesh.vertices = vertex;
                    mesh.triangles = idx;
                    var mr = go.AddComponent<MeshRenderer>();

                    Material material = LDTTools.MakeMaterial(_areaColor);

                    mr.sharedMaterial = material;
                    mf.mesh = mesh;

                    go.transform.position = _originPoint;
                    Selection.activeGameObject = go;
                    mr.enabled = false;

                    _viewRegurationArea.SetColor(_areaColor);
                    _viewRegurationArea.SetInvalidColor(_areaInvalidColor);
                    _viewRegurationArea.SetScreenHeight(_screenHeight);
                    _viewRegurationArea.SetScreenHeight(_screenWidth);
                    _viewRegurationArea.CheckCollitionDrawLine(_originPoint, _viewpoint, vertex, _targetGameObject, go);

                    // go.transform.LookAt(_viewpoint, Vector3.up);
                }

                GUI.color = Color.white;
                if (GUILayout.Button("キャンセル"))
                {
                    _isEdit = false;
                    SceneView.RepaintAll();
                }
            }
            else
            {
                GUI.color = Color.white;

                if (GUILayout.Button("眺望対象指定"))
                {
                    _isEdit = true;
                }

            }
        }

        public void OnSceneGUI()
        {
            if (_isEdit) {

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
                        _targetGameObject = hits[0].collider.gameObject;
                        _viewpoint = hits[0].point;
                    }
                }

                Handles.color = Color.blue;
                Handles.CubeHandleCap(0, _viewpoint, Quaternion.Euler(0, 0, 0), 10.0f, EventType.Repaint);
            }
        }
    }
}