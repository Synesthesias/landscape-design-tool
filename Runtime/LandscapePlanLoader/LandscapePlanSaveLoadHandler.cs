using System.Collections.Generic;
using UnityEngine;
using ToolBox.Serialization;

namespace Landscape2.Runtime.LandscapePlanLoader
{
    /// <summary>
    /// The class that handles the saving and loading of the landscape plan.
    /// </summary>
    public sealed class LandscapePlanSaveLoadHandler
    {
        public void SaveInfo()
        {
            // Copy current area data to save data
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
            
            // Save data
            DataSerializer.Save("PlanAreas", planAreaSaveDatas);
        }

        public void LoadInfo()
        {
            // Load saved data
            List<PlanAreaSaveData> LoadedPlanAreaDatas = DataSerializer.Load<List<PlanAreaSaveData>>("PlanAreas");

            if (LoadedPlanAreaDatas != null)
            {
                // Create object from loaded point data
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
