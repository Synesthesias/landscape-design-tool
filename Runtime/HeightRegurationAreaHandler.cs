using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LandscapeDesignTool
{
    public class HeightRegurationAreaHandler : MonoBehaviour
    {
        public float areaHeight = 10;
        public float areaRadius = 10;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

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
                            Vector3 targetPoint = hit.collider.bounds.center;
                            Debug.Log(targetPoint);
                            Debug.Log(hit.point);
                            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                            cylinder.layer = LayerMask.NameToLayer("RegulationArea");
                            HeightRegurationArea area = cylinder.AddComponent<HeightRegurationArea>();
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
            }
        }
    }
#endif
}
