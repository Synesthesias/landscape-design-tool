using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TriangleNet;
using TriangleNet.Geometry;

namespace LandscapeDesignTool
{
    public class AnyPolygonRegurationAreaHandler : MonoBehaviour
    {
        public float areaHeight;
        [SerializeField] List<Vector3> vertex = new List<Vector3>();

        public List<Vector2> _Contours;
        public Color AreaColor;
        Material _areaMaterial;
        Color OverlayColor;
        public List<Vector2> Vertexes = new List<Vector2>();


        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnDrawGizmosSelected()
        {

            Gizmos.color = Color.blue;

            foreach (Vector3 val in vertex)
            {
                Vector3 v0 = new Vector3(val.x, val.y + 5, val.z);
                Gizmos.DrawCube(v0, new Vector3(10, 10, 10));
            }

            Gizmos.color = Color.red;
            if (vertex.Count > 2)
            {
                for (int i = 0; i < vertex.Count - 1; i++)
                {
                    Vector3 val0 = vertex[i];
                    Vector3 v0 = new Vector3(val0.x, val0.y + 5, val0.z);
                    Vector3 val1 = vertex[i + 1];
                    Vector3 v1 = new Vector3(val1.x, val1.y + 5, val1.z);

                    Gizmos.DrawLine(v0, v1);
                }

                if (vertex.Count > 3)
                {

                    Vector3 val0 = vertex[vertex.Count - 1];
                    Vector3 v0 = new Vector3(val0.x, val0.y + 5, val0.z);
                    Vector3 val1 = vertex[0];
                    Vector3 v1 = new Vector3(val1.x, val1.y + 5, val1.z);

                    Gizmos.DrawLine(v0, v1);

                }
            }


        }

        public void AddPoint(Vector3 p)
        {
            vertex.Add(p);
        }

        public void ClearPoint()
        {
            vertex.Clear();
            // SceneView.RepaintAll();
        }

        public void GenMesh()
        {
            _Contours = new List<Vector2>();

            {
                int i = 0;
                foreach (Vector3 v3 in vertex)
                {
                    Vector3 v0 = v3;
                    Vector2 cont = new Vector2(v3.x, v3.z);
                    _Contours.Add(cont);
                    Vertexes.Add(cont);

                    Vector3 v1;
                    if (i < vertex.Count-1)
                    {
                        v1 = vertex[i + 1];

                    }
                    else
                    {
                        v1 = vertex[0];
                    }
                    float length = Vector3.Distance(v0, v1);
                    int d = (int)(length / 3.0f);

                    float dx = (v1.x - v0.x) / (float)d;
                    float dy = (v1.z - v0.z) / (float)d;

                    for (int j = 1; j < d; j++)
                    {
                        float x = v0.x + dx * (float)j;
                        float y = v0.z + dy * (float)j;
                        Vector2 v2 = new Vector2(x, y);
                        _Contours.Add(v2);

                    }


                    i++;
                }
            }

            Polygon poly = new Polygon();
            poly.Add(_Contours);
            var triangleNetMesh = (TriangleNetMesh)poly.Triangulate();

            GameObject go = new GameObject("Upper");
            go.layer = LayerMask.NameToLayer("RegulationArea");

            var mf = go.AddComponent<MeshFilter>();
            var mesh = triangleNetMesh.GenerateUnityMesh();

            Vector3[] nv = new Vector3[mesh.vertices.Length];

            // RaycastHit hit;
            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                Vector3 ov = mesh.vertices[i];
                Vector3 tmpv = new Vector3(ov.x, 3000, ov.y);

                RaycastHit[] hits;
                bool isGround = false;
                hits = Physics.RaycastAll(tmpv, new Vector3(0, -1, 0), Mathf.Infinity);
                foreach (var hit in hits)
                {
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                    {
                        if (Physics.Raycast(tmpv, new Vector3(0, -1, 0), Mathf.Infinity))
                        {
                            nv[i] = new Vector3(ov.x, hit.point.y + areaHeight, ov.y);
                            isGround = true;
                        }
                    }
                }
                if( isGround == false)
                {
                    nv[i] = new Vector3(ov.x, 0, ov.y);
                }
            }
            mesh.vertices = nv;

            Debug.Log(mesh.bounds.ToString());

            var mr = go.AddComponent<MeshRenderer>();
            Material material = LDTTools.MakeMaterial(AreaColor);
            _areaMaterial = material;

            mr.sharedMaterial = material;
            mf.mesh = mesh;
            MeshCollider meshCollider = go.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;

            mesh.RecalculateBounds();

