using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ToolBox.Serialization;
using System.Linq;

using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;  // Taskを使用するために必要

namespace Landscape2.Runtime
{
    public abstract class SaveMode
    {
        public virtual void SetPlateauAssets(Dictionary<GameObject, string> assets) {}
        public virtual void SaveInfo() {}
        public virtual void LoadInfo() {}

    }

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
        private SaveMode currentMode;
        private SaveSystem _saveSystem;
        private SaveProps saveProps;
        private SaveHumans saveHumans;
        private ArrangeAsset arrangeAsset;
        public Dictionary<GameObject, string> plateauAssets;
        public async void InstantiateSaveSystem(SaveSystem saveSystem)
        {
            _saveSystem = saveSystem;
            // アセットの取得
            AsyncOperationHandle<IList<GameObject>> propsAssetHandle = Addressables.LoadAssetsAsync<GameObject>("PlateauProps_Assets", null);
            IList<GameObject> propsAssets = await propsAssetHandle.Task;
            AsyncOperationHandle<IList<GameObject>> humansAssetHandle = Addressables.LoadAssetsAsync<GameObject>("PlateauHumans_Assets", null);
            IList<GameObject> humansAssets = await humansAssetHandle.Task;
            plateauAssets = new Dictionary<GameObject, string>();
            foreach (GameObject prop in propsAssets)
            {
                plateauAssets[prop] = "Props";
            }
            foreach (GameObject human in humansAssets)
            {
                plateauAssets[human] = "Humans";
            }
            OnEnable();
        }
        public void OnEnable()
        {   
            saveProps = new SaveProps();
            saveHumans = new SaveHumans();

            saveHumans.SetPlateauAssets(plateauAssets);
            saveProps.SetPlateauAssets(plateauAssets);

            SetSaveMode();
            SetLoadMode("All");
        }
        public void SetSaveMode()
        {
            currentMode = saveHumans;
            _saveSystem.SaveEvent += currentMode.SaveInfo;
            currentMode = saveProps;
            _saveSystem.SaveEvent += currentMode.SaveInfo;
        }
        public void SetLoadMode(string loadMode) 
        {
            if(loadMode == "All")
            {
                currentMode = saveHumans;
                _saveSystem.LoadEvent += currentMode.LoadInfo;
                currentMode = saveProps;
                _saveSystem.LoadEvent += currentMode.LoadInfo;
            }
            else if(loadMode == "Humans")
            {
                currentMode = saveHumans;
                _saveSystem.LoadEvent += currentMode.LoadInfo;
            }
            else if(loadMode == "Props")
            {
                currentMode = saveProps;
                _saveSystem.LoadEvent += currentMode.LoadInfo;
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
