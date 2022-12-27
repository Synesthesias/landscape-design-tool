using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LandscapeDesignTool.Editor
{
    [CustomEditor(typeof(RegulationArea))]
    public class RegulationAreaEditor : UnityEditor.Editor
    {
        Color _overlayColor = new Color(1, 0, 0, 0.5f);
        bool isBuildSelecting = false;
        GameObject hitTarget = null;

        public override void OnInspectorGUI()
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            var regulationArea = (RegulationArea)target;

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("建物のカラーを変更します", MessageType.Info);
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
            
            EditorGUILayout.LabelField($"頂点数: {regulationArea.GetVertex().Count}");
        }

        private void OnSceneGUI()
        {
            var regulationArea = target as RegulationArea;

            for (int i = 0; i < regulationArea.Vertices.Count; ++i)
            {
                Handles.color = Color.blue;
                EditorGUI.BeginChangeCheck();
                Vector3 pos = Handles.FreeMoveHandle(regulationArea.Vertices[i], Quaternion.identity, 10f, Vector3.zero, Handles.SphereHandleCap);

                if (pos != regulationArea.Vertices[i])
                {
                    // 地面の高さに修正
                    int layerMask = 1 << 31; // Ground
                    var origin = pos + Vector3.up * 10000f;
                    var ray = new Ray(origin, Vector3.down);
                    Physics.Raycast(ray, out RaycastHit hitInfo, float.PositiveInfinity, layerMask);
                    if (hitInfo.transform != null)
                        pos = hitInfo.point;
                }

                if (EditorGUI.EndChangeCheck())
                {
                    regulationArea.Vertices[i] = pos;
                    regulationArea.GenMesh();
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

                    float minDistance = float.MaxValue;

                    if (hits.Length > 0)
                    {
                        bool isRegurationArea = false;

                        for (int i = 0; i < hits.Length; i++)
                        {
                            RaycastHit hit = hits[i];

                            int layer = LayerMask.NameToLayer("Building");

                            if (hit.collider.gameObject.layer == layer)
                            {
                                if (hit.distance < minDistance)
                                {
                                    hitTarget = hit.collider.gameObject;
                                    minDistance = hit.distance;
                                    Debug.Log("hit " + hit.collider.name + " " + minDistance + " " + hit.point.ToString());
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
                            _overlayColor = new Color(1, 0, 0, 0.5f);
                            bool find = false;
                            foreach (Material mat in oldmat)
                            {
                                if (mat.name == LDTTools.MaterialName)
                                {
                                    find = true;
                                    _overlayColor = mat.GetColor("_BaseColor");
                                }
                            }

                            SelectColorPopup.Init(_overlayColor, ColorSelected, ColorRemove, find);
                        }
                    }
                }
            }
        }

        void ColorSelected(Color col)
        {
            Debug.Log("Selected " + col.ToString());
            List<Material> oldmat = new List<Material>();
            hitTarget.GetComponent<Renderer>().GetSharedMaterials(oldmat);
            _overlayColor = new Color(1, 0, 0, 0.5f);
            bool f = false;
            foreach (Material mat in oldmat)
            {
                if (mat.name == LDTTools.MaterialName)
                {
                    mat.SetColor("_BaseColor", col);
                    f = true;
                }
            }

            if (f) return;

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

        void ColorRemove()
        {
            List<Material> oldmat = new List<Material>();
            hitTarget.GetComponent<Renderer>().GetSharedMaterials(oldmat);
            for (int i = 0; i < oldmat.Count; i++)
            {
                if (oldmat[i].name == LDTTools.MaterialName)
                    oldmat.RemoveAt(i);
            }

            hitTarget.GetComponent<Renderer>().sharedMaterials = oldmat.ToArray();
        }
    }
}