            GameObject rootNode = new GameObject("RegurationPolygonArea");
            rootNode.layer = LayerMask.NameToLayer("RegulationArea");
            ShapeItem si = rootNode.AddComponent<ShapeItem>();
            si.material = material;
            si.height = areaHeight;
            si.oldHeight = areaHeight;
            si.SetVertex(Vertexes);

            rootNode.transform.parent = gameObject.transform;
            go.transform.parent = rootNode.transform;
            GenerateSide(rootNode);

            ClearPoint();
        }

        void GenerateSide(GameObject parent)
        {

            // int ng = 0;
            _Contours = new List<Vector2>();

            {
                int ni = 0;
                foreach (Vector3 v3 in vertex)
                {
                    Vector3 v0 = v3;
                    Vector2 cont = new Vector2(v3.x, v3.z);
                    _Contours.Add(cont);

                    Vector3 v1;
                    if (ni < vertex.Count - 1)
                    {
                        v1 = vertex[ni + 1];

                    }
                    else
                    {
                        v1 = vertex[0];
                    }
                    float length = Vector3.Distance(v0, v1);
                    int d = (int)(length / 3.0f);

                    float dx = (v1.x - v0.x) / (float)d;
                    float dy = (v1.z - v0.z) / (float)d;

                    for (int nj = 1; nj < d; nj++)
                    {
                        float x = v0.x + dx * (float)nj;
                        float y = v0.z + dy * (float)nj;
                        Vector2 v2 = new Vector2(x, y);
                        _Contours.Add(v2);

                    }


                    ni++;
                }
            }

            Vector3[] nv = new Vector3[_Contours.Count * 2];
            int[] triangles = new int[(_Contours.Count+1) * 2 * 3];

            int i = 0;
            int j = 0;
            int n = 0;

            foreach (Vector2 pt in _Contours)
            {
                Vector3 tmpv = new Vector3(pt.x, 3000, pt.y);

                RaycastHit[] hits;
                hits = Physics.RaycastAll(tmpv, new Vector3(0, -1, 0), Mathf.Infinity);
                bool isGround = false;
                foreach (var hit in hits)
                {
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                    {
                        nv[i++] = new Vector3(pt.x, hit.point.y, pt.y);
                        nv[i++] = new Vector3(pt.x, hit.point.y + areaHeight, pt.y);
                        isGround = true;
                    }
                }
                if( isGround == false)
                {
                    nv[i++] = new Vector3(pt.x, 0, pt.y);
                    nv[i++] = new Vector3(pt.x, areaHeight, pt.y);
                }
            }

            int c1 = _Contours.Count-1;
            for ( int count = 0; count< c1; count++)
            {
                triangles[n++] = count*2 + 2;
                triangles[n++] = count*2 + 1;
                triangles[n++] = count*2;
                triangles[n++] = count*2 + 2;
                triangles[n++] = count*2 + 3;
                triangles[n++] = count*2 + 1;
            }
            triangles[n++] = 0;
            triangles[n++] = (c1 - 1) * 2 + 1; 
            triangles[n++] = (c1 - 1) * 2;
            triangles[n++] = 1;
            triangles[n++] = (c1 - 1) * 2 + 1;
            triangles[n++] = 0;

            GameObject go = new GameObject("Side");
            go.layer = LayerMask.NameToLayer("RegulationArea");
            var mf = go.AddComponent<MeshFilter>();
            var mesh = new Mesh();
            mf.mesh = mesh;
            mesh.vertices = nv;
            mesh.triangles = triangles;

            mesh.RecalculateBounds();

            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = _areaMaterial;

            go.transform.parent = parent.transform;


        }



#if UNITY_EDITOR
        [CustomEditor(typeof(AnyPolygonRegurationAreaHandler))]
        [CanEditMultipleObjects]

        public class AnyPolygonRegurationAreEditor : Editor
        {
            Color _areaColor = new Color(0, 1, 1, 0.5f);
            float _height;
            bool _pointing = false;
            bool _initCamera = false;
            int _nvertex = 0;
            Color _overlayColor = new Color(1, 0, 0, 0.5f);
            bool isBuildSelecting = false;
            List<Vector3> _vertexList = new List<Vector3>();
            SerializedProperty _vertexProp;
            GameObject hitTarget = null;

            private void Awake()
            {
                _height = Selection.activeGameObject.GetComponent<AnyPolygonRegurationAreaHandler>().areaHeight;
                _vertexProp = this.serializedObject.FindProperty("vertex");
            }

