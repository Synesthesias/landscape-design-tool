using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using LandscapeDesignTool;


public class RegurationAreaHandler : MonoBehaviour
{

    [SerializeField] float screenWidth = 80.0f;
    [SerializeField] float screenHeight = 80.0f;
    [SerializeField] Color _areaColor = new Color(0, 1, 0, 0.2f);
    [SerializeField] Color _areaInvalidColor = new Color(1, 0, 0, 0.2f);
    [SerializeField] Vector3 _originPoinnt;
    [SerializeField] Vector3 _targetPoint;
    [SerializeField] GameObject _targetObject;

    float _interval = 3.0f;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void SetColor( Color c)
    {
        _areaColor = c;
    }
    public Color GetColor()
    {
        return _areaColor;
    }
    public void SetInvalidColor(Color c)
    {
        _areaInvalidColor = c;
    }
    public Color GetInvalidColor()
    {
        return _areaInvalidColor;
    }
    public void SetScreenWidth(float w)
    {
        screenWidth = w;
    }
    public float GetScreenWidth()
    {
        return screenWidth;
    }
    public void SetScreenHeight(float w)
    {
        screenHeight = w;
    }
    public float GetScreenHeight()
    {
        return screenHeight;
    }

    public void CheckCollitionDrawLine(Vector3 originPoint, Vector3 targetPoint, Vector3[] contour, GameObject hit, GameObject parent)
    {
        _originPoinnt = new Vector3(originPoint.x, originPoint.y, originPoint.z);
        _targetPoint = new Vector3(targetPoint.x, targetPoint.y, targetPoint.z);
        _targetObject = hit;


        // CheckCollitionBuildingLine(_originPoinnt, _targetPoint, hit, parent);

    }

    void TransfomPoint(Vector3 originPoint, Vector3 targetPoin)
    {

        float l = Vector3.Distance(_originPoinnt, _targetPoint);

        Vector3 p2 = _targetPoint - _originPoinnt;
        Vector3 p1 = new Vector3(0, 0, l);

        GameObject eye = new GameObject("eye");
        eye.transform.position = new Vector3(0, 0, 0);
        eye.transform.LookAt(p2, Vector3.up);
        var matrix = Matrix4x4.identity;
        Quaternion eular = eye.transform.rotation;
        Quaternion r2 = Quaternion.Euler(eular.eulerAngles);
        matrix.SetTRS(Vector3.zero, r2, Vector3.one);

        var newpos = matrix.MultiplyPoint3x4(p1);
        DestroyImmediate(eye);

        GameObject cube1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube1.transform.position = newpos + _originPoinnt;

    }


