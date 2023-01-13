using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using EGIS.ShapeFileLib;
using PLATEAU.CityInfo;
using PLATEAU.Geometries;

namespace LandscapeDesignTool
{
    public class ShapeFileEditorHelper
    {
        private float _areaHeight = 10;
        private string _shapefileLoadPath;
        private string _generateGameObjName = "LoadedShapeFile";

        private List<List<Vector2>> _contours;
        
        private PLATEAUInstancedCityModel _cityModel;

#if UNITY_EDITOR

        public void DrawGui()
        {
            _cityModel =
                (PLATEAUInstancedCityModel)EditorGUILayout.ObjectField("対象都市", _cityModel,
                    typeof(PLATEAUInstancedCityModel), true);
            EditorGUILayout.LabelField("読込ファイル:");
            string displayPath = string.IsNullOrEmpty(_shapefileLoadPath) ? "未選択" : _shapefileLoadPath;

            EditorGUILayout.LabelField(displayPath);
            if (GUILayout.Button("ファイル選択"))
            {
                string selectedPath = EditorUtility.OpenFilePanel("ShapeFile選択", "", "shp");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    _shapefileLoadPath = selectedPath;
                }
            }
            
            if (string.IsNullOrEmpty(_shapefileLoadPath)) return;

            _generateGameObjName = EditorGUILayout.TextField("ゲームオブジェクト名: ", _generateGameObjName);

            _areaHeight =
                EditorGUILayout.FloatField("高さ",
                    _areaHeight);
            if (_cityModel == null)
            {
                EditorGUILayout.HelpBox("対象都市を指定してください。", MessageType.Error);
            }
            else if (GUILayout.Button("メッシュデータの作成"))
            {
                var parentObj = new GameObject(_generateGameObjName);
                BuildMesh(_shapefileLoadPath, parentObj.transform, _cityModel.GeoReference);
            }
        }
#endif

#if UNITY_EDITOR
        void BuildMesh(string shapefilePath, Transform parentTransform, GeoReference geoRef)
        {

            Debug.Log(shapefilePath);
            var shp = new ShapeFile(shapefilePath);
            ShapeFileEnumerator sfEnum = shp.GetShapeFileEnumerator();

            _contours = new List<List<Vector2>>();

            while (sfEnum.MoveNext())
            {
                System.Collections.ObjectModel.ReadOnlyCollection<PointD[]> pointRecords = sfEnum.Current;


                int i = 0;
                foreach (PointD[] pts in pointRecords)
                {
                    if (pts.Length < 3) continue;
                    List<Vector2> contour = new List<Vector2>();

                    Debug.Log(string.Format("[NumPoints:{0}]", pts.Length));


                    for (int n = 0; n < pts.Length; ++n)
                    {
                        var refP = geoRef.ReferencePoint;
                        Vector2 p = new Vector2((float)(pts[n].X - refP.X), (float)(pts[n].Y - refP.Z));
                        contour.Add(p);
                    }
                    _contours.Add(contour);

                    i++;
                }

            }

            foreach (var contour in _contours)
            {
                var regulationArea = RegulationArea.Create(parentTransform);

                for (int i = 0; i < contour.Count; i++)
                {
                    var point = contour[i];
                    var pos = new Vector3(point.x, 0, point.y);
                    regulationArea.TryAddVertexOnGround(pos);
                }
                regulationArea.GenMesh();
            }

        }
#endif


    }
}
