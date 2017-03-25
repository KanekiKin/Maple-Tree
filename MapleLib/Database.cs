// Project: MapleSeed
// File: Database.cs
// Updated By: Jared
// 

#region usings

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using libWiiSharp;
using MapleLib.Collections;
using MapleLib.Common;
using MapleLib.Structs;
using WebClient = MapleLib.Network.Web.WebClient;

#endregion

namespace MapleLib
{
    public static class Database
    {
        public static MapleDictionary TitleDb { get; } = new MapleDictionary(Settings.TitleDirectory);
        
        private static void CleanUpdate(string outputDir, TMD tmd)
        {
            try {
                if (!Settings.StoreEncryptedContent) {
                    Toolbelt.AppendLog("  - Deleting Encrypted Contents...");
                    foreach (var t in tmd.Contents) {
                        if (!File.Exists(Path.Combine(outputDir, t.ContentID.ToString("x8")))) continue;
                        File.Delete(Path.Combine(outputDir, t.ContentID.ToString("x8")));
                    }
                }

                Toolbelt.AppendLog("  - Deleting CDecrypt, libeay32, and msvcr120d...");
                File.Delete(Path.Combine(outputDir, "CDecrypt.exe"));
                File.Delete(Path.Combine(outputDir, "libeay32.dll"));
                File.Delete(Path.Combine(outputDir, "msvcr120d.dll"));
            }
            catch {
                // ignored
            }
        }

        private static async Task<byte[]> DownloadData(string url)
        {
            byte[] data = {};

            try {
                data = await WebClient.DownloadDataAsync(url);
            }
            catch (WebException e) {
                TextLog.MesgLog.AddHistory($"{e.Message}\n{e.StackTrace}");
            }

            return data;
        }

        private static async Task<TMD> DownloadTmd(string url, string saveTo)
        {
            var data = await DownloadData(url);
            if (data.Length <= 0) return null;
            var tmd = TMD.Load(data);

            if (tmd.TitleVersion > 999) return null;
            Toolbelt.AppendLog("  - Parsing TMD...");
            Toolbelt.AppendLog($"    + Title Version: {tmd.TitleVersion}");
            Toolbelt.AppendLog($"    + {tmd.NumOfContents} Contents");

            File.WriteAllBytes(saveTo, data);
            return tmd;
        }

        private static async Task<Ticket> DownloadTicket(string url, string saveTo)
        {
            var data = await DownloadData(url);
            if (data.Length <= 0) return null;
            var tik = Ticket.Load(data);

            if (tik == null) return null;
            Toolbelt.AppendLog("  - Parsing Ticket...");
            Toolbelt.AppendLog($"    + Title Version: {tik.NumOfDLC}");

            File.WriteAllBytes(saveTo, data);
            return tik;
        }

        private static async Task<TMD> LoadTmd(Title title, string outputDir, string titleUrl, string version)
        {
            var tmdFile = Path.Combine(outputDir, "tmd");

            if (string.IsNullOrEmpty(title.TitleID) || string.IsNullOrEmpty(title.TitleKey))
                return null;

            version = int.Parse(version) == 0 ? "" : $".{version}";
            if (await DownloadTmd(titleUrl + $"tmd{version}", tmdFile) == null) {
                var titleId = title.TitleID.ToLower();
                var titleKey = title.TitleKey.ToLower();
                var address = "192.99.69.253";
                var url = $"http://{address}/?key={titleKey}&title={titleId}&type=tmd";

                await DownloadTmd(url, tmdFile);
            }

            var file = new FileInfo(tmdFile);
            if (!file.Exists || file.Length <= 0)
                return null;

            return TMD.Load(tmdFile);
        }

        private static async Task<Ticket> LoadTicket(Title title, string outputDir, string titleUrl)
        {
            var cetkFile = Path.Combine(outputDir, "cetk");

            if (await DownloadTicket($"{titleUrl}cetk", cetkFile) == null) {
                var titleId = title.TitleID.ToLower();
                var titleKey = title.TitleKey.ToLower();
                var address = "192.99.69.253";
                var url = $"http://{address}/?key={titleKey}&title={titleId}&type=tik";

                await DownloadTicket(url, cetkFile);
            }

            var file = new FileInfo(cetkFile);
            if (!file.Exists || file.Length <= 0)
                return null;

            return Ticket.Load(cetkFile);
        }

