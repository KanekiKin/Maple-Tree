// Project: MapleSeed
// File: Settings.cs
// Updated By: Jared
// 

#region usings

using System;
using System.IO;
using System.Windows.Forms;
using IniParser;
using MapleLib.Common;
using MapleLib.Properties;

#endregion

namespace MapleLib
{
    public class Settings
    {
        public Settings()
        {
            if (!File.Exists(ConfigFile) || new FileInfo(ConfigFile).Length <= 0)
                File.WriteAllText(ConfigFile, Resources.Settings_DefaultSettings);
        }

        public string TitleDirectory {
            get {
                var value = GetKeyValue("TitleDirectory");

                if (!string.IsNullOrEmpty(value) && Directory.Exists(value)) return value;
                var fbd = new FolderBrowserDialog
                {
                    Description = @"Cemu Title Directory" + Environment.NewLine + @"(Where you store games)"
                };
                var result = fbd.ShowDialog();

                if (string.IsNullOrWhiteSpace(fbd.SelectedPath) || result == DialogResult.Cancel) {
                    MessageBox.Show(@"Title Directory is required. Shutting down.");
                    Application.Exit();
                }

                value = fbd.SelectedPath;
                WriteKeyValue("TitleDirectory", value);
                return value;
            }

            set { WriteKeyValue("TitleDirectory", Path.GetFullPath(value)); }
        }

        public string CemuDirectory {
            get {
                var value = GetKeyValue("CemuDirectory");

                if (!string.IsNullOrEmpty(value) && File.Exists(Path.Combine(value,"cemu.exe"))) return value;
                var ofd = new OpenFileDialog
                {
                    CheckFileExists = true,
                    Filter = @"Cemu Excutable |cemu.exe"
                };

                var result = ofd.ShowDialog();
                if (string.IsNullOrWhiteSpace(ofd.FileName) || result != DialogResult.OK) {
                    MessageBox.Show(@"Cemu Directory is required to launch titles.");
                    return string.Empty;
                }

                value = Path.GetDirectoryName(ofd.FileName);
                WriteKeyValue("CemuDirectory", value);
                return value;
            }

            set { WriteKeyValue("CemuDirectory", Path.GetFullPath(value)); }
        }

        public string Username {
            get { return GetKeyValue("Username"); }

            set { WriteKeyValue("Username", value); }
        }

        public string DiscordEmail
        {
            get
            {
                var value = GetKeyValue("DiscordEmail");
                if (string.IsNullOrEmpty(value))
                    WriteKeyValue("DiscordEmail", value = "");
                return value;
            }

            set { WriteKeyValue("DiscordEmail", value); }
        }

        public string DiscordPass
        {
            get {
                var value = GetKeyValue("DiscordPass");
                if (string.IsNullOrEmpty(value))
                    WriteKeyValue("DiscordPass", value = "");

                return Helper.DecryptStr(value);
            }

            set { WriteKeyValue("DiscordPass", Helper.EncryptStr(value)); }
        }

        public string Hub {
            get {
                var value = GetKeyValue("Hub");
                if (string.IsNullOrEmpty(value))
                    WriteKeyValue("Hub", value = "mapletree.tsumes.com");
                return value;
            }

            set { WriteKeyValue("Hub", value); }
        }

        public string Serial {
            get {
                var value = GetKeyValue("Serial");
                if (string.IsNullOrEmpty(value)) WriteKeyValue("Serial", Toolkit.UniqueID());
                return value;
            }
        }

        public bool FullScreenMode {
            get {
                var value = GetKeyValue("FullScreenMode");
                if (string.IsNullOrEmpty(value))
                    WriteKeyValue("FullScreenMode", false.ToString());

                return GetKeyValue("FullScreenMode") == "True";
            }

            set { WriteKeyValue("FullScreenMode", value.ToString()); }
        }

        public bool Cemu173Patch
        {
            get
            {
                var value = GetKeyValue("Cemu173Patch");
                if (string.IsNullOrEmpty(value))
                    WriteKeyValue("Cemu173Patch", true.ToString());

                return GetKeyValue("Cemu173Patch") == "True";
            }

            set { WriteKeyValue("Cemu173Patch", value.ToString()); }
        }

        public bool StoreEncryptedContent
        {
            get
            {
                var value = GetKeyValue("StoreEncryptedContent");
                if (string.IsNullOrEmpty(value))
                    WriteKeyValue("StoreEncryptedContent", true.ToString());

                return GetKeyValue("StoreEncryptedContent") == "True";
            }

            set { WriteKeyValue("StoreEncryptedContent", value.ToString()); }
        }

        public static Settings Instance => Toolbelt.Settings;
        private static string AppFolder => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static string ConfigFile => Path.Combine(AppFolder, "MapleSeed.ini");
        private static string ConfigName => "MapleTree";

        private string GetKeyValue(string key)
        {
            var parser = new FileIniDataParser();
            var data = parser.ReadFile(ConfigFile);
            return data[ConfigName][key] ?? "";
        }

        private void WriteKeyValue(string key, string value)
        {
            var parser = new FileIniDataParser();
            var data = parser.ReadFile(ConfigFile);
            data[ConfigName][key] = value;
            parser.WriteFile(ConfigFile, data);
        }
    }
}