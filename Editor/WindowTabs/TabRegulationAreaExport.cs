using System.Collections.Generic;
using System.IO;
using System.Linq;
using PLATEAU.CityGML;
using PLATEAU.CityInfo;
using UnityEditor;
using UnityEngine;

namespace LandscapeDesignTool.Editor.WindowTabs
{
    public class TabRegulationAreaExport : IGuiTabContents
    {
        // string _regulationAreaExportPath = "";
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

            // using (new EditorGUI.DisabledScope(true))
            // {
            //     EditorGUILayout.TextField("エクスポート先", _regulationAreaExportPath);
            // }

            if (GUILayout.Button("エクスポート"))
            {
                var selectedPath = EditorUtility.SaveFilePanel("保存先", "", "Shapefile", "shp");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    // _regulationAreaExportPath = selectedPath;
                    ExportShapefile(selectedPath, _cityModel);
                    var dirPath = new DirectoryInfo(selectedPath).FullName;
                    EditorUtility.RevealInFinder(dirPath);
                }
            }
        }

        private static void ExportShapefile(string exportPath, PLATEAUInstancedCityModel cityModel)
        {
            List<List<Vector2>> contours = new List<List<Vector2>>();

            GameObject[] objects = GameObject.FindGameObjectsWithTag("RegulationArea");
            GameObject[] viewregurationObjects = GameObject.FindGameObjectsWithTag("ViewRegulationArea");


            int objCount = objects.Length + viewregurationObjects.Length;
            // int objCount = objects.Length;
            string[] types = new string[objCount];
            Color[] cols1 = new Color[objCount];
            Color[] cols2 = new Color[objCount];
            float[] heights = new float[objCount];
            Vector2[,] v2 = new Vector2[objCount, 2];
            Vector3[] originpoint = new Vector3[objCount];
            Vector3[] targetpoint = new Vector3[objCount];
            string[] shapetype = new string[objCount];

            int counter = 0;
            var referencePoint = cityModel.GeoReference.ReferencePoint;
            for (int i = 0; i < objects.Length; i++)
            {
                shapetype[counter] = "RegurationArea";
                if (objects[counter].GetComponent<RegulationArea>())
                {
                    List<Vector2> p = new List<Vector2>();
                    RegulationArea obj =
                        objects[counter].GetComponent<RegulationArea>();
                    types[counter] = "PolygonArea";
                    heights[counter] = obj.GetHeight();
                    cols1[counter] = obj.GetAreaColor();
                    cols2[counter] = obj.GetAreaColor();
                    v2[counter, 0] = new Vector2(0, 0);
                    v2[counter, 1] = new Vector2(0, 0);
                    originpoint[counter] = Vector3.zero;
                    targetpoint[counter] = Vector3.zero;

                    List<Vector2> cnt = obj.GetVertex2D();
                    var convertedCnt = cnt.Select(c =>
                            new Vector2(c.x + (float)referencePoint.X, c.y + (float)referencePoint.Z))
                        .ToList();
                    contours.Add(convertedCnt);

                    counter++;

                }
            }

            Debug.Log("varea : " + viewregurationObjects.Length);
            for (int i = 0; i < viewregurationObjects.Length; i++)
            {
                shapetype[counter] = "ViewRegulationArea";
                if (viewregurationObjects[i].GetComponent<ViewRegulation>())
                {
                    List<Vector2> p = new List<Vector2>();
                    ViewRegulation obj =
                        viewregurationObjects[i].GetComponent<ViewRegulation>();
                    types[counter] = "no type";
                    heights[counter] = 0;
                    cols1[counter] = obj.LineColorValid;
                    cols2[counter] = obj.LineColorInvalid;
                    v2[counter, 0] = new Vector2(obj.ScreenWidth, obj.ScreenHeight);
                    v2[counter, 1] = new Vector2(0, 0);
                    originpoint[counter] = obj.StartPos;
                    targetpoint[counter] = obj.EndPos;

                    GameObject outlineNode = obj.transform.Find("CoveringMesh").gameObject;
                    MeshFilter mf = outlineNode.GetComponent<MeshFilter>();
                    Matrix4x4 localToWorld = outlineNode.transform.localToWorldMatrix;
                    int n = 0;
                    List<Vector2> cnt = new List<Vector2>();

                    Debug.Log("nvertex : " + mf.mesh.vertices.Length);
                    foreach ( Vector3 point in mf.mesh.vertices)
                    {
                        Vector3 world_v = localToWorld.MultiplyPoint3x4(point);

                        if( n== 0 || n==1 || n == 3)
                        {
                            cnt.Add(new Vector2(world_v.x + (float)referencePoint.X, world_v.z + (float)referencePoint.Z));
                        }

                        n++;
                    }
                    contours.Add(cnt);


                    counter++;

                }
       
            }

            LDTTools.WriteShapeFile(exportPath, shapetype, types, cols1,cols2, heights, originpoint, targetpoint, v2,
                contours);
        }

        public void OnSceneGUI()
        {
        }

        public void Update()
        {
            
        }
    }
}