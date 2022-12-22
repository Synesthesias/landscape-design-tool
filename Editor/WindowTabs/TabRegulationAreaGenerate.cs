using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LandscapeDesignTool.Editor.WindowTabs
{
    public class TabRegulationAreaGenerate
    {
        int _regulationType;
        float _regulationHeight = 10;
        Color _areaColor = new Color(0, 1, 1, 0.5f);
        bool _regulationAreaEdit = false;
        bool _isRecurationAreaEdit = false;
        AnyPolygonRegurationAreaHandler polygonHandler;
        List<Vector3> vertex = new List<Vector3>();
        private EditorWindow _parentWindow;

        public TabRegulationAreaGenerate(EditorWindow parentWindow)
        {
            _parentWindow = parentWindow;
        }
        
        public void OnGUI(GUIStyle labelStyle)
        {
            EditorGUILayout.Space();

                EditorGUILayout.LabelField("<size=15>規制エリア作成</size>", labelStyle);
                EditorGUILayout.HelpBox("規制リアの高さを設定しタイプを選択して規制エリア作成をクリックしてください", MessageType.Info);
                string[] options = { "多角形", "円" };
                _regulationHeight = EditorGUILayout.FloatField("高さ", _regulationHeight);
                _areaColor = EditorGUILayout.ColorField("色の設定", _areaColor);

                _regulationType = EditorGUILayout.Popup(_regulationType, options);
                /*
                if (GUILayout.Button("規制エリア作成"))
                {

                    LDTTools.CheckLayers();
                    LDTTools.CheckTag("RegulationArea");

                    if (_regurationType == 0)
                    {
                        /  *
                        GameObject grp = GameObject.Find("AnyPolygonRegurationArea");
                        if (!grp)
                        {
                            grp = new GameObject();
                            grp.name = "AnyPolygonRegurationArea";
                            grp.layer = LayerMask.NameToLayer("RegulationArea");

                            AnyPolygonRegurationAreaHandler handler = grp.AddComponent<AnyPolygonRegurationAreaHandler>();
                            handler.areaHeight = _regurationHeight;
                        }
                        *  /


                        GameObject go = new GameObject();
                        go.layer = LayerMask.NameToLayer("RegulationArea");
                        go.name = LDTTools.GetNumberWithTag("RegulationArea", "規制エリア");
                        go.tag = "RegulationArea";
                        Selection.activeObject = go;
                        AnyPolygonRegurationAreaHandler handler = go.AddComponent<AnyPolygonRegurationAreaHandler>();
                        Debug.Log(_regurationHeight);
                        handler.SetHeight(_regurationHeight);
                        handler.SetAreaColor(_areaColor);
                        _regurationAreaEdit = false;
                        Repaint();
                    }
                    else
                    {
                        /  *
                        GameObject grp = GameObject.Find("AnyCirclnRegurationArea");
                        if (!grp)
                        {
                            grp = new GameObject();
                            grp.name = "AnyCircleRegurationArea";
                            grp.layer = LayerMask.NameToLayer("RegulationArea");

                            AnyCircleRegurationAreaHandler handler = grp.AddComponent<AnyCircleRegurationAreaHandler>();
                            handler.areaHeight = _regurationHeight;



                        }
                        *  /
                        GameObject go = new GameObject();
                        go.layer = LayerMask.NameToLayer("RegulationArea");
                        go.name = LDTTools.GetNumberWithTag("RegulationArea", "規制エリア");
                        go.tag = "RegulationArea";
                        Selection.activeObject = go;
                        AnyCircleRegurationAreaHandler handler = go.AddComponent<AnyCircleRegurationAreaHandler>();
                        Debug.Log(_regurationHeight);
                        handler.SetHeight(_regurationHeight);
                        handler.SetAreaColor(_areaColor);
                        _regurationAreaEdit = false;
                        Repaint();
                    }

                }
                */
                if (_regulationAreaEdit)
                {
                    GUI.color = Color.green;
                    if (GUILayout.Button("頂点作成を完了し多角形を生成"))
                    {
                        _regulationAreaEdit = false;
                        /*
                        GameObject go = new GameObject();
                        go.layer = LayerMask.NameToLayer("RegulationArea");
                        go.name = LDTTools.GetNumberWithTag("RegulationArea", "規制エリア");
                        go.tag = "RegulationArea";
                        Selection.activeObject = go;
                        AnyPolygonRegurationAreaHandler handler = go.AddComponent<AnyPolygonRegurationAreaHandler>();
                        Debug.Log(_regurationHeight);
                        handler.SetHeight(_regurationHeight);
                        handler.SetAreaColor(_areaColor);
                        handler.SetVertex(vertex);
                        */
                        vertex.Clear();
                        polygonHandler.GenMesh();

                        _parentWindow.Repaint();
                    }


                    GUI.color = Color.white;
                    if (GUILayout.Button("頂点をクリア"))
                    {
                        vertex.Clear();
                    }


                }
                else
                {
                    GUI.color = Color.white;
                    if (GUILayout.Button("頂点作成"))
                    {
                        _regulationAreaEdit = true;
                        SceneView sceneView = SceneView.sceneViews[0] as SceneView;
                        // sceneView.Focus();
                        GameObject go = new GameObject();
                        go.layer = LayerMask.NameToLayer("RegulationArea");
                        go.name = LDTTools.GetNumberWithTag("RegulationArea", "規制エリア");
                        go.tag = "RegulationArea";
                        Selection.activeObject = go;
                        polygonHandler = go.AddComponent<AnyPolygonRegurationAreaHandler>();
                        Debug.Log(_regulationHeight);
                        polygonHandler.SetHeight(_regulationHeight);
                        polygonHandler.SetAreaColor(_areaColor);

                        _parentWindow.Repaint();
                    }

                }

                /*
                var ev = Event.current;
                RaycastHit hit;
                if (ev.type == EventType.KeyUp && ev.keyCode == KeyCode.LeftShift)
                {
                    Debug.Log("Shift");
                    Vector3 mousePosition = Event.current.mousePosition;

                    Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                    {
                        Debug.Log(hit.point);
                    }
                }
                */
        }

        public void OnSceneGUI()
        {
            if (_regulationAreaEdit)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                var ev = Event.current;
                if( ev.type == EventType.MouseDown)
                {
                    RaycastHit[] hits;
                    int layerMask = 1 << 31;
                    Vector3 mousePosition = Event.current.mousePosition;

                    Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
                    hits = Physics.RaycastAll(ray, Mathf.Infinity, layerMask);
                    if( hits != null)
                    {
                        vertex.Add(hits[0].point);
                    }

                    int id = 100;
                    Handles.color = Color.blue;
                    polygonHandler.SetVertex(vertex);
                    /*
                    foreach ( Vector3 v in vertex)
                    {
                        Vector3 p = new Vector3(v.x, v.y + 5, v.z);
                        Debug.Log(p);
                        Handles.CubeHandleCap(id, p, Quaternion.Euler(Vector3.forward), 10.0f, EventType.Repaint);
                        id++;
                    }
                    */
                }
            }
        }
    }
}