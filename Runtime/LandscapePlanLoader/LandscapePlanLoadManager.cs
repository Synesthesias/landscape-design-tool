using System.Collections.Generic;
using UnityEngine;
using SFB;
using PlateauToolkit.Maps;
using UnityEngine.Rendering.HighDefinition;

namespace Landscape2.Runtime.LandscapePlanLoader
{
    /// <summary>
    /// GISデータの読み込みとメッシュの生成を管理するクラス
    /// </summary>
    public sealed class LandscapePlanLoadManager
    {
        private readonly Material wallMaterial;
        private readonly Material ceilingMaterial;

        public LandscapePlanLoadManager()
        {
            wallMaterial = Resources.Load<Material>("Materials/PlanAreaWallMaterial");
            ceilingMaterial = Resources.Load<Material>("Materials/PlanAreaCeilingMaterial");
            ceilingMaterial.SetFloat("_Alpha", 0.2f);
            ceilingMaterial.SetFloat("_Intensity", 3f);
        }

        /// <summary>
        /// 指定されたフォルダパスからGISデータを読み込み、メッシュオブジェクトを生成するメソッド
        /// </summary>
        /// <param name="gisTargetFolderPath"> .shp、.dbfファイルを含むフォルダのパス </param>
        public void LoadShapefile(string gisTargetFolderPath)
        {
            List<GameObject> listOfGISObjects;
            List<List<List<Vector3>>> listOfAreaPointDatas;

            // GISデータの読み込みとメッシュオブジェクトの生成
            using (ShapefileRenderManager shapefileRenderManager = new ShapefileRenderManager(gisTargetFolderPath, 0 /*RenderMode:Mesh*/, 0, false, false, SupportedEncoding.ShiftJIS, null))
            {
                if (shapefileRenderManager.Read(0, out listOfGISObjects, out listOfAreaPointDatas))
                {
                    Debug.Log("Loading GIS data completed");
                }
                else
                {
                    Debug.LogError("Loading GIS data failed.");
                    return;
                }
            }

            if (listOfGISObjects == null || listOfGISObjects.Count == 0)
            {
                Debug.LogError("No GIS data was included");
                return;
            }

            LandscapePlanMeshModifier landscapePlanMeshModifier = new LandscapePlanMeshModifier();
            WallGenerator wallGenerator = new WallGenerator();

            // 区画の制限高さに合わせてメッシュデータを変形し、周囲に壁を生成する
            for (int i = 0; i < listOfGISObjects.Count; i++)
            {
                GameObject gisObject = listOfGISObjects[i];
                List<List<Vector3>> areaPointData = listOfAreaPointDatas[i];

                //GISデータのプロパティを取得
               DbfComponent dbf = gisObject.GetComponent<DbfComponent>();
                if (dbf == null)
                {
                    Debug.LogError("GisObject have no DbfComponent");
                    return;
                }

                MeshFilter gisObjMeshFilter = gisObject.GetComponent<MeshFilter>();
                MeshRenderer gisObjMeshRenderer = gisObject.GetComponent<MeshRenderer>();
                if (gisObjMeshFilter == null)
                {
                    Debug.LogError($"{gisObject.name} have no MeshFilter Component");
                    return;
                }
                if (gisObjMeshRenderer == null)
                {
                    Debug.LogError($"{gisObject.name} have no MeshRenderer Component");
                    return;
                }

                Mesh mesh = gisObjMeshFilter.sharedMesh;
                if (mesh == null)
                {
                    Debug.LogError($"Mesh in MeshFilter of {gisObject.name} is null");
                    return;
                }


                //メッシュを変形
                if (!landscapePlanMeshModifier.TryModifyMeshToTargetHeight(mesh, 0, gisObject.transform.position))
                {
                    Debug.LogError($"{gisObject.name} is out of range of the loaded map");
                    return;
                }

                //新規のAreaPropertyを生成し初期化
                float initLimitHeight = float.TryParse(GetPropertyValueOf("HEIGHT", dbf), out float heightValue) ? heightValue : 0; // 区画の制限高さを取得
                int id = int.TryParse(GetPropertyValueOf("ID", dbf), out int idValue) ? idValue : 0;
                AreaProperty areaProperty = new AreaProperty(
                    id,
                    GetPropertyValueOf("AREANAME", dbf),
                    initLimitHeight,
                    10,
                    DbfStringToColor(GetPropertyValueOf("COLOR", dbf)),
                    new Material(wallMaterial),
                    new Material(ceilingMaterial),
                    Mathf.Max(300, initLimitHeight + 50),
                    gisObject.transform.position + mesh.bounds.center,
                    gisObject.transform,
                    areaPointData
                    );


                //上面Meshのマテリアルを設定
                areaProperty.CeilingMaterial.color = areaProperty.Color;
                gisObjMeshRenderer.material = areaProperty.CeilingMaterial;

                //区画のメッシュから下向きに壁を生成
               GameObject[] walls = wallGenerator.GenerateWall(mesh, areaProperty.WallMaxHeight, Vector3.down, areaProperty.WallMaterial);
                for(int j = 0; j < walls.Length; j++)
                {
                    walls[j].transform.SetParent(gisObject.transform);
                    walls[j].transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                    walls[j].name = $"AreaWall_{areaProperty.ID}_{j}";
                }
                areaProperty.WallMaterial.color = areaProperty.Color;
                areaProperty.WallMaterial.SetFloat("_DisplayRate", areaProperty.LimitHeight / areaProperty.WallMaxHeight);
                areaProperty.WallMaterial.SetFloat("_LineCount", areaProperty.LimitHeight / areaProperty.LineOffset);
                areaProperty.SetLocalPosition(new Vector3(
                    areaProperty.Transform.localPosition.x,
                    areaProperty.LimitHeight,
                    areaProperty.Transform.localPosition.z
                    ));
                gisObject.name = $"Area_{areaProperty.ID}";

                //区画データリストにAreaPropertyを追加登録
                AreasDataComponent.AddNewProperty(areaProperty);
            }
            Debug.Log("Mesh modification and wall generation completed");
        }

