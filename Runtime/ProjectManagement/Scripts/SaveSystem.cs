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
using Landscape2.Runtime.CameraPositionMemory;

using SFB;

namespace Landscape2.Runtime
{
    public class SaveSystem : ISubComponent
    {
        private VisualElement projectManagementUI;
        public event Action SaveEvent = delegate { };
        public event Action LoadEvent = delegate { };

        public SaveSystem(VisualElement globalNavi)
        {
            // UIの設定
            Button saveButton = globalNavi.Q<Button>("SaveButton");
            Button loadButton = globalNavi.Q<Button>("LoadButton");

            saveButton.clicked += SaveProject;
            loadButton.clicked += LoadProject;
        }

        void SaveProject()
        {
            var path = StandaloneFileBrowser.SaveFilePanel("Create File", "", "", "data");
            DataSerializer._savePath = path;

            SaveEvent();
            DataSerializer.SaveFile();

            Debug.Log("Project saved.");
        }

        void LoadProject()
        {
            var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", "data", false);
            string path = "";
            if (paths.Length > 0)
            {
                path = paths[0];
            }
            DataSerializer._savePath = path;

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

        public void LateUpdate(float deltaTime)
        {
        }

    }
}
