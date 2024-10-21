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
    public class AssetsSubscribeSaveSystem
    {
        private SaveSystem saveSystem;
        private SaveLoadHandler saveLoadHandler;

        public AssetsSubscribeSaveSystem(SaveSystem saveSystemInstance)
        {
            saveSystem = saveSystemInstance;
            GetPlateauAssetList();       
        }
        private async void GetPlateauAssetList()
        {
            AsyncOperationHandle<IList<GameObject>> plateauAssetHandle = Addressables.LoadAssetsAsync<GameObject>("Plateau_Assets", null);
            IList<GameObject> plateauAssetsList = await plateauAssetHandle.Task;
            saveLoadHandler = new SaveLoadHandler(plateauAssetsList);
            SetSaveMode();
            SetLoadMode();
        }

        public void SetSaveMode()
        {
            saveSystem.SaveEvent += saveLoadHandler.SaveInfo;
        }
        
        public void SetLoadMode() 
        {
            saveSystem.LoadEvent += saveLoadHandler.LoadInfo;
        }
    }
}
