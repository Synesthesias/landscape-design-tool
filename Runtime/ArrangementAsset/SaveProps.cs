using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ToolBox.Serialization;
using System.Linq;

namespace Landscape2.Runtime
{
    public class SaveProps : SaveMode
    {
        Dictionary<GameObject, string> plateauAssets;

        public override void SetPlateauAssets(Dictionary<GameObject, string> assets)
        {
            plateauAssets = assets;
        }

        public override void SaveInfo()
        {
            List<TransformData> assetsTransformData = new List<TransformData>();
            
            GameObject createdAssets = GameObject.Find("PropsAssets");
            foreach(Transform asset in createdAssets.transform)
            {
                assetsTransformData.Add(new TransformData(asset));
            }

            DataSerializer.Save("Props", assetsTransformData);
        }

        public override void LoadInfo()
        {
            List<TransformData> loadedTransformData = DataSerializer.Load<List<TransformData>>("Props");
            if (loadedTransformData != null)
            {
                foreach(TransformData assetData in loadedTransformData)
                {
                    string assetName = assetData.name;
                    GameObject asset = plateauAssets.Keys.FirstOrDefault(p => p.name == assetName);
                    GameObject createdAssets = GameObject.Find("PropsAssets");
                    GameObject generatedAsset = GameObject.Instantiate(asset,assetData.position, assetData.rotation, createdAssets.transform) as GameObject;
                    generatedAsset.transform.localScale = assetData.scale;
                    generatedAsset.name = asset.name;
                }
            }
            else
            {
                Debug.LogError("No saved game data found.");
            }
        }
    }
}