        /// <summary>
        /// セーブデータからGISデータを読み込み、メッシュオブジェクトを生成するメソッド
        /// </summary>
        /// <param name="saveDatas"> ロードした区画セーブデータ </param>
        public void LoadFromSaveData(List<PlanAreaSaveData> saveDatas)
        {
            List<GameObject> listOfGISObjects;
            List<List<List<Vector3>>> listOfAreaPointDatas;

            // 景観区画の頂点データを取得
            listOfAreaPointDatas = new List<List<List<Vector3>>>();
            foreach (PlanAreaSaveData saveData in saveDatas)
            {
                listOfAreaPointDatas.Add(saveData.PointData);
            }

            // メッシュオブジェクトの生成
            PointDataRenderManager m_PointDataRenderManager = new PointDataRenderManager();
            if (m_PointDataRenderManager.DrawShapes("LoadedPlanArea", listOfAreaPointDatas, out listOfGISObjects))
            {
                Debug.Log("Loading GIS data completed");
            }
            else
            {
                Debug.LogError("Loading GIS data failed.");
                return;
            }

            if (listOfGISObjects == null || listOfGISObjects.Count == 0)
            {
                Debug.LogError("No GIS data was saved");
                return;
            }

            LandscapePlanMeshModifier landscapePlanMeshModifier = new LandscapePlanMeshModifier();
            WallGenerator wallGenerator = new WallGenerator();

            // 区画の制限高さに合わせてメッシュデータを変形し、周囲に壁を生成する
            for (int i = 0; i < listOfGISObjects.Count; i++)
            {
                GameObject gisObject = listOfGISObjects[i];
                List<List<Vector3>> areaPointData = listOfAreaPointDatas[i];
                PlanAreaSaveData saveData = saveDatas[i];

                MeshFilter gisObjMeshFilter = gisObject.GetComponent<MeshFilter>();
                MeshRenderer gisObjMeshRenderer = gisObject.GetComponent<MeshRenderer>();
                if (gisObjMeshFilter == null)
                {
                    Debug.LogError($"{gisObject.name} have no MeshFilter Component");
                    return;
                }
                if (gisObjMeshRenderer == null)
                {
                    Debug.LogError($"{gisObject.name} have no MeshRenderer Component");
                    return;
                }

                Mesh mesh = gisObjMeshFilter.sharedMesh;
                if (mesh == null)
                {
                    Debug.LogError($"Mesh in MeshFilter of {gisObject.name} is null");
                    return;
                }

                // Meshを変形
                if (!landscapePlanMeshModifier.TryModifyMeshToTargetHeight(mesh, 0, gisObject.transform.position))
                {
                    Debug.LogError($"{gisObject.name} is out of range of the loaded map");
                    return;
                }
                // 新規のAreaPropertyを生成し初期化
                float initLimitHeight = saveData.LimitHeight;
                AreaProperty areaProperty = new AreaProperty(
                    saveData.Id,
                    saveData.Name,
                    initLimitHeight,
                    saveData.LineOffset,
                    saveData.Color,
                    new Material(wallMaterial),
                    new Material(ceilingMaterial),
                    Mathf.Max(300, initLimitHeight + 50),
                    gisObject.transform.position + mesh.bounds.center,
                    gisObject.transform,
                    areaPointData
                    );

                // 上面Meshのマテリアルを設定
                areaProperty.CeilingMaterial.color = areaProperty.Color;
                gisObjMeshRenderer.material = areaProperty.CeilingMaterial;

                // 区画のメッシュから下向きに壁を生成
                GameObject[] walls = wallGenerator.GenerateWall(mesh, areaProperty.WallMaxHeight, Vector3.down, areaProperty.WallMaterial);
                for (int j = 0; j < walls.Length; j++)
                {
                    walls[j].transform.SetParent(gisObject.transform);
                    walls[j].transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                    walls[j].name = $"AreaWall_{areaProperty.ID}_{j}";
                }
                areaProperty.WallMaterial.color = areaProperty.Color;
                areaProperty.WallMaterial.SetFloat("_DisplayRate", areaProperty.LimitHeight / areaProperty.WallMaxHeight);
                areaProperty.WallMaterial.SetFloat("_LineCount", areaProperty.LimitHeight / areaProperty.LineOffset);
                areaProperty.SetLocalPosition(new Vector3(
                    areaProperty.Transform.localPosition.x,
                    areaProperty.LimitHeight,
                    areaProperty.Transform.localPosition.z
                    ));
                gisObject.name = $"Area_{areaProperty.ID}";

                // 区画データリストにAreaPropertyを追加登録
                AreasDataComponent.AddNewProperty(areaProperty);

            }
            Debug.Log("Mesh modification and wall generation completed");
        }


