using System.Collections.Generic;
using System;
using UnityEngine;

namespace Landscape2.Runtime.LandscapePlanLoader
{
    /// <summary>
    /// 読み込んだ区画データを管理するクラス
    /// </summary>
    public sealed class AreasDataComponent : ISubComponent
    {
        // 区画データリスト
        private static List<AreaProperty> properties;

        // 変更適用前の情報を保持する区画データリスト
        private static List<AreaPropertySnapshot> propertiesSnapshot;

        // 区画データ数に変更があった際のイベント
        public static event Action AreaCountChangedEvent = delegate { };

        public AreasDataComponent()
        {
            properties = new List<AreaProperty>();
            propertiesSnapshot = new List<AreaPropertySnapshot>();
        }

        /// <summary>
        /// 区画データを新規に追加するメソッド
        /// </summary>
        public static void AddNewProperty(AreaProperty newProperty)
        {
            AreaPropertySnapshot newPropertyOrigin = new AreaPropertySnapshot(
                newProperty.ID,
                newProperty.name,
                newProperty.limitHeight,
                newProperty.lineOffset,
                newProperty.color,
                newProperty.referencePosition,
                newProperty.transform.localPosition
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
            properties[index].name = propertiesSnapshot[index].name;
            properties[index].limitHeight = propertiesSnapshot[index].limitHeight;
            properties[index].lineOffset = propertiesSnapshot[index].lineOffset;
            properties[index].color = propertiesSnapshot[index].color;
            properties[index].SetLocalPosition(propertiesSnapshot[index].position);

            properties[index].ceilingMaterial.color = properties[index].color;
            properties[index].wallMaterial.color = properties[index].color;
            properties[index].wallMaterial.SetFloat("_DisplayRate", properties[index].limitHeight / properties[index].wallMaxHeight);
            properties[index].wallMaterial.SetFloat("_LineCount", properties[index].limitHeight / properties[index].lineOffset);

            return true;
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
                properties[index].name,
                properties[index].limitHeight,
                properties[index].lineOffset,
                properties[index].color,
                properties[index].referencePosition,
                properties[index].transform.localPosition
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

            GameObject.Destroy(properties[index].transform.gameObject);
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


        public void OnDisable()
        {
        }
        public void OnEnable()
        {
        }
        public void Start()
        {
        }
        public void Update(float deltaTime)
        {
        }
    }

    /// <summary>
    /// 区画データのプロパティを保持するクラス
    /// </summary>
    public class AreaProperty
    {
        public int ID { get; set; }
        public string name { get; set; }
        public float limitHeight { get; set; }
        public float lineOffset { get; set; }
        public Color color { get; set; }
        public Material wallMaterial { get; private set; }
        public Material ceilingMaterial { get; private set; }

        public float wallMaxHeight { get; private set; }
        public Transform transform { get; private set; }
        public Vector3 referencePosition { get; private set; }
        public List<Vector3> pointData { get; private set; }

        public AreaProperty(int ID, string name, float limitHeight, float lineOffset, Color areaColor, Material wallMaterial, Material ceilingMaterial, float wallMaxHeight, Vector3 referencePos, Transform areaTransform, List<Vector3> pointData)
        {
            this.ID = ID;
            this.name = name;
            this.limitHeight = limitHeight;
            this.lineOffset = lineOffset;
            this.color = areaColor;
            this.wallMaterial = wallMaterial;
            this.ceilingMaterial = ceilingMaterial;
            this.wallMaxHeight = wallMaxHeight;
            this.referencePosition = referencePos;
            this.transform = areaTransform;
            this.pointData = pointData;
        }

        public void SetLocalPosition(Vector3 newPosition)
        {
            referencePosition += newPosition - transform.position;
            transform.localPosition = newPosition;
        }
    }

    /// <summary>
    /// 変更適用前の区画データのプロパティを保持するクラス
    /// </summary>
    public class AreaPropertySnapshot
    {
        public int ID { get; private set; }
        public string name { get; private set; }
        public float limitHeight { get; private set; }
        public float lineOffset { get; private set; }
        public Color color { get; private set; }
        public Vector3 position { get; private set; }
        public Vector3 referencePosition { get; private set; }


        public AreaPropertySnapshot(int ID, string name, float limitHeight, float lineOffset, Color areaColor, Vector3 referencePos, Vector3 areaPosition)
        {
            this.ID = ID;
            this.name = name;
            this.limitHeight = limitHeight;
            this.lineOffset = lineOffset;
            this.color = new Color(areaColor.r, areaColor.g, areaColor.b, areaColor.a);
            this.referencePosition = referencePos;
            this.position = areaPosition;
        }

        public void SetValues(int ID, string name, float limitHeight, float lineOffset, Color areaColor, Vector3 referencePos, Vector3 areaPosition)
        {
            this.ID = ID;
            this.name = name;
            this.limitHeight = limitHeight;
            this.lineOffset = lineOffset;
            this.color = new Color(areaColor.r, areaColor.g, areaColor.b, areaColor.a);
            this.referencePosition = referencePos;
            this.position = areaPosition;
        }
    }
}
