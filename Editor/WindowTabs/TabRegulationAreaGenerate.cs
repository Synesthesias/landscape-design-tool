using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LandscapeDesignTool.Editor.WindowTabs
{
    public class TabRegulationAreaGenerate
    {
        float _regulationHeight = 10;
        Color _areaColor = new Color(0, 1, 1, 0.5f);
        bool _isCreatingContour = false;
        RegulationArea polygon;
        List<Vector3> vertex = new List<Vector3>();
        private EditorWindow _parentWindow;
        bool _isEditingContour = false;
        GameObject _selectObject = null;

        public TabRegulationAreaGenerate(EditorWindow parentWindow)
        {
            _parentWindow = parentWindow;
        }

        public void OnGUI(GUIStyle labelStyle)
        {
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("<size=15>規制エリア作成</size>", labelStyle);
            EditorGUILayout.HelpBox("規制エリアの高さを設定しタイプを選択して規制エリア作成をクリックしてください", MessageType.Info);

            _regulationHeight = EditorGUILayout.FloatField("高さ", _regulationHeight);
            _areaColor = EditorGUILayout.ColorField("色の設定", _areaColor);
            GUI.enabled = true;

            if (_isCreatingContour)
            {
                DrawCreateContourButton();
            }
            else
            {
                if (!_isEditingContour)
                {
                    GUI.color = Color.white;
                    if (GUILayout.Button("新規規制エリア作成"))
                    {
                        _isCreatingContour = true;
                        GameObject go = new GameObject();
                        go.layer = LayerMask.NameToLayer("RegulationArea");
                        go.name = LDTTools.GetNumberWithTag("RegulationArea", "規制エリア");
                        go.tag = "RegulationArea";
                        Selection.activeObject = go;
                        polygon = go.AddComponent<RegulationArea>();
                        Debug.Log(_regulationHeight);
                        polygon.SetHeight(_regulationHeight);
                        polygon.SetAreaColor(_areaColor);

                        _parentWindow.Repaint();
                    }

                    RegurationAreaList();
                }
                else
                {
                    if (GUILayout.Button("編集完了"))
                    {
                        RegulationArea handler =
                            _selectObject.GetComponent<RegulationArea>();
                        handler.SetHeight(_regulationHeight);
                        handler.SetAreaColor(_areaColor);
                        handler.ClearPoint();
                        handler.SetVertex(vertex);
                        handler.DoEdit();

                        _isEditingContour = false;
                        MarkSceneDirty();
                    }

                    if (GUILayout.Button("キャンセル"))
                        _isEditingContour = false;
                }
            }
        }

        public void OnSceneGUI()
        {
            if (!_isCreatingContour)
                return;

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            var ev = Event.current;
            if (!(ev.type == EventType.MouseDown && !ev.shift && !ev.control && !ev.alt))
                return;
            
            RaycastHit[] hits;
            int layerMask = 1 << 31;
            Vector3 mousePosition = Event.current.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

            hits = Physics.RaycastAll(ray, Mathf.Infinity, layerMask);
            if (hits != null)
                vertex.Add(hits[0].point);

            polygon.SetVertex(vertex);
        }

        void RegurationAreaList()
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("編集は下記リストより選択してください", MessageType.Info);
            GameObject[] objects = GameObject.FindGameObjectsWithTag("RegulationArea");
            int n = -1;
            foreach (var obj in objects)
            {
                n++;

                if (!GUILayout.Button(obj.name))
                    continue;
                
                Selection.activeGameObject = objects[n];
                _selectObject = objects[n];
                _isEditingContour = true;
                
                if (!objects[n].GetComponent<RegulationArea>())
                    continue;
                
                RegulationArea handler =
                    objects[n].GetComponent<RegulationArea>();
                _regulationHeight = handler.GetHeight();
                _areaColor = handler.GetAreaColor();
                List<Vector3> vs = handler.GetVertex();
                vertex.Clear();
                foreach (var v in vs)
                    vertex.Add(v);
            }
        }

        private void DrawCreateContourButton()
        {
            GUI.color = Color.green;
            if (GUILayout.Button("頂点作成を完了し多角形を生成"))
            {
                _isCreatingContour = false;
                vertex.Clear();
                polygon.GenMesh();

                _parentWindow.Repaint();
                MarkSceneDirty();
            }

            GUI.color = Color.white;
            if (GUILayout.Button("頂点をクリア"))
            {
                vertex.Clear();
                MarkSceneDirty();
            }
        }

        private void MarkSceneDirty()
        {
            EditorSceneManager.MarkAllScenesDirty();
        }
    }
}