using UnityEditor;
using UnityEngine;

namespace LandscapeDesignTool.Editor.WindowTabs
{
    public class TabHeightRegulationGenerate : IGuiTabContents
    {
        float _heightAreaHeight = 30.0f;
        float _heightAreaRadius = 100.0f;
        bool _heightRegulationAreaEdit = false;
        // Vector3 _targetViewPoint;
        HeightRegulationAreaHandler _heightRegulationArea;
        Color _areaColor = new Color(0, 1, 1, 0.5f);
        bool _editMode = false;

        /// <summary>
        /// 高さ規制は円柱で表示されますが、その円柱の高さです。
        /// この円柱の高さのうち、規制の高さ分が地面の上に出て、残りは地面の下に埋まります。
        /// 広い範囲を指定しても円柱が浮かないように長めにします。
        /// </summary>
        private const float heightRegulationDisplayLength = 3000f;
        
        public void OnGUI()
        {
            LDTTools.CheckTag("HeightRegulationArea");
            EditorGUILayout.Space();
            
            LandscapeEditorStyle.Header("表示設定");
            LandscapeEditorStyle.ButtonSwitchDisplay(HeightRegulationRendererSetActive);
            
            LandscapeEditorStyle.Header("高さ規制エリア作成");
            EditorGUILayout.HelpBox("高さ規制リアの高さ直径を設定しタイプを選択して規制エリア作成をクリックしてください", MessageType.Info);
            _heightAreaHeight = EditorGUILayout.FloatField("高さ", _heightAreaHeight);
            _heightAreaRadius = EditorGUILayout.FloatField("直径", _heightAreaRadius);
            _areaColor = EditorGUILayout.ColorField("色の設定", _areaColor);

            if (_heightRegulationAreaEdit)
            {
                if (_editMode)
                {
                    if (GUILayout.Button("編集完了"))
                    {
                        _editMode = false;
                        _heightRegulationAreaEdit = false;

                        SetupRegulationArea(_heightRegulationArea);
                    }
                }
                else
                {
                    GUI.color = Color.green;
                    if (GUILayout.Button("高さ規制エリア作成"))
                    {
                        _heightRegulationAreaEdit = false;
                        SetupRegulationArea(_heightRegulationArea);
                    }

                    GUI.color = Color.white;
                    if (GUILayout.Button("キャンセル"))
                    {
                        _heightRegulationAreaEdit = false;
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
                        _heightRegulationAreaEdit = false;
                        SetupRegulationArea(_heightRegulationArea);
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
                            _heightRegulationAreaEdit = true;
                            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                            cylinder.layer = LayerMask.NameToLayer("RegulationArea");
                            HeightRegulationAreaHandler area = cylinder.AddComponent<HeightRegulationAreaHandler>();
                            _heightRegulationArea = area;
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

        private void SetupRegulationArea(HeightRegulationAreaHandler regulationArea)
        {
            // Unityのデフォルト円柱は高さが2mであることに注意
            // regulationArea.transform.localScale = new Vector3(_heightAreaRadius, _heightAreaHeight / 2f, _heightAreaRadius);
            regulationArea.transform.localScale =
                new Vector3(_heightAreaRadius, heightRegulationDisplayLength / 2f, _heightAreaRadius);
            
            regulationArea.SetColor(_areaColor);
            regulationArea.SetHeight(_heightAreaHeight);
            regulationArea.SetRadius(_heightAreaRadius);

            var targetPoint = regulationArea.GetPoint();
            regulationArea.transform.position = new Vector3(targetPoint.x, targetPoint.y - heightRegulationDisplayLength / 2f + _heightAreaHeight, targetPoint.z);
            Material mat = LDTTools.MakeMaterial(_areaColor);
            regulationArea.GetComponent<Renderer>().material = mat;
        }

        public void OnSceneGUI()
        {
            if (_heightRegulationAreaEdit)
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
                        _heightRegulationArea.SetPoint(hit.point);
                    }

                }
            }
        }

        public void Update()
        {
            
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
                    HeightRegulationAreaHandler harea = objects[n].GetComponent<HeightRegulationAreaHandler>();
                    _heightAreaHeight = harea.GetHeight();
                    _heightAreaRadius = harea.GetRadius();
                    _areaColor = harea.GetColor();
                    // _targetViewPoint = harea.GetPoint();
                    _heightRegulationArea = harea;
                }

                n++;
            }
        }

        private static void HeightRegulationRendererSetActive(bool isActive)
        {
            var regulations = Object.FindObjectsOfType<HeightRegulationAreaHandler>();
            foreach (var reg in regulations)
            {
                var renderer = reg.GetComponent<MeshRenderer>();
                if (renderer == null) continue;
                renderer.enabled = isActive;
            }
        }
    }
}