using Cinemachine;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Landscape2.Runtime.Common;

namespace Landscape2.Runtime
{
    public class LandscapeCamera
    {

        private CinemachineVirtualCamera vcam1;
        private CinemachineVirtualCamera vcam2;
        private GameObject walker;
        private RaycastHit hit;
        
        // デフォルトのカメラ設定を保持
        private (Vector3, Quaternion) vcam1Defaults;
        private (Vector3, Quaternion) vcam2Defaults;

        public LandscapeCameraState cameraState { get; private set; }
        public event Action OnSetCameraCalled;

        public LandscapeCamera(CinemachineVirtualCamera vcam1, CinemachineVirtualCamera vcam2, GameObject walker)
        {
            cameraState = LandscapeCameraState.PointOfView;
            this.vcam1 = vcam1;
            this.vcam2 = vcam2;
            this.walker = walker;
            
            // 初期のデフォルト値を保存
            SetCameraDefaults();
            
            SwitchCamera(vcam1, vcam2);
        }

        /// <summary>
        /// カメラの状態を取得する
        /// </summary>
        /// <returns></returns>
        public LandscapeCameraState GetCameraState()
        {
            return cameraState;
        }
        
        /// <summary>
        /// カメラのデフォルト値を設定する
        /// </summary>
        public void SetCameraDefaults()
        {
            // vcam1のデフォルト値を保存
            vcam1Defaults = (vcam1.transform.position, vcam1.transform.rotation);
            
            // vcam2のデフォルト値を保存
            vcam2Defaults = (vcam2.transform.position, vcam2.transform.rotation);
        }

        /// <summary>
        /// 歩行者視点カメラのPositionを変更する
        /// </summary>
        /// <param name="pos"></param>
        public void SetWalkerPos(Vector3 pos)
        {
            var cc = walker.GetComponent<CharacterController>();
            cc.enabled = false;
            walker.transform.position = pos;
            cc.enabled = true;
        }

        /// <summary>
        /// 歩行者視点カメラのPositionを取得する
        /// </summary>
        /// <returns></returns>
        public Vector3 GetWalkerPos()
        {
            return walker.transform.position;
        }

        /// <summary>
        /// カメラの状態を変更する
        /// </summary>
        /// <param name="camState"></param>
        public void SetCameraState(LandscapeCameraState camState)
        {
            cameraState = camState;

            if (camState != LandscapeCameraState.Walker)
            {
                CameraMoveByUserInput.IsKeyboardActive = true;
                CameraMoveByUserInput.IsMouseActive = true;
                // WalkerMoveByUserInput.IsActive = false;
                SwitchCamera(vcam1, vcam2);
            }
            else
            {
                CameraMoveByUserInput.IsKeyboardActive = false;
                CameraMoveByUserInput.IsMouseActive = false;
                // WalkerMoveByUserInput.IsActive = true;
                SwitchCamera(vcam2, vcam1);
            }
            OnSetCameraCalled?.Invoke();
        }

        /// <summary>
        /// CinemachineVirtualCameraを切り替える
        /// </summary>
        /// <param name="activeCamera"></param>
        /// <param name="inactiveCamera"></param>
        private void SwitchCamera(CinemachineVirtualCamera activeCamera, CinemachineVirtualCamera inactiveCamera)
        {
            activeCamera.Priority = 10;
            inactiveCamera.Priority = 0;
        }

