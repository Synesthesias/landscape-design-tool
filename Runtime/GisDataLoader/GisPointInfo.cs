using System;
using System.Collections.Generic;
using UnityEngine;

namespace Landscape2.Runtime.GisDataLoader
{
    /// <summary>
    /// GISのポイント情報を管理するクラス
    /// </summary>
    [Serializable]
    public class GisPointInfo
    {
        public int ID { get; private set; }
        
        // 属性のindex
        public int AttributeIndex { get; private set; }

        // 施設名
        public string FacilityName { get; private set; }
        
        // 登録されたピンの表示名
        public string DisplayName { get; private set; }
        
        // 施設の位置
        public Vector3 FacilityPosition { get; private set; }
        
        // 表示状態かどうか
        public bool IsShow { get; private set; }
        public void SetShow(bool isShow) => IsShow = isShow;
        
        // ポイントの色
        public GisPointColor Color { get; private set; }

        public GisPointInfo(
            int index,
            int attributeIndex,
            string facilityName,
            string displayName,
            Vector3 facilityPosition,
            bool isShow)
        {
            ID = index;
            AttributeIndex = attributeIndex;
            FacilityName = facilityName;
            DisplayName = displayName;
            FacilityPosition = facilityPosition;
            IsShow = isShow;
            
            // すべての色を列挙
            var allColors = (GisPointColor[])System.Enum.GetValues(typeof(GisPointColor));

            // IDを使って色を決定
            Color = allColors[attributeIndex % allColors.Length];
        }
    }
    
    [Serializable]
    public enum GisPointColor
    {
        Red,
        Green,
        Blue,
        Yellow,
    }
    
    public static class GisPointColorExtension
    {
        public static Color ToColor(this GisPointColor color)
        {
            switch (color)
            {
                case GisPointColor.Red:
                    return Color.red;
                case GisPointColor.Green:
                    return Color.green;
                case GisPointColor.Blue:
                    return Color.blue;
                case GisPointColor.Yellow:
                    return Color.yellow;
                default:
                    return Color.white;
            }
        }
    }
}