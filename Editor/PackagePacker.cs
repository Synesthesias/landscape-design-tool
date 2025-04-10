using PLATEAU.Util;
using UnityEditor;
using UnityEditor.PackageManager;

namespace PLATEAU.Editor.DebugPlateau
{
    /// <summary>
    /// Packagesフォルダに入っているLandscape Design Tool を tarball 形式で出力します。
    /// デプロイで利用します。
    /// </summary>
    static class PackagePacker
    {
        private const string PACKAGE_NAME = "com.synesthesias.landscape-design-tool";
        [MenuItem("PLATEAU/Debug/Pack Landscape Package to tarball")]
        public static void PackLandScape()
        {
            string destDir = EditorUtility.SaveFolderPanel("出力先", "", "");
            if (string.IsNullOrEmpty(destDir))
            {
                // ユーザーがキャンセルした場合
                return;
            }
            
            try
            {
                Client.Pack($"Packages/{PACKAGE_NAME}", destDir);
                EditorUtility.DisplayDialog("成功", $"パッケージが正常に出力されました: {destDir}", "OK");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("エラー", $"パッケージの出力中にエラーが発生しました: {e.Message}", "OK");
                Debug.LogError($"パッケージング中のエラー: {e}");
            }
        }
    }
}
