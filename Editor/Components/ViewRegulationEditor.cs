using UnityEditor;
using UnityEngine;

namespace LandscapeDesignTool.Editor
{
    [CustomEditor(typeof(ViewRegulation))]
    [CanEditMultipleObjects]
    public class ViewRegulationEditor : UnityEditor.Editor
    {
        int selectIndex = 0;
        bool selectingTarget = false;
        GameObject vpgroup;
        Color _areaColor = new Color(0, 1, 0, 0.2f);
        Color _areaInvalidColor = new Color(1, 0, 0, 0.2f);
        private float _wsize;
        private float _hsize;
        float _interval = 3.0f;
        SerializedProperty _prop;

        private void Awake()
        {
            _wsize = Selection.activeGameObject.GetComponent<ViewRegulation>().screenWidth;
            _hsize = Selection.activeGameObject.GetComponent<ViewRegulation>().screenHeight;
        }

        private void drawArrayProperty(string prop_name)
        {
            EditorGUIUtility.LookLikeInspector();
            _prop = this.serializedObject.FindProperty(prop_name);

            EditorGUILayout.PropertyField(_prop, new GUIContent("除外オブジェクト"), true);

            this.serializedObject.ApplyModifiedProperties();

            this.serializedObject.ApplyModifiedProperties();

            EditorGUIUtility.LookLikeControls();


        }

        public override void OnInspectorGUI()
        {
            var style = new GUIStyle(EditorStyles.label);
            style.richText = true;
            this.serializedObject.Update();

            SceneView sceneView = SceneView.lastActiveSceneView;

            EditorGUILayout.HelpBox("視点場を選択して眺望対象をシーン内で選択してください", MessageType.Info);
            _wsize = EditorGUILayout.FloatField("眺望対象での横サイズ(m)", _wsize);
            _hsize = EditorGUILayout.FloatField("眺望対象での縦サイズ(m)", _hsize);
            _areaColor = EditorGUILayout.ColorField("色の設定", _areaColor);
            _areaInvalidColor = EditorGUILayout.ColorField("規制色の設定", _areaInvalidColor);
            _interval = EditorGUILayout.FloatField("障害物の判定間隔(m)", _interval);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("<size=12>視点場</size>", style);

            vpgroup = Object.FindObjectOfType<LandscapeViewPointGroup>().gameObject;

            if (vpgroup == null || vpgroup.transform.childCount == 0)
            {
                EditorGUILayout.HelpBox("視点場を作成してください", MessageType.Error);
                return;
            }

            string[] options = new string[vpgroup.transform.childCount];
            for (int i = 0; i < vpgroup.transform.childCount; i++)
            {
                LandscapeViewPoint vp = vpgroup.transform.GetChild(i).GetComponent<LandscapeViewPoint>();
                options[i] = vp.Name;
            }
            selectIndex = EditorGUILayout.Popup(selectIndex, options);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("<size=12>眺望対象</size>", style);
            GUI.color = selectingTarget == false
                ? Color.white
                : Color.green;
            if (GUILayout.Button("眺望対象の選択"))
            {
                sceneView.Focus();
                selectingTarget = true;
            }
        }

        public enum SurfaceType
        {
            Opaque,
            Transparent
        }

        private void OnSceneGUI()
        {
            var ev = Event.current;

            RaycastHit hit;
            if (ev.type == EventType.KeyUp && ev.keyCode == KeyCode.LeftShift)
            {

                Transform origin = vpgroup.transform.GetChild(selectIndex);
                Vector3 originPoint = origin.position;
                Vector3 mousePosition = Event.current.mousePosition;


                Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    Debug.Log(hit.collider.name);
                    Debug.Log(hit.collider.bounds.center);

                    Vector3 targetPoint = hit.collider.bounds.center;

                    selectingTarget = false;
                    float length = Vector3.Distance(originPoint, targetPoint);

                    DrawViewRegulation(originPoint, targetPoint, length, hit);
                }
                ev.Use();
            }
        }

