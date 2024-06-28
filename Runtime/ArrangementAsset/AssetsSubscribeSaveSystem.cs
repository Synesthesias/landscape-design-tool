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

    public class AssetsSubscribeSaveSystem
    {
        private SaveSystem _saveSystem;
        private SaveLoadHandler saveLoadHandler;

        public async void InstantiateSaveSystem(SaveSystem saveSystem)
        {
            AsyncOperationHandle<IList<GameObject>> plateauAssetHandle = Addressables.LoadAssetsAsync<GameObject>("Plateau_Assets", null);
            IList<GameObject> plateauAssetsList = await plateauAssetHandle.Task;
            // SaveModeクラスにアセットを渡す
            saveLoadHandler = new SaveLoadHandler(plateauAssetsList);
            // SaveSystem_Assetsの初期化
            _saveSystem = saveSystem;
            SetSaveMode();
            SetLoadMode();
        }

        public void SetSaveMode()
        {
            _saveSystem.SaveEvent += saveLoadHandler.SaveInfo;
        }
        
        public void SetLoadMode() 
        {
            _saveSystem.LoadEvent += saveLoadHandler.LoadInfo;
        }
    }
}
