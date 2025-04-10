using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Landscape2.Runtime
{
    public class BIMImportBuildHook : IPreprocessBuildWithReport
    {
        public int callbackOrder => 1;

        private readonly string ifcExeName = "IfcConvert";
        private readonly string ifcPath = Path.Combine(Application.dataPath, "IfcConvert");

        /// <summary>
        /// build前にifcconverterの有無をvalidationにかける
        /// </summary>
        /// <param name="report"></param>
        /// <exception cref="BuildFailedException"></exception>
        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log($"{nameof(BIMImportBuildHook)}.OnPreprocessBuild");
            // フォルダが無いとダメ!
            if (!Directory.Exists(ifcPath))
            {
                Directory.CreateDirectory(ifcPath);
                Debug.Log($"IfcConverterフォルダを作成しました: {ifcPath}");
            }

            Debug.Log($"pass ifcPath : {ifcPath}");

            // 実行ファイルが無いのもダメ!
            var ifcExecWithoutExt = ifcExeName;
            var ifcExec = ifcExeName;
#if UNITY_STANDALONE_WIN
            ifcExec = $"{ifcExec}.exe";
#endif
            var exeFullPath = Path.Combine(ifcPath, ifcExec);
            var resourcesExePath = Path.Combine(Application.dataPath, "Resources", "IfcConvert.exe");

            // 通常のパスとResources配下の両方を確認
            if (!File.Exists(exeFullPath) && !File.Exists(resourcesExePath))
            {
                string errMes = $"{ifcExec} がありません";
                EditorUtility.DisplayDialog("Error in BIMLoader", errMes, "cancel");
                throw new BuildFailedException(errMes);
            }

            // Resources配下に存在する場合は、そちらをコピー元として使用
            var sourcePath = File.Exists(resourcesExePath) ? resourcesExePath : exeFullPath;
            Debug.Log($"pass exeFullpath: {sourcePath}");

            // streamingassets以下にcopy
            var dstPath = Path.Combine(Application.streamingAssetsPath, ifcExecWithoutExt);
            var dstAssetPath = Path.GetRelativePath(Path.Combine(Application.dataPath, ".."), dstPath);
            if (Directory.Exists(dstPath))
            {
                // 既にあるのでcopyをしない
                return;
            }

            var relativeSourcePath = Path.GetRelativePath(Path.Combine(Application.dataPath, ".."), sourcePath);
            var result = AssetDatabase.CopyAsset(relativeSourcePath, dstAssetPath);
            Debug.Log($"copy : {result}\n{sourcePath}({relativeSourcePath})\n{dstAssetPath}");
            if (!result)
            {
                string errMes = $"{relativeSourcePath} を \n{dstPath} に\ncopyを行う際に失敗しました";
                EditorUtility.DisplayDialog("Error in BIMLoader", errMes, "cancel");
                throw new BuildFailedException(errMes);
            }
        }



    }
}
