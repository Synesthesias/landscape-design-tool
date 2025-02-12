using PLATEAU.CityInfo;
using PLATEAU.Native;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Landscape2.Runtime
{
    public class BIMImport : ISubComponent
    {
        enum LayoutMode
        {
            Trans,
            Rot,
            Scale,
            None
        };
        private const float K_GroundCheckLength = 10000.0f;

        VisualElement uiRoot;

        BIMImportUI ui;

        BIMLayoutUI layoutUI;

        EditMode editMode = new();

        string filePath;

        GameObject currentLoadIfcObject;

        List<GameObject> importedIFCList = new();
        List<Mesh> importedColliderMeshes = new();

        // uiの表示状態
        bool uiStatus = false;

        LayoutMode layoutMode;

        PLATEAUInstancedCityModel instancedCityModel;

        bool IsRootUIVisible => uiRoot.style.display == DisplayStyle.Flex;

        public BIMImport(VisualElement uiRoot)
        {
            this.uiRoot = uiRoot;
            ui = new(uiRoot);

            ui.openFileAction += (path) =>
            {
                if (string.IsNullOrEmpty(path))
                {
                    Debug.LogWarning("pathがemptyかnullです");
                    return;
                }

                Debug.Log($"path: {path}");
                filePath = path;
                ui.FileNameLabel = Path.GetFileName(filePath);
            };

            // import開始
            ui.importButtonOnClickAction += async () =>
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    Debug.LogWarning($"filePathがnull or emptyです");
                    return;

                }

                var ifcObj = await BIMLoader.Instance.LoadBIM(filePath);

                if (ifcObj == null)
                {
                    Debug.LogWarning($"ifc:[{filePath}] is null!!!");
                    return;
                }

                ifcObj.layer = LayerMask.NameToLayer("Default");

                // 配置用情報作成
                var lat = ui.LatitudeValue;
                var lon = ui.LongitudeValue;
                var height = ui.HeightSliderValue;
                var yRot = ui.YawSliderValue;


                SetInstanceTransformFromInputState(ifcObj, lat, lon, height, yRot);
                var mesh = CombineIFCMesh(ifcObj);

                var mc = ifcObj.AddComponent<MeshCollider>();
                mc.sharedMesh = mesh;

                importedColliderMeshes.Add(mesh);

                // TODO: 配置できたIFCファイルは
                // bytearrayとしてProjectに保存できる様にする
                // copy先のpathとlatlon,height,yawを保持する

                SetupEditMode(ifcObj);
            };

            InitializeLayoutUI(uiRoot);
            ChangeEditMode(LayoutMode.None);
        }

        void InitializeLayoutUI(VisualElement uiRoot)
        {
            layoutUI = new(uiRoot);

            layoutUI.OnClickTransButton += () =>
            {
                ChangeEditMode(LayoutMode.Trans);
            };

            layoutUI.OnClickRotateButton += () =>
            {
                ChangeEditMode(LayoutMode.Rot);
            };

            layoutUI.OnClickScaleButton += () =>
            {
                ChangeEditMode(LayoutMode.Scale);
            };

            layoutUI.OnClickDeleteButton += () =>
            {
                ChangeEditMode(LayoutMode.None);
                layoutUI.ReleaseTarget();
                layoutUI.Show(false);

                if (importedIFCList.Contains(currentLoadIfcObject))
                {
                    importedIFCList.Remove(currentLoadIfcObject);
                }
                GameObject.DestroyImmediate(currentLoadIfcObject, true);
                currentLoadIfcObject = null;
                ui.Show(true);
            };

            layoutUI.OnClickOKButton += () =>
            {
                ChangeEditMode(LayoutMode.None);
                layoutUI.ReleaseTarget();
                layoutUI.Show(false);
                importedIFCList.Add(currentLoadIfcObject);
                currentLoadIfcObject = null;
                ui.Show(true);
            };
        }


        private void SetInstanceTransformFromInputState(GameObject instance, float lat, float lon, int height, int yRot)
        {
            // latlon,heightからunity座標系を取得する
            var geoCoordinate = new GeoCoordinate(lat, lon, 0f);
            PlateauVector3d plateauPosition = instancedCityModel.GeoReference.Project(geoCoordinate);
            var unityPosition = new Vector3((float)plateauPosition.X, (float)plateauPosition.Y, (float)plateauPosition.Z);

            // heightは地面高さ+uiのheightがy座標になる
            if (TryGetColliderHeight(unityPosition, out var colliderHeight))
            {
                unityPosition.y = colliderHeight + height;
            }

            instance.transform.SetPositionAndRotation(
                unityPosition,
                Quaternion.Euler(0f, yRot, 0f)
            );
        }


        /// <summary>
        /// PlateauSandboxPrefabPlacementから拾ってきました。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="colliderHeight"></param>
        /// <returns></returns>
        private bool TryGetColliderHeight(Vector3 position, out float colliderHeight)
        {
            var rayStartPosition = new Vector3(position.x, K_GroundCheckLength, position.z);
            float rayDistance = K_GroundCheckLength * 2;

            // Send a ray downward to get the height of the collider.
            var ray = new Ray(rayStartPosition, Vector3.down);

            var hitPointHeights = new List<float>();
            RaycastHit[] results = Physics.RaycastAll(ray, rayDistance);
            foreach (RaycastHit rayCastHit in results)
            {
                if (rayCastHit.transform.TryGetComponent(out PLATEAUCityObjectGroup cityObjectGroup))
                {
                    if (cityObjectGroup.CityObjects.rootCityObjects.Any(o => o.CityObjectType == PLATEAU.CityGML.CityObjectType.COT_Building))
                    {
                        // 建物であればスキップ
                        continue;
                    }

                    // その他のオブジェクトは配置可能
                    hitPointHeights.Add(rayCastHit.point.y);
                }
            }

            if (0 < hitPointHeights.Count)
            {
                // 一番上にヒットしたコライダーの高さを取得
                colliderHeight = hitPointHeights.Max();
                return true;
            }

            // Not found.
            colliderHeight = 0.0f;
            return false;
        }


        /// <summary>
        /// カメラの座標から前方向にrayを飛ばした結果最初に当ったobjectの当った位置を返す
        /// </summary>
        /// <returns>当った位置。何もあたらない場合はnull</returns>
        private Vector3? UpdateCameraRayTargetPosition()
        {
            Transform cam = Camera.main.transform;
            var result = Physics.RaycastAll(cam.position, cam.forward, float.MaxValue);
            if (result.Length < 1)
            {
                Debug.LogWarning("non hitting...");
                return null;
            }

            return result[0].point;
        }

        private void UpdateUILatLon(Vector3? hitPos)
        {
            GeoCoordinate coord = default;
            if (hitPos != null)
            {
                // 何かにあたった
                coord = instancedCityModel.GeoReference.Unproject(
                    new PlateauVector3d(
                        hitPos.Value.x,
                        hitPos.Value.y,
                        hitPos.Value.z
                    )
                );

            }
            else
            {
                coord = instancedCityModel.GeoReference.Unproject(
                    new PlateauVector3d(0, 0, 0)
                );
            }

            ui.LatitudeValue = (float)coord.Latitude;
            ui.LongitudeValue = (float)coord.Longitude;
            ui.HeightSliderValue = (int)coord.Height; // でいいのか？
        }


        public void OnDisable()
        {
            BIMLoader.Instance.Dispose();

            foreach (var mesh in importedColliderMeshes)
            {
                GameObject.DestroyImmediate(mesh);
            }
            importedColliderMeshes.Clear();
        }

        public void OnEnable()
        {
        }

        public void Start()
        {
            instancedCityModel = GameObject.FindFirstObjectByType<PLATEAUInstancedCityModel>(FindObjectsInactive.Include);
        }

        public void Update(float deltaTime)
        {
            var currentUIStatus = IsRootUIVisible;
            if (!uiStatus && currentUIStatus)
            {
                // 非表示 -> 表示
                ui.Show(true);
                var hitPos = UpdateCameraRayTargetPosition();
                UpdateUILatLon(hitPos);
            }

            ui.Update(deltaTime);

            if (ui.IsShow())
            {
                // ボタンクリック時
                if (Input.GetMouseButtonDown(0))
                {
                    var cam = Camera.main;
                    var ray = cam.ScreenPointToRay(Input.mousePosition);
                    var result = Physics.RaycastAll(ray.origin, ray.direction, float.MaxValue);
                    if (0 < result.Length)
                    {
                        foreach (var hit in result)
                        {
                            var go = hit.collider.gameObject;
                            if (importedIFCList.Contains(go))
                            {
                                if (currentLoadIfcObject != go)
                                {
                                    SetupEditMode(go);
                                    break;
                                }
                            }
                        }

                        if (currentLoadIfcObject == null)
                        {
                            if (!EventSystem.current.IsPointerOverGameObject())
                            {
                                UpdateUILatLon(result[0].point);
                            }
                        }

                    }
                }
            }


            if (layoutUI.IsShow)
            {
                var isEditingAssetTRS =
                    editMode.RuntimeTransformHandleScript != null &&
                    editMode.RuntimeTransformHandleScript.isDragging;
                CameraMoveByUserInput.IsCameraMoveActive = !isEditingAssetTRS;

                if (currentLoadIfcObject != null)
                {
                    layoutUI.Update(deltaTime);
                }

            }

            if (uiStatus && !currentUIStatus)
            {
                // 表示 -> 非表示

                if (layoutUI.IsShow)
                {
                    CameraMoveByUserInput.IsCameraMoveActive = true;
                    SetupLoadMode();
                }
                if (ui.IsShow())
                {
                    ui.Show(false);
                }
                // 読み込んだobjectは開放
                currentLoadIfcObject = null;
            }


            uiStatus = currentUIStatus;
        }

        public void LateUpdate(float deltaTime)
        {
        }

        void SetupLoadMode()
        {
            ui.Show(true);
            ChangeEditMode(LayoutMode.None);
            layoutUI.ReleaseTarget();
            layoutUI.Show(false);
        }


        void SetupEditMode(GameObject target)
        {
            currentLoadIfcObject = target;
            ChangeEditMode(LayoutMode.Trans);

            ui.Show(false);
            layoutUI.SetTarget(target);
            layoutUI.Show(true);

        }

        void ChangeEditMode(LayoutMode mode)
        {
            TransformType type = TransformType.Position;
            switch (mode)
            {
                case LayoutMode.Trans:
                    type = TransformType.Position;
                    layoutUI.TRSToggleState = BIMLayoutUI.TRSToggle.Transform;
                    break;
                case LayoutMode.Rot:
                    type = TransformType.Rotation;
                    layoutUI.TRSToggleState = BIMLayoutUI.TRSToggle.Rotate;
                    break;
                case LayoutMode.Scale:
                    type = TransformType.Scale;
                    layoutUI.TRSToggleState = BIMLayoutUI.TRSToggle.Scale;
                    break;
                default:
                    editMode?.OnCancel();
                    return;
            }

            layoutMode = mode;
            if (currentLoadIfcObject != null)
            {
                editMode?.CreateRuntimeHandle(currentLoadIfcObject, type);
            }
        }

        private Bounds? GetLargestBounds(GameObject gameObject)
        {
            if (gameObject == null)
            {
                Debug.LogWarning("GameObject が null です。");
                return null;
            }

            // 子オブジェクトを含むすべての Renderer を取得
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();

            if (renderers.Length <= 0)
            {
                Debug.LogWarning("Renderer が見つかりません。");
                return null;
            }

            // 最初の Renderer で初期 Bounds を設定
            Bounds combinedBounds = renderers[0].bounds;

            // すべての Renderer の Bounds を統合
            foreach (var renderer in renderers)
            {
                combinedBounds.Encapsulate(renderer.bounds);
            }

            return combinedBounds;
        }

        private Mesh CombineIFCMesh(GameObject ifc)
        {
            // 子オブジェクトから MeshFilter を取得
            MeshFilter[] meshFilters = ifc.GetComponentsInChildren<MeshFilter>();

            if (meshFilters.Length == 0)
            {
                Debug.LogWarning("結合する MeshFilter が見つかりません");
                return null;
            }

            // `parent` の Transform 情報を取得
            Transform parentTransform = ifc.transform;
            Matrix4x4 parentMatrix = parentTransform.worldToLocalMatrix; // ワールドからローカルへの変換

            // CombineInstance 配列を作成
            CombineInstance[] combineInstances = new CombineInstance[meshFilters.Length];

            for (int i = 0; i < meshFilters.Length; i++)
            {
                if (meshFilters[i].sharedMesh == null)
                {
                    continue;
                }

                Transform t = meshFilters[i].transform;

                // 各子オブジェクトの `TRS` を `parent` のローカル空間に変換
                Matrix4x4 localMatrix = parentMatrix * Matrix4x4.TRS(t.position, t.rotation, t.lossyScale);

                combineInstances[i] = new CombineInstance
                {
                    mesh = meshFilters[i].sharedMesh,
                    transform = localMatrix
                };
            }

            Mesh combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(combineInstances, true, true);

            return combinedMesh;
        }
    }


}
