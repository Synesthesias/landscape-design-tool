using UnityEngine;
using System.Collections.Generic;

namespace Landscape2.Runtime
{
    /// <summary>
    /// 入力システムのフォーカス制御を統一管理するインターフェース
    /// </summary>
    public interface IInputFocusHandler
    {
        void EnableInput();
        void DisableInput();
    }

    /// <summary>
    /// マウスフォーカス状態に応じて全ての入力システムを制御するマネージャー
    /// </summary>
    public static class InputFocusManager
    {
        private static List<IInputFocusHandler> handlers = new List<IInputFocusHandler>();
        private static MouseFocusMonitor mouseFocusMonitor;

        static InputFocusManager()
        {
            // MouseFocusMonitorを作成してマウスフォーカスを監視
            CreateMouseFocusMonitor();
        }

        /// <summary>
        /// MouseFocusMonitorを作成
        /// </summary>
        private static void CreateMouseFocusMonitor()
        {
            if (mouseFocusMonitor == null)
            {
                GameObject monitorObject = new GameObject("MouseFocusMonitor");
                Object.DontDestroyOnLoad(monitorObject);
                mouseFocusMonitor = monitorObject.AddComponent<MouseFocusMonitor>();
                mouseFocusMonitor.OnMouseFocusChanged += OnMouseFocusChanged;
            }
        }

        /// <summary>
        /// フォーカスハンドラーを登録
        /// </summary>
        /// <param name="handler">登録するハンドラー</param>
        public static void RegisterHandler(IInputFocusHandler handler)
        {
            if (handler != null && !handlers.Contains(handler))
            {
                handlers.Add(handler);
            }
        }

        /// <summary>
        /// フォーカスハンドラーの登録を解除
        /// </summary>
        /// <param name="handler">解除するハンドラー</param>
        public static void UnregisterHandler(IInputFocusHandler handler)
        {
            if (handler != null)
            {
                handlers.Remove(handler);
            }
        }

        /// <summary>
        /// マウスフォーカス変更時のコールバック
        /// </summary>
        /// <param name="isMouseInWindow">マウスがウィンドウ内にあるかどうか</param>
        private static void OnMouseFocusChanged(bool isMouseInWindow)
        {
            // リストのコピーを作成してイテレーション中の変更に対応
            var handlersCopy = new List<IInputFocusHandler>(handlers);
            
            foreach (var handler in handlersCopy)
            {
                try
                {
                    if (isMouseInWindow)
                    {
                        handler.EnableInput();
                    }
                    else
                    {
                        handler.DisableInput();
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"InputFocusManager: Error handling mouse focus change for {handler.GetType()}: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// InputAction用のフォーカスハンドラー実装
    /// </summary>
    public class InputActionFocusHandler : IInputFocusHandler
    {
        private readonly System.Action enableAction;
        private readonly System.Action disableAction;

        /// <summary>
        /// InputActionFocusHandlerのコンストラクタ
        /// </summary>
        /// <param name="enable">入力有効化時の処理</param>
        /// <param name="disable">入力無効化時の処理</param>
        public InputActionFocusHandler(System.Action enable, System.Action disable)
        {
            enableAction = enable;
            disableAction = disable;
        }

        public void EnableInput()
        {
            enableAction?.Invoke();
        }

        public void DisableInput()
        {
            disableAction?.Invoke();
        }
    }

#if ENABLE_INPUT_SYSTEM
    /// <summary>
    /// PlayerInput用のフォーカスハンドラー実装
    /// </summary>
    public class PlayerInputFocusHandler : IInputFocusHandler
    {
        private readonly UnityEngine.InputSystem.PlayerInput playerInput;

        /// <summary>
        /// PlayerInputFocusHandlerのコンストラクタ
        /// </summary>
        /// <param name="playerInput">制御対象のPlayerInput</param>
        public PlayerInputFocusHandler(UnityEngine.InputSystem.PlayerInput playerInput)
        {
            this.playerInput = playerInput;
        }

        public void EnableInput()
        {
            if (playerInput != null)
            {
                playerInput.enabled = true;
            }
        }

        public void DisableInput()
        {
            if (playerInput != null)
            {
                playerInput.enabled = false;
            }
        }
    }
#endif

    /// <summary>
    /// マウスフォーカスを監視するMonoBehaviour
    /// </summary>
    public class MouseFocusMonitor : MonoBehaviour
    {
        private bool isMouseInWindow;
        private int lastScreenWidth;
        private int lastScreenHeight;
        
        /// <summary>
        /// マウスフォーカス変更時のイベント
        /// </summary>
        public System.Action<bool> OnMouseFocusChanged;

        private void Start()
        {
            // 初期化時に実際のマウス位置で状態を決定
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            UpdateMouseInWindowState();
        }

        private void Update()
        {
            bool wasInWindow = isMouseInWindow;
            
            // 画面サイズ変化検出
            if (lastScreenWidth != Screen.width || lastScreenHeight != Screen.height)
            {
                lastScreenWidth = Screen.width;
                lastScreenHeight = Screen.height;
            }
            
            // マウス位置を基に状態を更新
            UpdateMouseInWindowState();
            
            // 状態が変化した場合のみイベント発火
            if (wasInWindow != isMouseInWindow)
            {
                OnMouseFocusChanged?.Invoke(isMouseInWindow);
            }
        }

        /// <summary>
        /// マウスがウィンドウ内にあるかどうかの状態を更新
        /// </summary>
        private void UpdateMouseInWindowState()
        {
            Vector3 mousePos = Input.mousePosition;
            
            // マウスがスクリーン範囲内にあるかチェック
            isMouseInWindow = mousePos.x >= 0 && mousePos.x < Screen.width && 
                             mousePos.y >= 0 && mousePos.y < Screen.height;
        }
    }
}