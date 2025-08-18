namespace Landscape2.Runtime
{
    /// <summary>
    /// UI全体のマウスオーバー状態を管理する静的クラス
    /// </summary>
    public static class UIStateManager
    {
        /// <summary>
        /// いずれかのUI上にマウスがあるかどうか（汎用）
        /// </summary>
        public static bool IsMouseOverUI { get; set; } = false;
        
        /// <summary>
        /// GlobalNavi上にマウスがあるかどうか
        /// </summary>
        public static bool IsMouseOverGlobalNavi { get; set; } = false;
        
        /// <summary>
        /// FooterNavi上にマウスがあるかどうか
        /// </summary>
        public static bool IsMouseOverFooterNavi { get; set; } = false;
        
        /// <summary>
        /// アセット配置UI上にマウスがあるかどうか
        /// </summary>
        public static bool IsMouseOverAssetUI { get; set; } = false;

        /// <summary>
        /// いずれかのUI上にマウスがあるかどうかの統合チェック
        /// </summary>
        public static bool IsMouseOverAnyUI =>
            IsMouseOverUI ||
            IsMouseOverGlobalNavi ||
            IsMouseOverFooterNavi ||
            IsMouseOverAssetUI;
        
        /// <summary>
        /// 全てのフラグをリセット
        /// </summary>
        public static void ResetAll()
        {
            IsMouseOverUI = false;
            IsMouseOverGlobalNavi = false;
            IsMouseOverFooterNavi = false;
            IsMouseOverAssetUI = false;
        }
    }
}