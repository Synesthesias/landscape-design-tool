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
            List<ArrangementSaveData> saveAssets = new List<ArrangementSaveData>();
            GameObject createdAssets = GameObject.Find("CreatedAssets");
            foreach(Transform asset in createdAssets.transform)
            {
                var saveData = new ArrangementSaveData();
                saveData.Save(asset);
                saveAssets.Add(saveData);
            }
            DataSerializer.Save("Assets", saveAssets);
        }

        public void LoadInfo()
        {
            List<ArrangementSaveData> loadedTransformData = DataSerializer.Load<List<ArrangementSaveData>>("Assets");
            if (loadedTransformData != null)
            {
                // 既存のアセットの削除
                GameObject createdAssets = GameObject.Find("CreatedAssets");
                foreach(Transform child in createdAssets.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }
                // ロードしたアセットの生成
                foreach(ArrangementSaveData savedData in loadedTransformData)
                {
                    var transformData = savedData.transformData;
                    string assetName = transformData.name;
                    GameObject asset = plateauAssets.FirstOrDefault(p => p.name == assetName);
                    GameObject generatedAsset = GameObject.Instantiate(asset, transformData.position, transformData.rotation, createdAssets.transform) as GameObject;
                    
                    // 生成したオブジェクトにデータを反映
                    savedData.Apply(generatedAsset);
                }
            }
            else
            {
                Debug.LogError("No saved project data found.");
            }
        }
    }
}
