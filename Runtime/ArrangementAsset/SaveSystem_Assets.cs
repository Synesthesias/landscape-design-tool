using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ToolBox.Serialization;
using System.Linq;

namespace Landscape2.Runtime
{
    [Serializable]
    public struct TransformData
    {
        public string name;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        // Transformを引数に取るコンストラクタ
        public TransformData(Transform transform)
        {
            name = transform.name;
            position = transform.position;
            rotation = transform.rotation;
            scale = transform.localScale;
        }
    }

    public class SaveSystem_Assets : ISubComponent
    {
        IList<GameObject> plateauAssets;

        public void OnEnable()
        {
            SaveSystem saveSystem = new SaveSystem();
            saveSystem.SaveEvent += SaveAssetInfo;
            saveSystem.LoadEvent += LoadAssetInfo;
        }

        public void LoadPlateauAssets(IList<GameObject> assets)
        {
            plateauAssets = assets;
        }

        private void SaveAssetInfo()
        {
            List<TransformData> assetsTransformData = new List<TransformData>();
            GameObject createdAssets = GameObject.Find("CreatedAssets");
            foreach(Transform asset in createdAssets.transform)
            {
                assetsTransformData.Add(new TransformData(asset));
            }
            DataSerializer.Save("plateauAssetGameObject", assetsTransformData);
        }

        private void LoadAssetInfo()
        {
            List<TransformData> loadedTransformData = DataSerializer.Load<List<TransformData>>("plateauAssetGameObject");
            if (loadedTransformData != null)
            {
                foreach(TransformData assetData in loadedTransformData)
                {
                    GameObject asset = plateauAssets.FirstOrDefault(p => p.name == assetData.name);
                    Debug.Log(assetData.name);
                    GameObject createdAssets = GameObject.Find("CreatedAssets");
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

        public void Update(float deltaTime)
        {
        }

        public void OnDisable()
        {
        }
    }
}
