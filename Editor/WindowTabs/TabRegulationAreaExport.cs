using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LandscapeDesignTool.Editor.WindowTabs
{
    public class TabRegulationAreaExport
    {
        string _regulationAreaExportPath = "";
        
        public void Draw(GUIStyle labelStyle)
        {
            EditorGUILayout.Space();
                EditorGUILayout.LabelField("<size=15>規制エリア出力</size>", labelStyle);
                List<string> type = new List<string>();
                List<LDTShapeFileHandler> fields = new List<LDTShapeFileHandler>();
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.TextField("エクスポート先", _regulationAreaExportPath);
                }
                if (GUILayout.Button("エクスポート先選択"))
                {
                    var selectedPath = EditorUtility.SaveFilePanel("保存先", "", "Shapefile", "shp");
                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        _regulationAreaExportPath = selectedPath;
                    }
                }
                List<LDTTools.AreaType> areaTypes = new List<LDTTools.AreaType>();

                if (GUILayout.Button("規制エリア出力"))
                {
                    List<List<Vector2>> contours = new List<List<Vector2>>();
                    GameObject grp = GameObject.Find("AnyPolygonRegurationArea");

                    GameObject[] objects = GameObject.FindGameObjectsWithTag("RegulationArea");
                    string[] types = new string[objects.Length];
                    Color[] cols = new Color[objects.Length];
                    float[] heights = new float[objects.Length];
                    Vector2[,] v2 = new Vector2[objects.Length, 2];
                    for (int i = 0; i < objects.Length; i++)
                    {
                        if (objects[i].GetComponent<AnyPolygonRegurationAreaHandler>())
                        {
                            List<Vector2> p = new List<Vector2>();
                            AnyPolygonRegurationAreaHandler obj = objects[i].GetComponent<AnyPolygonRegurationAreaHandler>();
                            types[i] = "PolygonArea";
                            heights[i] = obj.GetHeight();
                            cols[i] = obj.GetAreaColor();
                            v2[i, 0] = new Vector2(0, 0);
                            v2[i, 1] = new Vector2(0, 0);


                            List<Vector2> cnt = obj.GetVertexData();
                            contours.Add(cnt);

                        }
                    }
                    LDTTools.WriteShapeFile(_regulationAreaExportPath, "RegurationArea", types, cols, heights, v2, contours);

                    /*
                        List<int> instanceList = new List<int>();
                    if (grp)
                    {
                        int narea = grp.transform.childCount;
                        for (int i = 0; i < narea; i++)
                        {
                            GameObject go = grp.transform.GetChild(i).gameObject;
                            instanceList.Add(go.GetInstanceID());
                            ShapeItem handler = go.GetComponent<ShapeItem>();
                            if (handler)
                            {
                                List<Vector2> cnt = handler.Contours;
                                contours.Add(cnt);
                                type.Add("Polygon");
                                fields.Add(handler.fields);

                            }
                        }
                    }
                    grp = GameObject.Find("AnyCircleRegurationArea");
                    if (grp)
                    {
                        int narea = grp.transform.childCount;
                        for (int i = 0; i < narea; i++)
                        {
                            GameObject go = grp.transform.GetChild(i).gameObject;
                            ShapeItem handler = go.GetComponent<ShapeItem>();
                            if (handler)
                            {
                                List<Vector2> cnt = handler.Contours;
                                contours.Add(cnt);
                                type.Add("Circle");
                                fields.Add(handler.fields);
                            }
                        }
                    }
                    */

                }
        }
    }
}