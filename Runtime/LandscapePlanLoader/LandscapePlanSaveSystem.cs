using System.Collections.Generic;
using UnityEngine;
using ToolBox.Serialization;

namespace Landscape2.Runtime.LandscapePlanLoader
{
    /// <summary>
    /// 景観区画の保存と読み込み時の処理を管理するクラス
    /// </summary>
    public static class LandscapePlanSaveSystem
    {
        static public void SetEvent(SaveSystem saveSystem)
        {
            saveSystem.SaveEvent += SaveInfo;
            saveSystem.LoadEvent += LoadInfo;
        }

        /// <summary>
        /// 景観計画のデータセーブの処理
        /// </summary>
        static void SaveInfo()
        {
            // セーブデータ用クラスに現在の区画データをコピー
            List<PlanAreaSaveData> planAreaSaveDatas = new List<PlanAreaSaveData>();
            int areaDataCount = AreasDataComponent.GetPropertyCount();
            for (int i = 0; i < areaDataCount; i++)
            {
                AreaProperty areaProperty = AreasDataComponent.GetProperty(i);
                PlanAreaSaveData saveData = new PlanAreaSaveData(
                    areaProperty.ID,
                    areaProperty.Name,
                    areaProperty.LimitHeight,
                    areaProperty.LineOffset,
                    areaProperty.Color,
                    areaProperty.WallMaxHeight,
                    areaProperty.PointData,
                    areaProperty.IsApplyBuildingHeight
                    );

                planAreaSaveDatas.Add(saveData);
            }

            // データを保存
            DataSerializer.Save("PlanAreas", planAreaSaveDatas);
        }

        /// <summary>
        /// 景観計画のデータロードと生成の処理
        /// </summary>
        static void LoadInfo()
        {
            AreasDataComponent.ClearAllProperties();

            // 景観区画のセーブデータをロード
            List<PlanAreaSaveData> loadedPlanAreaDatas = DataSerializer.Load<List<PlanAreaSaveData>>("PlanAreas");

            if (loadedPlanAreaDatas != null)
            {
                // ロードした頂点座標データからMeshを生成
                LandscapePlanLoadManager landscapePlanLoadManager = new LandscapePlanLoadManager();
                landscapePlanLoadManager.LoadFromSaveData(loadedPlanAreaDatas);
            }
            else
            {
                Debug.LogError("No saved project data found.");
            }
        }
    }
}