    float CheckCollitionBuildingLine(Vector3 origin, Vector3 distination, GameObject target, GameObject parent)
    {

        float result = -1;

        float length = Vector3.Distance(origin, distination);
        int divx = (int)(screenWidth / _interval);
        int divy = (int)(screenHeight / _interval);


        for (int i = 0; i < divx + 1; i++)
        {
            for (int j = 0; j < divy + 1; j++)
            {

                float x = distination.x - (screenWidth / 2.0f) + _interval * i;
                float y = distination.y - (screenHeight / 2.0f) + _interval * j;
                Vector3 d = new Vector3(x, y, distination.z);
                RaycastHit hit;

                if (CollisionBuilding(origin, d, length, target, out hit))
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

        /*
        float l = Vector3.Distance(origin, distination);
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        GameObject pivot = new GameObject("ViewRegurationAreaByLine");
        GameObject pivot2 = new GameObject("ViewRegurationAreaByLine");
        pivot.transform.parent = parent.transform;
        pivot2.transform.parent = parent.transform;
        pivot2.transform.parent = pivot.transform;
        cube.transform.parent = pivot2.transform;
        pivot2.transform.rotation = Quaternion.Euler(90, 0, 0);

        cube.name = "ViewRegurationAreaByLine";
        cube.layer = LayerMask.NameToLayer("RegulationArea");
        cube.transform.localScale = new Vector3(1, l, 1);

        Material mat = LDTTools.MakeMaterial(col);
        cube.GetComponent<Renderer>().material = mat;
        cube.transform.localPosition = new Vector3(0, l / 2, 0);

        pivot.transform.position = origin;
        pivot.transform.LookAt(distination);
        
        */

        GameObject go = new GameObject("ViewRegurationAreaByLine");
        go.layer = LayerMask.NameToLayer("RegulationArea");

        LineRenderer lineRenderer = go.AddComponent<LineRenderer>();


        lineRenderer.SetPositions(point);
        lineRenderer.positionCount = point.Length;
        lineRenderer.startWidth = 1.0f;
        lineRenderer.endWidth = 1.0f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        // lineRenderer.material.SetColor("_Color", col);

        lineRenderer.startColor = col;
        lineRenderer.endColor = col;

    }
    bool CollisionBuilding(Vector3 origin, Vector3 distination, float length, GameObject target, out RaycastHit hitpoint)
    {
        bool result = false;

        hitpoint = new RaycastHit();

        Vector3 direction = distination - origin;
        float magnitude = direction.magnitude;
        Vector3 normal = direction / magnitude;


        RaycastHit[] hits;
        hits = Physics.RaycastAll(origin, normal, 10000);

        float mindistance = float.MaxValue;
        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                if (hit.collider.gameObject.name != target.name)
                {
                    bool hitIgnore = false;

                    int layerIgnoreRaycast = LayerMask.NameToLayer("RegulationArea");
                    if (hit.collider.gameObject.layer == layerIgnoreRaycast)
                    {
                        hitIgnore = true;
                    }

                    /*
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                    {
                        hitIgnore = true;
                    }
                    */

                   
                    if (hitIgnore == false)
                    {
                        result = true;
                        if (hit.distance < mindistance)
                        {
                            hitpoint = hit;
                            mindistance = hit.distance;
                        }
                    }
                }
            }
        }
        else
        {
            Debug.Log("No hits");
        }

        return result;
    }
}

