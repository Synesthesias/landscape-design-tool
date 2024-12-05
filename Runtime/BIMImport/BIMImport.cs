using PLATEAU.CityInfo;
using PLATEAU.Native;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
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


        BIMImportUI ui;

        BIMLayoutUI layoutUI;

        EditMode editMode = new();

        string filePath;

        GameObject currentLoadIfcObject;

        List<GameObject> importedIFCList = new();

        // uiの表示状態
        bool uiStatus = false;

        LayoutMode layoutMode;

        PLATEAUInstancedCityModel instancedCityModel;


        public BIMImport(VisualElement uiRoot)
        {
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

                // 配置用情報作成
                var lat = ui.LatitudeValue;
                var lon = ui.LongitudeValue;
                var height = ui.HeightSliderValue;
                var yRot = ui.YawSliderValue;


                SetInstanceTransformFromInputState(ifcObj, lat, lon, height, yRot);
                currentLoadIfcObject = ifcObj;

                // TODO: 配置できたGameObjectは
                // persistentdatapathへcopyする
                // copy先のpathとlatlon,height,yawを保持する

                ChangeEditMode(LayoutMode.Trans);

                ui.Show(false);
                layoutUI.SetTarget(ifcObj);
                layoutUI.Show(true);
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
                GameObject.DestroyImmediate(currentLoadIfcObject, true);
                ui.Show(true);
            };

            layoutUI.OnClickOKButton += () =>
            {
                ChangeEditMode(LayoutMode.None);
                layoutUI.ReleaseTarget();
                layoutUI.Show(false);
                importedIFCList.Add(currentLoadIfcObject);
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
            var currentUIStatus = ui.IsShow();
            if (!uiStatus && currentUIStatus)
            {
                // 非表示 -> 表示
                var hitPos = UpdateCameraRayTargetPosition();
                UpdateUILatLon(hitPos);
            }

            ui.Update(deltaTime);

            if (layoutUI.IsShow)
            {
                var isEditingAssetTRS = layoutUI.IsShow &&
                    editMode.RuntimeTransformHandleScript != null &&
                    editMode.RuntimeTransformHandleScript.isDragging;
                CameraMoveByUserInput.IsCameraMoveActive = !isEditingAssetTRS;

                if (currentLoadIfcObject != null)
                {
                    layoutUI.Update(deltaTime);
                }
                if (uiStatus && !currentUIStatus)
                {
                    // 表示 -> 非表示
                    CameraMoveByUserInput.IsCameraMoveActive = true;
                }

            }

            uiStatus = currentUIStatus;
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
            editMode?.CreateRuntimeHandle(currentLoadIfcObject, type);
        }
    }
}
