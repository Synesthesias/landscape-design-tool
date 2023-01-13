using System.Collections.Generic;
using System.Linq;
using PLATEAU.CityInfo;
using UnityEditor;
using UnityEngine;

namespace LandscapeDesignTool.Editor.WindowTabs
{
    public class TabRegulationAreaExport : IGuiTabContents
    {
        string _regulationAreaExportPath = "";
        private PLATEAUInstancedCityModel _cityModel;

        public void OnGUI()
        {
            EditorGUILayout.Space();
            LandscapeEditorStyle.Header("ShapeFile出力");

            string[] options = { "規制エリア", "高さ規制エリア", "眺望規制エリア" };

            _cityModel =
                (PLATEAUInstancedCityModel)EditorGUILayout.ObjectField("対象都市", _cityModel,
                    typeof(PLATEAUInstancedCityModel), true);
            if (_cityModel == null) return;

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

            if (GUILayout.Button("規制エリア出力"))
            {
                List<List<Vector2>> contours = new List<List<Vector2>>();

                GameObject[] objects = GameObject.FindGameObjectsWithTag("RegulationArea");
                int objCount = objects.Length;
                string[] types = new string[objCount];
                Color[] cols = new Color[objCount];
                float[] heights = new float[objCount];
                Vector2[,] v2 = new Vector2[objCount, 2];
                var referencePoint = _cityModel.GeoReference.ReferencePoint;
                for (int i = 0; i < objCount; i++)
                {
                    if (objects[i].GetComponent<RegulationArea>())
                    {
                        List<Vector2> p = new List<Vector2>();
                        RegulationArea obj =
                            objects[i].GetComponent<RegulationArea>();
                        types[i] = "PolygonArea";
                        heights[i] = obj.GetHeight();
                        cols[i] = obj.GetAreaColor();
                        v2[i, 0] = new Vector2(0, 0);
                        v2[i, 1] = new Vector2(0, 0);

                        List<Vector2> cnt = obj.GetVertex2D();
                        var convertedCnt = cnt.Select(c =>
                                new Vector2(c.x + (float)referencePoint.X, c.y + (float)referencePoint.Z))
                            .ToList();
                        contours.Add(convertedCnt);
                    }
                }

                LDTTools.WriteShapeFile(_regulationAreaExportPath, "RegurationArea", types, cols, heights, v2,
                    contours);
            }
        }

        public void OnSceneGUI()
        {
        }

        public void Update()
        {
            
        }
    }
}