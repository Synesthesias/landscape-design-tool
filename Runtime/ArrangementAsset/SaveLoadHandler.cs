using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ToolBox.Serialization;
using System.Linq;

namespace Landscape2.Runtime
{
    public class SaveLoadHandler
    {
        public IList<GameObject> plateauAssets;
        public SaveLoadHandler(IList<GameObject> assets)
        {
            plateauAssets = assets;
        }

        public void SaveInfo()
        {
            List<TransformData> assetsTransformData = new List<TransformData>();
            GameObject createdAssets = GameObject.Find("CreatedAssets");
            foreach(Transform asset in createdAssets.transform)
            {
                assetsTransformData.Add(new TransformData(asset));
            }
            DataSerializer.Save("Assets", assetsTransformData);
        }

        public void LoadInfo()
        {
            List<TransformData> loadedTransformData = DataSerializer.Load<List<TransformData>>("Assets");
            if (loadedTransformData != null)
            {
                // 既存のアセットの削除
                GameObject createdAssets = GameObject.Find("CreatedAssets");
                foreach(Transform child in createdAssets.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }
                // ロードしたアセットの生成
                foreach(TransformData assetData in loadedTransformData)
                {
                    string assetName = assetData.name;
                    GameObject asset = plateauAssets.FirstOrDefault(p => p.name == assetName);
                    GameObject generatedAsset = GameObject.Instantiate(asset,assetData.position, assetData.rotation, createdAssets.transform) as GameObject;
                    generatedAsset.transform.localScale = assetData.scale;
                    generatedAsset.name = asset.name;
                }
            }
            else
            {
                Debug.LogError("No saved project data found.");
            }
        }
    }
}
