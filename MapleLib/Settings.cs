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
    public static class Settings
    {
        public static string TitleDirectory {
            get { return GetKeyValue("TitleDirectory"); }

            set { WriteKeyValue("TitleDirectory", Path.GetFullPath(value)); }
        }

        public static string CemuDirectory {
            get { return GetKeyValue("CemuDirectory"); }

            set { WriteKeyValue("CemuDirectory", Path.GetFullPath(value)); }
        }

        public static string Username {
            get { return GetKeyValue("Username"); }

            set { WriteKeyValue("Username", value); }
        }

        public static string DiscordEmail {
            get {
                var value = GetKeyValue("DiscordEmail");
                if (string.IsNullOrEmpty(value))
                    WriteKeyValue("DiscordEmail", value = "");
                return value;
            }

            set { WriteKeyValue("DiscordEmail", value); }
        }

        public static string DiscordPass {
            get {
                var value = GetKeyValue("DiscordPass");
                if (string.IsNullOrEmpty(value))
                    WriteKeyValue("DiscordPass", value = "");

                return Helper.DecryptStr(value);
            }

            set { WriteKeyValue("DiscordPass", Helper.EncryptStr(value)); }
        }

        public static string Hub {
            get {
                var value = GetKeyValue("Hub");
                if (string.IsNullOrEmpty(value))
                    WriteKeyValue("Hub", value = "mapletree.tsumes.com");
                return value;
            }

            set { WriteKeyValue("Hub", value); }
        }
        
        public static bool FullScreenMode {
            get {
                var value = GetKeyValue("FullScreenMode");
                if (string.IsNullOrEmpty(value))
                    WriteKeyValue("FullScreenMode", false.ToString());

                return GetKeyValue("FullScreenMode") == "True";
            }

            set { WriteKeyValue("FullScreenMode", value.ToString()); }
        }

        public static bool Cemu173Patch {
            get {
                var value = GetKeyValue("Cemu173Patch");
                if (string.IsNullOrEmpty(value))
                    WriteKeyValue("Cemu173Patch", true.ToString());

                return GetKeyValue("Cemu173Patch") == "True";
            }

            set { WriteKeyValue("Cemu173Patch", value.ToString()); }
        }

        public static bool StoreEncryptedContent {
            get {
                var value = GetKeyValue("StoreEncryptedContent");
                if (string.IsNullOrEmpty(value))
                    WriteKeyValue("StoreEncryptedContent", true.ToString());

                return GetKeyValue("StoreEncryptedContent") == "True";
            }

            set { WriteKeyValue("StoreEncryptedContent", value.ToString()); }
        }

        private static string ConfigFile {
            get {
                var configFile = Path.Combine(ConfigFolder, "MapleSeed.ini");

                if (!Directory.Exists(ConfigFolder))
                    Directory.CreateDirectory(ConfigFolder);

                if (!File.Exists(configFile) || new FileInfo(configFile).Length <= 0)
                    File.WriteAllText(configFile, Resources.Settings_DefaultSettings);

                return configFile;
            }
        }
        
        private static string ConfigName => "MapleTree";
        private static string AppFolder => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static string ConfigFolder => Path.Combine(AppFolder, ConfigName);

        private static string CemuPatchPath => Path.Combine("mlc01", "usr", "title", "00050000");
        public static string BasePatchDir => Path.Combine(CemuDirectory, CemuPatchPath);

        private static string GetKeyValue(string key)
        {
            var parser = new FileIniDataParser();
            var data = parser.ReadFile(ConfigFile);
            return data[ConfigName][key] ?? "";
        }

        private static void WriteKeyValue(string key, string value)
        {
            var parser = new FileIniDataParser();
            var data = parser.ReadFile(ConfigFile);
            data[ConfigName][key] = value;
            parser.WriteFile(ConfigFile, data);
        }
    }
}