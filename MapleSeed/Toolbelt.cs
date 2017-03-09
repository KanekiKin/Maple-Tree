// Project: MapleSeed
// File: Toolbelt.cs
// Updated By: Jared
// 

#region usings

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using libWiiSharp;
using MapleLib;
using MapleSeed.Properties;
using NUS_Downloader;

#endregion

namespace MapleSeed
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
                string dir = Settings.Instance.CemuDirectory;

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

                if (fi.Extension == ".wud" || fi.Extension == ".wux") files = new[] { gamePath };
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
            TextLog.MesgLog.WriteLog(msg + '\n', color);
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

        public static void CDecrypt(string tdir)
        {
            try {
                var cdecrypt = Path.Combine(tdir, "CDecrypt.exe");
                var libeay32 = Path.Combine(tdir, "libeay32.dll");
                var msvcr120d = Path.Combine(tdir, "msvcr120d.dll");

                if (!GZip.Decompress(Resources.CDecrypt, cdecrypt))
                    AppendLog("Error decrypting contents!\r\n       Could not extract CDecrypt.");

                if (!GZip.Decompress(Resources.libeay32, libeay32))
                    AppendLog("Error decrypting contents!\r\n       Could not extract libeay32.");

                if (!GZip.Decompress(Resources.msvcr120d, msvcr120d))
                    AppendLog("Error decrypting contents!\r\n       Could not extract msvcr120d.");

                var cdecryptP = new Process
                {
                    StartInfo =
                    {
                        FileName = cdecrypt,
                        Arguments = "tmd cetk",
                        WorkingDirectory = tdir,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        UseShellExecute = false
                    }
                };

                cdecryptP.Start();
                while (!cdecryptP.StandardOutput.EndOfStream) {
                    cdecryptP.StandardOutput.ReadLine();
                    AppendLog(cdecryptP.StandardOutput.ReadLine());
                    Application.DoEvents();
                }
                cdecryptP.WaitForExit();
                cdecryptP.Dispose();

                AppendLog("Finished decrypting contents.");
            }
            catch (Exception ex) {
                AppendLog("Error decrypting contents!\r\n" + ex.Message);
            }
        }
    }
}