            public override void OnInspectorGUI()
            {

                SceneView sceneView = SceneView.lastActiveSceneView;

                EditorGUILayout.HelpBox("高さ、領域色、多角形ポイントを設定して任意規制領域を生成します", MessageType.Info);

                _height = EditorGUILayout.FloatField("高さ(m)", _height);
                _areaColor = EditorGUILayout.ColorField("色の設定", _areaColor);

                EditorGUILayout.Space();
                if (_pointing == false)
                {
                    GUI.color = Color.white;
                    if (GUILayout.Button("多角形頂点の作成"))
                    {
                        sceneView.Focus();
                        _pointing = true;
                        isBuildSelecting = false;
                        /*
                        sceneView.rotation = Quaternion.Euler(90, 0, 0);
                        sceneView.orthographic = true;
                        sceneView.size = 300.0f;
                        */
                        _nvertex = 0;
                    }
                }
                else
                {
                    GUI.color = Color.green;
                    if (GUILayout.Button("頂点作成を完了し多角形を生成"))
                    {
                        _pointing = false;
                        sceneView.orthographic = false;
                        Selection.activeGameObject.GetComponent<AnyPolygonRegurationAreaHandler>().Vertexes.Clear();
                        Selection.activeGameObject.GetComponent<AnyPolygonRegurationAreaHandler>().AreaColor = _areaColor;
                        Selection.activeGameObject.GetComponent<AnyPolygonRegurationAreaHandler>().GenMesh();
                    }
                    GUI.color = Color.white;
                    if (GUILayout.Button("頂点をクリア"))
                    {
                        Selection.activeGameObject.GetComponent<AnyPolygonRegurationAreaHandler>().ClearPoint();
                        SceneView.RepaintAll();
                        _pointing = false;
                    }

                }


                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("建物のカラーを変更します", MessageType.Info);
                // _overlayColor = EditorGUILayout.ColorField("色の設定", _overlayColor);
                if (isBuildSelecting == false)
                {
                    GUI.color = Color.white;
                    if (GUILayout.Button("建物のカラーを変更"))
                    {
                        sceneView.Focus();
                        isBuildSelecting = true;
                    }
                }
                else
                {
                    GUI.color = Color.green;
                    if (GUILayout.Button("カラー変更を終了"))
                    {
                        isBuildSelecting = false;

                    }
                }
            }

