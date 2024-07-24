using System.Collections.Generic;
using UnityEngine;
using ToolBox.Serialization;

namespace Landscape2.Runtime.LandscapePlanLoader
{
    /// <summary>
    /// 景観区画の保存と読み込み時の処理を管理するクラス
    /// </summary>
    public sealed class LandscapePlanSaveLoadHandler
    {
        public void SaveInfo()
        {
            // セーブデータ用クラスに現在の区画データをコピー
            List<PlanAreaSaveData> planAreaSaveDatas = new List<PlanAreaSaveData>();
            int areaDataCount = AreasDataComponent.GetPropertyCount();
            for (int i = 0; i < areaDataCount; i++)
            {
                AreaProperty areaProperty = AreasDataComponent.GetProperty(i);
                PlanAreaSaveData saveData = new PlanAreaSaveData(
                    areaProperty.ID,
                    areaProperty.name,
                    areaProperty.limitHeight,
                    areaProperty.lineOffset,
                    areaProperty.color,
                    areaProperty.wallMaxHeight,
                    areaProperty.pointData
                    );

                planAreaSaveDatas.Add(saveData);
            }
            
            // データを保存
            DataSerializer.Save("PlanAreas", planAreaSaveDatas);
        }

        public void LoadInfo()
        {
            // 景観区画のセーブデータをロード
            List<PlanAreaSaveData> LoadedPlanAreaDatas = DataSerializer.Load<List<PlanAreaSaveData>>("PlanAreas");

            if (LoadedPlanAreaDatas != null)
            {
                // ロードした頂点座標データからMeshを生成
                LandscapePlanLoadManager landscapePlanLoadManager = new LandscapePlanLoadManager();
                landscapePlanLoadManager.LoadFromSaveData(LoadedPlanAreaDatas);
            }
            else
            {
                Debug.LogError("No saved project data found.");
            }
        }
    }
}
