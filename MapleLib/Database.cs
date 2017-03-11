// Project: MapleSeed
// File: Database.cs
// Updated By: Jared
// 

#region usings

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using libWiiSharp;
using MapleLib.Common;
using MapleLib.Network.Web;
using MapleLib.Structs;
using Newtonsoft.Json;

#endregion

namespace MapleLib
{
    public class Database
    {
        private static string TitleKeys => "https://wiiu.titlekeys.com";
        private static string DatabaseFile => Path.Combine(Path.GetTempPath(), "MapleSeed_DB.json");
        private static List<WiiUTitle> DbObject { get; set; } = new List<WiiUTitle>();

        public static async Task Initialize()
        {
            try {
                if (Toolbelt.Database == null)
                    Toolbelt.Database = new Database();

                if (Toolbelt.Settings == null)
                    Toolbelt.Settings = new Settings();

                if (DateTime.Now > new FileInfo(DatabaseFile).LastWriteTime.AddDays(1))
                    await WebClient.DownloadFileAsync(TitleKeys + "/json", DatabaseFile);
            }
            catch (Exception e) {
                Toolbelt.AppendLog($"{e.Message}\n{e.StackTrace}");
            }

            var json = File.ReadAllText(DatabaseFile);
            DbObject = JsonConvert.DeserializeObject<List<WiiUTitle>>(json);
            DbObject.RemoveAll(t => t.ToString().Contains("()"));
            Toolbelt.AppendLog("Database Update to date!");
        }

        public void updateGame(string titleId, string fullPath)
        {
            titleId = titleId.Replace("00050000", "0005000e");
#pragma warning disable 4014
            UpdateGame(titleId, fullPath, "Patch");
#pragma warning restore 4014
        }
        
        public async Task UpdateGame(string titleId, string fullPath, string contentType)
        {
            var cemu = Toolbelt.Settings.CemuDirectory;
            var basePatchDir = Path.Combine(cemu, "mlc01", "usr", "title", "00050000");

            var game = FindByTitleId(titleId);
            if (game.TitleID.IsNullOrEmpty()) {
                Toolbelt.AppendLog($"Unable to locate title using ID {titleId}");
            }
            else {
                if (string.IsNullOrEmpty(contentType)) {
                    contentType = game.ContentType;
                }

                Toolbelt.SetStatus($"Downloading {contentType} Content for '{titleId}'");

                if (Settings.Instance.Cemu173Patch) {
                    var lower8Digits = game.TitleID.Substring(8).ToUpper();

                    if (contentType == "eShop/Application") {
                        fullPath = Path.GetFullPath(fullPath);
                    }

                    if (contentType == "Patch") {
                        game = FindByTitleId("0005000e" + lower8Digits);
                        fullPath = Path.Combine(basePatchDir, lower8Digits);
                    }

                    if (contentType == "DLC") {
                        game = FindByTitleId("0005000c" + lower8Digits);
                        fullPath = Path.Combine(basePatchDir, lower8Digits, "aoc");
                    }
                }

                await DownloadTitle(game, fullPath);
            }
        }

        public static WiiUTitle Find(string game_name)
        {
            if (game_name == null) return new WiiUTitle();

            var fullPath = Path.Combine(Toolbelt.Settings.TitleDirectory, game_name);
            var entries = Directory.GetFileSystemEntries(fullPath, "meta.xml", SearchOption.AllDirectories);
            if (entries.Length <= 0) return new WiiUTitle {Name = "No meta.xml found!"};

            var xml = new XmlDocument();
            xml.Load(entries[0]);
            var titleId_tag = xml.GetElementsByTagName("title_id");

            if (titleId_tag.Count > 0) {
                var titleId = titleId_tag[0].InnerText;

                var title = DbObject.Find(t => t.TitleID.ToLower() == titleId.ToLower());

                return title;
            }

            return new WiiUTitle {Name = "NULL"};
        }

