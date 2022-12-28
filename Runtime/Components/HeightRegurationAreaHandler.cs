using UnityEngine;
using UnityEditor;

namespace LandscapeDesignTool
{
    public class HeightRegurationAreaHandler : MonoBehaviour
    {
        [SerializeField] float areaHeight = 10;
        [SerializeField] float areaRadius = 10;
        [SerializeField] Vector3 targetPoint= Vector3.zero;
        [SerializeField] Color areaColor;
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

            int n = 0;
            float size = 50f;

            Vector3 v0 = new Vector3(targetPoint.x, targetPoint.y, targetPoint.z);
            Gizmos.DrawCube(v0, new Vector3(10, 10, 10));
        }

        public void SetHeight( float h)
        {
            areaHeight = h;
        }
        public float GetHeight()
        {
            return areaHeight;
        }
        public void SetRadius(float r)
        {
            areaRadius = r;
        }
        public float GetRadius()
        {
            return areaRadius;
        }
        public void SetPoint( Vector3 p)
        {
            targetPoint = new Vector3(p.x, p.y, p.z);
        }
        public Vector3 GetPoint()
        {
            return targetPoint;
        }
        public void SetColor(Color c)
        {
            areaColor = c;
        }
        public Color GetColor()
        {
            return areaColor;
        }


#if UNITY_EDITOR
            [CustomEditor(typeof(HeightRegurationAreaHandler))]
        [CanEditMultipleObjects]
        public class HeightRegurationAreaEditor : Editor
        {

            Color _areaColor = new Color(0, 1, 1, 0.5f);
            float _height;
            bool _pointing = false;
            float _radius = 10;
            private void Awake()
            {
                _height = Selection.activeGameObject.GetComponent<HeightRegurationAreaHandler>().areaHeight;
                _radius = Selection.activeGameObject.GetComponent<HeightRegurationAreaHandler>().areaRadius;
            }

            public override void OnInspectorGUI()
            {
                 /*
                 SceneView sceneView = SceneView.lastActiveSceneView;
                 EditorGUILayout.HelpBox("í≠ñ]ëŒè€Ç©ÇÁÇÃçÇÇ≥ãKêßÉGÉäÉAÇê∂ê¨ÇµÇ‹Ç∑", MessageType.Info);

                 _height = EditorGUILayout.FloatField("çÇÇ≥(m)", _height);
                 _radius = EditorGUILayout.FloatField("îºåa(m)", _radius);

                 _areaColor = EditorGUILayout.ColorField("êFÇÃê›íË", _areaColor);
                 EditorGUILayout.Space();
                 if (_pointing == false)
                 {
                     GUI.color = Color.white;
                     if (GUILayout.Button("í≠ñ]ëŒè€ÇëIë"))
                     {
                         sceneView.Focus();
                         _pointing = true;
                     }
                 }
                 else
                 {
                     GUI.color = Color.green;
                     if (GUILayout.Button("í≠ñ]ëŒè€ÇëIë"))
                     {
                         _pointing = false;
                     }
                 }
                 */
            }
            private void OnSceneGUI()
            {
                
                /*if (_pointing)
                {
                    var ev = Event.current;


                    RaycastHit hit;
                    if (ev.type == EventType.KeyUp && ev.keyCode == KeyCode.LeftShift)
                    {
                        Vector3 mousePosition = Event.current.mousePosition;


                        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                        {
                            Debug.Log("height");
                            Vector3 targetPoint = hit.collider.bounds.center;
                            Debug.Log(targetPoint);
                            Debug.Log(hit.point);
                            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                            cylinder.layer = LayerMask.NameToLayer("RegulationArea");
                            var area = cylinder.AddComponent<HeightRegurationAreaHandler>();
                            cylinder.transform.localScale = new Vector3(_radius, _height, _radius);
                            area.areaColor = _areaColor;
                            area.areaHeight = _height;
                            area.areaRadius = _radius;

                            cylinder.transform.position = new Vector3( targetPoint.x, _height/2.0f, targetPoint.z);
                            Material mat = LDTTools.MakeMaterial(_areaColor);
                            cylinder.GetComponent<Renderer>().material = mat;

                            cylinder.transform.SetParent(Selection.activeGameObject.transform);
                            _pointing = false;

                        }
                        
                    }
                }
                */
            }
        }
#endif
    }
}
