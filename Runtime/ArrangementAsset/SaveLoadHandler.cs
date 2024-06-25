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
        public Dictionary<GameObject, string> plateauHumansAssets;
        private string tagName;
        private string parentName;
        public SaveLoadHandler(AssetsCategory assetsCategory,Dictionary<GameObject, string> assets)
        {
            tagName = assetsCategory.ToString();
            parentName = tagName + "Assets";
            plateauHumansAssets = assets;
        }

        public void SaveInfo()
        {
            List<TransformData> assetsTransformData = new List<TransformData>();
            GameObject createdAssets = GameObject.Find(parentName);
            foreach(Transform asset in createdAssets.transform)
            {
                assetsTransformData.Add(new TransformData(asset));
            }
            DataSerializer.Save(tagName, assetsTransformData);
        }

        public void LoadInfo()
        {
            List<TransformData> loadedTransformData = DataSerializer.Load<List<TransformData>>(tagName);
            if (loadedTransformData != null)
            {
                foreach(TransformData assetData in loadedTransformData)
                {
                    string assetName = assetData.name;
                    GameObject asset = plateauHumansAssets.Keys.FirstOrDefault(p => p.name == assetName);
                    GameObject createdAssets = GameObject.Find(parentName);
                    // ロードしたアセットの生成
                    GameObject generatedAsset = GameObject.Instantiate(asset,assetData.position, assetData.rotation, createdAssets.transform) as GameObject;
                    generatedAsset.transform.localScale = assetData.scale;
                    generatedAsset.name = asset.name;
                }
            }
            else
            {
                Debug.LogError("No saved project humans data found.");
            }
        }
    }
}
