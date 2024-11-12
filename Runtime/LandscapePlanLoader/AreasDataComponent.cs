using System.Collections.Generic;
using System;
using UnityEngine;

namespace Landscape2.Runtime.LandscapePlanLoader
{
    /// <summary>
    /// 読み込んだ区画データを管理するクラス
    /// </summary>
    public static class AreasDataComponent
    {
        // 区画データリスト
        private static readonly List<AreaProperty> properties = new List<AreaProperty>();

        // 変更適用前の情報を保持する区画データリスト
        private static readonly List<AreaPropertySnapshot> propertiesSnapshot = new List<AreaPropertySnapshot>();

        // 区画データ数に変更があった際のイベント
        public static event Action AreaCountChangedEvent = delegate { };

        /// <summary>
        /// 区画データを新規に追加するメソッド
        /// </summary>
        public static void AddNewProperty(AreaProperty newProperty)
        {
            AreaPropertySnapshot newPropertyOrigin = new AreaPropertySnapshot(
                newProperty.ID,
                newProperty.Name,
                newProperty.LimitHeight,
                newProperty.LineOffset,
                newProperty.Color,
                newProperty.ReferencePosition,
                newProperty.Transform.localPosition
                );

            properties.Add(newProperty);
            propertiesSnapshot.Add(newPropertyOrigin);

            AreaCountChangedEvent();
        }

        /// <summary>
        /// 区画データを読み込み時の値にリセットするメソッド
        /// </summary>
        /// <returns>リセットが成功した場合はtrue、指定したindexがリストの範囲外の場合はfalse</returns>
        public static bool TryResetProperty(int index)
        {
            if (index < 0 || index >= properties.Count) return false;

            properties[index].ID = propertiesSnapshot[index].ID;
            properties[index].Name = propertiesSnapshot[index].Name;
            properties[index].LimitHeight = propertiesSnapshot[index].LimitHeight;
            properties[index].LineOffset = propertiesSnapshot[index].LineOffset;
            properties[index].Color = propertiesSnapshot[index].Color;
            properties[index].SetLocalPosition(propertiesSnapshot[index].Position);

            properties[index].CeilingMaterial.color = properties[index].Color;
            properties[index].WallMaterial.color = properties[index].Color;
            properties[index].WallMaterial.SetFloat("_DisplayRate", properties[index].LimitHeight / properties[index].WallMaxHeight);
            properties[index].WallMaterial.SetFloat("_LineCount", properties[index].LimitHeight / properties[index].LineOffset);

            return true;
        }

        /// <summary>
        /// 全ての区画データを削除するメソッド
        /// </summary>
        public static void ClearAllProperties()
        {
            for (int i = 0; i < properties.Count; i++)
            {
                GameObject.Destroy(properties[i].Transform.gameObject);
            }
            properties.Clear();
            propertiesSnapshot.Clear();

            AreaCountChangedEvent();
        }

        /// <summary>
        /// 区画の編集前データを更新するメソッド
        /// </summary>
        /// <returns>リセットが成功した場合はtrue、指定したindexがリストの範囲外の場合はfalse</returns>
        public static bool TryUpdateSnapshotProperty(int index)
        {
            if (index < 0 || index >= properties.Count) return false;

            propertiesSnapshot[index].SetValues(
                properties[index].ID,
                properties[index].Name,
                properties[index].LimitHeight,
                properties[index].LineOffset,
                properties[index].Color,
                properties[index].ReferencePosition,
                properties[index].Transform.localPosition
                );
            return true;
        }

        /// <summary>
        /// 対象の区画データを取得するメソッド
        /// </summary>
        public static AreaProperty GetProperty(int index)
        {
            if (index < 0 || index >= properties.Count) return null;
            return properties[index];
        }

        /// <summary>
        /// 対象区画の読み込み時のデータを取得するメソッド
        /// </summary>
        public static AreaPropertySnapshot GetSnapshotProperty(int index)
        {
            if (index < 0 || index >= propertiesSnapshot.Count) return null;
            return propertiesSnapshot[index];
        }

