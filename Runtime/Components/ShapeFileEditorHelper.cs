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
        // public Material areaMat;
        // private Color _materialColor = new Color(0, 160f/255f, 233f/255f, 0.5f);
        private float _areaHeight = 10;
        private string _shapefileLoadPath;
        private string _generateGameObjName = "LoadedShapeFile";

        private List<List<Vector2>> _contours;

        // public List<GameObject> _groupRoot;
        private PLATEAUInstancedCityModel _cityModel;

#if UNITY_EDITOR
        // [CustomEditor(typeof(ShapeFileEditorHelper))]
        // public class ShapeFileEditor : Editor
        // {
        //
        //     public override void OnInspectorGUI()
        //     {
        //         ((ShapeFileEditorHelper)target).DrawGui();
        //     }
        // }

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

            // _materialColor = EditorGUILayout.ColorField("色", _materialColor);

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
            // ShapeFile shp = new ShapeFile(Application.dataPath + "/plugins/LandscapeDesignTool/ShapeFiles/" + Selection.activeGameObject.GetComponent<ShapeFileEditorHelper>().shapefileName);
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
                        //  Vertex vtx = new Vertex((float)pts[n].X, (float)pts[n].Y);
                        contour.Add(p);
                    }
                    _contours.Add(contour);

                    i++;
                }

            }

            // _groupRoot = new List<GameObject>();

            // var material = LDTTools.MakeMaterial(_materialColor);
            
            foreach (var contour in _contours)
            {
                var regulationArea = RegulationArea.Create(parentTransform);
                
                // 最後の頂点は飛ばします。
                // 最初と最後の頂点位置が同じになる多角形表現から、最後の頂点を除きます。
                for (int i = 0; i < contour.Count - 1; i++)
                {
                    var point = contour[i];
                    var pos = new Vector3(point.x, 0, point.y);
                    regulationArea.TryAddVertexOnGround(pos);
                }
                regulationArea.GenMesh();
                // regulationArea.GetComponent<MeshRenderer>().material.color = _materialColor;
            }

            // GenerateTriangle(parentTransform, material);
            // GenerateWall(material);

        }
#endif

        // void GenerateTriangle(Transform parentTransform, Material material)
        // {
        //     // Debug.Log("GenerateTriangle");
        //     RaycastHit hit;
        //
        //     foreach (List<Vector2> cont in _Contours)
        //     {
        //
        //         Polygon poly = new Polygon();
        //         poly.Add(cont);
        //         var triangleNetMesh = (TriangleNetMesh)poly.Triangulate();
        //
        //         GameObject go = new GameObject("Upper");
        //
        //         var mf = go.AddComponent<MeshFilter>();
        //         var mesh = triangleNetMesh.GenerateUnityMesh();
        //
        //         Vector3[] nv = new Vector3[mesh.vertices.Length];
        //
        //         for (int i = 0; i < mesh.vertices.Length; i++)
        //         {
        //             Vector3 ov = mesh.vertices[i];
        //             Vector3 tmpv = new Vector3(ov.x, 3000, ov.y);
        //             if (Physics.Raycast(tmpv, new Vector3(0, -1, 0), out hit, Mathf.Infinity))
        //             {
        //                 nv[i] = new Vector3(ov.x, hit.point.y + areaHeight, ov.y);
        //             }
        //             else
        //             {
        //
        //                 nv[i] = new Vector3(ov.x, 0, ov.y);
        //             }
        //         }
        //         mesh.vertices = nv;
        //
        //         Debug.Log(mesh.bounds.ToString());
        //
        //         var mr = go.AddComponent<MeshRenderer>();
        //         mr.sharedMaterial = material;
        //
        //         mf.mesh = mesh;
        //
        //         mesh.RecalculateBounds();
        //
        //         GameObject rootNode = new GameObject("ShapeItem");
        //         ShapeItem si = rootNode.AddComponent<ShapeItem>();
        //         si.material = mr.sharedMaterial;
        //         si.height = areaHeight;
        //         si.oldHeight = areaHeight;
        //
        //         rootNode.transform.parent = parentTransform.transform;
        //         _groupRoot.Add(rootNode);
        //         go.transform.parent = rootNode.transform;
        //     }
        //
        // }
        //
        // void GenerateWall(Material material)
        // {
        //
        //     RaycastHit hit;
        //
        //     int ng = 0;
        //     foreach (List<Vector2> cont in _Contours)
        //     {
        //         Vector3[] nv = new Vector3[cont.Count * 2];
        //         int[] triangles = new int[cont.Count * 2 * 3];
        //
        //         int i = 0;
        //         int j = 0;
        //         int n = 0;
        //
        //         foreach (Vector2 pt in cont)
        //         {
        //             Vector3 tmpv = new Vector3(pt.x, 3000, pt.y);
        //             if (Physics.Raycast(tmpv, new Vector3(0, -1, 0), out hit, Mathf.Infinity))
        //             {
        //                 nv[i++] = new Vector3(pt.x, hit.point.y, pt.y);
        //                 nv[i++] = new Vector3(pt.x, hit.point.y + areaHeight, pt.y);
        //             }
        //             else
        //             {
        //                 nv[i++] = new Vector3(pt.x, 0, pt.y);
        //                 nv[i++] = new Vector3(pt.x, areaHeight, pt.y);
        //             }
        //
        //
        //         }
        //         foreach (Vector2 pt in cont)
        //         {
        //             triangles[n++] = j;
        //             triangles[n++] = j + 1;
        //             triangles[n++] = j + 2;
        //             triangles[n++] = j + 1;
        //             triangles[n++] = j + 3;
        //             triangles[n++] = j + 2;
        //             if (j < cont.Count) j++;
        //         }
        //
        //         GameObject go = new GameObject("Side");
        //         var mf = go.AddComponent<MeshFilter>();
        //         var mesh = new Mesh();
        //         mf.mesh = mesh;
        //         mesh.vertices = nv;
        //         mesh.triangles = triangles;
        //         var mr = go.AddComponent<MeshRenderer>();
        //         mr.sharedMaterial = material;
        //
        //         GameObject rootNode = _groupRoot[ng];
        //         go.transform.parent = rootNode.transform;
        //
        //         ng++;
        //     }
        // }


    }
}
