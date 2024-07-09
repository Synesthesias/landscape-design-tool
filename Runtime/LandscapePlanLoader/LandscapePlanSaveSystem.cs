using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Landscape2.Runtime.LandscapePlanLoader
{
    public struct PlanAreaSaveData
    {
        public int ID { get; set; }
        //public string name { get; set; }
        //public float limitHeight { get; set; }
        //public float lineOffset { get; set; }
        //public Color color { get; set; }
        //public float wallMaxHeight { get; set; }
        //public List<Vector3> pointData { get; set; }
    }
    public class LandscapePlanSaveSystem
    {
        private SaveSystem _saveSystem;
        private LandscapePlanSaveLoadHandler saveLoadHandler;

        public void InstantiateSaveSystem(SaveSystem saveSystem)
        {
            saveLoadHandler = new LandscapePlanSaveLoadHandler();
            // SaveSystem_Assetsの初期化
            _saveSystem = saveSystem;
            SetSaveMode();
            SetLoadMode();
        }

        public void SetSaveMode()
        {
            _saveSystem.SaveEvent += saveLoadHandler.SaveInfo;
        }

        public void SetLoadMode()
        {
            _saveSystem.LoadEvent += saveLoadHandler.LoadInfo;
        }
    }
}
