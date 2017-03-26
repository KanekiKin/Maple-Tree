using System;
using System.Diagnostics;
using System.IO;
using MapleLib;
using MapleLib.Common;

namespace Updater
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.Title = @"MapleSeed Updater";

            if (args.Length <= 0)
                return;

            var msPath = args[0];
            if (!File.Exists(msPath))
                return;

            KillMapleSeed(msPath);
            Replace(msPath, Backup(msPath));

            Toolbelt.StartProcess(msPath, "", Path.GetDirectoryName(msPath)).Wait();
        }

        private static void Replace(string msPath, string newLoc)
        {
            if (string.IsNullOrEmpty(msPath) || string.IsNullOrEmpty(newLoc))
                return;

            File.Move(newLoc, msPath);
        }

        private static string Backup(string msPath)
        {
            var filename = Path.GetFileName(msPath);
            if (string.IsNullOrEmpty(filename))
                return string.Empty;

            if (!Directory.Exists(Settings.ConfigDirectory))
                Directory.CreateDirectory(Settings.ConfigDirectory);

            var newLoc = Path.Combine(Settings.ConfigDirectory, $"{filename}.bak");
            File.Move(msPath, newLoc);
            return newLoc;
        }

        private static void KillMapleSeed(string msPath)
        {
            var filename = Path.GetFileNameWithoutExtension(msPath);
            var processes = Process.GetProcessesByName(filename);
            foreach (var process in processes) process.Kill();
        }
    }
}