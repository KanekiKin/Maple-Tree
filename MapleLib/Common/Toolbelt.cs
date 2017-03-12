// Project: MapleSeed
// File: Toolbelt.cs
// Updated By: Jared
// 

#region usings

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using libWiiSharp;
using MapleLib.Properties;
using NUS_Downloader;

#endregion

namespace MapleLib.Common
{
    public static class Toolbelt
    {
        public static readonly string Version = $" - Git {Resources.version.Trim('\n')}";
        public static string Serial => Settings.Serial;
        public static Database Database { get; internal set; }
        public static Settings Settings { get; internal set; }

        public static bool LaunchCemu(string game)
        {
            try {
                string rpx = null, gamePath;
                var dir = Settings.Instance.CemuDirectory;

                if (string.IsNullOrEmpty(dir))
                    return false;

                if (game != null) {
                    gamePath = Path.Combine(Settings.TitleDirectory, game);
                }
                else {
                    RunCemu(Path.Combine(dir, "cemu.exe"), "");
                    return true;
                }

                string[] files;
                var fi = new FileInfo(game);

                if (fi.Extension == ".wud" || fi.Extension == ".wux") files = new[] {gamePath};
                else files = Directory.GetFiles(gamePath, "*.rpx", SearchOption.AllDirectories);

                if (files.Length > 0) rpx = files[0];
                var cemuPath = Path.Combine(dir, "cemu.exe");
                if (File.Exists(cemuPath) && File.Exists(rpx))
                    RunCemu(cemuPath, rpx);
                else
                    SetStatus("Could not find a valid .rpx");
            }
            catch (Exception e) {
                AppendLog($"{e.Message}\n{e.StackTrace}", Color.DarkRed);
                return false;
            }

            return true;
        }

        public static string RIC(string str)
        {
            return RemoveInvalidCharacters(str);
        }

        private static string RemoveInvalidCharacters(string str)
        {
            return
                Path.GetInvalidPathChars()
                    .Aggregate(str, (current, c) => current.Replace(c.ToString(), " "))
                    .Replace(':', ' ').Replace("(", "").Replace(")", "");
        }

        public static void AppendLog(string msg, Color color = default(Color))
        {
            TextLog.MesgLog.WriteLog(msg, color);
        }

        public static void SetStatus(string msg, Color color = default(Color))
        {
            TextLog.StatusLog.WriteLog(msg, color);
        }

        public static string SizeSuffix(long bytes)
        {
            const int scale = 1024;
            string[] orders = {"GB", "MB", "KB", "Bytes"};
            var max = (long) Math.Pow(scale, orders.Length - 1);

            foreach (var order in orders) {
                if (bytes > max)
                    return $"{decimal.Divide(bytes, max):##.##} {order}";

                max /= scale;
            }
            return "0 Bytes";
        }

        public static bool IsValid(TMD_Content content, string contentFile)
        {
            if (!File.Exists(contentFile)) return false;

            return (ulong) new FileInfo(contentFile).Length == content.Size;
        }

        private static void RunCemu(string cemuPath, string rpx)
        {
            try {
                var workingDir = Path.GetDirectoryName(cemuPath);
                if (workingDir == null) return;

                var o1 = Settings.FullScreenMode ? "-f" : "";
                var process = new Process
                {
                    StartInfo =
                    {
                        FileName = cemuPath,
                        Arguments = $"{o1} -g \"{rpx}\"",
                        WorkingDirectory = workingDir,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        UseShellExecute = false
                    }
                };

                process.Start();
                AppendLog("Started playing a game!");
            }
            catch (Exception ex) {
                AppendLog("Error!\r\n" + ex.Message);
            }
        }

