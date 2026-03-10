using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;

/// <summary>
/// NamedPipeを使用して認証トリガーを受信するクラス
/// </summary>
namespace Landscape2.Runtime.LicenseAuth
{
    public class NamedPipeListener : MonoBehaviour
    {
        private const int SW_RESTORE = 9;

        private const int SW_SHOW = 5;

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool AllowSetForegroundWindow(int dwProcessId);

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        public const string PipeName = "keikantoolyokotemachi-pipe";

        private CancellationTokenSource cts;

        private Task listenTask;

        private NamedPipeServerStream server;

        public event Action<string> OnAuthCompleted;

        /// <summary>
        /// ウィンドウを最前面に持ってくる
        /// </summary>
        public static void BringToFront()
        {
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
            var handle = Process.GetCurrentProcess().MainWindowHandle;

            if (handle == IntPtr.Zero)  return;

            // 強制復元（最小化されていた場合）
            ShowWindow(handle, SW_RESTORE);

            // 他プロセスがフォーカス中の場合に前面化できるようにする
            var foreWnd = GetForegroundWindow();
            var foreThread = GetWindowThreadProcessId(foreWnd, IntPtr.Zero);
            var thisThread = GetWindowThreadProcessId(handle, IntPtr.Zero);

            // 入力を共有（同一スレッド扱い）して前面化を許可
            if (foreThread != thisThread)
            {
                AttachThreadInput(foreThread, thisThread, true);
                SetForegroundWindow(handle);
                AttachThreadInput(foreThread, thisThread, false);
            }
            else
            {
                SetForegroundWindow(handle);
            }
#endif
        }

        private void Start()
        {
            cts = new CancellationTokenSource();

            listenTask = StartListeningAsync(cts.Token);
        }

        private Task StartListeningAsync(CancellationToken token)
        {
            return Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    using var localServer = new NamedPipeServerStream(PipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

                    server = localServer;

                    try
                    {
                        await server.WaitForConnectionAsync(token);

                        using var reader = new StreamReader(server);

                        var message = await reader.ReadLineAsync();

                        if (!string.IsNullOrEmpty(message))
                        {
                            UnityEngine.Debug.Log($"認証メッセージ受信: {message}");

                            NamedPipeDispatcher.Enqueue(() =>
                            {
                                BringToFront();

                                OnAuthCompleted?.Invoke(message);
                            });
                        }
                        else
                        {
                            UnityEngine.Debug.LogWarning("受信したメッセージが空です。");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogWarning(ex);
                    }
                }
            });
        }

        private async void OnDestroy()
        {
            cts?.Cancel();

            if (listenTask != null)
            {
                await listenTask; // タスクが完了するのを待つ（Disposeと競合させない）
            }

            server?.Dispose();

            cts?.Dispose();
        }
    }
}