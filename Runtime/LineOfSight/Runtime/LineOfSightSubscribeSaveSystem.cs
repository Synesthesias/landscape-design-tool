using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ToolBox.Serialization;

using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;

namespace Landscape2.Runtime
{
    public class LineOfSightSubscribeSaveSystem
    {
        private LineOfSightDataComponent lineOfSightDataComponent;
        private LineOfSightUI lineOfSightUI;
        private SaveSystem saveSystem;
        private Sprite viewPointIconSprite;
        private Sprite landmarkIconSprite;

        public LineOfSightSubscribeSaveSystem(SaveSystem saveSystemInstance, LineOfSightDataComponent lineOfSightDataComponentInstance, LineOfSightUI lineOfSightUIInstance)
        {
            lineOfSightDataComponent = lineOfSightDataComponentInstance;
            lineOfSightUI = lineOfSightUIInstance;
            saveSystem = saveSystemInstance;
            LoadPointIcon();
            SetSaveEvent();
            SetLoadEvent();
        }

        private async void LoadPointIcon()
        {
            AsyncOperationHandle<Sprite> viewPointIconHandle = Addressables.LoadAssetAsync<Sprite>("ViewPointIcon");
            viewPointIconSprite = await viewPointIconHandle.Task;
            AsyncOperationHandle<Sprite> landmarkIconHandle = Addressables.LoadAssetAsync<Sprite>("LandmarkIcon");
            landmarkIconSprite = await landmarkIconHandle.Task;
        }

        private void SetSaveEvent()
        {
            saveSystem.SaveEvent += SaveDict;
        }

        private void SetLoadEvent()
        {
            saveSystem.LoadEvent += LoadViewPointDict;
            saveSystem.LoadEvent += LoadAnalyzeDataDict;
        }

        /// <summary>
        /// 各データを取得し、保存する
        /// </summary>
        private void SaveDict()
        {
            var viewPointDict = lineOfSightDataComponent.GetPointDict(LineOfSightType.viewPoint);
            var landmarkDict = lineOfSightDataComponent.GetPointDict(LineOfSightType.landmark);
            var analyzeViewPointDataDict = lineOfSightDataComponent.GetAnalyzeViewPoinDatatDict();
            var analyzeLandmarkDataDict = lineOfSightDataComponent.GetAnalyzeLandmarkDataDict();
            DataSerializer.Save("ViewPointDict", viewPointDict);
            DataSerializer.Save("LandmarktDict", landmarkDict);
            DataSerializer.Save("AnalyzeViewPointDataDict", analyzeViewPointDataDict);
            DataSerializer.Save("AnalyzeLandmarkDataDict", analyzeLandmarkDataDict);
        }

        /// <summary>
        /// 視点場、眺望対象をロードする
        /// </summary>
        private void LoadViewPointDict()
        {
            // 全てのボタンを削除
            lineOfSightUI.ClearButton(LineOfSightType.viewPoint);
            lineOfSightUI.ClearButton(LineOfSightType.landmark);
            // 辞書を初期化
            lineOfSightDataComponent.ClearDict(LineOfSightType.viewPoint);
            lineOfSightDataComponent.ClearDict(LineOfSightType.landmark);
            // 既存のポイントの削除
            var viewPointMarkers = GameObject.Find("ViewPointMarkers");
            foreach (Transform child in viewPointMarkers.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            var landamrkMarkers = GameObject.Find("LandmarkMarkers");
            foreach (Transform child in landamrkMarkers.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            // 辞書をロード
            var viewPointDict = DataSerializer.Load<Dictionary<string, LineOfSightDataComponent.PointData>>("ViewPointDict");
            foreach (KeyValuePair<string, LineOfSightDataComponent.PointData> point in viewPointDict)
            {
                // 辞書に登録
                lineOfSightDataComponent.AddPointDict(LineOfSightType.viewPoint, point.Key, point.Value);
                // ボタンの作成
                lineOfSightUI.CreateViewPointButton(point.Key);
                // ポイントの生成
                var loadPoint = new GameObject();
                var spriteRenderer = loadPoint.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = viewPointIconSprite;
                var boxCollider = loadPoint.AddComponent<BoxCollider>();
                boxCollider.size = spriteRenderer.sprite.bounds.size;
                loadPoint.transform.position = point.Value.pointPos;
                loadPoint.transform.parent = viewPointMarkers.transform;

                var go = new GameObject
                {
                    name = "yOffset"
                };
                go.transform.parent = loadPoint.transform;
                go.transform.localPosition = new Vector3(0f, point.Value.yOffset, 0f);

                loadPoint.name = point.Key;
            }
            var landmarkDict = DataSerializer.Load<Dictionary<string, LineOfSightDataComponent.PointData>>("LandmarktDict");
            foreach (KeyValuePair<string, LineOfSightDataComponent.PointData> point in landmarkDict)
            {
                // 辞書に登録
                lineOfSightDataComponent.AddPointDict(LineOfSightType.landmark, point.Key, point.Value);
                // ボタンの作成
                lineOfSightUI.CreateLandmarkButton(point.Key);
                // ポイントの生成
                var loadPoint = new GameObject();
                var spriteRenderer = loadPoint.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = landmarkIconSprite;
                var boxCollider = loadPoint.AddComponent<BoxCollider>();
                boxCollider.size = spriteRenderer.sprite.bounds.size;
                loadPoint.transform.position = point.Value.pointPos;

                var go = new GameObject
                {
                    name = "yOffset"
                };
                go.transform.parent = loadPoint.transform;
                go.transform.localPosition = new Vector3(0f, point.Value.yOffset, 0f);

                loadPoint.transform.parent = landamrkMarkers.transform;
                loadPoint.name = point.Key;
            }
        }

        /// <summary>
        /// 解析結果をロードする
        /// </summary>
        private void LoadAnalyzeDataDict()
        {
            // 表示されている解析データの初期化
            lineOfSightUI.ClearButton(LineOfSightType.analyzeViewPoint);
            // 視点場解析のロード
            lineOfSightDataComponent.ClearDict(LineOfSightType.analyzeViewPoint);
            var analyzedViewPointDataDict = DataSerializer.Load<Dictionary<string, AnalyzeViewPointElements>>("AnalyzeViewPointDataDict");
            foreach (KeyValuePair<string, AnalyzeViewPointElements> point in analyzedViewPointDataDict)
            {
                // 辞書に登録
                lineOfSightDataComponent.AddAnalyzeViewPoinDatatDict(point.Key, point.Value);
                // ボタンの作製
                lineOfSightUI.CreateAnalyzeViewPointButton(point.Value);
            }

            // 眺望対象解析のロード
            lineOfSightDataComponent.ClearDict(LineOfSightType.analyzeLandmark);
            var analyzeLandmarkDataDict = DataSerializer.Load<Dictionary<string, AnalyzeLandmarkElements>>("AnalyzeLandmarkDataDict");
            foreach (KeyValuePair<string, AnalyzeLandmarkElements> point in analyzeLandmarkDataDict)
            {
                lineOfSightDataComponent.AnddAnalyzeLandmarkDataDict(point.Key, point.Value);
                lineOfSightUI.CreateAnalyzeLandmarkButton(point.Value);
            }

        }
    }
}