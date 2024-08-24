using System.Collections.Generic;
using UnityEngine;
using SFB;
using PlateauToolkit.Maps;

namespace Landscape2.Runtime.LandscapePlanLoader
{
    /// <summary>
    /// GISデータの読み込みとメッシュの生成を管理するクラス
    /// </summary>
    public sealed class LandscapePlanLoadManager
    {
        List<GameObject> listOfGISObjects;
        List<List<Vector3>> listOfAreaPointDatas;
        Material wallMaterial;
        Material ceilingMaterial;

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
            // GISデータの読み込みとメッシュオブジェクトの生成
            using (ShapefileRenderManager m_ShapefileRenderManager = new ShapefileRenderManager(gisTargetFolderPath, 0 /*RenderMode:Mesh*/, 0, false, false, SupportedEncoding.ShiftJIS, null))
            {
                if (m_ShapefileRenderManager.Read(0, out listOfGISObjects, out listOfAreaPointDatas))
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
                List<Vector3> areaPointData = listOfAreaPointDatas[i];

                // GISデータのプロパティを取得
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


                // メッシュを変形
                if (!landscapePlanMeshModifier.TryModifyMeshToTargetHeight(mesh, 0, gisObject.transform.position))
                {
                    Debug.LogError($"{gisObject.name} is out of range of the loaded map");
                    return;
                }

                // 新規のAreaPropertyを生成し初期化
                float initLimitHeight = float.Parse(GetPropertyValueOf("HEIGHT", dbf)); // 区画の制限高さを取得
                AreaProperty areaProperty = new AreaProperty(
                    int.Parse(GetPropertyValueOf("ID", dbf)),
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


                // 上面Meshのマテリアルを設定
                areaProperty.ceilingMaterial.color = areaProperty.color;
                gisObjMeshRenderer.material = areaProperty.ceilingMaterial;

                // 区画のメッシュから下向きに壁を生成
                GameObject wall = wallGenerator.GenerateWall(mesh, areaProperty.wallMaxHeight, Vector3.down, areaProperty.wallMaterial);
                wall.transform.SetParent(gisObject.transform);
                wall.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                areaProperty.wallMaterial.color = areaProperty.color;
                areaProperty.wallMaterial.SetFloat("_DisplayRate", areaProperty.limitHeight / areaProperty.wallMaxHeight);
                areaProperty.wallMaterial.SetFloat("_LineCount", areaProperty.limitHeight / areaProperty.lineOffset);
                areaProperty.SetLocalPosition(new Vector3(
                    areaProperty.transform.localPosition.x,
                    areaProperty.limitHeight,
                    areaProperty.transform.localPosition.z
                    ));
                wall.name = $"AreaWall_{areaProperty.ID}";
                gisObject.name = $"Area_{areaProperty.ID}";

                // 区画データリストにAreaPropertyを追加登録
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
            // 景観区画の頂点データを取得
            listOfAreaPointDatas = new List<List<Vector3>>();
            foreach (PlanAreaSaveData saveData in saveDatas)
            {
                listOfAreaPointDatas.Add(saveData.pointData);
                Debug.Log($"Area {saveData.id} loaded");
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
                List<Vector3> areaPointData = listOfAreaPointDatas[i];
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
                float initLimitHeight = saveData.limitHeight;
                AreaProperty areaProperty = new AreaProperty(
                    saveData.id,
                    saveData.name,
                    initLimitHeight,
                    saveData.lineOffset,
                    saveData.color,
                    new Material(wallMaterial),
                    new Material(ceilingMaterial),
                    Mathf.Max(300, initLimitHeight + 50),
                    gisObject.transform.position + mesh.bounds.center,
                    gisObject.transform,
                    areaPointData
                    );

                // 上面Meshのマテリアルを設定
                areaProperty.ceilingMaterial.color = areaProperty.color;
                gisObjMeshRenderer.material = areaProperty.ceilingMaterial;

                // 区画のメッシュから下向きに壁を生成
                GameObject wall = wallGenerator.GenerateWall(mesh, areaProperty.wallMaxHeight, Vector3.down, areaProperty.wallMaterial);
                wall.transform.SetParent(gisObject.transform);
                wall.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                areaProperty.wallMaterial.color = areaProperty.color;
                areaProperty.wallMaterial.SetFloat("_DisplayRate", areaProperty.limitHeight / areaProperty.wallMaxHeight);
                areaProperty.wallMaterial.SetFloat("_LineCount", areaProperty.limitHeight / areaProperty.lineOffset);
                wall.name = $"AreaWall_{areaProperty.ID}";
                gisObject.name = $"Area_{areaProperty.ID}";
                areaProperty.SetLocalPosition(new Vector3(
                    areaProperty.transform.localPosition.x,
                    areaProperty.limitHeight,
                    areaProperty.transform.localPosition.z
                    ));

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
        /// <param name="colorString"> "r,g,b,a"のフォーマットで記述された色のstringデータ </param>
        Color DbfStringToColor(string colorString)
        {
            string[] colorValues = colorString.Split(',');

            if (colorValues.Length == 4 &&
                float.TryParse(colorValues[0], out float r) &&
                float.TryParse(colorValues[1], out float g) &&
                float.TryParse(colorValues[2], out float b) &&
                float.TryParse(colorValues[3], out float a))
            {
                return new Color(r, g, b, a);
            }

            Debug.LogError("Invalid color string format.");
            return Color.clear;
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

            return null;
        }
    }
}