        /// <summary>
        /// フォルダ選択用のダイアログを開き、パスを取得するメソッド
        /// </summary>
        /// <returns>フォルダパス</returns>
        public string BrowseFolder()
        {
            var paths = StandaloneFileBrowser.OpenFolderPanel("Open Folder", "", false);

            if (paths.Length > 0) return paths[0];
            return null;
        }

        /// <summary>
        /// 色のstirngデータをColorに変換するメソッド
        /// </summary>
        /// <param name="colorString"> "r,g,b"のフォーマットで記述された色のstringデータ </param>
        Color DbfStringToColor(string colorString)
        {
            string[] colorValues = colorString.Split(',');

            if (colorValues.Length >= 3 &&
                float.TryParse(colorValues[0], out float r) &&
                float.TryParse(colorValues[1], out float g) &&
                float.TryParse(colorValues[2], out float b))
            {
                return new Color(r, g, b, 1);
            }

            Debug.LogError("Invalid color string format. Color data must be in the form 'R,G,B' with values ranging from 0 to 1. For example, '0.2,0.2,0.2'.");
            return Color.white;
        }

        /// <summary>
        /// 区画オブジェクトのDbfComponentから指定された名前の属性値を取得するメソッド
        /// </summary>
        /// <param name="propertyName">取得したいdbfの属性名</param>
        /// <param name="dbf">取得対象の区画オブジェクトにアタッチされているDbfComponentクラス</param>
        string GetPropertyValueOf(string propertyName, DbfComponent dbf)
        {
            int index = dbf.PropertyNames.IndexOf(propertyName);
            if (index != -1) return dbf.Properties[index];

            Debug.LogError($"Attribute name '{propertyName}' was not found.");
            return "";
        }
    }
}