        public static async Task CDecrypt(string workingDir)
        {
            try {
                var cdecrypt = Path.Combine(workingDir, "CDecrypt.exe");
                var libeay32 = Path.Combine(workingDir, "libeay32.dll");
                var msvcr120d = Path.Combine(workingDir, "msvcr120d.dll");

                if (!GZip.Decompress(Resources.CDecrypt, cdecrypt))
                    AppendLog("Error decrypting contents!\r\n       Could not extract CDecrypt.");

                if (!GZip.Decompress(Resources.libeay32, libeay32))
                    AppendLog("Error decrypting contents!\r\n       Could not extract libeay32.");

                if (!GZip.Decompress(Resources.msvcr120d, msvcr120d))
                    AppendLog("Error decrypting contents!\r\n       Could not extract msvcr120d.");

                var processes = Process.GetProcessesByName("CDecrypt");
                foreach (var proc in processes) proc.Kill();

                using (TextWriter writer = File.CreateText(Path.Combine(workingDir, "result.log"))) {
                    var args = "tmd cetk";
                    var exitCode = await StartProcess(cdecrypt, args, workingDir, null, true, false, writer);
                    TextLog.MesgLog.WriteLog($@"Process Exited with Exit Code {exitCode}!");
                }
            }
            catch (TaskCanceledException) {
                TextLog.MesgLog.WriteError(@"Process Timed Out!");
            }
            catch (Exception ex) {
                AppendLog("Error decrypting contents!\r\n" + ex.Message);
            }
        }

        private static async Task<int> StartProcess(string filename, string arguments, string workingDirectory,
            int? timeout = null, bool createNoWindow = true, bool shellEx = false, TextWriter outputTextWriter = null,
            TextWriter errorTextWriter = null)
        {
            using (var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = createNoWindow,
                    Arguments = arguments,
                    FileName = filename,
                    RedirectStandardOutput = outputTextWriter != null,
                    RedirectStandardError = errorTextWriter != null,
                    UseShellExecute = shellEx,
                    WorkingDirectory = workingDirectory
                }
            }) {
                process.Start();
                var cancellationTokenSource = timeout.HasValue
                    ? new CancellationTokenSource(timeout.Value)
                    : new CancellationTokenSource();

                var tasks = new List<Task>(2) {process.WaitForExitAsync(cancellationTokenSource.Token)};
                if (outputTextWriter != null)
                    tasks.Add(ReadAsync(
                        x => {
                            process.OutputDataReceived += x;
                            process.BeginOutputReadLine();
                        },
                        x => process.OutputDataReceived -= x,
                        outputTextWriter,
                        cancellationTokenSource.Token));

                if (errorTextWriter != null)
                    tasks.Add(ReadAsync(
                        x => {
                            process.ErrorDataReceived += x;
                            process.BeginErrorReadLine();
                        },
                        x => process.ErrorDataReceived -= x,
                        errorTextWriter,
                        cancellationTokenSource.Token));

                await Task.WhenAll(tasks);
                return process.ExitCode;
            }
        }

        /// <summary>
        ///     Waits asynchronously for the process to exit.
        /// </summary>
        /// <param name="process">The process to wait for cancellation.</param>
        /// <param name="cancellationToken">
        ///     A cancellation token. If invoked, the task will return
        ///     immediately as cancelled.
        /// </param>
        /// <returns>A Task representing waiting for the process to end.</returns>
        private static Task WaitForExitAsync(this Process process,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            process.EnableRaisingEvents = true;

            var taskCompletionSource = new TaskCompletionSource<object>();

            EventHandler handler = null;
            handler = (sender, args) => {
                process.Exited -= handler;
                taskCompletionSource.TrySetResult(null);
            };
            process.Exited += handler;

            if (cancellationToken != default(CancellationToken))
                cancellationToken.Register(
                    () => {
                        process.Exited -= handler;
                        taskCompletionSource.TrySetCanceled();
                    });

            return taskCompletionSource.Task;
        }

        /// <summary>
        ///     Reads the data from the specified data recieved event and writes it to the
        ///     <paramref name="textWriter" />.
        /// </summary>
        /// <param name="addHandler">Adds the event handler.</param>
        /// <param name="removeHandler">Removes the event handler.</param>
        /// <param name="textWriter">The text writer.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static Task ReadAsync(this Action<DataReceivedEventHandler> addHandler,
            Action<DataReceivedEventHandler> removeHandler, TextWriter textWriter,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var taskCompletionSource = new TaskCompletionSource<object>();

            DataReceivedEventHandler handler = null;
            handler = (sender, e) => {
                if (e.Data == null) {
                    removeHandler(handler);
                    taskCompletionSource.TrySetResult(null);
                }
                else {
                    textWriter.WriteLine(e.Data);
                    //TextLog.MesgLog.WriteLog(e.Data, Color.DarkSlateBlue);
                }
            };

            addHandler(handler);

            if (cancellationToken != default(CancellationToken))
                cancellationToken.Register(
                    () => {
                        removeHandler(handler);
                        taskCompletionSource.TrySetCanceled();
                    });

            return taskCompletionSource.Task;
        }
    }
}