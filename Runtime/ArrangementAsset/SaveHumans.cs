// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using ToolBox.Serialization;
// using System.Linq;

// namespace Landscape2.Runtime
// {
//     public class SaveHumans
//     {
//         public Dictionary<GameObject, string> plateauHumansAssets;

//         public override void SetPlateauAssets(Dictionary<GameObject, string> assets)
//         {
//             plateauHumansAssets = assets;
//         }

//         public override void SaveInfo()
//         {
//             List<TransformData> assetsTransformData = new List<TransformData>();
//             GameObject createdAssets = GameObject.Find("HumansAssets");
//             foreach(Transform asset in createdAssets.transform)
//             {
//                 assetsTransformData.Add(new TransformData(asset));
//             }
//             DataSerializer.Save("Humans", assetsTransformData);
//         }

//         public override void LoadInfo()
//         {
//             List<TransformData> loadedTransformData = DataSerializer.Load<List<TransformData>>("Humans");
//             if (loadedTransformData != null)
//             {
//                 foreach(TransformData assetData in loadedTransformData)
//                 {
//                     string assetName = assetData.name;
//                     GameObject asset = plateauHumansAssets.Keys.FirstOrDefault(p => p.name == assetName);
//                     GameObject createdAssets = GameObject.Find("HumansAssets");
//                     // ロードしたアセットの生成
//                     GameObject generatedAsset = GameObject.Instantiate(asset,assetData.position, assetData.rotation, createdAssets.transform) as GameObject;
//                     generatedAsset.transform.localScale = assetData.scale;
//                     generatedAsset.name = asset.name;
//                 }
//             }
//             else
//             {
//                 Debug.LogError("No saved project humans data found.");
//             }
//         }
//     }
// }