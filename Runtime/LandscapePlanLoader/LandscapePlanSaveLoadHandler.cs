using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ToolBox.Serialization;

namespace Landscape2.Runtime.LandscapePlanLoader
{
    public class LandscapePlanSaveLoadHandler
    {
        public void SaveInfo()
        {
            Debug.Log("SaveInfo");
            List<PlanAreaSaveData> planAreaSaveDatas = new List<PlanAreaSaveData>();

            int areaDataCount = AreasDataComponent.GetPropertyCount();
            for (int i = 0; i < areaDataCount; i++)
            {
                PlanAreaSaveData saveData = new PlanAreaSaveData();
                AreaProperty areaProperty = AreasDataComponent.GetProperty(i);

                saveData.ID = areaProperty.ID;
                //saveData.name = areaProperty.name;
                //saveData.limitHeight = areaProperty.limitHeight;
                //saveData.lineOffset = areaProperty.lineOffset;
                //saveData.color = areaProperty.color;
                //saveData.wallMaxHeight = areaProperty.wallMaxHeight;
                //saveData.pointData = areaProperty.pointData;

                planAreaSaveDatas.Add(saveData);
            }
            Debug.Log(planAreaSaveDatas[3].ID);
            DataSerializer.Save("PlanAreas", planAreaSaveDatas);
        }

        public void LoadInfo()
        {
            List<PlanAreaSaveData> LoadedPlanAreaDatas = DataSerializer.Load<List<PlanAreaSaveData>>("PlanAreas");
            if (LoadedPlanAreaDatas != null)
            {
                Debug.Log(LoadedPlanAreaDatas[3].ID);
                //LandscapePlanLoadManager landscapePlanLoadManager = new LandscapePlanLoadManager();
                //landscapePlanLoadManager.LoadFromSaveData(LoadedPlanAreaDatas);
            }
            else
            {
                Debug.LogError("No saved project data found.");
            }
        }
    }
}