        /// <summary>
        /// 対象の区画データを削除するメソッド
        /// </summary>
        /// <returns>削除が成功した場合はtrue、指定したindexがリスト範囲外の場合はfalse</returns>
        public static bool TryRemoveProperty(int index)
        {
            if (index < 0 || index >= properties.Count) return false;

            // 建物高さをリセット
            properties[index].ApplyBuildingHeight(false);
            
            // 削除
            GameObject.Destroy(properties[index].Transform.gameObject);
            properties.RemoveAt(index);
            propertiesSnapshot.RemoveAt(index);

            AreaCountChangedEvent();

            return true;
        }
        /// <summary>
        /// 区画データリストの長さを取得するメソッド
        /// </summary>
        public static int GetPropertyCount()
        {
            return properties.Count;
        }

        /// <summary>
        /// 区画の高さを建物に適用
        /// </summary>
        public static bool ApplyBuildingHeight(int index, bool isApply)
        {
            if (index < 0 || index >= properties.Count) return false;
            
            var property = properties[index];
            
            // 高さを適用
            property.ApplyBuildingHeight(isApply);
            
            // コライダーをセットし直す
            var mesh = property.Transform.gameObject.GetComponent<MeshFilter>().sharedMesh;
            property.Transform.gameObject.GetComponent<AreaPlanningCollisionHandler>().SetCollider(mesh);
            return true;
        }
    }

    /// <summary>
    /// 区画データのプロパティを保持するクラス
    /// </summary>
    public class AreaProperty
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public float LimitHeight { get; set; }
        public float LineOffset { get; set; }
        public Color Color { get; set; }
        public Material WallMaterial { get; private set; }
        public Material CeilingMaterial { get; private set; }

        public float WallMaxHeight { get; private set; }
        public Transform Transform { get; private set; }
        public Vector3 ReferencePosition { get; private set; }
        public List<List<Vector3>> PointData { get ; set; }
        public bool IsApplyBuildingHeight { get; private set; }

        private AreaPlanningBuildingHeight areaBuildingHeight;
        
        public AreaProperty(int id, string name, float limitHeight, float lineOffset, Color areaColor, Material wallMaterial, Material ceilingMaterial, float wallMaxHeight, Vector3 referencePos, Transform areaTransform, List<List<Vector3>> pointData, bool isApplyBuildingHeight)
        {
            ID = id;
            Name = name;
            LimitHeight = limitHeight;
            LineOffset = lineOffset;
            Color = areaColor;
            WallMaterial = wallMaterial;
            CeilingMaterial = ceilingMaterial;
            WallMaxHeight = wallMaxHeight;
            ReferencePosition = referencePos;
            Transform = areaTransform;
            PointData = pointData;
            IsApplyBuildingHeight = isApplyBuildingHeight;
            
            areaBuildingHeight = new AreaPlanningBuildingHeight(areaTransform);
        }

        public void SetLocalPosition(Vector3 newPosition)
        {
            ReferencePosition += newPosition - Transform.position;
            Transform.localPosition = newPosition;
        }
        
        public void ApplyBuildingHeight(bool isApply)
        {
            IsApplyBuildingHeight = isApply;
            
            // 高さを一度リセット
            areaBuildingHeight.Reset();
            if (isApply)
            {
                areaBuildingHeight.SetHeight(LimitHeight);
            }
        }
    }

    /// <summary>
    /// 変更適用前の区画データのプロパティを保持するクラス
    /// </summary>
    public class AreaPropertySnapshot
    {
        public int ID { get; private set; }
        public string Name { get; private set; }
        public float LimitHeight { get; private set; }
        public float LineOffset { get; private set; }
        public Color Color { get; private set; }
        public Vector3 Position { get; private set; }
        public Vector3 ReferencePosition { get; private set; }


        public AreaPropertySnapshot(int id, string name, float limitHeight, float lineOffset, Color areaColor, Vector3 referencePos, Vector3 areaPosition)
        {
            ID = id;
            Name = name;
            LimitHeight = limitHeight;
            LineOffset = lineOffset;
            Color = new Color(areaColor.r, areaColor.g, areaColor.b, areaColor.a);
            ReferencePosition = referencePos;
            Position = areaPosition;
        }

        public void SetValues(int id, string name, float limitHeight, float lineOffset, Color areaColor, Vector3 referencePos, Vector3 areaPosition)
        {
            ID = id;
            Name = name;
            LimitHeight = limitHeight;
            LineOffset = lineOffset;
            Color = new Color(areaColor.r, areaColor.g, areaColor.b, areaColor.a);
            ReferencePosition = referencePos;
            Position = areaPosition;
        }
    }
}