        public static IEnumerable<WiiUTitle> FindTitles(string game_name)
        {
            if (game_name == null) return new List<WiiUTitle>();

            var titles = DbObject.FindAll(t => Toolbelt.RIC(t.ToString()).ToLower().Contains(game_name.ToLower()));

            return new List<WiiUTitle>(titles);
        }

        public static WiiUTitle FindByTitleId(string titleId)
        {
            var title = DbObject.Find(t => string.Equals(t.TitleID, titleId, StringComparison.CurrentCultureIgnoreCase));

            return titleId.IsNullOrEmpty() || title.ToString().IsNullOrEmpty() ? new WiiUTitle() : title;
        }

        private void CleanUpdate(string outputDir, TMD tmd)
        {
            try {
                Toolbelt.AppendLog("  - Deleting Encrypted Contents...");
                foreach (var t in tmd.Contents)
                    if (File.Exists(Path.Combine(outputDir, t.ContentID.ToString("x8")))) {
                        //File.Delete(Path.Combine(outputDir, t.ContentID.ToString("x8")));
                    }

                Toolbelt.AppendLog("  - Deleting CDecrypt and libeay32...");
                File.Delete(Path.Combine(outputDir, "CDecrypt.exe"));
                File.Delete(Path.Combine(outputDir, "libeay32.dll"));
                File.Delete(Path.Combine(outputDir, "msvcr120d.dll"));
            }
            catch {
                // ignored
            }
        }

        private async Task<TMD> LoadTmd(string outputDir, string titleUrl)
        {
            try {
                var tmdFile = Path.Combine(outputDir, "tmd");

                Toolbelt.AppendLog("  - Downloading TMD...");
                await WebClient.DownloadFileAsync(Path.Combine(titleUrl, "tmd"), tmdFile);

                Toolbelt.AppendLog("  - Loading TMD...");
                var tmd = TMD.Load(File.ReadAllBytes(tmdFile));

                Toolbelt.AppendLog("  - Parsing TMD...");
                Toolbelt.AppendLog($"    + Title Version: {tmd.TitleVersion}");
                Toolbelt.AppendLog($"    + {tmd.NumOfContents} Contents");

                return tmd;
            }
            catch (Exception ex) {
                Toolbelt.AppendLog($"   + Downloading TMD Failed...\n{ex.Message}");
            }

            return null;
        }

        private async Task<int> LoadTicket(WiiUTitle wiiUTitle, string outputDir, string titleUrl)
        {
            var cetk = Path.Combine(outputDir, "cetk");

            if (!File.Exists(cetk)) {
                try
                {
                    Toolbelt.AppendLog("  - Downloading Ticket...");

                    var cetkUrl = Path.Combine(titleUrl, "cetk");
                    await WebClient.DownloadFileAsync(cetkUrl, cetk);
                }
                catch (Exception)
                {
                    try {
                        await WebClient.DownloadFileAsync("http://pixxy.in/upload/ticket", cetk);
                        var ticket = Ticket.Load(File.ReadAllBytes(cetk));
                        ticket.TitleID = BitConverter.ToUInt64(Hex2Binary(wiiUTitle.TitleID), 0);
                        ticket.TitleKey = Hex2Binary(wiiUTitle.TitleKey);

                        /*if (wiiUTitle.Ticket == "1")
                        {
                            var WII_TIK_URL = "https://wiiu.titlekeys.com/ticket/";
                            var cetkUrl = $"{WII_TIK_URL}{wiiUTitle.TitleID.ToLower()}.tik";
                            await WebClient.DownloadFileAsync(cetkUrl, cetk);
                        }*/
                    }
                    catch(Exception e) {
                        Toolbelt.AppendLog($"   + Downloading Ticket Failed...\n{e.Message}");
                        return 0;
                    }
                }
            }

            // Parse Ticket
            Toolbelt.AppendLog("   + Loading Ticket...");
            var tik = Ticket.Load(File.ReadAllBytes(cetk));
            return 1;
        }

