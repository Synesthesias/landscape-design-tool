using UnityEngine.InputSystem;
using UnityEngine;
using Cinemachine;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace Landscape2.Runtime
{
    public class WalkerMoveByUserInput : LandscapeInputActions.IWalkerMoveActions, ISubComponent
    {
        private readonly CinemachineVirtualCamera camera;
        private readonly GameObject mainCam;
        public static bool IsActive = false;
        private GameObject walker;
        private LandscapeInputActions.WalkerMoveActions input;
        private Vector2 deltaWASD;
        private float deltaUpDown;
        private float deltaWheel;

        private bool isRightClicking = false;
        private CameraMoveData cameraMoveSpeedData;
        private bool enableGravity;

        public WalkerMoveByUserInput(CinemachineVirtualCamera camera, GameObject walker, bool enableGravity = true)
        {
            this.camera = camera;
            this.walker = walker;
            this.mainCam = Camera.main.gameObject;
            this.enableGravity = enableGravity;
        }
        public void OnEnable()
        {
            input = new LandscapeInputActions.WalkerMoveActions(new LandscapeInputActions());
            input.SetCallbacks(this);
            input.Enable();
        }

        public void OnDisable()
        {
            input.Disable();
        }

        public void Start()
        {
            cameraMoveSpeedData = Resources.Load<CameraMoveData>("CameraMoveSpeedData");
        }

        public void Update(float deltaTime)
        {
            var transposer = camera.GetCinemachineComponent<CinemachineTransposer>();
            // 重力による落下の制御
            if (enableGravity)
            {
                walker.GetComponent<CharacterController>().Move(cameraMoveSpeedData.walkerMoveSpeed * deltaTime * Vector3.down * 9.8f);
            }
            if (IsActive)
            {
                MoveUpDown(cameraMoveSpeedData.walkerOffsetYSpeed * deltaUpDown * deltaTime, transposer);
                MoveWASD(cameraMoveSpeedData.walkerMoveSpeed * deltaTime * deltaWASD);
                MoveForward(cameraMoveSpeedData.walkerMoveSpeed * 0.01f * deltaWheel);

                var deltaMouseXY = Mouse.current.delta.ReadValue();
                RotateCamera(cameraMoveSpeedData.rotateSpeed * deltaMouseXY);
            }
        }

        private void RotateCamera(Vector2 rotationDelta)
        {
            if (isRightClicking == false)
                return;

            var newAngles = camera.transform.eulerAngles;
            newAngles.x -= rotationDelta.y;
            newAngles.y += rotationDelta.x;
            newAngles.z = 0f;
            camera.transform.eulerAngles = newAngles;
        }

        private void MoveForward(float walkerMoveDelta)
        {
            // 前後移動は元のまま（制限なし）
            var dir = new Vector3(0f, 0f, walkerMoveDelta);
            var rot = mainCam.transform.eulerAngles;
            dir = Quaternion.Euler(new Vector3(0.0f, rot.y, rot.z)) * dir;
            walker.GetComponent<CharacterController>().Move(dir);
        }

        /// <summary>
        /// InputActionsからカメラのWASD移動のキーボード操作を受け取り、歩行者カメラを移動します。
        /// </summary>
        /// <param name="context"></param>
        public void OnWASD(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                var delta = context.ReadValue<Vector2>();
                deltaWASD = delta;
            }
            else if (context.canceled)
            {
                deltaWASD = Vector2.zero;
            }
        }

        /// <summary>
        /// InputActionsからマウスのホイール操作を受け取り、歩行者カメラを移動します。
        /// </summary>
        /// <param name="context"></param>
        public void OnWheel(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                var delta = context.ReadValue<float>();
                deltaWheel = delta;
            }
            else if (context.canceled)
            {
                deltaWheel = 0f;
            }
        }

        /// <summary>
        /// InputActionsからカメラのEQキーボード入力を受け取り、歩行者カメラの高度を変更します。
        /// </summary>
        /// <param name="context"></param>
        public void OnUpDown(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                var delta = context.ReadValue<float>();
                deltaUpDown = delta;
            }
            else if (context.canceled)
            {
                deltaUpDown = 0f;
            }
        }

        /// <summary>
        /// InputActionsから右クリックを受け取り、右クリックされているときのみ歩行者カメラを回転できるようにする。
        /// </summary>
        /// <param name="context"></param>
        public void OnRotateCameraByMouse(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                isRightClicking = true;
            }
            else if (context.canceled)
            {
                isRightClicking = false;
            }
        }

        /// <summary>
        /// 歩行者カメラ移動
        /// </summary>
        /// <param name="moveDelta"></param>
        public void MoveWASD(Vector2 moveDelta)
        {
            // 左右移動を少し遅くする
            Vector2 adjustedMoveDelta = moveDelta;
            adjustedMoveDelta.x *= 0.5f; // 左右移動を遅く
            
            var dir = new Vector3(adjustedMoveDelta.x, 0.0f, adjustedMoveDelta.y);
            var rot = mainCam.transform.eulerAngles;
            dir = Quaternion.Euler(new Vector3(0.0f, rot.y, rot.z)) * dir;
            walker.GetComponent<CharacterController>().Move(-dir);
        }

        /// <summary>
        /// 歩行者カメラ高さ変更
        /// </summary>
        /// <param name="moveDelta"></param>
        /// <param name="trans"></param>
        private void MoveUpDown(float moveDelta, CinemachineTransposer trans)
        {
            if (trans != null)
            {
                Vector3 currentOffset = trans.m_FollowOffset;
                currentOffset.y += moveDelta;
                if (currentOffset.y < 0.5f)
                {
                    ResetOffsetY(trans);
                }
                else
                {
                    trans.m_FollowOffset = currentOffset;
                }
            }
            else
            {
                Debug.LogError("CinemachineTransposer component not found");
            }
        }

        /// <summary>
        /// 歩行者カメラ高さリセット
        /// </summary>
        /// <param name="trans"></param>
        private void ResetOffsetY(CinemachineTransposer trans)
        {
            if (trans != null)
            {
                Vector3 currentOffset = trans.m_FollowOffset;
                currentOffset.y = 1.5f;
                trans.m_FollowOffset = currentOffset;
            }
            else
            {
                Debug.LogError("CinemachineTransposer component not found");
            }
        }

        public async void AddRotateWithDuration(Vector2 rotationDelta)
        {
            const float duration = 0.2f; // 移動時間

            var startAngles = camera.transform.eulerAngles;
            var newAngles = camera.transform.eulerAngles;
            newAngles.x -= rotationDelta.y;
            newAngles.y += rotationDelta.x;
            newAngles.z = 0f;

            var elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                camera.transform.rotation = Quaternion.Lerp(Quaternion.Euler(startAngles), Quaternion.Euler(newAngles), elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                await Task.Yield();
            }
            camera.transform.eulerAngles = newAngles;
        }

        public Transform GetWalkerCameraTransform()
        {
            return camera.transform;
        }

        public void LateUpdate(float deltaTime)
        {
        }

    }
}
