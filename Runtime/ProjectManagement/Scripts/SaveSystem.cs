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
        private VisualElement projectManagementUI;
        private ChangeLoadMode changeLoadMode;
        public event Action SaveEvent = delegate { };
        public event Action LoadEvent = delegate { };

        public SaveSystem()
        {
            changeLoadMode = new ChangeLoadMode();
            changeLoadMode.CreateSaveSystemInstance(this);

                        // UIの設定
            projectManagementUI = new UIDocumentFactory().CreateWithUxmlName("ProjectManagementUI");
            Button saveButton = projectManagementUI.Q<Button>("SaveButton");
            Button loadButton = projectManagementUI.Q<Button>("LoadButton");
            saveButton.clicked += SaveProject;
            loadButton.clicked += LoadProject;
        }
        
        void SaveProject()
        {
            var inputPath = projectManagementUI.Q<TextField>("Path");
            var inputFileName = projectManagementUI.Q<TextField>("FileName");
            DataSerializer._savePath = Path.Combine(inputPath.value,inputFileName.value);

            SaveEvent();
            DataSerializer.SaveFile();

            Debug.Log("Project saved.");
        }

        void LoadProject()
        {
            var inputPath = projectManagementUI.Q<TextField>("Path");
            var inputFileName = projectManagementUI.Q<TextField>("FileName");
            DataSerializer._savePath = Path.Combine(inputPath.value,inputFileName.value);

            DataSerializer.LoadFile();
            LoadEvent();

            Debug.Log("Project loaded.");
        }

        public void ResetLoadEvent()
        {
            LoadEvent = null;
        }

        public void OnEnable()
        {
        }
        public void Start()
        {
        }
        public void Update(float deltaTime)
        {
        }
        public void OnDisable()
        {
        }
    }
}
