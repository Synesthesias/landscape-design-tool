using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HeightRestrictedArea : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    
#if UNITY_EDITOR
    [CustomEditor(typeof(HeightRestrictedArea))]
    public class HeightRestrictedAreaEditor : Editor
    {
        private bool _point_edit_in = false;
        private Vector3 _cubePosition;
        public override void OnInspectorGUI()
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (GUILayout.Button("点追加"))
            {
                // sceneView.camera.orthographic = true;

                sceneView.orthographic = true;
                sceneView.rotation = Quaternion.Euler(90,0,0);
                sceneView.size = 2000.0f;
                _point_edit_in = true;
            }

            if (GUILayout.Button("点追加完了"))
            {
                _point_edit_in = false;
                sceneView.orthographic = false;
            }

            var e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.P)
            {
                if (_point_edit_in)
                {
                    Vector3 p = sceneView.camera.transform.position;
                    Debug.Log(p.ToString());
                    Vector3 mousePos = Event.current.mousePosition;
                    mousePos.y = sceneView.camera.pixelHeight - mousePos.y;
                    Ray ray = sceneView.camera.ScreenPointToRay(mousePos);
                    Vector3 mousep = ray.GetPoint(0.0f);

                    RaycastHit hit;
                    if (Physics.Raycast(mousep, new Vector3(0, -1, 0), out hit, Mathf.Infinity))
                    {
                        Debug.Log(hit.point.ToString());
                        Handles.color = Color.red;
                        Handles.DrawWireCube(hit.point,new Vector3(100,100,100));
                        sceneView.Repaint();
                    }

                }
                else
                {
                    Debug.Log("edit in false");
                }
            }

        }

        /*
        void OnDrawGizmosSelected()
        {
            Debug.Log("DrawGismo");
            Gizmos.color = Color.red;
            Gizmos.DrawCube(_cubePosition, new Vector3(100, 100, 100));
        }
        */
        
        void Update()
        {
            Repaint();
        }


    }

#endif
}

