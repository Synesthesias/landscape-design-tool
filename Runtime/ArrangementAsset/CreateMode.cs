using Landscape2.Runtime.Common;
using PLATEAU.CityInfo;
using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
// 入力処理
using UnityEngine.InputSystem;
// FirstOrDefault()の使用
using System.Linq;
using UnityEngine.UIElements;
// OnSelectの処理待ちに使用
using System.Threading.Tasks;

namespace Landscape2.Runtime
{
    // ArrangeModeクラスの派生クラス
    public class CreateMode : ArrangeMode
    {
        private Camera cam;
        private Ray ray;

        public GameObject selectedAsset;
        private GameObject generatedAsset;
        private AssetPlacedDirectionComponent component;
        private bool isMouseOverUI;

        private Vector3? assetSize = null;
        private float buriedHeight = 0.0f; // 地面に埋まっている高さ

        public void OnEnable(VisualElement element)
        {
            var rightUI = element.Q<VisualElement>("RightContainer");
            var leftUI = element.Q<VisualElement>("LeftContainer");

            foreach (var child in rightUI.Children())
            {
                child.RegisterCallback<MouseEnterEvent>(OnMouseEnter); 
                child.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
            }

            foreach (var child in leftUI.Children())
            {
                child.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
                child.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
            }
        }

        public override void Update()
        {
            if (generatedAsset != null)
            {
                cam = Camera.main;
                ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMaskUtil.GetGroundClickLayerMask()))
                {
                    if (!CanPlaceAsset(generatedAsset, hit.transform.gameObject))
                    {
                        return;
                    }
                    var point = hit.point;
                    point.y += buriedHeight;
                    generatedAsset.transform.position = point;
                    if (component != null)
                    {
                        component.setPlaceNormal = hit.normal;
                    }
                }
            }
        }

        private bool CanPlaceAsset(GameObject assetToPlace, GameObject hitObject)
        {
            if (assetToPlace.GetComponent<PlateauSandboxBuilding>())
            {
                // 建物の場合は、地面と道路のみに配置
                return CityObjectUtil.IsGround(hitObject) ||
                       hitObject.layer == LayerMask.NameToLayer("Ground");
            }

            // UIStateManagerのグローバル状態をチェック
            if (UIStateManager.IsMouseOverAnyUI || isMouseOverUI)
            {
                return false;
            }
            return true;
        }

        public void generateAssets(GameObject obj)
        {
            cam = Camera.main;
            ray = cam.ScreenPointToRay(Input.mousePosition);
            // UIStateManagerのグローバル状態もチェック
            if ((UIStateManager.IsMouseOverAnyUI || isMouseOverUI) && generatedAsset != null)
            {
                ArrangementAssetListUI.OnCancelAsset.Invoke(generatedAsset);
                GameObject.Destroy(generatedAsset);
                assetSize = null;
            }

            var generatedPosition = Vector3.zero;
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                if (CanPlaceAsset(obj, hit.transform.gameObject))
                {
                    generatedPosition = hit.point;
                }
            }
            GameObject parent = GameObject.Find("CreatedAssets");
            generatedAsset = GameObject.Instantiate(obj, generatedPosition, Quaternion.identity, parent.transform) as GameObject;

            var lod = generatedAsset.GetComponent<LODGroup>();
            if (lod != null)
            {
                lod.enabled = false;
            }

            assetSize = GetGameObjectSize(generatedAsset);

            generatedAsset.name = obj.name;
            int generateLayer = LayerMask.NameToLayer("Ignore Raycast");
            SetLayerRecursively(generatedAsset, generateLayer);

            // アセット生成時に必要コンポーネント付与
            AssetPlacedDirectionComponent.TryAdd(generatedAsset);
            component = generatedAsset.GetComponent<AssetPlacedDirectionComponent>();
        }

        public void SetAsset(string assetName, IList<GameObject> plateauAssets)
        {
            // 選択されたアセットを取得
            selectedAsset = plateauAssets.FirstOrDefault(p => p.name == assetName);
            generateAssets(selectedAsset);
        }

        void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (obj == null)
                return;

            obj.layer = newLayer;

            foreach (Transform child in obj.transform)
            {
                if (child == null)
                    continue;

                SetLayerRecursively(child.gameObject, newLayer);
            }
        }

        public Vector3 GetGameObjectSize(GameObject gameObject)
        {

            if (gameObject == null)
            {
                Debug.LogWarning("GameObject が null です。");
                return Vector3.zero;
            }

            // GameObject 内のすべての Renderer を取得
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();

            if (renderers.Length == 0)
            {
                Debug.LogWarning("Renderer が見つかりません。");
                return Vector3.zero;
            }

            // 初期化：最初の Renderer の Bounds を基準にする
            Bounds combinedBounds = new Bounds(Vector3.zero, Vector3.zero);
            bool initialized = false;

            foreach (var renderer in renderers)
            {
                // 各 Renderer の Bounds を取得
                Bounds localBounds = renderer.bounds;

                // ワールド座標系でのスケールを適用
                Vector3 worldMin = renderer.transform.TransformPoint(localBounds.min);
                Vector3 worldMax = renderer.transform.TransformPoint(localBounds.max);

                // スケール適用後の Bounds を作成
                Bounds worldBounds = new Bounds();
                worldBounds.SetMinMax(worldMin, worldMax);

                // 統合
                if (!initialized)
                {
                    combinedBounds = worldBounds;
                    initialized = true;
                }
                else
                {
                    combinedBounds.Encapsulate(worldBounds);
                }
            }

            // 統合された Bounds のサイズを返す
            return combinedBounds.size;
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            isMouseOverUI = true;
            // UIStateManagerにアセットUI状態を設定
            UIStateManager.IsMouseOverAssetUI = true;
        }

        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            isMouseOverUI = false;
            // UIStateManagerのアセットUI状態をクリア
            UIStateManager.IsMouseOverAssetUI = false;
        }

        public void OnSelect()
        {
            if (selectedAsset != null)
            {
                // アセット作成通知
                ArrangementAssetListUI.OnCreatedAsset.Invoke(generatedAsset);
                if (generatedAsset.TryGetComponent(out component))
                {
                    // 配置完了
                    component.SetPlaced();
                }

                // プロジェクトに通知
                ProjectSaveDataManager.Add(ProjectSaveDataType.Asset, generatedAsset.gameObject.GetInstanceID().ToString());

                SetLayerRecursively(generatedAsset, 0);

                SetLayerRecursively(generatedAsset, 0);
                generateAssets(selectedAsset);
            }
        }

        public override void OnCancel()
        {
            selectedAsset = null;
            assetSize = null;
            GameObject.Destroy(generatedAsset);
            generatedAsset = null;
        }
    }
}