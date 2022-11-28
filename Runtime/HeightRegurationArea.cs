using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LandscapeDesignTool
{
    public class HeightRegurationArea : MonoBehaviour
    {
        public float areaHeight;
        public Color areaColor;
        public float areaRadius;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }


#if UNITY_EDITOR
        [CustomEditor(typeof(HeightRegurationArea))]
        [CanEditMultipleObjects]
        public class HeightRegurationEditor : Editor
        {

            Color _areaColor = new Color(0, 1, 1, 0.5f);
            float _height;
            bool _pointing = false;
            float _radius = 10;
            private void Awake()
            {
                _height = Selection.activeGameObject.GetComponent<HeightRegurationArea>().areaHeight;
                _radius = Selection.activeGameObject.GetComponent<HeightRegurationArea>().areaRadius;
            }

            public override void OnInspectorGUI()
            {

                SceneView sceneView = SceneView.lastActiveSceneView;
                EditorGUILayout.HelpBox("í≠ñ]ëŒè€Ç©ÇÁÇÃçÇÇ≥ãKêßÉGÉäÉAÇèCê≥ÇµÇ‹Ç∑", MessageType.Info);

                _height = EditorGUILayout.FloatField("çÇÇ≥(m)", _height);
                _radius = EditorGUILayout.FloatField("îºåa(m)", _radius);

                _areaColor = EditorGUILayout.ColorField("êFÇÃê›íË", _areaColor);
                EditorGUILayout.Space();
                GUI.color = Color.white;
                if (GUILayout.Button("ïœçXÇìKâû"))
                {
                    Selection.activeGameObject.transform.localScale = new Vector3(_radius, _height, _radius);
                    Selection.activeGameObject.transform.position = new Vector3(Selection.activeGameObject.transform.position.x, _height / 2.0f, Selection.activeGameObject.transform.position.z);

                    Material mat = LDTTools.MakeMaterial(_areaColor);
                    Selection.activeGameObject.GetComponent<Renderer>().material = mat;
                }
            }
        }
#endif
    }
}
