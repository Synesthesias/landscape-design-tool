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
    public class SaveSystem : ISubComponent
    {
        private IList<GameObject> plateauAssets;
        private VisualElement projectManagementUI;
        
        public SaveSystem()
        {
            projectManagementUI = new UIDocumentFactory().CreateWithUxmlName("ProjectManagementUI");
            Button saveButton = projectManagementUI.Q<Button>("Save");
            Button loadButton = projectManagementUI.Q<Button>("Load");
            saveButton.clicked += SaveGame;
            loadButton.clicked += LoadGame;
            
        }
        public async void OnEnable()
        {
            AsyncOperationHandle<IList<GameObject>> assetHandle = Addressables.LoadAssetsAsync<GameObject>("PlateauProps_Assets", null);
            plateauAssets = await assetHandle.Task;
        }
        public void Update(float deltaTime)
        {
            if (Input.GetKeyDown(KeyCode.S)) // Sキーが押されたとき
            {
                SaveGame();
            }

            if (Input.GetKeyDown(KeyCode.L)) // Lキーが押されたとき
            {
                LoadGame();
            }
        }
        void SaveGame()
        {
            var inputPath = projectManagementUI.Q<TextField>("Path");
            var inputFileName = projectManagementUI.Q<TextField>("FileName");
            DataSerializer._savePath = Path.Combine(inputPath.value,inputFileName.value);
            List<TransformData> assetsTransformData = new List<TransformData>();
            GameObject createdAssets = GameObject.Find("CreatedAssets");
            foreach(Transform asset in createdAssets.transform)
            {
                assetsTransformData.Add(new TransformData(asset));
            }
            DataSerializer.Save("TRandomCubesTransformData", assetsTransformData);
            DataSerializer.SaveFile();

            // セーブ機能の呼び出し
            Debug.Log("Game saved.");
        }

        void LoadGame()
        {
            var inputPath = projectManagementUI.Q<TextField>("Path");
            var inputFileName = projectManagementUI.Q<TextField>("FileName");
            DataSerializer._savePath = Path.Combine(inputPath.value,inputFileName.value);
            DataSerializer.LoadFile();
            List<TransformData> loadedTransformData = DataSerializer.Load<List<TransformData>>("TRandomCubesTransformData");
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
                Debug.Log("Game loaded.");
            }
            else
            {
                Debug.LogError("No saved game data found.");
            }
        }
        public void OnDisable()
        {
        }
    }
}
