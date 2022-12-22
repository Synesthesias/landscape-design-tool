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
            EditorGUILayout.LabelField("<size=15>ShapeFile出力</size>", labelStyle);
            List<string> type = new List<string>();
            List<LDTShapeFileHandler> fields = new List<LDTShapeFileHandler>();
            int outputType = 0;

            string[] options = { "規制エリア", "高さ規制エリア", "眺望規制エリア" };

            outputType = EditorGUILayout.Popup(outputType, options);

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

            if (outputType == 0)
            {
                if (GUILayout.Button("規制エリア出力"))
                {
                    List<List<Vector2>> contours = new List<List<Vector2>>();

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
                            AnyPolygonRegurationAreaHandler obj =
                                objects[i].GetComponent<AnyPolygonRegurationAreaHandler>();
                            types[i] = "PolygonArea";
                            heights[i] = obj.GetHeight();
                            cols[i] = obj.GetAreaColor();
                            v2[i, 0] = new Vector2(0, 0);
                            v2[i, 1] = new Vector2(0, 0);


                            List<Vector2> cnt = obj.GetVertexData();
                            contours.Add(cnt);

                        }
                        else if (objects[i].GetComponent<AnyCircleRegurationAreaHandler>())
                        {

                            AnyCircleRegurationAreaHandler obj =
                                objects[i].GetComponent<AnyCircleRegurationAreaHandler>();
                            types[i] = "PolygonArea";
                            heights[i] = obj.GetHeight();
                            cols[i] = obj.GetAreaColor();
                            v2[i, 0] = new Vector2(obj.GetCenter().x, obj.GetCenter().z);
                            v2[i, 1] = new Vector2(obj.GetAreaRadius().x, obj.GetAreaRadius().z);
                            List<Vector2> cnt = obj.GetVertex();
                            contours.Add(cnt);
                        }
                    }

                    LDTTools.WriteShapeFile(_regulationAreaExportPath, "RegurationArea", types, cols, heights, v2,
                        contours);

                }
            }
        }
    }
}