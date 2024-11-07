using UnityEngine;
using UnityEngine.UIElements;

namespace Landscape2.Runtime.GisDataLoader
{
    /// <summary>
    /// GISデータをロードして表示するプレゼンター
    /// </summary>
    public class GisDataLoaderUI : ISubComponent
    {
        // UI
        private GisLoadUI gisLoadUI;
        private GisPointListUI gisPointListUI;
        private GisPointPinsUI gisPointPinsUI;
        
        // カメラが移動したか
        private bool isCameraMoved = false;

        public GisDataLoaderUI(VisualElement root, SaveSystem saveSystem)
        {
            // ローダー
            var gisDataLoader = new GisLoader(); 
            
            // モデル
            var gisPointInfos = new GisPointInfos();
            
            // 各UIを初期化
            gisLoadUI = new GisLoadUI(gisDataLoader, gisPointInfos, root);
            gisPointListUI = new GisPointListUI(gisPointInfos, root);
            gisPointPinsUI = new GisPointPinsUI(gisPointInfos);

            // セーブシステム
            var gisSaveSystem = new GisDataSaveSystem(saveSystem, gisPointInfos);
        }
        
        public void Update(float deltaTime)
        {
            if (Camera.main.transform.hasChanged)
            {
                isCameraMoved = true;
                Camera.main.transform.hasChanged = false;
            }

            // 視点が変更された場合のみ処理
            if (!isCameraMoved)
            {
                return;
            }
            
            // ピンの更新
            gisPointPinsUI?.OnUpdate();
            
            isCameraMoved = false;
        }

        public void OnEnable()
        {
        }

        public void OnDisable()
        {
        }

        public void Start()
        {
        }
    }
}