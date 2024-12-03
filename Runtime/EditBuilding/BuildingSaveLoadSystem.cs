using PLATEAU.CityInfo;
using System.Collections.Generic;
using System.Linq;
using ToolBox.Serialization;
using UnityEngine;

namespace Landscape2.Runtime.BuildingEditor
{
    /// <summary>
    /// 建物編集の保存と読み込み時の処理を管理するクラス
    /// </summary>   
    public class BuildingSaveLoadSystem
    {
        public void SetEvent(SaveSystem saveSystem)
        {
            saveSystem.SaveEvent += SaveInfo;
            saveSystem.LoadEvent += LoadInfo;
        }

        /// <summary>
        /// 景観計画のデータセーブの処理
        /// </summary>
        public void SaveInfo()
        {
            // セーブデータ用クラスに現在の建物編集データをコピー
            List<BuildingSaveData> buildingSaveDatas = new List<BuildingSaveData>();
            int buildingDataCount = BuildingsDataComponent.GetPropertyCount();
            for (int i = 0; i < buildingDataCount; i++)
            {
                BuildingProperty buildingProperty = BuildingsDataComponent.GetProperty(i);
                BuildingSaveData saveData = new BuildingSaveData(
                    buildingProperty.GmlID,
                    buildingProperty.ColorData,
                    buildingProperty.SmoothnessData
                    );

                buildingSaveDatas.Add(saveData);
            }

            // データを保存
            DataSerializer.Save("Buildings", buildingSaveDatas);
        }

        /// <summary>
        /// 景観計画のデータロードの処理
        /// </summary>
        public void LoadInfo()
        {
            BuildingsDataComponent.ClearAllProperties();

            // 建物編集のセーブデータをロード
            List<BuildingSaveData> loadedBuildingDatas = DataSerializer.Load<List<BuildingSaveData>>("Buildings");

            if (loadedBuildingDatas != null)
            {
                BuildingsDataComponent.ClearAllProperties();
                BuildingsDataComponent.AddAllNewProperty(loadedBuildingDatas.Select(data => new BuildingProperty(data.GmlID, data.ColorData, data.SmoothnessData)).ToList());
            }
            else
            {
                Debug.LogError("No saved project data found.");
            }
        }
    }
}
