using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



namespace LandscapeDesignTool.Editor
{
#if UNITY_EDITOR
    public class LandscapeDesign :EditorWindow
    {

        int _regulationType;
        float _regulationHeight = 10;
        float _screenWidth = 80.0f;
        float _screenHeight = 80.0f;
        float _heightAreaHeight = 30.0f;
        float _heightAreaRadius = 100.0f;
        Color _areaColor = new Color(0, 1, 1, 0.5f);

        string _regulationAreaExportPath = "";
        bool _regulationAreaEdit = false;
        AnyPolygonRegurationAreaHandler polygonHandler;
        AnyCircleRegurationAreaHandler circleHandler;
        bool isMouseDown = false;
        bool _editMode = false;
        GameObject _selectObject = null;

        Vector3 _circleCenter;
        Vector3 _circleEdge;

        List<Vector3> vertex = new List<Vector3>();

        int outputType = 0;

        bool _heightReguratoinAreaEdit = false;
        Vector3 _targetViewPoint;
        HeightRegurationAreaHandler _heightRegurationArea;



        private readonly string[] _tabToggles = { "規制エリア作成", "眺望規制作成", "高さ規制エリア作成", "ShapeFile書き出し" };
        private int _tabIndex;

        [MenuItem("PLATEAU/景観まちづくり/景観計画")]
        public static void ShowWindow()
        {
            TagAdder.ConfigureTags();
            EditorWindow.GetWindow(typeof(LandscapeDesign), true, "景観計画画面");
        }

        void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (_tabIndex == 0)
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

                                    Repaint();
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
            else if (_tabIndex == 2)
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
                            _targetViewPoint = hit.point;
                            _heightRegurationArea.SetPoint(_targetViewPoint);
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
                        AnyPolygonRegurationAreaHandler handler = objects[n].GetComponent<AnyPolygonRegurationAreaHandler>();
                        _regulationHeight = handler.GetHeight();
                        _areaColor = handler.GetAreaColor();
                        List<Vector3> vs = handler.GetVertex();
                        vertex.Clear();
                        foreach ( var v in vs)
                        {
                            vertex.Add(v);
                        }
                        
