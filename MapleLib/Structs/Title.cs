﻿// Project: MapleLib
// File: TitleEntry.cs
// Updated By: Jared
// 

using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using MapleLib.Common;
using MapleLib.Network.Web;
using MapleLib.Properties;

namespace MapleLib.Structs
{
    public class Title : eShopTitle
    {
        public string FolderLocation { get; set; }
        public string MetaLocation { get; set; }

        public override string ToString()
        {
            var cType = ContentType.Contains("App") ? "" : $"[{ContentType}]";
            return Toolbelt.RIC($"{cType}[{Region}] {Name}");
        }

        public int GetTitleVersion()
        {
            var metaLocation = Path.Combine(Settings.BasePatchDir, Lower8Digits(), "meta", "meta.xml");
            if (!File.Exists(metaLocation))
                return 0;

            var versionStr = Helper.XmlGetStringByTag(metaLocation, "title_version");

            int version;
            return int.TryParse(versionStr, out version) ? version : 0;
        }

        public async Task DownloadContent(string version = "0")
        {
            await this.DownloadContent(ContentType, version);
        }

        public async Task DownloadUpdate(string version = "0")
        {
            await this.DownloadContent("Patch", version);
        }

        public async Task DownloadDLC()
        {
            await this.DownloadContent("DLC", "0");
        }

        public void DeleteContent()
        {
            var path = Path.GetFullPath(FolderLocation);

            var result = MessageBox.Show(string.Format(Resources.ActionWillDeleteAllContent, path),
                Resources.PleaseConfirmAction, MessageBoxButtons.OKCancel);

            if (result == DialogResult.OK)
                Directory.Delete(path, true);
        }

        public void DeleteUpdateContent()
        {
            var updatePath = Path.Combine(Settings.BasePatchDir, Lower8Digits());

            if (Directory.Exists(Path.Combine(updatePath, "code")))
                Directory.Delete(Path.Combine(updatePath, "code"), true);

            if (Directory.Exists(Path.Combine(updatePath, "meta")))
                Directory.Delete(Path.Combine(updatePath, "meta"), true);

            if (Directory.Exists(Path.Combine(updatePath, "content")))
                Directory.Delete(Path.Combine(updatePath, "content"), true);

            if (File.Exists(Path.Combine(updatePath, "result.log")))
                File.Delete(Path.Combine(updatePath, "result.log"));
        }

        public void DeleteAddOnContent()
        {
            var updatePath = Path.Combine(Settings.BasePatchDir, Lower8Digits(), "aoc");

            if (Directory.Exists(updatePath))
                Directory.Delete(updatePath, true);
        }
    }
}