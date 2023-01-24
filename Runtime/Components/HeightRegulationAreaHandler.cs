using UnityEngine;
using UnityEditor;

namespace LandscapeDesignTool
{
    public class HeightRegulationAreaHandler : MonoBehaviour
    {
        [SerializeField] float areaHeight = 10;
        [SerializeField] float areaDiameter = 10;
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
        public void SetDiameter(float r)
        {
            areaDiameter = r;
        }
        public float GetDiameter()
        {
            return areaDiameter;
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
            [CustomEditor(typeof(HeightRegulationAreaHandler))]
        [CanEditMultipleObjects]
        public class HeightRegurationAreaEditor : Editor
        {

            Color _areaColor = new Color(0, 1, 1, 0.5f);
            float _height;
            bool _pointing = false;
            float _radius = 10;
            private void Awake()
            {
                _height = Selection.activeGameObject.GetComponent<HeightRegulationAreaHandler>().areaHeight;
                _radius = Selection.activeGameObject.GetComponent<HeightRegulationAreaHandler>().areaDiameter;
            }

            public override void OnInspectorGUI()
            {
                var regulation = (HeightRegulationAreaHandler)target;
                EditorGUILayout.LabelField($"高さ: {regulation.areaHeight}");
                EditorGUILayout.LabelField($"直径: {regulation.areaDiameter}");
            }
        }
#endif
    }
}