                        Debug.Log(vertex.Count);
                        _regulationType = 0;
                    }
                    else if (objects[n].GetComponent<AnyCircleRegurationAreaHandler>())
                    {
                        AnyCircleRegurationAreaHandler handler = objects[n].GetComponent<AnyCircleRegurationAreaHandler>();
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

        void HeightRegurationAreaList()
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
                    _targetViewPoint = harea.GetPoint();
                    _heightRegurationArea = harea;

                }
                n++;
            }
        }

        private void OnGUI()
        {

            var style = new GUIStyle(EditorStyles.label);
            style.richText = true;


            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                _tabIndex = GUILayout.Toolbar(_tabIndex, _tabToggles, new GUIStyle(EditorStyles.toolbarButton), GUI.ToolbarButtonSize.FitToContents);
            }

            if (_tabIndex == 0)
            {
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("<size=15>規制エリア作成</size>", style);
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

                            Repaint();
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

                                Repaint();
                            }

                            RegurationAreaList();
                        }
                        else
                        {
                            if (GUILayout.Button("編集完了"))
                            {
                                AnyPolygonRegurationAreaHandler handler = _selectObject.GetComponent<AnyPolygonRegurationAreaHandler>();
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
                            Repaint();
                        }
                        GUI.color = Color.white;
                        if (GUILayout.Button("点をクリア"))
                        {
                            circleHandler.ClearPoint();
                            Repaint();
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
                                Repaint();
                            }
                            RegurationAreaList();
                        }
                        else
                        {
                            if (GUILayout.Button("編集完了"))
                            {
                                AnyCircleRegurationAreaHandler handler = _selectObject.GetComponent<AnyCircleRegurationAreaHandler>();
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
            else if (_tabIndex == 1)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("<size=15>眺望対象からの眺望規制作成</size>", style);
                EditorGUILayout.HelpBox("眺望対象地点での幅と高さを設定し眺望規制作成をクリックしてください", MessageType.Info);

                _screenWidth = EditorGUILayout.FloatField("眺望対象地点での幅", _screenWidth);
                _screenHeight = EditorGUILayout.FloatField("眺望対象地点での高さ", _screenHeight);

                if (GUILayout.Button("眺望規制作成"))
                {
                    LDTTools.CheckLayers();

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

            }
            else if (_tabIndex == 2)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("<size=15>高さ規制エリア作成</size>", style);
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

                        _heightRegurationArea.transform.localScale = new Vector3(_heightAreaRadius, _heightAreaHeight, _heightAreaRadius);
                            _heightRegurationArea.SetColor(_areaColor);
                            _heightRegurationArea.SetHeight(_heightAreaHeight);
                            _heightRegurationArea.SetRadius(_heightAreaRadius);

                            _heightRegurationArea.transform.position = new Vector3(_targetViewPoint.x, _heightAreaHeight / 2.0f, _targetViewPoint.z);
                            Material mat = LDTTools.MakeMaterial(_areaColor);
                            _heightRegurationArea.GetComponent<Renderer>().material = mat;
                        }
                    }
                    else
                    {
                        GUI.color = Color.green;
                        if (GUILayout.Button("高さ規制エリア作成"))
                        {
                            _heightReguratoinAreaEdit = false;

                            _heightRegurationArea.transform.localScale = new Vector3(_heightAreaRadius, _heightAreaHeight, _heightAreaRadius);
                            _heightRegurationArea.SetColor(_areaColor);
                            _heightRegurationArea.SetHeight(_heightAreaHeight);
                            _heightRegurationArea.SetRadius(_heightAreaRadius);

                            _heightRegurationArea.transform.position = new Vector3(_targetViewPoint.x, _heightAreaHeight / 2.0f, _targetViewPoint.z);
                            Material mat = LDTTools.MakeMaterial(_areaColor);
                            _heightRegurationArea.GetComponent<Renderer>().material = mat;
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

                            _heightRegurationArea.transform.localScale = new Vector3(_heightAreaRadius, _heightAreaHeight, _heightAreaRadius);
                            _heightRegurationArea.SetColor(_areaColor);
                            _heightRegurationArea.SetHeight(_heightAreaHeight);
                            _heightRegurationArea.SetRadius(_heightAreaRadius);

                            _heightRegurationArea.transform.position = new Vector3(_targetViewPoint.x, _heightAreaHeight / 2.0f, _targetViewPoint.z);
                            Material mat = LDTTools.MakeMaterial(_areaColor);
                            _heightRegurationArea.GetComponent<Renderer>().material = mat;
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
                        HeightRegurationAreaList();
                    }
                }
            }
            else if (_tabIndex == 3)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("<size=15>ShapeFile出力</size>", style);
                List<string> type = new List<string>();
                List<LDTShapeFileHandler> fields = new List<LDTShapeFileHandler>();

                string[] options = { "規制エリア", "高さ規制エリア","眺望規制エリア" };

                outputType = EditorGUILayout.Popup(outputType, options);

                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.TextField("エクスポート先", _regulationAreaExportPath);
                }
                if (GUILayout.Button("エクスポート先選択"))
                {
                    var selectedPath = EditorUtility.SaveFilePanel("保存先", "", "Shapefile", "shp");
                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        _regulationAreaExportPath = selectedPath;
                    }
                }
                List<LDTTools.AreaType> areaTypes = new List<LDTTools.AreaType>();

                if (outputType == 0)
                {
                    if (GUILayout.Button("規制エリア出力"))
                    {
                        List<List<Vector2>> contours = new List<List<Vector2>>();

                        GameObject[] objects = GameObject.FindGameObjectsWithTag("RegulationArea");
                        string[] types = new string[objects.Length];
                        Color[] cols = new Color[objects.Length];
                        float[] heights = new float[objects.Length];
                        Vector2[,] v2 = new Vector2[objects.Length, 2];
                        for (int i = 0; i < objects.Length; i++)
                        {
                            if (objects[i].GetComponent<AnyPolygonRegurationAreaHandler>())
                            {
                                List<Vector2> p = new List<Vector2>();
                                AnyPolygonRegurationAreaHandler obj = objects[i].GetComponent<AnyPolygonRegurationAreaHandler>();
                                types[i] = "PolygonArea";
                                heights[i] = obj.GetHeight();
                                cols[i] = obj.GetAreaColor();
                                v2[i, 0] = new Vector2(0, 0);
                                v2[i, 1] = new Vector2(0, 0);


                                List<Vector2> cnt = obj.GetVertexData();
                                contours.Add(cnt);

                            }
                            else if (objects[i].GetComponent<AnyCircleRegurationAreaHandler>())
                            {

                                AnyCircleRegurationAreaHandler obj = objects[i].GetComponent<AnyCircleRegurationAreaHandler>();
                                types[i] = "PolygonArea";
                                heights[i] = obj.GetHeight();
                                cols[i] = obj.GetAreaColor();
                                v2[i, 0] = new Vector2(obj.GetCenter().x, obj.GetCenter().z);
                                v2[i, 1] = new Vector2(obj.GetAreaRadius().x, obj.GetAreaRadius().z);
                                List<Vector2> cnt = obj.GetVertex();
                                contours.Add(cnt);
                            }
                        }
                        LDTTools.WriteShapeFile(_regulationAreaExportPath, "RegurationArea", types, cols, heights, v2, contours);

                    }
                }
            }
        }

    }


#endif
}