            private void OnSceneGUI()
            {
                if (_pointing)
                {
                    var ev = Event.current;
                    RaycastHit hit;
                    if (ev.type == EventType.KeyUp && ev.keyCode == KeyCode.LeftShift)
                    {

                        Vector3 mousePosition = Event.current.mousePosition;

                        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                        {
                            /*
                            _vertexProp.InsertArrayElementAtIndex(_vertexProp.arraySize+1 );
                            SerializedProperty element = _vertexProp.GetArrayElementAtIndex(_vertexProp.arraySize);
                            element.vector3Value = new Vector3(hit.point.x, hit.point.y, hit.point.z);
                            */
                            Debug.Log(hit.point);

                            Selection.activeGameObject.GetComponent<AnyPolygonRegurationAreaHandler>().AddPoint(new Vector3(hit.point.x, hit.point.y, hit.point.z));
                            /*
                            for (int i=0; i<_vertexProp.arraySize; i++)
                            {
                                SerializedProperty prop = _vertexProp.GetArrayElementAtIndex(i);
                                Vector3 val = prop.vector3Value;
                                Debug.Log(val);

                            }
                            */
                            // _vertexList.Add(new Vector3(hit.point.x, hit.point.y, hit.point.z));
                        }
                    }
                }
                if (isBuildSelecting)
                {
                    var ev = Event.current;
                    if (ev.type == EventType.KeyUp && ev.keyCode == KeyCode.LeftShift)
                    {

                        Vector3 mousePosition = Event.current.mousePosition;
                        RaycastHit[] hits;
                        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
                        hits = Physics.RaycastAll(ray, Mathf.Infinity);

                        float mindistance = float.MaxValue;

                        if (hits.Length > 0)
                        {

                            bool isRegurationArea = false;

                            for (int i = 0; i < hits.Length; i++)
                            {
                                RaycastHit hit = hits[i];

                                int layer = LayerMask.NameToLayer("Building");

                                if (hit.collider.gameObject.layer == layer)
                                {
                                    if (hit.distance < mindistance)
                                    {
                                        hitTarget = hit.collider.gameObject;
                                        mindistance = hit.distance;
                                        Debug.Log("hit " + hit.collider.name + " " + mindistance + " " + hit.point.ToString());
                                        Bounds box = hitTarget.GetComponent<Renderer>().bounds;
                                        float x = (box.min.x + box.max.x) / 2.0f;
                                        float z = (box.min.z + box.max.z) / 2.0f;
                                        Vector3 bottomCenter = new Vector3(x, box.min.y, z);
                                        Debug.Log(bottomCenter);

                                        RaycastHit[] hits2;

                                        Physics.queriesHitBackfaces = true;
                                        hits2 = Physics.RaycastAll(bottomCenter, new Vector3(0, 1, 0), Mathf.Infinity);

                                        Debug.Log(hits2.Length);
                                        if (hits.Length > 0)
                                        {
                                            for (int j = 0; j < hits2.Length; j++)
                                            {
                                                RaycastHit hit2 = hits2[j];
                                                Debug.Log(hit2.collider.gameObject.name);
                                                int layer2 = LayerMask.NameToLayer("RegulationArea");
                                                if (hit2.collider.gameObject.layer == layer2)
                                                {
                                                    isRegurationArea = true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (isRegurationArea)
                            {

                                Material material = LDTTools.MakeMaterial(_overlayColor);
                                int nmat;
                                List<Material> oldmat = new List<Material>();
                                hitTarget.GetComponent<Renderer>().GetSharedMaterials(oldmat);
                                _overlayColor = new Color(1,0,0,0.5f);
                                bool find = false;
                                foreach (Material mat in oldmat)
                                {
                                    if (mat.name == LDTTools.MaterialName)
                                    {
                                        find = true;
                                        _overlayColor = mat.GetColor("_BaseColor");
                                    }
                                }

                                Debug.Log("Popup");
                                SelectColorPopup.Init(_overlayColor, ColorSelected, ColorRemove, find);


                            }
                        }
                    }

                        /*
                    if (hitTarget != null)
                    {
                        Bounds box = hitTarget.GetComponent<Renderer>().bounds;
                        float x = (box.min.x + box.max.x) / 2.0f;
                        float z = (box.min.z + box.max.z) / 2.0f;
                        Vector3 bottomCenter = new Vector3(x, box.min.y, z);
                        Debug.Log(bottomCenter);

                        RaycastHit[] hits;

                        Physics.queriesHitBackfaces = true;
                        hits = Physics.RaycastAll(bottomCenter, new Vector3(0, 1, 0), Mathf.Infinity);

                        bool isRegurationArea = false;
                        Debug.Log(hits.Length);
                        if (hits.Length > 0)
                        {
                            for (int i = 0; i < hits.Length; i++)
                            {
                                RaycastHit hit = hits[i];
                                Debug.Log(hit.collider.gameObject.name);
                                int layer = LayerMask.NameToLayer("RegulationArea");
                                if( hit.collider.gameObject.layer == layer)
                                {
                                    isRegurationArea = true;
                                }
                            }
                        }


                        if (isRegurationArea)
                        {

                            Material material = LDTTools.MakeMaterial(_overlayColor);
                            int nmat;
                            List<Material> oldmat = new List<Material>();
                            _overlayColor = Color.red;
                            foreach( Material mat in oldmat)
                            {
                                if(mat.name == LDTTools.MaterialName)
                                {
                                    _overlayColor = mat.GetColor("_BaseColor");
                                }
                            }

                            SelectColorPopup.Init(_overlayColor, ColorSelected);
                        }
                    }
                        */
                }
            }

            void ColorSelected( Color col)
            {
                Debug.Log("Selected " + col.ToString());
                List<Material> oldmat = new List<Material>();
                hitTarget.GetComponent<Renderer>().GetSharedMaterials(oldmat);
                _overlayColor = new Color(1,0,0,0.5f);
                bool f = false;
                foreach (Material mat in oldmat)
                {
                    if (mat.name == LDTTools.MaterialName)
                    {
                         mat.SetColor("_BaseColor", col);
                        f = true;
                    }
                }
                if( f == false)
                {
                    int nmat;
                    Material material = LDTTools.MakeMaterial(col);
                    nmat = oldmat.Count;
                    Material[] matArray = new Material[nmat + 1];
                    for (int i = 0; i < nmat; i++)
                    {
                        matArray[i] = oldmat[i];
                    }
                    matArray[nmat] = material;

                    hitTarget.GetComponent<Renderer>().materials = matArray;
                }
            }

            void ColorRemove()
            {
                int nmat;
                List<Material> oldmat = new List<Material>();
                hitTarget.GetComponent<Renderer>().GetSharedMaterials(oldmat);
                for (int i = 0; i < oldmat.Count; i++)
                {
                    if (oldmat[i].name == LDTTools.MaterialName)
                    {
                        oldmat.RemoveAt(i);
                    }
                }

                hitTarget.GetComponent<Renderer>().sharedMaterials = oldmat.ToArray();
            }

        }

#endif
    }
}