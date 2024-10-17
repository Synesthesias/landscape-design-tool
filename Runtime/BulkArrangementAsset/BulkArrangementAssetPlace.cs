using PlateauToolkit.Sandbox.Runtime;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Landscape2.Runtime
{
    public class BulkArrangementAssetPlace
    {
        private CancellationTokenSource cancellation;
        private PlateauSandboxPrefabPlacement prefabPlacement;
        
        public async Task PlaceAll(BulkArrangementAsset bulkArrangementAsset)
        {
            prefabPlacement = new PlateauSandboxPrefabPlacement();
            if (!prefabPlacement.IsValidCityModel())
            {
                Debug.LogWarning("配置範囲内に3D都市モデルが存在しません");
                return;
            }
            
            if (bulkArrangementAsset.AssetTypes.All(n => n.PrefabConstantID < 0))
            {
                Debug.LogWarning("プレハブを設定してください");
                return;
            }
            
            // 作成するAssetの親オブジェクトを更新しておく
            prefabPlacement.SetParentObjectName("CreatedAssets");
            
            foreach (var placeData in bulkArrangementAsset.GetParsedData())
            {
                var assetItem = bulkArrangementAsset.AssetTypes.FirstOrDefault(n => n.CategoryName == placeData.AssetType);
                if (assetItem == null || assetItem.PrefabConstantID < 0)
                {
                    continue;
                }

                var prefab = ArrangementAssetLoader.GetAsset(assetItem.PrefabConstantID);
                if (prefab == null)
                {
                    continue;
                }

                try
                {
                    bool isIgnoreHeight = placeData.IsIgnoreHeight | bulkArrangementAsset.IsIgnoreHeight;
                    var context = new PlateauSandboxPrefabPlacement.PlacementContext()
                    {
                        m_Latitude = double.Parse(placeData.Latitude),
                        m_Longitude = double.Parse(placeData.Longitude),
                        m_Height = isIgnoreHeight ? 0 : float.Parse(placeData.Height),
                        m_Prefab = prefab,
                        m_IsIgnoreHeight = isIgnoreHeight,
                        m_IsPlaced = false,
                        m_ObjectName = prefab.name
                    };
                    prefabPlacement.AddContext(context);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("配置情報の取得に失敗しました: " + e.Message);
                }
            }

            if (prefabPlacement.PlacingCount == 0)
            {
                Debug.LogWarning("配置できるアセットがありません");
                return;
            }

            Debug.Log($"アセットの一括配置を開始しました。{prefabPlacement.PlacingCount}個の配置");

            if (cancellation == null)
            {
                cancellation = new CancellationTokenSource();
            }
            await prefabPlacement.PlaceAllAsync(cancellation.Token);
        }

        public void Stop()
        {
            if (cancellation == null || prefabPlacement == null)
            {
                return;
            }
            
            cancellation.Cancel();
            cancellation.Dispose();
            cancellation = null;
            prefabPlacement.StopPlace();
       }
        
        public bool IsAllPlaceSuccess()
        {
            return prefabPlacement.PlacementContexts.All(context => context.m_IsPlaced);
        }

        public bool IsAllPlaceFailed()
        {
            return prefabPlacement.PlacementContexts.All(context => !context.m_IsPlaced);
        }
    }
}