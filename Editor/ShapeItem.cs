using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LandscapeDesignTool.Editor
{
    public class ShapeItem : MonoBehaviour
    {
        public Material material;
        public float height;

        public float oldHeight;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

#if UNITY_EDITOR
        [CustomEditor(typeof(ShapeItem))]
        public class SapeItemEditor :UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                Selection.activeGameObject.GetComponent<ShapeItem>().material =
        (Material)EditorGUILayout.ObjectField("マテリアル",
            Selection.activeGameObject.GetComponent<ShapeItem>().material, typeof(Material), false);

                Selection.activeGameObject.GetComponent<ShapeItem>().height =
                    EditorGUILayout.FloatField("高さ",
                        Selection.activeGameObject.GetComponent<ShapeItem>().height);
                if (GUILayout.Button("変更する"))
                {
                    Selection.activeGameObject.GetComponent<ShapeItem>().ReConstruct();
                }
            }
        }
#endif

        void ReConstruct()
        {
            GameObject upper = gameObject.transform.Find("Upper").gameObject;

            MeshRenderer mr = upper.GetComponent<MeshRenderer>();
            MeshFilter mf = upper.GetComponent<MeshFilter>();
            // Vector3[] ov = mf.mesh.vertices;

            Vector3[] nv = new Vector3[mf.sharedMesh.vertices.Length];

            for (int i = 0; i < mf.sharedMesh.vertices.Length; i++)
            {
                Vector3 ov = mf.sharedMesh.vertices[i];

                nv[i] = new Vector3(ov.x, (ov.y - oldHeight) + height, ov.z);

            }
            mf.sharedMesh.vertices = nv;
            mr.sharedMaterial = material;


            GameObject side = gameObject.transform.Find("Side").gameObject;

            mr = side.GetComponent<MeshRenderer>();
            mf = side.GetComponent<MeshFilter>();

            nv = new Vector3[mf.sharedMesh.vertices.Length];
            mf.sharedMesh.vertices.CopyTo(nv, 0);

            for (int i = 1; i < mf.sharedMesh.vertices.Length; i += 2)
            {
                Vector3 ov = nv[i];
                nv[i] = new Vector3(ov.x, (ov.y - oldHeight) + height, ov.z);

            }

            mf.sharedMesh.vertices = nv;
            mr.sharedMaterial = material;

            oldHeight = height;

        }
    }
}
