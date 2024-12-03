using System.Collections.Generic;
using System;
using UnityEngine;

namespace Landscape2.Runtime.BuildingEditor
{
    /// <summary>
    /// 読み込んだ建物編集データを管理するクラス
    /// </summary>
    public sealed class BuildingsDataComponent
    {
        // 建物編集データリスト
        private static readonly List<BuildingProperty> properties = new List<BuildingProperty>();
        // 建物編集がロードされた際のイベント
        public static event Action BuildingDataLoaded = delegate { };

        /// <summary>
        /// 建物編集データを新規に追加するメソッド
        /// </summary>
        public static void AddNewProperty(BuildingProperty newProperty)
        {
            properties.Add(newProperty);
        }

        /// <summary>
        /// セーブデータを基に建物編集データを新規に追加するメソッド
        /// </summary>
        public static void AddAllNewProperty(List<BuildingProperty> newProperties)
        {
            foreach (var newProperty in newProperties)
            {
                AddNewProperty(newProperty);
            }
            // 建物編集データがロードされたことを通知
            BuildingDataLoaded();
        }

        /// <summary>
        /// 全ての建物編集データを削除するメソッド
        /// </summary>
        public static void ClearAllProperties()
        {
            properties.Clear();
        }

        /// <summary>
        /// 対象の建物編集データを取得するメソッド
        /// </summary>
        public static BuildingProperty GetProperty(int index)
        {
            if (index < 0 || index >= properties.Count) return null;
            return properties[index];
        }

        /// <summary>
        /// 建物編集データリストの長さを取得するメソッド
        /// </summary>
        public static int GetPropertyCount()
        {
            return properties.Count;
        }

        /// <summary>
        /// gmlIDが一致する建物編集データが存在するかを調べるメソッド
        /// </summary>
        public static bool IsContainsProperty(string gmlID)
        {
            return properties.Exists(x => x.GmlID == gmlID);
        }

        /// <summary>
        /// 建物編集の変更内容を適用
        /// </summary>
        public static bool TryApplyBuildingEdit(string gmlID, List<Color> colors, List<float> smoothness)
        {
            var index = properties.FindIndex(x => x.GmlID == gmlID);
            if (index < 0 || index >= properties.Count) return false;
            
            var property = properties[index];

            // 建物の色とSmoothnessを保存
            property.ColorData = colors;
            property.SmoothnessData = smoothness;
            return true;
        }
    }

    /// <summary>
    /// 建物編集データのプロパティを保持するクラス
    /// </summary>
    public class BuildingProperty
    {
        public string GmlID { get; private set; }
        public List<Color> ColorData { get; set; }
        public List<float> SmoothnessData { get; set; }

        public BuildingProperty(string gmlID, List<Color> colorData, List<float> smoothnessData)
        {
            GmlID = gmlID;
            ColorData = colorData;
            SmoothnessData = smoothnessData;
        }
    }
}
