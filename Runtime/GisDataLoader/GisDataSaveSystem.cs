using System.Collections.Generic;
using ToolBox.Serialization;

namespace Landscape2.Runtime.GisDataLoader
{
    /// <summary>
    /// GISポイントのセーブシステム
    /// </summary>
    public class GisDataSaveSystem
    {
        private readonly string saveKey = "GisPoint";
        
        private GisPointInfos gisPointInfos;
        
        public GisDataSaveSystem(SaveSystem saveSystem, GisPointInfos gisPointInfos)
        {
            saveSystem.SaveEvent += Save;
            saveSystem.LoadEvent += Load; 
            
            this.gisPointInfos = gisPointInfos;
        }

        private void Save()
        {
            // データを保存
            DataSerializer.Save(saveKey, gisPointInfos.Points);
        }

        private void Load()
        {
            // データをロード
            var loadedData = DataSerializer.Load<List<GisPointInfo>>(saveKey);
            gisPointInfos.SetPoints(loadedData);
        }
    }
}