using UnityEngine;
using UnityEngine.InputSystem;

namespace Landscape2.Runtime
{
    /// <summary>
    /// ユーザーの操作によってカメラを動かします。
    /// </summary>
    public class CameraMoveByUserInput : LandscapeInputActions.ICameraMoveActions, ISubComponent
    {
	    private readonly Camera camera;
	    private Vector2 horizontalMoveByKeyboard;
	    private LandscapeInputActions.CameraMoveActions input;
	    private const float MoveSpeedByKeyboard = 10f;

	    /// <summary>
	    /// キーボードでの移動を有効にするかどうかです
	    /// </summary>
	    public static bool IsKeyboardActive = true;

	    public CameraMoveByUserInput(Camera camera)
	    {
		    this.camera = camera;
	    }
	    
	    public void OnEnable()
	    {
		    // ユーザーの操作を受け取る準備
		    input = new LandscapeInputActions.CameraMoveActions(new LandscapeInputActions());
		    input.SetCallbacks(this);
		    input.Enable();
	    }

	    public void OnDisable()
	    {
		    input.Disable();
	    }
	    
		/// <summary>
		/// InputActionsからカメラ移動のキーボード操作を受け取り、カメラを移動します。
		/// </summary>
        public void OnHorizontalMoveCameraByKeyboard(InputAction.CallbackContext context)
        {
	        if (!IsKeyboardActive) return;
			if (context.performed)
			{
				var delta = context.ReadValue<Vector2>();
				horizontalMoveByKeyboard = delta;
			}else if (context.canceled)
			{
				horizontalMoveByKeyboard = Vector2.zero;
			}
		}

		public void Update(float deltaTime)
		{
			var trans = camera.transform;
			float moveFactor = MoveSpeedByKeyboard * deltaTime;
			MoveCameraHorizontal(moveFactor * horizontalMoveByKeyboard, trans);
		}
		
		/// <summary>
		/// カメラ水平移動
		/// </summary>
		private void MoveCameraHorizontal(Vector2 moveDelta, Transform cameraTrans)
		{
			var dir = new Vector3(moveDelta.x, 0.0f, moveDelta.y);
			var rotY = cameraTrans.eulerAngles.y;
			dir = Quaternion.Euler(new Vector3(0.0f, rotY, 0.0f)) * dir;
			cameraTrans.position -= dir;
		}
    }
}