        public static async Task DownloadTitle(Title title, string outputDir, string contentType, string version)
        {
            #region Setup

            var workingId = title.TitleID.ToLower();

            if (contentType == "Patch") {
                workingId = $"0005000E{title.Lower8Digits}".ToLower();

                if (Settings.Cemu173Patch)
                    outputDir = Path.Combine(Settings.BasePatchDir, title.Lower8Digits);
            }

            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            var nusUrls = new List<string>
            {
                "http://ccs.cdn.wup.shop.nintendo.net/ccs/download/",
                "http://nus.cdn.shop.wii.com/ccs/download/",
                "http://ccs.cdn.c.shop.nintendowifi.net/ccs/download/"
            };

            Toolbelt.AppendLog($"Output Directory '{outputDir}'");

            #endregion

            #region TMD

            Toolbelt.AppendLog("  - Loading TMD...");
            TMD tmd = null;

            foreach (var nusUrl in nusUrls) {
                string titleUrl = $"{nusUrl}{workingId}/";
                tmd = await LoadTmd(title, outputDir, titleUrl, version);

                if (tmd != null)
                    break;
            }

            if (tmd == null) {
                TextLog.MesgLog.WriteError("Could not locate TMD. Is this content request valid?");
                return;
            }

            #endregion

            #region Ticket

            Toolbelt.AppendLog("  - Loading Ticket...");
            Ticket ticket = null;

            foreach (var nusUrl in nusUrls) {
                string titleUrl = $"{nusUrl}{workingId}/";
                ticket = await LoadTicket(title, outputDir, titleUrl);

                if (ticket != null)
                    break;
            }

            if (ticket == null) {
                TextLog.MesgLog.WriteError("Could not locate Ticket. Is this content request valid?");
                return;
            }

            #endregion

            #region Content

            Toolbelt.AppendLog($"[+] [{contentType}] {title.Name} v{tmd.TitleVersion}");
            Toolbelt.SetStatus($"Output Directory: {outputDir}");

            foreach (var nusUrl in nusUrls) {
                var url = nusUrl + workingId;
                if (await DownloadContent(tmd, outputDir, url) != 1)
                    continue;

                Toolbelt.AppendLog(string.Empty);
                Toolbelt.AppendLog("  - Decrypting Content");
                Toolbelt.AppendLog("  + This may take a minute. Please wait...");
                Toolbelt.SetStatus("Decrypting Content. This may take a minute. Please wait...", Color.OrangeRed);
                await Toolbelt.CDecrypt(outputDir);
                CleanUpdate(outputDir, tmd);
                break;
            }

            #endregion

            WebClient.ResetDownloadProgressChanged();
            Toolbelt.AppendLog($"[+] [{contentType}] {title.Name} v{tmd.TitleVersion} Finished.");
            Toolbelt.SetStatus($"[+] [{contentType}] {title.Name} v{tmd.TitleVersion} Finished.");
        }

        private static async Task<int> DownloadContent(TMD tmd, string outputDir, string titleUrl)
        {
            var result = 0;
            for (var i = 0; i < tmd.NumOfContents; i++) {
                var i1 = i;
                result = await Task.Run(async () => {
                    var numc = tmd.NumOfContents;
                    var size = Toolbelt.SizeSuffix((long) tmd.Contents[i1].Size);
                    Toolbelt.AppendLog($"Downloading Content #{i1 + 1} of {numc}... ({size})");
                    var contentPath = Path.Combine(outputDir, tmd.Contents[i1].ContentID.ToString("x8"));

                    if (Toolbelt.IsValid(tmd.Contents[i1], contentPath)) {
                        //Toolbelt.AppendLog("   + Using Local File, Skipping...");
                    }
                    else
                        try {
                            var downloadUrl = $"{titleUrl}/{tmd.Contents[i1].ContentID:x8}";
                            await WebClient.DownloadFileAsync(downloadUrl, contentPath);
                        }
                        catch (Exception ex) {
                            Toolbelt.AppendLog($"Downloading Content #{i1 + 1} of {numc} failed...\n{ex.Message}");
                            return 0;
                        }
                    return 1;
                });
                if (result == 0)
                    break;
            }
            return result;
        }
    }
}