        /// <summary>
        /// 歩行者視点以外のカメラに切り替える
        /// </summary>
        public void SwitchView()
        {
            if (cameraState == LandscapeCameraState.PointOfView)
            {
                cameraState = LandscapeCameraState.SelectWalkPoint;
                OnSetCameraCalled?.Invoke();
                CameraMoveByUserInput.IsKeyboardActive = false;
                CameraMoveByUserInput.IsMouseActive = false;
                WalkerMoveByUserInput.IsActive = false;
            }
            else if (cameraState == LandscapeCameraState.SelectWalkPoint || cameraState == LandscapeCameraState.Walker)
            {
                cameraState = LandscapeCameraState.PointOfView;
                OnSetCameraCalled?.Invoke();
                CameraMoveByUserInput.IsKeyboardActive = true;
                CameraMoveByUserInput.IsMouseActive = true;
                WalkerMoveByUserInput.IsActive = false;
                
                // 歩行者視点から俯瞰カメラへ戻る時にカメラ位置を調整
                SetCameraWithBackwardOffset(vcam2.transform, vcam1.transform.transform.parent, vcam1Defaults);
                
                SwitchCamera(vcam1, vcam2);
            }
        }

        /// <summary>
        /// UI上にマウスがあるときカメラ操作を行わないようにする
        /// </summary>
        /// <param name="onUi"></param>
        public void OnUserInputTrigger(bool onUi)
        {
            if (onUi)
            {
                CameraMoveByUserInput.IsKeyboardActive = false;
                CameraMoveByUserInput.IsMouseActive = false;
                WalkerMoveByUserInput.IsActive = false;
            }
            else
            {
                if (cameraState == LandscapeCameraState.PointOfView)
                {
                    CameraMoveByUserInput.IsKeyboardActive = true;
                    CameraMoveByUserInput.IsMouseActive = true;
                    WalkerMoveByUserInput.IsActive = false;
                }
                else if (cameraState == LandscapeCameraState.SelectWalkPoint)
                {
                    CameraMoveByUserInput.IsKeyboardActive = false;
                    CameraMoveByUserInput.IsMouseActive = false;
                    // WalkerMoveByUserInput.IsActive = false;
                }
                else
                {
                    CameraMoveByUserInput.IsKeyboardActive = false;
                    CameraMoveByUserInput.IsMouseActive = false;
                    // WalkerMoveByUserInput.IsActive = true;
                }
            }
        }

