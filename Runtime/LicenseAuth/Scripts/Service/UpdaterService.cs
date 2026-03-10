using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.Diagnostics;

namespace Landscape2.Runtime.LicenseAuth
{
    public class UpdaterService
    {
        public event Action<float> OnProgressChanged;

        public event Action OnDownloadCompleted;

        public event Action<string> OnError;

        private readonly string downloadUrl;
        private readonly string savePath;

        public UpdaterService(string url)
        {
            downloadUrl = url;

            // 同じファイル名で永続化パスに保存
            savePath = Path.Combine(Application.persistentDataPath, GetSafeFileNameFromUrl(url));

            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }
        }

        public async Task StartUpdateAsync()
        {
            using var request = UnityWebRequest.Get(downloadUrl);
            request.downloadHandler = new DownloadHandlerFile(savePath) { removeFileOnAbort = true };
            request.timeout = 60;

            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                OnProgressChanged?.Invoke(operation.progress);
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                OnProgressChanged?.Invoke(1f);
                OnDownloadCompleted?.Invoke();
            }
            else
            {
                OnError?.Invoke(request.error);
            }
        }

        public void LaunchInstaller()
        {
            try
            {
                var safePath = RewriteToTemp(savePath); // MoTW を削除した一時ファイルに置き換え

                var psi = new ProcessStartInfo
                {
                    FileName = safePath,
                    Arguments = "/silent /restartapps /forcecloseapplications",
                    UseShellExecute = true,
                    Verb = "runas"
                };

                Process.Start(psi);

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false; // エディタ実行を停止
#else
                Application.Quit();
#endif
            }
            catch (Exception ex)
            {
                // インストール中のキャンセルなど。特に何もしない。
            }
        }

        // ダウンロードパスにクエリ文字列があると保存に失敗するため安全なファイル名を取得
        private static string GetSafeFileNameFromUrl(string url)
        {
            var uri = new Uri(url);
            var name = Path.GetFileName(Uri.UnescapeDataString(uri.LocalPath)); // ← ? 以降は除外される
            var invalid = Path.GetInvalidFileNameChars();
            name = string.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
            return string.IsNullOrEmpty(name) ? "download.bin" : name;
        }

        public static string RewriteToTemp(string sourcePath)
        {
            var name = Path.GetFileName(sourcePath);
            var tempPath = Path.Combine(Path.GetTempPath(), name);

            using (var src = File.Open(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var dst = File.Create(tempPath)) // 新規作成で書き直し
            {
                src.CopyTo(dst);
            }

            return tempPath;
        }
    }
}