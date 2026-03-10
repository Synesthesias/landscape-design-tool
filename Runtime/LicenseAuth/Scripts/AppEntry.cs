using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.IO;
using System.Runtime.InteropServices;

using UnityEngine;

/// <summary>
/// 認証トリガーを送信するためのクラス
/// </summary>
namespace Landscape2.Runtime.LicenseAuth
{
    /// <summary>
    /// 既存プロセスにNamedPipeで認証トリガーを送信するクラス
    /// </summary>
    public static class AppEntry
    {
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;

        public static class NotifyAuthTrigger
        {
            /// <summary>
            /// 認証トリガーを送信する
            /// </summary>
            /// <param name="token"></param>
            public static void Send(string token)
            {
                try
                {
                    using var client = new NamedPipeClientStream(".", NamedPipeListener.PipeName, PipeDirection.Out);

                    client.Connect(500); // 最大500ms待つ

                    using var writer = new StreamWriter(client) { AutoFlush = true };

                    writer.WriteLine(token);

                    writer.Flush();

                    UnityEngine.Debug.Log("認証トリガー送信完了: " + token);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogWarning("認証トリガー送信失敗: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// ウィンドウを隠す
        /// </summary>
        private static void HideWindow()
        {
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        IntPtr hwnd = Process.GetCurrentProcess().MainWindowHandle;

        if (hwnd != IntPtr.Zero)
        {
            ShowWindow(hwnd, SW_HIDE);
        }
#endif
        }

#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        /// <summary>
        /// 引数がある場合、認証トリガーを送信して即時終了する
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Awake()
        {
            HideWindow(); // 即時ウィンドウを隠す

            var args = Environment.GetCommandLineArgs();

            foreach (var arg in args)
            {
                if (arg.StartsWith(AuthManager.URLScheme))
                {
                    var token = ExtractQuery(arg);

                    NotifyAuthTrigger.Send(token);

                    Application.Quit();
                }
            }
        }
#endif

        /// <summary>
        /// クエリをURIから抽出する
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        private static string ExtractQuery(string uri)
        {
            try
            {
                var parsed = new Uri(uri);

                var query = parsed.Query.TrimStart('?');

                return query;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning("トークン抽出失敗: " + e.Message);
            }

            return "";
        }
    }
}