using PLATEAU.CityInfo;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Landscape2.Runtime.BuildingEditor
{
    /// <summary>
    /// 建物編集のセーブデータ項目
    /// </summary>
    [Serializable]
    public struct BuildingSaveData
    {
        [SerializeField] private string gmlID;
        [SerializeField] private List<Color> colorData;
        [SerializeField] private List<float> smoothnessData;

        public string GmlID { get => gmlID; }
        public List<Color> ColorData { get => colorData; }
        public List<float> SmoothnessData { get => smoothnessData; }

        public BuildingSaveData(string gmlID, List<Color> colorData, List<float> smoothnessData)
        {
            this.gmlID = gmlID;
            this.colorData = colorData;
            this.smoothnessData = smoothnessData;
        }
    }
}