        private byte[] Hex2Binary(string hex)
        {
            var chars = hex.ToCharArray();
            var bytes = new List<byte>();
            for (int index = 0; index < chars.Length; index += 2)
            {
                var chunk = new string(chars, index, 2);
                bytes.Add(byte.Parse(chunk, NumberStyles.AllowHexSpecifier));
            }
            return bytes.ToArray();
        }

        private Ticket CreateTicket(string title_id, int title_version, string outputDir, bool patch_demo = false,
            bool patch_dlc = false)
        {
            var cetk = Path.Combine(outputDir, "cetk");

            var title = FindByTitleId(title_id);

            var tik = new Ticket {TitleID = ulong.Parse(title_id)};
            tik.SetTitleKey(title.TitleKey);

            tik.Save(cetk);
            return tik;
        }

        private async Task<int> DownloadContent(TMD tmd, string outputDir, string titleUrl, string name)
        {
            for (var i = 0; i < tmd.NumOfContents; i++) {
                var numc = tmd.NumOfContents;
                var size = Toolbelt.SizeSuffix((long) tmd.Contents[i].Size);
                Toolbelt.AppendLog($"  - Downloading Content #{i + 1} of {numc}... ({size})");
                Toolbelt.SetStatus($"Downloading '{name}' Content #{i + 1} of {numc}... ({size})", Color.OrangeRed);

                var contentPath = Path.Combine(outputDir, tmd.Contents[i].ContentID.ToString("x8"));
                if (Toolbelt.IsValid(tmd.Contents[i], contentPath))
                    Toolbelt.AppendLog("   + Using Local File, Skipping...");
                else
                    try {
                        var downloadUrl = titleUrl + tmd.Contents[i].ContentID.ToString("x8");
                        var outputdir = Path.Combine(outputDir, tmd.Contents[i].ContentID.ToString("x8"));
                        await WebClient.DownloadFileAsync(downloadUrl, outputdir);
                    }
                    catch (Exception ex) {
                        Toolbelt.AppendLog($"  - Downloading Content #{i + 1} of {numc} failed...\n{ex.Message}");
                        Toolbelt.SetStatus(
                            $"Downloading '{name}' Content #{i + 1} of {numc} failed... Check Console for Error!");
                        return 0;
                    }
            }
            return 1;
        }

        private async Task DownloadTitle(WiiUTitle wiiUTitle, string fullPath)
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

            Toolbelt.AppendLog($"Downloading Title {wiiUTitle.TitleID} v[Latest]...");

            const string wiiNusUrl = "http://nus.cdn.shop.wii.com/ccs/download/";
            const string wiiWupUrl = "http://ccs.cdn.wup.shop.nintendo.net/ccs/download/";
            string titleUrl = $"{wiiWupUrl}{wiiUTitle.TitleID}/";
            string titleUrl2 = $"{wiiNusUrl}{wiiUTitle.TitleID}/";
            
            TMD tmd;
            if ((tmd = await LoadTmd(outputDir, titleUrl)) != null) {
                if (await LoadTicket(wiiUTitle, outputDir, titleUrl) == 1) {
                    if (await DownloadContent(tmd, outputDir, titleUrl2, wiiUTitle.ToString()) == 1) {
                        Toolbelt.AppendLog("  - Decrypting Content...");
                        await Toolbelt.CDecrypt(outputDir, tmd);
                        CleanUpdate(outputDir, tmd);
                    }
                    Toolbelt.AppendLog($"Downloading Title '{wiiUTitle}' v{tmd.TitleVersion} Finished.");
                    Toolbelt.SetStatus($"Downloading Title '{wiiUTitle}' v{tmd.TitleVersion} Finished.", Color.Green);
                }
            }
            else {
                Toolbelt.SetStatus($"Downloading Title '{wiiUTitle}' Failed.", Color.DarkRed);
            }

            WebClient.ResetDownloadProgressChanged();
        }
    }
}