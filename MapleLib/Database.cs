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
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using libWiiSharp;
using MapleLib.Common;
using MapleLib.Properties;
using MapleLib.Structs;
using Newtonsoft.Json;
using WebClient = MapleLib.Network.Web.WebClient;

#endregion

namespace MapleLib
{
    public class Database
    {
        private static string TitleKeys => "https://wiiu.titlekeys.com";
        private static List<WiiUTitle> DbObject { get; set; } = new List<WiiUTitle>();
        public static List<Title> TitleDbObject { get; } = new List<Title>();

        public static async Task Initialize()
        {
            try {
                Toolbelt.AppendLog("[Database] Rebuilding Title Cache Entries", Color.DarkViolet);

                if (Toolbelt.Database == null)
                    Toolbelt.Database = new Database();

                if (Toolbelt.Settings == null)
                    Toolbelt.Settings = new Settings();

                var data = await WebClient.DownloadData(TitleKeys + "/json");

                if (data.Length <= 0)
                    throw new WebException("[Database] Unable to download Wii U title database.");

                File.WriteAllBytes("titlekeys.json", data);

                var json = Encoding.UTF8.GetString(data);
                DbObject = JsonConvert.DeserializeObject<List<WiiUTitle>>(json);
                DbObject.RemoveAll(t => t.ToString().Contains("()"));

                LoadDatabase(DbObject);
                Toolbelt.AppendLog($"[Database] Valid Title Entries Added: {TitleDbObject.Count}", Color.DarkViolet);
            }
            catch (Exception e) {
                Toolbelt.AppendLog($"{e.Message}\n{e.StackTrace}");
            }
        }

        private static void LoadDatabase(List<WiiUTitle> dbObject)
        {
            LoadTitles(dbObject);
        }

        private static void LoadTitles(List<WiiUTitle> dbObject)
        {
            var titles = new List<string>(Resources.titles.Split('\n'));
            var updates = new List<string>(Resources.updates.Split('\n'));

            foreach (var title in dbObject) {
                TitleDbObject.Add(new Title
                {
                    Id = title.TitleID,
                    Key = title.TitleKey,
                    Name = title.Name,
                    Region = title.Region
                });
            }

            foreach (var titleObj in TitleDbObject) {
                var update = updates.Find(x => x.Contains(titleObj.Lower8Digits));
                var title = titles.Find(x => x.Contains(titleObj.Id.ToUpper()));
                
                if (!string.IsNullOrEmpty(title)) {
                    var parts2 = title.Split('|');
                    if (parts2.Length >= 0) {
                        titleObj.ProductCode = parts2[2];
                    }
                }

                if (!string.IsNullOrEmpty(update))
                {
                    var parts1 = update.Split('|');
                    if (parts1.Length >= 0) {
                        titleObj.Versions = parts1[2].Split(',').Select(s => s.Trim()).ToList();
                    }
                }
            }
        }

        public static bool HasDLC(string titleId)
        {
            var lower8Digits = titleId.Substring(8);
            titleId = $"0005000c{lower8Digits}".ToLower();
            var title = DbObject.Find(y => y.TitleID.ToLower() == titleId);
            return title != null;
        }

        public static bool HasUpdates(string titleId)
        {
            var lower8Digits = titleId.Substring(8);
            titleId = $"0005000e{lower8Digits}".ToLower();
            var title = TitleDbObject.Find(y => y.Id.ToLower() == titleId);
            return title?.Versions.Count > 0;
        }

        public void updateGame(string titleId, string fullPath)
        {
            titleId = titleId.Replace("00050000", "0005000e");
#pragma warning disable 4014
            UpdateGame(titleId, fullPath, "Patch");
#pragma warning restore 4014
        }

