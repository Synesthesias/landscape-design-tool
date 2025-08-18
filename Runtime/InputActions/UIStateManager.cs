namespace Landscape2.Runtime
{
    /// <summary>
    /// UI全体のマウスオーバー状態を管理する静的クラス
    /// </summary>
    public static class UIStateManager
    {
        /// <summary>
        /// いずれかのUI上にマウスがあるかどうか
        /// </summary>
        public static bool IsMouseOverUI { get; set; } = false;
        
        /// <summary>
        /// UIマウスオーバーフラグをリセット
        /// </summary>
        public static void Reset()
        {
            IsMouseOverUI = false;
        }
    }
}