/*
#if UNITY_EDITOR
[CustomEditor(typeof(RegurationAreaHandler))]
    [CanEditMultipleObjects]
    public class RegurationAreaEditor : Editor
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
            _wsize = Selection.activeGameObject.GetComponent<RegurationAreaHandler>().screenWidth;
            _hsize = Selection.activeGameObject.GetComponent<RegurationAreaHandler>().screenHeight;
            Debug.Log(_wsize);
        }

        public override void OnInspectorGUI()
        {
            var style = new GUIStyle(EditorStyles.label);
            style.richText = true;
            this.serializedObject.Update();

            SceneView sceneView = SceneView.lastActiveSceneView;

            EditorGUILayout.HelpBox("éãì_èÍÇëIëÇµÇƒí≠ñ]ëŒè€ÇÉVÅ[Éìì‡Ç≈ëIëÇµÇƒÇ≠ÇæÇ≥Ç¢", MessageType.Info);
            _wsize = EditorGUILayout.FloatField("í≠ñ]ëŒè€Ç≈ÇÃâ°ÉTÉCÉY(m)", _wsize);
            _hsize = EditorGUILayout.FloatField("í≠ñ]ëŒè€Ç≈ÇÃècÉTÉCÉY(m)", _hsize);
            _areaColor = EditorGUILayout.ColorField("êFÇÃê›íË", _areaColor);
            _areaInvalidColor = EditorGUILayout.ColorField("ãKêßêFÇÃê›íË", _areaInvalidColor);
            _interval = EditorGUILayout.FloatField("è·äQï®ÇÃîªíËä‘äu(m)", _interval);


            //  drawArrayProperty("ignoreObject");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("<size=12>éãì_èÍ</size>", style);

            vpgroup = GameObject.Find("ViewPointGroup");
            if (!vpgroup)
            {

                EditorGUILayout.HelpBox("éãì_èÍÇçÏê¨ÇµÇƒÇ≠ÇæÇ≥Ç¢", MessageType.Error);
            }
            else
            {
                if (vpgroup.transform.childCount == 0)
                {

                    EditorGUILayout.HelpBox("éãì_èÍÇçÏê¨ÇµÇƒÇ≠ÇæÇ≥Ç¢", MessageType.Error);

                }
                else
                {
                    string[] options = new string[vpgroup.transform.childCount];
                    for (int i = 0; i < vpgroup.transform.childCount; i++)
                    {
                        LandscapeViewPoint vp = vpgroup.transform.GetChild(i).GetComponent<LandscapeViewPoint>();
                        options[i] = vp.GetDescription();
                    }
                    selectIndex = EditorGUILayout.Popup(selectIndex, options);
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("<size=12>í≠ñ]ëŒè€</size>", style);
                    if (selectingTarget == false)
                    {
                        GUI.color = Color.white;
                    }
                    else
                    {
                        GUI.color = Color.green;

                    }
                    if (GUILayout.Button("í≠ñ]ëŒè€ÇÃëIë"))
                    {
                        sceneView.Focus();
                        selectingTarget = true;
                    }
                }
            }
        }
        public enum SurfaceType
        {
            Opaque,
            Transparent
        }

        public void OnSceneGUI()
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

                    // CheckCollitionCone(originPoint, targetPoint, length, hit);

                    CheckCollitionDrawLine(originPoint, targetPoint, length, hit);




                    // Repaint();

                }
                ev.Use();
            }
        }

        public void CheckCollitionDrawLine(Vector3 originPoint, Vector3 targetPoint, float length, RaycastHit hit)
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

            CheckCollitionBuildingLine(originPoint, targetPoint, hit.collider.gameObject, go);

        }


        float CheckCollitionBuildingLine(Vector3 origin, Vector3 distination, GameObject target, GameObject parent)
        {

            float result = -1;

            float length = Vector3.Distance(origin, distination);
            int divx = (int)(_wsize / _interval);
            int divy = (int)(_hsize / _interval);


            for (int i = 0; i < divx + 1; i++)
            {
                for (int j = 0; j < divy + 1; j++)
                {


                    float x = distination.x - (_wsize / 2.0f) + _interval * i;
                    float y = distination.y - (_hsize / 2.0f) + _interval * j;
                    Vector3 d = new Vector3(x, y, distination.z);
                    RaycastHit hit;

                    if (CollisionBuilding(origin, d, length, target, out hit))
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
            // lineRenderer.material.SetColor("_Color", col);

            lineRenderer.startColor = col;
            lineRenderer.endColor = col;

            go.transform.parent = parent.transform;
        }

        void CheckCollitionCone(Vector3 originPoint, Vector3 targetPoint, float length, RaycastHit hit)
        {
            RaycastHit hitpoint;
            float result = CheckCollitionBuilding(originPoint, targetPoint, length, hit.collider.gameObject, out hitpoint);

            Debug.Log("hit " + hitpoint.collider.name + " " + hitpoint.point.ToString() + " " + result);

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

        float CheckCollitionBuilding(Vector3 origin, Vector3 distination, float length, GameObject target, out RaycastHit hitpoint)
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

                    if (CollisionBuilding(origin, d, length, target, out hit))
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

        bool CollisionBuilding(Vector3 origin, Vector3 distination, float length, GameObject target, out RaycastHit hitpoint)
        {
            bool result = false;

            hitpoint = new RaycastHit();

            Vector3 direction = distination - origin;
            float magnitude = direction.magnitude;
            Vector3 normal = direction / magnitude;


            RaycastHit[] hits;
            hits = Physics.RaycastAll(origin, normal, 10000);

            float mindistance = float.MaxValue;
            if (hits.Length > 0)
            {
                for (int i = 0; i < hits.Length; i++)
                {
                    RaycastHit hit = hits[i];
                    if (hit.collider.gameObject.name != target.name)
                    {
                        bool hitIgnore = false;

                        int layerIgnoreRaycast = LayerMask.NameToLayer("RegulationArea");
                        if (hit.collider.gameObject.layer == layerIgnoreRaycast)
                        {
                            hitIgnore = true;
                        }

                        if( hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                        {
                            hitIgnore = true;
                        }


                        if (hitIgnore == false)
                        {
                            result = true;
                            if (hit.distance < mindistance)
                            {
                                hitpoint = hit;
                                mindistance = hit.distance;
                                // Debug.Log("hit " + hit.collider.name + " " + mindistance + " " + hit.point.ToString());
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.Log("No hits");
            }

            return result;
        }
    }
#endif
}
*/