        private static void CleanUpdate(string outputDir, TMD tmd)
        {
            try {
                if (!Settings.Instance.StoreEncryptedContent) {
                    Toolbelt.AppendLog("  - Deleting Encrypted Contents...");
                    foreach (var t in tmd.Contents) {
                        if (!File.Exists(Path.Combine(outputDir, t.ContentID.ToString("x8")))) continue;
                        File.Delete(Path.Combine(outputDir, t.ContentID.ToString("x8")));
                        //File.Delete(Path.Combine(outputDir, "cetk"));
                        //File.Delete(Path.Combine(outputDir, "tmd"));
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

        public async Task UpdateGame(string titleId, string fullPath, string contentType, string version = "0")
        {
            fullPath = Path.GetFullPath(fullPath);
            var cemu = Toolbelt.Settings.CemuDirectory;
            var basePatchDir = Path.Combine(cemu, "mlc01", "usr", "title", "00050000");

            var lower8Digits = titleId.Substring(8).ToUpper();
            var tempId = string.Empty;

            if (contentType == "eShop/Application") {
                tempId = $"00050000{lower8Digits}";
            }

            if (contentType == "Patch") {
                tempId = $"0005000e{lower8Digits}";

                if (Settings.Instance.Cemu173Patch)
                    fullPath = Path.Combine(basePatchDir, lower8Digits);
            }

            if (contentType == "DLC") {
                tempId = $"0005000c{lower8Digits}";

                if (Settings.Instance.Cemu173Patch)
                    fullPath = Path.Combine(basePatchDir, lower8Digits, "aoc");
            }

            var game = FindByTitleId(tempId);

            if (game == null) {
                TextLog.MesgLog.WriteError($"Unable to locate title content: {contentType}");
                TextLog.MesgLog.WriteError($"Please verify this title Id is correct: {tempId}");
            }
            else {
                await DownloadTitle(game, fullPath, version);
            }
        }

        public static Title Find(string game_name)
        {
            if (game_name == null) return new Title();

            var fullPath = Path.Combine(Toolbelt.Settings.TitleDirectory, game_name);
            var entries = Directory.GetFileSystemEntries(fullPath, "meta.xml", SearchOption.AllDirectories);
            if (entries.Length <= 0) return new Title { Name = "No meta.xml found!"};

            var xml = new XmlDocument();
            xml.Load(entries[0]);
            using (var titleIdTag = xml.GetElementsByTagName("title_id")) {
                if (titleIdTag.Count <= 0) return new Title {Name = "NULL"};

                var titleId = titleIdTag[0].InnerText;

                var id = titleId.Substring(8).ToUpper();
                var title = TitleDbObject.Find(t => t.Lower8Digits == id);

                return title;
            }
        }

        public static IEnumerable<Title> FindTitles(string game_name)
        {
            if (game_name == null) return new List<Title>();

            var titles = TitleDbObject.FindAll(t => Toolbelt.RIC(t.ToString()).ToLower().Contains(game_name.ToLower()));

            return new List<Title>(titles);
        }

        public static Title FindByTitleId(string titleId)
        {
            var title = TitleDbObject.Find(t => t.Id.ToLower() == titleId.ToLower());

            return titleId.IsNullOrEmpty() || title == null ? null : title;
        }

        private static async Task<TMD> LoadTmd(Title wiiUTitle, string outputDir, string titleUrl, string version)
        {
            var tmdFile = Path.Combine(outputDir, "tmd");

            if (!File.Exists(tmdFile) || new FileInfo(tmdFile).Length <= 0) {
                byte[] data;
                try {
                    Toolbelt.AppendLog("  - Downloading TMD...");
                    data = await WebClient.DownloadData(Path.Combine(titleUrl, "tmd." + version));
                }
                catch (Exception) {
                    var titleId = wiiUTitle.Id.ToLower();
                    var titleKey = wiiUTitle.Key.ToLower();
                    var address = "192.99.69.253";
                    data = await WebClient.DownloadData($"http://{address}/?key={titleKey}&title={titleId}&type=tmd");
                }
                File.WriteAllBytes(tmdFile, data);
            }

            if (!File.Exists(tmdFile) || new FileInfo(tmdFile).Length <= 0)
                return null;

            Toolbelt.AppendLog("  - Loading TMD...");
            var tmd = TMD.Load(tmdFile);

            if (tmd.TitleVersion > 999) return null;
            Toolbelt.AppendLog("  - Parsing TMD...");
            Toolbelt.AppendLog($"    + Title Version: {tmd.TitleVersion}");
            Toolbelt.AppendLog($"    + {tmd.NumOfContents} Contents");
            return tmd;
        }

        private async Task<Ticket> LoadTicket(Title wiiUTitle, string outputDir, string titleUrl)
        {
            var cetkFile = Path.Combine(outputDir, "cetk");
            Toolbelt.AppendLog("  - Downloading Ticket...");
            byte[] data = {};

            if (!File.Exists(cetkFile))
            {
                try {
                    data = await WebClient.DownloadData($"{titleUrl}cetk");
                }
                catch (Exception)
                {
                    try {
                        var titleId = wiiUTitle.Id.ToLower();
                        var titleKey = wiiUTitle.Key.ToLower();
                        var address = "192.99.69.253";
                        data = await WebClient.DownloadData($"http://{address}/?key={titleKey}&title={titleId}&type=tik");
                    }
                    catch (Exception e)
                    {
                        Toolbelt.AppendLog($"   + Downloading Ticket Failed...\n{e.Message}");
                    }
                }

                File.WriteAllBytes(Path.Combine(outputDir, "cetk"), data);
            }

            if (!File.Exists(cetkFile)) return null;
            Toolbelt.AppendLog("   + Loading Ticket...");
            var ticket = Ticket.Load(data);

            return ticket?.NumOfDLC == 0 ? null : ticket;
        }

        private async Task<int> DownloadContent(TMD tmd, string outputDir, string titleUrl, string name)
        {
            var result = 0;
            for (var i = 0; i < tmd.NumOfContents; i++) {
                var i1 = i;
                result = await Task.Run(async () => {
                    var numc = tmd.NumOfContents;
                    var size = Toolbelt.SizeSuffix((long)tmd.Contents[i1].Size);
                    Toolbelt.AppendLog($"  - Downloading Content #{i1 + 1} of {numc}... ({size})");

                    var contentPath = Path.Combine(outputDir, tmd.Contents[i1].ContentID.ToString("x8"));
                    if (Toolbelt.IsValid(tmd.Contents[i1], contentPath))
                        Toolbelt.AppendLog("   + Using Local File, Skipping...");
                    else
                        try
                        {
                            var downloadUrl = $"{titleUrl}/{tmd.Contents[i1].ContentID:x8}";
                            var outputdir = Path.Combine(outputDir, tmd.Contents[i1].ContentID.ToString("x8"));
                            await WebClient.DownloadFileAsync(downloadUrl, outputdir);
                        }
                        catch (Exception ex)
                        {
                            Toolbelt.AppendLog($"  - Downloading Content #{i1 + 1} of {numc} failed...\n{ex.Message}");
                            Toolbelt.SetStatus($"Downloading '{name}' Content #{i1 + 1} of {numc} failed. Check Console");
                            return 0;
                        }
                    return 1;
                });
                if (result == 0)
                    break;
            }
            return result;
        }

        private async Task DownloadTitle(Title title, string fullPath, string version)
        {
            var outputDir = Path.GetFullPath(fullPath);

            if (fullPath.EndsWith(".rpx")) {
                var folder = Path.GetDirectoryName(fullPath);
                if (folder.EndsWith("code"))
                    outputDir = Path.GetDirectoryName(folder);
            }

            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            Toolbelt.AppendLog($"Output Directory '{outputDir}'");

            var nusUrls = new List<string>
            {
                "http://ccs.cdn.wup.shop.nintendo.net/ccs/download/",
                "http://nus.cdn.shop.wii.com/ccs/download/",
                "http://ccs.cdn.c.shop.nintendowifi.net/ccs/download/"
            };

            TMD tmd = null;
            foreach (var nusUrl in nusUrls) {
                string titleUrl = $"{nusUrl}{title.Id.ToLower()}/";

                if ((tmd = await LoadTmd(title, outputDir, titleUrl, version)) != null)
                    break;
            }

            if (tmd == null) {
                TextLog.MesgLog.WriteError("Could not locate TMD. Is this content request valid?");
                return;
            }

            Ticket ticket = null;
            foreach (var nusUrl in nusUrls) {
                string titleUrl = $"{nusUrl}{title.Id.ToLower()}/";

                if ((ticket = await LoadTicket(title, outputDir, titleUrl)) != null)
                    break;
            }

            if (ticket == null || ticket.NumOfDLC != tmd.TitleVersion)
                return;

            Toolbelt.AppendLog($"Downloading '{title.ContentType}' Content for '{title}' v[{tmd.TitleVersion}]");
            Toolbelt.SetStatus($"Downloading '{title.ContentType}' Content for '{title}' v[{tmd.TitleVersion}]");

            foreach (var nusUrl in nusUrls) {
                var url = nusUrl + title.Id.ToLower();
                if (await DownloadContent(tmd, outputDir, url, title.ToString()) != 1)
                    continue;

                Toolbelt.AppendLog(string.Empty);
                Toolbelt.AppendLog("  - Decrypting Content");
                Toolbelt.AppendLog("  + This may take a minute. Please wait...");
                Toolbelt.SetStatus("Decrypting Content. This may take a minute. Please wait...", Color.OrangeRed);
                Toolbelt.AppendLog(string.Empty);
                await Toolbelt.CDecrypt(outputDir);
                CleanUpdate(outputDir, tmd);
                break;
            }

            WebClient.ResetDownloadProgressChanged();
            Toolbelt.AppendLog($"Downloading {title.ContentType} Content for '{title}' v{tmd.TitleVersion} Finished.");
            Toolbelt.SetStatus($"Downloading {title.ContentType} Content for '{title}' v{tmd.TitleVersion} Finished.", Color.Green);
        }
    }
}