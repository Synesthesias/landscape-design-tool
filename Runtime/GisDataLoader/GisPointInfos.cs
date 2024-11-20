using Landscape2.Runtime.UiCommon;
using PLATEAU.CityGML;
using PLATEAU.CityInfo;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Landscape2.Runtime.GisDataLoader
{
    /// <summary>
    /// GISデータのポイントリスト
    /// </summary>
    public class GisPointInfos
    {
        public List<GisPointInfo> Points { get; private set; } = new();
        
        public UnityEvent<int> OnCreate = new();
        public UnityEvent<int, bool> OnUpdateDisplay = new();
        public UnityEvent<int> OnDelete = new();
        public UnityEvent OnDeleteAll = new();
        
        private const string ErrorTitle = "GISデータ登録";
        private int pointIndex = 0;
        
        public void Regist(string name, List<GisData> gisDataList, int selectAttributeIndex, Color color)
        {
            if (Points.Any(p => p.AttributeIndex == selectAttributeIndex))
            {
                ModalUI.ShowModal("GISデータ登録", $"選択された属性はすでに登録されています。", false, true);
                return;
            }

            foreach (var gisData in gisDataList)
            {
                var facilityName = gisData.Attributes[selectAttributeIndex].Value;
                
                // 施設の位置を取得
                var facilityPosition = GetFacilityPosition(gisData.WorldPoints[0]);
                if (facilityPosition == Vector3.zero)
                {
                    Debug.LogWarning($"{facilityName} 施設の位置が取得できませんでした。");
                    continue;
                }

                // ポイント情報を登録
                Points.Add(new GisPointInfo(
                    pointIndex,
                    selectAttributeIndex,
                    facilityName,
                    name,
                    facilityPosition,
                    color,
                    true
                ));
                pointIndex++;
            }

            OnCreate.Invoke(selectAttributeIndex);
            
            // 完了
            ModalUI.ShowModal("データ読み込み完了", "GISデータを登録しました", true, false, () =>
            {
            }); 
        }

        public GisPointInfo Get(int ID)
        {
            return Points.FirstOrDefault(p => p.ID == ID);
        }

        public GisPointInfo GetByAttribute(int attributeIndex)
        {
            return Points.FirstOrDefault(p => p.AttributeIndex == attributeIndex);
        }

        public List<GisPointInfo> GetAttributeAll(int attributeIndex)
        {
            return Points.FindAll(p => p.AttributeIndex == attributeIndex);
        }

        public void SetShow(int attributeIndex, bool isShow)
        {
            var points = Points.FindAll(p => p.AttributeIndex == attributeIndex);
            foreach (var point in points)
            {
                point.SetShow(isShow);
            }

            OnUpdateDisplay.Invoke(attributeIndex, isShow);
        }

        public void Delete(int attributeIndex)
        {
            Points.RemoveAll(p => p.AttributeIndex == attributeIndex);
            
            OnDelete.Invoke(attributeIndex);
        }
        
        public void DeleteAll()
        {
            Points.Clear();
            
            OnDeleteAll.Invoke();
        }
        
        public void SetPoints(List<GisPointInfo> points)
        {
            Points = points;
            
            // 属性インデックスでフィルターして更新
            foreach (var attributeIndex in Points.Select(p => p.AttributeIndex).Distinct())
            {
                OnCreate.Invoke(attributeIndex);
            }
        }
        
        private Vector3 GetFacilityPosition(Vector3 worldPosition)
        {
            // レイの設定
            Vector3 rayDirection = Vector3.down;
            const float RayLength = 10000f;
            var rayOrigin = new Vector3(worldPosition.x, RayLength, worldPosition.z);
        
            // 該当の位置からレイを飛ばして、該当の施設を取得
            var ray = new Ray(rayOrigin, Vector3.down);

            // レイキャスト地面を突き抜けるように長めに設定
            RaycastHit[] results = Physics.RaycastAll(ray, RayLength * 2);
            Vector3 hitPosition = Vector3.zero;
            foreach (RaycastHit rayCastHit in results)
            {
                if (rayCastHit.transform.TryGetComponent(out PLATEAUCityObjectGroup cityObject))
                {
                    hitPosition = rayCastHit.transform.position;
                    if (IsFacility(cityObject))
                    {
                        var bounds = cityObject.GetComponent<MeshCollider>().bounds;
                        return new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
                    }
                }
            }
            
            return hitPosition;
        }
        
        private bool IsFacility(PLATEAUCityObjectGroup cityObject)
        {
            // 建物が含まれていれば施設と判定
            return cityObject.CityObjects.rootCityObjects.Any(o => o.CityObjectType == CityObjectType.COT_Building);
        }
    }
}