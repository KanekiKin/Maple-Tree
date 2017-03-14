// Project: MapleLib
// File: TitleEntry.cs
// Updated By: Jared
// 

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using MapleLib.Common;

namespace MapleLib.Structs
{
    public class Title
    {
        public string Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string ImageCode { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public bool WTKTicket { get; set; }
        public bool CDN { get; set; }
        public string FolderLocation { get; set; }
        public string MetaLocation { get; set; }
        public string Upper8Digits => Id.Length >= 8 ? Id.Substring(0, 8).ToUpper() : string.Empty;
        public string Lower8Digits => Id.Length >= 8 ? Id.Substring(8).ToUpper() : string.Empty;
        public List<string> Versions { get; set; } = new List<string>();
        public List<Title> DLC { get; } = new List<Title>();

        public string Image { get; set; }

        public string ContentType {
            get {
                var header = Id.Substring(0, 8).ToUpper();

                switch (header) {
                    case "00050010":
                    case "0005001B":
                        return "System Application";

                    case "00050000":
                        return "eShop/Application";

                    case "00050002":
                        return "Demo";

                    case "0005000E":
                        return "Patch";

                    case "0005000C":
                        return "DLC";
                }

                return "Unknown";
            }
        }

        public override string ToString()
        {
            return Toolbelt.RIC($"[{Region}][{ImageCode}] {Name}");
        }

        public async Task DownloadContent(string version = "0")
        {
            try {
                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Can't download content without a valid TItle ID.");

                if (string.IsNullOrEmpty(FolderLocation))
                    throw new Exception("Can't download content without a valid output Location.");

                await Database.DownloadTitle(this, FolderLocation, ContentType, version);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        public async Task DownloadUpdate(string version = "0")
        {
            try {
                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Can't download content without a valid TItle ID.");

                if (string.IsNullOrEmpty(FolderLocation))
                    throw new Exception("Can't download content without a valid output Location.");

                await Database.DownloadTitle(this, FolderLocation, "Patch", version);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }
    }
}