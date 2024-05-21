using System.Collections.Generic;
using UnityEngine;
using SFB;

namespace LandscapeDesignTool2.Runtime.LandscapePlanLoader
{
    public sealed class LandscapePlanLoadManager
    {
        ShapefileRenderManager m_ShapefileRenderManager;
        SupportedEncoding m_SupportedEncoding = SupportedEncoding.ShiftJIS;

        List<GameObject> m_ListOfGISObjects = new List<GameObject>();


        /// <summary>
        /// Shapefileの読み込み処理を実行する
        /// </summary>
        /// <param name="gisTargetFolderPath">ShapeFileが格納されているフォルダパス</param>
        public void LoadShapefile(string gisTargetFolderPath)
        {
            using (m_ShapefileRenderManager = new ShapefileRenderManager(gisTargetFolderPath, 0 /*RenderMode:Mesh*/, 0, false, false, m_SupportedEncoding, null))
            {
                if (m_ShapefileRenderManager.Read(0, ref m_ListOfGISObjects))
                {
                    Debug.Log("GISデータ読み込み完了");
                }
                else
                {
                    Debug.LogError(
                        "GISデータ読み込み失敗\n" +
                        "フォルダに有効なSHPとDBFファイルが含まれていることを再確認してください。"
                        );
                }
            }

            LandscapePlanMeshModifier landscapePlanMeshModifier = new LandscapePlanMeshModifier();
            WallGenerator wallGenerator = new WallGenerator();
            foreach (GameObject gisObject in m_ListOfGISObjects)
            {
                landscapePlanMeshModifier.SetGISObjToLimitHeight(gisObject);
                wallGenerator.GenerateWallFromMeshObj(gisObject);
            }
        }

        public string BrowseFolder()
        {
            var paths = StandaloneFileBrowser.OpenFolderPanel("Open Folder", "", false);
            
            if (paths.Length > 0) return paths[0];
            return null;
        }

        
    }

}