        void DrawViewRegulation(Vector3 originPoint, Vector3 targetPoint, float length, RaycastHit hit)
        {

            Vector3[] vertex = new Vector3[6];
            vertex[0] = new Vector3(0, 0, 0);
            vertex[1] = new Vector3(-_wsize / 2.0f, -_hsize / 2.0f, length);
            vertex[2] = new Vector3(-_wsize / 2.0f, _hsize / 2.0f, length);
            vertex[3] = new Vector3(_wsize / 2.0f, _hsize / 2.0f, length);
            vertex[4] = new Vector3(_wsize / 2.0f, -_hsize / 2.0f, length);
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


            GameObject go = new GameObject("ViewRegurationArea");
            go.layer = LayerMask.NameToLayer("RegulationArea");

            var mf = go.AddComponent<MeshFilter>();
            var mesh = new Mesh();
            mesh.vertices = vertex;
            mesh.triangles = idx;
            var mr = go.AddComponent<MeshRenderer>();

            Material material = LDTTools.MakeMaterial(_areaColor);

            mr.sharedMaterial = material;
            mf.mesh = mesh;

            go.transform.position = originPoint;
            go.transform.LookAt(targetPoint, Vector3.up);
            go.transform.parent = Selection.activeGameObject.transform;
            mr.enabled = false;

            DrawViewLine(originPoint, targetPoint, go);
        }


        float DrawViewLine(Vector3 origin, Vector3 destination, GameObject parent)
        {

            float result = -1;
            
            int divx = (int)(_wsize / _interval);
            int divy = (int)(_hsize / _interval);

            for (int i = 0; i < divx + 1; i++)
            {
                for (int j = 0; j < divy + 1; j++)
                {
                    float x = destination.x - (_wsize / 2.0f) + _interval * i;
                    float y = destination.y - (_hsize / 2.0f) + _interval * j;
                    Vector3 d = new Vector3(x, y, destination.z);
                    RaycastHit hit;

                    if (RaycastBuildings(origin, d, out hit))
                    {
                        DrawLine(origin, hit.point, parent, _areaColor);
                        DrawLine(hit.point, d, parent, _areaInvalidColor);
                    }
                    else
                    {
                        DrawLine(origin, d, parent, _areaColor);
                    }
                }
            }

            return result;
        }

        void DrawLine(Vector3 origin, Vector3 distination, GameObject parent, Color col)
        {
            Vector3[] point = new Vector3[2];
            point[0] = origin;
            point[1] = distination;

            GameObject go = new GameObject("ViewRegurationAreaByLine");
            go.layer = LayerMask.NameToLayer("RegulationArea");

            LineRenderer lineRenderer = go.AddComponent<LineRenderer>();

            lineRenderer.SetPositions(point);
            lineRenderer.positionCount = point.Length;
            lineRenderer.startWidth = 1.0f;
            lineRenderer.endWidth = 1.0f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

            lineRenderer.startColor = col;
            lineRenderer.endColor = col;

            go.transform.parent = parent.transform;
        }

