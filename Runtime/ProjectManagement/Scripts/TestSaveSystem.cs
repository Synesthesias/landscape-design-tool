// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using System.IO;
// using System.Threading.Tasks;
// using UnityEngine.AddressableAssets;
// using UnityEngine.ResourceManagement.AsyncOperations;
// using System.Linq;


// namespace TestSaveSystem
// {
//     public struct TransformData
//     {
//         public string name;
//         public Vector3 position;
//         public Quaternion rotation;
//         public Vector3 scale;

//         // Transformを引数に取るコンストラクタ
//         public TransformData(Transform transform)
//         {
//             name = transform.name;
//             position = transform.position;
//             rotation = transform.rotation;
//             scale = transform.localScale;
//         }
//     }
//     public class TestSaveSystem : MonoBehaviour
//     {
//         private IList<GameObject> plateauAssets;


//         public async void OnEnable()
//         {
//             AsyncOperationHandle<IList<GameObject>> assetHandle = Addressables.LoadAssetsAsync<GameObject>("PlateauProps_Assets", null);
//             plateauAssets = await assetHandle.Task;
//         }
//         void Update()
//         {
//             if (Input.GetKeyDown(KeyCode.S)) // Sキーが押されたとき
//             {
//                 SaveGame();
//             }

//             if (Input.GetKeyDown(KeyCode.L)) // Lキーが押されたとき
//             {
//                 LoadGame();
//             }
//             if (Input.GetKeyDown(KeyCode.I)) // Lキーが押されたとき
//             {
//                 DataSerializer._savePath = Path.Combine(Application.dataPath, "SaveAssets1_0.data");
//             }
//             if (Input.GetKeyDown(KeyCode.J)) // Lキーが押されたとき
//             {
//                 DataSerializer._savePath = Path.Combine(Application.dataPath, "SaveAssets2_0.data");
//             }
//         }
//         void SaveGame()
//         {

//             List<TransformData> assetsTransformData = new List<TransformData>();
//             GameObject createdAssets = GameObject.Find("CreatedAssets");
//             foreach(Transform asset in createdAssets.transform)
//             {
//                 assetsTransformData.Add(new TransformData(asset));
//             }
//             DataSerializer.Save("TRandomCubesTransformData", assetsTransformData);
//             DataSerializer.SaveFile();

//             // セーブ機能の呼び出し
//             Debug.Log("Game saved.");
//         }

//         void LoadGame()
//         {
//             DataSerializer.LoadFile();
//             List<TransformData> loadedTransformData = DataSerializer.Load<List<TransformData>>("TRandomCubesTransformData");
//             if (loadedTransformData != null)
//             {
//                 foreach(TransformData assetData in loadedTransformData)
//                 {
//                     GameObject asset = plateauAssets.FirstOrDefault(p => p.name == assetData.name);
//                     Debug.Log(assetData.name);
//                     GameObject createdAssets = GameObject.Find("CreatedAssets");
//                     GameObject generatedAsset = GameObject.Instantiate(asset,assetData.position, assetData.rotation, createdAssets.transform) as GameObject;
//                     generatedAsset.transform.localScale = assetData.scale;
//                     generatedAsset.name = asset.name;
//                 }
//                 Debug.Log("Game loaded.");
//             }
//             else
//             {
//                 Debug.LogError("No saved game data found.");
//             }
//         }
//     }
// }
