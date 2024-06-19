using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using ToolBox.Serialization;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Linq;

using UnityEngine.UIElements;
using Landscape2.Runtime.UiCommon;

namespace Landscape2.Runtime
{
    public class SaveSystem : ISubComponent
    {
        private IList<GameObject> plateauAssets;
        private VisualElement projectManagementUI;
        public event Action SaveEvent = delegate { };
        public event Action LoadEvent = delegate { };
        SaveSystem_Assets saveSystem_Assets;
        public SaveSystem()
        {
            saveSystem_Assets = new SaveSystem_Assets();
            saveSystem_Assets.InstantiateSaveSystem(this);
            projectManagementUI = new UIDocumentFactory().CreateWithUxmlName("ProjectManagementUI");
            Button saveButton = projectManagementUI.Q<Button>("SaveButton");
            Button loadButton = projectManagementUI.Q<Button>("LoadButton");
            saveButton.clicked += SaveGame;
            loadButton.clicked += LoadGame;
            DropdownField loadTypeDropdown = projectManagementUI.Q<DropdownField>("LoadType");
            loadTypeDropdown.choices = new List<string> { "All", "Props", "Humans"};
            loadTypeDropdown.value = "All";
            loadTypeDropdown.RegisterValueChangedCallback(evt =>
            {
                OnDropdownValueChanged(evt.newValue);
            });
        }

        public async void OnEnable()
        {
            AsyncOperationHandle<IList<GameObject>> assetHandle = Addressables.LoadAssetsAsync<GameObject>("PlateauProps_Assets", null);
            plateauAssets = await assetHandle.Task;
        }

        public void Update(float deltaTime)
        {
        }
        
        void SaveGame()
        {
            var inputPath = projectManagementUI.Q<TextField>("Path");
            var inputFileName = projectManagementUI.Q<TextField>("FileName");
            DataSerializer._savePath = Path.Combine(inputPath.value,inputFileName.value);

            SaveEvent();
            DataSerializer.SaveFile();

            Debug.Log("Game saved.");
        }

        void LoadGame()
        {
            var inputPath = projectManagementUI.Q<TextField>("Path");
            var inputFileName = projectManagementUI.Q<TextField>("FileName");
            DataSerializer._savePath = Path.Combine(inputPath.value,inputFileName.value);

            DataSerializer.LoadFile();
            LoadEvent();

            Debug.Log("Game loaded.");
        }
        private void OnDropdownValueChanged(string dropdownValue)
        {
            LoadEvent = delegate { };
            saveSystem_Assets.SetLoadMode(dropdownValue);
            
            Debug.Log(dropdownValue);
        }

        
        public void OnDisable()
        {
        }
    }
}
