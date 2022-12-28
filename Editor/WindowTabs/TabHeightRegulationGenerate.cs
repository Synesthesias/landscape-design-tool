using UnityEditor;
using UnityEngine;

namespace LandscapeDesignTool.Editor.WindowTabs
{
    public class TabHeightRegulationGenerate
    {
        float _heightAreaHeight = 30.0f;
        float _heightAreaRadius = 100.0f;
        bool _heightReguratoinAreaEdit = false;
        // Vector3 _targetViewPoint;
        HeightRegurationAreaHandler _heightRegurationArea;
        Color _areaColor = new Color(0, 1, 1, 0.5f);
        bool _editMode = false;
        
        public void Draw(GUIStyle labelStyle)
        {
            EditorGUILayout.Space();
                EditorGUILayout.LabelField("<size=15>高さ規制エリア作成</size>", labelStyle);
                EditorGUILayout.HelpBox("高さ規制リアの高さ半径を設定しタイプを選択して規制エリア作成をクリックしてください", MessageType.Info);
                _heightAreaHeight = EditorGUILayout.FloatField("高さ", _heightAreaHeight);
                _heightAreaRadius = EditorGUILayout.FloatField("半径", _heightAreaRadius);
                _areaColor = EditorGUILayout.ColorField("色の設定", _areaColor);

                if (_heightReguratoinAreaEdit)
                {
                    if (_editMode) {
                   
                        if (GUILayout.Button("編集完了"))
                        {
                            _editMode = false;
                            _heightReguratoinAreaEdit = false;

                            SetupRegulationArea(_heightRegurationArea);
                        }
                    }
                    else
                    {
                        GUI.color = Color.green;
                        if (GUILayout.Button("高さ規制エリア作成"))
                        {
                            _heightReguratoinAreaEdit = false;
                            SetupRegulationArea(_heightRegurationArea);
                        }
                        GUI.color = Color.white;
                        if (GUILayout.Button("キャンセル"))
                        {
                            _heightReguratoinAreaEdit = false;
                        }
                    }
                }
                else
                {
                    if (_editMode)
                    {

                        if (GUILayout.Button("編集完了"))
                        {
                            _editMode = false;
                            _heightReguratoinAreaEdit = false;
                            SetupRegulationArea(_heightRegurationArea);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("規制地点を選択"))
                        {
                            GUI.color = Color.white;
                            GameObject grp = GameObject.Find("HeitRegurationAreaGroup");
                            if (!grp)
                            {
                                _heightReguratoinAreaEdit = true;
                                GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                                cylinder.layer = LayerMask.NameToLayer("RegulationArea");
                                HeightRegurationAreaHandler area = cylinder.AddComponent<HeightRegurationAreaHandler>();
                                _heightRegurationArea = area;
                                cylinder.transform.localScale = new Vector3(0, 0, 0);
                                cylinder.name = LDTTools.GetNumberWithTag("HeightRegulationArea", "高さ規制エリア");
                                cylinder.tag = "HeightRegulationArea";

                                Selection.activeGameObject = cylinder;
                            }
                        }
                        HeightRegulationAreaList();
                    }
                }
        }

        private void SetupRegulationArea(HeightRegurationAreaHandler regulationArea)
        {
            // Unityのデフォルト円柱は高さが2mであることに注意
            regulationArea.transform.localScale = new Vector3(_heightAreaRadius, _heightAreaHeight / 2f, _heightAreaRadius);
            regulationArea.SetColor(_areaColor);
            regulationArea.SetHeight(_heightAreaHeight);
            regulationArea.SetRadius(_heightAreaRadius);

            var targetPoint = regulationArea.GetPoint();
            regulationArea.transform.position = new Vector3(targetPoint.x, _heightAreaHeight / 2.0f + targetPoint.y, targetPoint.z);
            Material mat = LDTTools.MakeMaterial(_areaColor);
            regulationArea.GetComponent<Renderer>().material = mat;
        }

        public void OnSceneGUI()
        {
            if (_heightReguratoinAreaEdit)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

                var ev = Event.current;
                if (ev.type == EventType.MouseDown)
                {
                    RaycastHit hit;
                    int layerMask = 1 << 31 | 1 << 29;
                    Vector3 mousePosition = Event.current.mousePosition;
                    Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

                    if(Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
                    {
                        // _targetViewPoint = hit.point;
                        _heightRegurationArea.SetPoint(hit.point);
                    }

                }
            }
        }

        void HeightRegulationAreaList()
        {

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("編集は下記リストより選択してください", MessageType.Info);
            GameObject[] objects = GameObject.FindGameObjectsWithTag("HeightRegulationArea");
            int n = 0;
            foreach (var obj in objects)
            {
                if (GUILayout.Button(obj.name))
                {
                    _editMode = true;
                    Selection.activeGameObject = objects[n];
                    HeightRegurationAreaHandler harea = objects[n].GetComponent<HeightRegurationAreaHandler>();
                    _heightAreaHeight = harea.GetHeight();
                    _heightAreaRadius = harea.GetRadius();
                    _areaColor = harea.GetColor();
                    // _targetViewPoint = harea.GetPoint();
                    _heightRegurationArea = harea;
                }

                n++;
            }
        }
    }
}