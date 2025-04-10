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
            Client.Pack($"Packages/{PACKAGE_NAME}", destDir);
        }
    }
}