        /// <summary>
        /// 歩行者視点カメラに切り替える
        /// </summary>
        /// <returns></returns>
        public bool SwitchWalkerView()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            // Ignore RaycastとHiddenBuildingレイヤーを除外してレイキャスト
            bool canRaycast = Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMaskUtil.GetGroundClickLayerMask());
            if (canRaycast)
            {
                SwitchCamera(vcam2, vcam1);

                var cc = walker.GetComponent<CharacterController>();
                cc.enabled = false;
                walker.transform.position = new Vector3(hit.point.x, hit.point.y + 1.0f, hit.point.z);
                //var pov = vcam2.GetCinemachineComponent<CinemachinePOV>();
                //var vcam1Euler = vcam1.transform.rotation.eulerAngles;
                //pov.m_VerticalAxis.Value = 0f;
                //pov.m_HorizontalAxis.Value = vcam1Euler.y;
                cc.enabled = true;

                // WalkerMoveByUserInput.IsActive = true;
                cameraState = LandscapeCameraState.Walker;
                OnSetCameraCalled?.Invoke();
                
                // 俯瞰カメラから歩行者選択モードへ切り替え時にカメラ位置を調整
                SetCameraWithBackwardOffset(vcam1.transform, vcam2.transform, vcam2Defaults);
            }
            return canRaycast;
        }

        /// <summary>
        /// ターゲット位置にフォーカスする
        /// </summary>
        /// <param name="target"></param>
        /// <param name="endCallback"></param>
        /// <param name="distanceMultiplier">距離の倍率</param>
        public void FocusPoint(Transform target, UnityAction endCallback = null, float distanceMultiplier = 4f)
        {
            if (cameraState != LandscapeCameraState.PointOfView)
            {
                return;
            }
            FocusPointAsync(target, distanceMultiplier, endCallback);
        }

        private async void FocusPointAsync(Transform target, float distanceMultiplier, UnityAction endCallback = null)
        {
            const float upAngle = 45f; // 斜めから見下ろす
            const float duration = 0.5f; // 移動時間

            var startRotation = vcam1.transform.rotation;
            var startPosition = vcam1.transform.position;

            // カメラの回転
            var endRotation = Quaternion.LookRotation(target.transform.position - vcam1.transform.position);
            endRotation = Quaternion.Euler(new Vector3(upAngle, endRotation.eulerAngles.y, endRotation.eulerAngles.z));

            // カメラの距離
            var distance = 12.5f * distanceMultiplier; // 元は50f。meshを持っていなくてもdistanceMultiplierを設定したかったので修正
            var renderer = GetTargetMesh(target.gameObject);
            if (renderer != null)
            {
                // メッシュのサイズからカメラの距離を決定
                distance = renderer.bounds.size.magnitude * distanceMultiplier;
            }

            // カメラの位置
            var endPosition = target.transform.position - endRotation * Vector3.forward * distance; // 一定の距離を保つ

            // カメラの移動
            var elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                vcam1.transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
                vcam1.transform.rotation = Quaternion.Lerp(startRotation, endRotation, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                await Task.Yield(); // フレームの終わりまで待機
            }
            vcam1.transform.rotation = endRotation;
            vcam1.transform.position = endPosition;

            endCallback?.Invoke();
        }

        private Renderer GetTargetMesh(GameObject target)
        {
            var renderer = target.GetComponent<Renderer>();
            if (renderer != null)
            {
                return renderer as MeshRenderer;
            }

            if (target.TryGetComponent<LODGroup>(out var meshLod))
            {
                var lods = meshLod.GetLODs();
                if (lods.Length > 0)
                {
                    var renderers = lods[0].renderers;
                    if (renderers.Length > 0)
                    {
                        return renderers[0];
                    }
                }
            }

            var rendererChildren = target.GetComponentsInChildren<Renderer>();
            if (rendererChildren.Length > 0)
            {
                return rendererChildren[0];
            }
            return null;
        }

        /// <summary>
        /// 参照元カメラの位置・回転から、後方オフセット付きでターゲットカメラコントローラーを設定します。
        /// </summary>
        /// <param name="sourceCamera">参照元のカメラTransform</param>
        /// <param name="targetCameraController">対象のカメラコントローラーTransform</param>
        /// <param name="targetDefaults">ターゲットカメラのデフォルト設定</param>
        private void SetCameraWithBackwardOffset(Transform sourceCamera, Transform targetCameraController, (Vector3, Quaternion) targetDefaults)
        {
            if (sourceCamera != null && targetCameraController != null)
            {
                // 子オブジェクトがある場合はリセット
                if (targetCameraController.childCount > 0)
                {
                    foreach (Transform child in targetCameraController)
                    {
                        child.localPosition = Vector3.zero;
                        child.localRotation = Quaternion.identity;
                    }
                }
                
                // ソースカメラから後方オフセット付きで計算
                Vector3 eulerAngles = sourceCamera.rotation.eulerAngles;
                float yRotation = eulerAngles.y; // 方向を保持
                
                Quaternion adjustedRotation = Quaternion.Euler(targetDefaults.Item2.eulerAngles.x, yRotation, 0f);
                
                // Y軸回転のみを使用して水平後方ベクトルを計算
                Quaternion yRotationOnly = Quaternion.Euler(0f, yRotation, 0f);
                Vector3 backwardDirection = yRotationOnly * Vector3.back;
                float backwardDistance = 75.0f; // 後方に下がる距離
                
                // 後方位置を反映
                Vector3 adjustedPosition = sourceCamera.position + (backwardDirection * backwardDistance);
                
                // Y座標はデフォルト高さに設定
                adjustedPosition.y = targetDefaults.Item1.y;
                
                targetCameraController.SetPositionAndRotation(adjustedPosition, adjustedRotation);
            }
            else
            {
                Debug.LogError("ソースカメラまたは対象カメラコントローラーが設定されていません。");
            }
        }
    }
}
