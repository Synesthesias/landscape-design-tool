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

    public class SaveSystem_Assets
    {
        private SaveMode currentLoadMode;
        public Dictionary<GameObject, string> plateauAssets;
        private SaveSystem _saveSystem;
        private SaveProps saveProps;
        private SaveHumans saveHumans;

        public async void InstantiateSaveSystem(SaveSystem saveSystem)
        {
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
            // SaveModeクラスにアセットを渡す
            saveProps = new SaveProps();
            saveHumans = new SaveHumans();
            saveHumans.SetPlateauAssets(plateauAssets);
            saveProps.SetPlateauAssets(plateauAssets);
            // SaveSystem_Assetsの初期化
            _saveSystem = saveSystem;
            SetSaveMode();
            SetLoadMode("All");
        }

        public void SetSaveMode()
        {
            _saveSystem.SaveEvent += saveHumans.SaveInfo;
            _saveSystem.SaveEvent += saveProps.SaveInfo;
        }
        
        public void SetLoadMode(string loadMode) 
        {
            if(loadMode == "All")
            {
                _saveSystem.LoadEvent += saveHumans.LoadInfo;
                _saveSystem.LoadEvent += saveProps.LoadInfo;
            }
            else if(loadMode == "Humans")
            {
                currentLoadMode = saveHumans;
                _saveSystem.LoadEvent += currentLoadMode.LoadInfo;
            }
            else if(loadMode == "Props")
            {
                currentLoadMode = saveProps;
                _saveSystem.LoadEvent += currentLoadMode.LoadInfo;
            }
        }
    }
}