        void CheckCollitionCone(Vector3 originPoint, Vector3 targetPoint, float length, RaycastHit hit)
        {
            float result = CheckCollitionBuilding(originPoint, targetPoint, out _);
            
            if (result == -1)
            {
                Vector3[] vertex = new Vector3[6];
                vertex[0] = new Vector3(0, 0, 0);
                vertex[1] = new Vector3(-_wsize / 2.0f, -_hsize / 2.0f, length);
                vertex[2] = new Vector3(-_wsize / 2.0f, _hsize / 2.0f, length);
                vertex[3] = new Vector3(_wsize / 2.0f, _hsize / 2.0f, length);
                vertex[4] = new Vector3(_wsize / 2.0f, -_hsize / 2.0f, length);
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


                GameObject go = new GameObject("ViewRegurationArea");

                var mf = go.AddComponent<MeshFilter>();
                var mesh = new Mesh();
                mesh.vertices = vertex;
                mesh.triangles = idx;
                var mr = go.AddComponent<MeshRenderer>();

                Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.SetFloat("_Surface", (float)SurfaceType.Transparent);
                material.SetColor("_BaseColor", _areaColor);


                mr.sharedMaterial = material;
                mf.mesh = mesh;


                go.transform.position = originPoint;
                go.transform.LookAt(targetPoint, Vector3.up);
                go.transform.parent = Selection.activeGameObject.transform;
            }
            else
            {
                float divwidth = (_wsize * result) / length;
                float divheight = (_hsize * result) / length;

                Vector3[] vertex = new Vector3[10];
                vertex[0] = new Vector3(0, 0, 0);
                vertex[1] = new Vector3(-_wsize / 2.0f, -_hsize / 2.0f, length);
                vertex[2] = new Vector3(-_wsize / 2.0f, _hsize / 2.0f, length);
                vertex[3] = new Vector3(_wsize / 2.0f, _hsize / 2.0f, length);
                vertex[4] = new Vector3(_wsize / 2.0f, -_hsize / 2.0f, length);

                vertex[5] = new Vector3(-divwidth / 2.0f, -divheight / 2.0f, result);
                vertex[6] = new Vector3(-divwidth / 2.0f, divheight / 2.0f, result);
                vertex[7] = new Vector3(divwidth / 2.0f, divheight / 2.0f, result);
                vertex[8] = new Vector3(divwidth / 2.0f, -divheight / 2.0f, result);

                vertex[9] = new Vector3(0, 0, result);

                int[] idx1 = {
                        0, 5, 6,
                        0, 6, 7,
                        0, 7, 8,
                        0, 8, 5,
                        9, 6, 5,
                        9, 7, 6,
                        9, 8, 7,
                        9, 5, 8 };

                int[] idx2 = {
                        5,1,2,
                        5,2,6,
                        6,2,3,
                        6,3,7,
                        7,3,4,
                        7,4,8,
                        8,4,5,
                        8,5,9,
                        5, 2, 1,
                        5, 3, 2,
                        5, 4, 3,
                        5, 1, 4
                };

                // Safe area
                GameObject go = new GameObject("ViewRegurationArea");

                var mf = go.AddComponent<MeshFilter>();
                var mesh = new Mesh();
                mesh.vertices = vertex;
                mesh.triangles = idx1;
                var mr = go.AddComponent<MeshRenderer>();

                Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.SetFloat("_Surface", (float)SurfaceType.Transparent);
                material.SetColor("_BaseColor", _areaColor);

                mr.sharedMaterial = material;
                mf.mesh = mesh;

                go.transform.position = originPoint;
                go.transform.LookAt(targetPoint, Vector3.up);
                go.transform.parent = Selection.activeGameObject.transform;

                // Invalid area
                GameObject goInvalid = new GameObject("ViewRegurationAreaInvalid");

                var mfInvalid = goInvalid.AddComponent<MeshFilter>();
                var meshInvalid = new Mesh();
                meshInvalid.vertices = vertex;
                meshInvalid.triangles = idx2;
                var mrInvalid = goInvalid.AddComponent<MeshRenderer>();

                Material materialInvalid = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                materialInvalid.SetFloat("_Surface", (float)SurfaceType.Transparent);
                materialInvalid.SetColor("_BaseColor", _areaInvalidColor);

                mrInvalid.sharedMaterial = materialInvalid;
                mfInvalid.mesh = meshInvalid;

                goInvalid.transform.position = originPoint;
                goInvalid.transform.LookAt(targetPoint, Vector3.up);
                goInvalid.transform.parent = go.transform;
            }
        }

        float CheckCollitionBuilding(Vector3 origin, Vector3 distination, out RaycastHit hitpoint)
        {
            float result = -1;
            hitpoint = new RaycastHit();

            int divx = (int)(_wsize / _interval);
            int divy = (int)(_hsize / _interval);

            float mindistance = float.MaxValue;
            for (int i = 0; i < divx + 1; i++)
            {
                for (int j = 0; j < divy + 1; j++)
                {
                    float x = distination.x - (_wsize / 2.0f) + _interval * i;
                    float y = distination.y - (_hsize / 2.0f) + _interval * j;
                    Vector3 d = new Vector3(x, y, distination.z);
                    RaycastHit hit;

                    if (RaycastBuildings(origin, d, out hit))
                    {
                        if (hit.distance < mindistance)
                        {
                            hitpoint = hit;
                            mindistance = hit.distance;
                            result = mindistance;
                        }
                    }
                }
            }

            return result;
        }

        bool RaycastBuildings(Vector3 origin, Vector3 destination, out RaycastHit hitInfo)
        {
            bool result = false;

            hitInfo = new RaycastHit();

            Vector3 direction = (destination - origin).normalized;

            RaycastHit[] hits;
            hits = Physics.RaycastAll(origin, direction, 10000);

            float minDistance = float.MaxValue;
            if (hits.Length <= 0)
                return result;
            
            foreach (var hit in hits)
            {
                if (hit.collider.gameObject.name == target.name)
                    continue;

                int layerIgnoreRaycast = LayerMask.NameToLayer("RegulationArea");
                
                if (hit.collider.gameObject.layer == layerIgnoreRaycast)
                    continue;

                result = true;
                        
                if (hit.distance >= minDistance)
                    continue;

                hitInfo = hit;
                minDistance = hit.distance;
            }

            return result;
        }
    }
}
