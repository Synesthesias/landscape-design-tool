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
        AnyCircleRegurationAreaHandler circleHandler;
        bool isMouseDown = false;
        bool _editMode = false;
        GameObject _selectObject = null;
        Vector3 _circleCenter;
        Vector3 _circleEdge;

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
            if (_editMode)
            {
                GUI.enabled = false;
            }
            else
            {
                GUI.enabled = true;
            }

            _regulationType = EditorGUILayout.Popup(_regulationType, options);
            GUI.enabled = true;

            if (_regulationType == 0)
            {
                if (_regulationAreaEdit)
                {
                    GUI.color = Color.green;
                    if (GUILayout.Button("頂点作成を完了し多角形を生成"))
                    {
                        _regulationAreaEdit = false;
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
                    if (_editMode == false)
                    {
                        GUI.color = Color.white;
                        if (GUILayout.Button("新規規制エリア作成"))
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

                        RegurationAreaList();
                    }
                    else
                    {
                        if (GUILayout.Button("編集完了"))
                        {
                            AnyPolygonRegurationAreaHandler handler =
                                _selectObject.GetComponent<AnyPolygonRegurationAreaHandler>();
                            handler.SetHeight(_regulationHeight);
                            handler.SetAreaColor(_areaColor);
                            handler.ClearPoint();
                            handler.SetVertex(vertex);
                            handler.DoEdit();

                            _editMode = false;
                        }

                        if (GUILayout.Button("キャンセル"))
                        {
                            _editMode = false;
                        }
                    }
                }
            }
            else
            {
                if (_regulationAreaEdit)
                {
                    GUI.color = Color.green;
                    if (GUILayout.Button("円による規制エリアを生成"))
                    {
                        circleHandler.GenMesh();
                        _regulationAreaEdit = false;
                        _parentWindow.Repaint();
                    }

                    GUI.color = Color.white;
                    if (GUILayout.Button("点をクリア"))
                    {
                        circleHandler.ClearPoint();
                        _parentWindow.Repaint();
                    }
                }
                else
                {
                    if (_editMode == false)
                    {
                        if (GUILayout.Button("新規規制エリア作成"))
                        {
                            _regulationAreaEdit = true;

                            GameObject go = new GameObject();
                            go.layer = LayerMask.NameToLayer("RegulationArea");
                            go.name = LDTTools.GetNumberWithTag("RegulationArea", "規制エリア");
                            go.tag = "RegulationArea";
                            Selection.activeObject = go;
                            circleHandler = go.AddComponent<AnyCircleRegurationAreaHandler>();
                            circleHandler.SetHeight(_regulationHeight);
                            circleHandler.SetAreaColor(_areaColor);
                            _parentWindow.Repaint();
                        }

                        RegurationAreaList();
                    }
                    else
                    {
                        if (GUILayout.Button("編集完了"))
                        {
                            AnyCircleRegurationAreaHandler handler =
                                _selectObject.GetComponent<AnyCircleRegurationAreaHandler>();
                            handler.SetHeight(_regulationHeight);
                            handler.SetAreaColor(_areaColor);
                            handler.ClearPoint();
                            handler.SetCenter(_circleCenter);
                            handler.SetArcRadius(_circleEdge);
                            handler.FinishArc();
                            handler.DoEdit();
                            _editMode = false;
                        }

                        if (GUILayout.Button("キャンセル"))
                        {
                            _editMode = false;
                        }
                    }
                }
            }
        }

        public void OnSceneGUI()
        {
            if (_regulationAreaEdit)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                var ev = Event.current;
                if (ev.type == EventType.MouseDown)
                {
                    RaycastHit[] hits;
                    int layerMask = 1 << 31;
                    Vector3 mousePosition = Event.current.mousePosition;
                    Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

                    if (_regulationType == 0)
                    {
                        hits = Physics.RaycastAll(ray, Mathf.Infinity, layerMask);
                        if (hits != null)
                        {
                            vertex.Add(hits[0].point);
                        }

                        int id = 100;
                        Handles.color = Color.blue;
                        polygonHandler.SetVertex(vertex);
                    }
                    else
                    {
                        hits = Physics.RaycastAll(ray, Mathf.Infinity, layerMask);
                        if (hits != null)
                        {
                            circleHandler.SetCenter(hits[0].point);
                            isMouseDown = true;
                        }
                    }
                }

                if (ev.type == EventType.MouseUp || ev.type == EventType.MouseMove)
                {
                    if (isMouseDown)
                    {
                        RaycastHit[] hits;
                        int layerMask = 1 << 31;
                        Vector3 mousePosition = Event.current.mousePosition;
                        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
                        if (_regulationType == 1)
                        {
                            hits = Physics.RaycastAll(ray, Mathf.Infinity, layerMask);
                            if (hits != null)
                            {
                                circleHandler.SetArcRadius(hits[0].point);

                                _parentWindow.Repaint();
                                if (ev.type == EventType.MouseUp)
                                {
                                    circleHandler.FinishArc();
                                    isMouseDown = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        void RegurationAreaList()
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("編集は下記リストより選択してください", MessageType.Info);
            GameObject[] objects = GameObject.FindGameObjectsWithTag("RegulationArea");
            int n = 0;
            foreach (var obj in objects)
            {
                if (GUILayout.Button(obj.name))
                {
                    Selection.activeGameObject = objects[n];
                    _selectObject = objects[n];
                    _editMode = true;
                    if (objects[n].GetComponent<AnyPolygonRegurationAreaHandler>())
                    {
                        AnyPolygonRegurationAreaHandler handler =
                            objects[n].GetComponent<AnyPolygonRegurationAreaHandler>();
                        _regulationHeight = handler.GetHeight();
                        _areaColor = handler.GetAreaColor();
                        List<Vector3> vs = handler.GetVertex();
                        vertex.Clear();
                        foreach (var v in vs)
                        {
                            vertex.Add(v);
                        }

                        Debug.Log(vertex.Count);
                        _regulationType = 0;
                    }
                    else if (objects[n].GetComponent<AnyCircleRegurationAreaHandler>())
                    {
                        AnyCircleRegurationAreaHandler handler =
                            objects[n].GetComponent<AnyCircleRegurationAreaHandler>();
                        _regulationHeight = handler.GetHeight();
                        _areaColor = handler.GetAreaColor();
                        Vector3 v = handler.GetCenter();
                        _circleCenter = new Vector3(v.x, v.y, v.z);
                        v = handler.GetAreaRadius();
                        _circleEdge = new Vector3(v.x, v.y, v.z);
                        _regulationType = 1;
                    }
                }

                n++;
            }
        }
    }
}