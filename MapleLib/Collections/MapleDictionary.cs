// Project: MapleLib
// File: MapleDictionary.cs
// Updated By: Jared
// 

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MapleLib.Common;
using MapleLib.Properties;
using MapleLib.Structs;
using Newtonsoft.Json;
using WebClient = MapleLib.Network.Web.WebClient;

namespace MapleLib.Collections
{
    public class MapleDictionary : Dictionary<string, Title>
    {
        private static List<WiiUTitle> _jsonObj;
        private static readonly List<string> Updates = new List<string>(Resources.updates.Split('\n'));
        private static readonly List<string> Titles = new List<string>(Resources.titles.Split('\n'));

        private string _baseDir { get; set; }

        private List<string> _directories { get; set; }

        public static event EventHandler<Title> OnAddTitle;

        public async Task<MapleDictionary> Init(string baseDir)
        {
            if (string.IsNullOrEmpty(baseDir))
                throw new Exception("MapleDictionary.Init(baseDir) cannot be null");

            _baseDir = baseDir;

            await BuildDatabase();

            return this;
        }

        public async Task BuildDatabase(bool notice = true)
        {
            await Task.Run(() => {
                _directories = Directory.GetDirectories(_baseDir).ToList();

                foreach (var loadiineDirectory in _directories)
                    BuildTitleList(loadiineDirectory);

                if (notice)
                    Toolbelt.AppendLog($"[Database] [+] Loadiine Titles: {Count}", Color.DarkViolet);
            });

            if (notice)
                Toolbelt.AppendLog("[Database] Preloading Images", Color.DarkViolet);

            await PreloadImages();

            await BuildDLCList();

            await Task.Run(() => {
                var count = Values.Sum(title => title.DLC.Count);
                count += Values.Sum(title => title.Versions.Count);

                if (notice)
                    Toolbelt.AppendLog($"[Database] [+] DLC & Updates: {count}", Color.DarkViolet);
            });
        }

        private async Task PreloadImages()
        {
            foreach (var value in Values) {
                await Task.Run(async () => value.Image = await FindImage(value.Id, value.MetaLocation));
            }
        }

        private async Task BuildDLCList()
        {
            await Task.Run(() => {
                foreach (var value in Values) {
                    var id = value.Lower8Digits;
                    var wtitles = _jsonObj.Where(x => x.Lower8Digits == id && x.ContentType == "DLC");

                    foreach (var wiiUTitle in wtitles) {
                        value.DLC.Add(new Title
                        {
                            Id = wiiUTitle.TitleID.ToUpper(),
                            Key = wiiUTitle.TitleKey,
                            Name = wiiUTitle.Name,
                            Region = wiiUTitle.Region,
                            WTKTicket = wiiUTitle.Ticket == "1",
                            FolderLocation = Path.Combine(Settings.BasePatchDir, id, "aoc")
                        });
                    }
                }
            });
        }

        private async void BuildTitleList(string path)
        {
            var fileSystemEntries = Directory.GetFiles(path, "meta.xml", SearchOption.AllDirectories).ToList();

            if (!fileSystemEntries.Any())
                return;

            try {
                foreach (var fileSystemEntry in fileSystemEntries) {
                    var titleId = Helper.XmlGetStringByTag(fileSystemEntry, "title_id");

                    if (string.IsNullOrEmpty(titleId))
                        throw new Exception("MapleDictionary.Init.titleId cannot return null");

                    var title = await BuildTitle(titleId, fileSystemEntry);

                    Add(titleId, title);
                    OnAddTitle?.Invoke(this, title);
                }
            }
            catch (Exception e) {
                TextLog.MesgLog.AddHistory(e.Message);
            }
        }

        public static async Task<Title> BuildTitle(string titleId, string location, bool newTitle = false)
        {
            try {
                if (_jsonObj == null) {
                    var jsonFile = Path.GetFullPath("titlekeys.json");

                    if (!File.Exists(jsonFile)) {
                        Toolbelt.AppendLog("[Database] Rebuilding Title Cache Entries", Color.DarkViolet);

                        var data = await WebClient.DownloadDataAsync("https://wiiu.titlekeys.com" + "/json");

                        if (data.Length <= 0)
                            throw new WebException("[Database] Unable to download Wii U title database.");

                        File.WriteAllBytes(jsonFile, data);
                    }

                    var json = Encoding.UTF8.GetString(File.ReadAllBytes(jsonFile));
                    _jsonObj = JsonConvert.DeserializeObject<List<WiiUTitle>>(json);
                }

                WiiUTitle jtitle;
                if ((jtitle = _jsonObj?.Find(x => x.TitleID.ToUpper() == titleId.ToUpper())) == null)
                    throw new Exception("MapleDictionary.BuildTitleList.jtitle cannot return null");

                var folder = Path.GetDirectoryName(Path.GetDirectoryName(location));

                if (!Directory.Exists(folder))
                    throw new Exception("MapleDictionary.BuildTitleList.FolderLocation is not valid");

                var title = new Title
                {
                    Id = titleId,
                    MetaLocation = location,
                    FolderLocation = folder,
                    Key = jtitle.TitleKey,
                    Name = jtitle.Name,
                    Region = jtitle.Region,
                    WTKTicket = jtitle.Ticket == "1"
                };

                if (newTitle) {
                    title.Image = await FindImage(title.Id, title.MetaLocation);
                    return title;
                }

                SetCodes(title);

                var update = Updates.Find(x => x.Contains(title.Lower8Digits));
                if (string.IsNullOrEmpty(update)) return title;
                var parts1 = update.Split('|');
                if (parts1.Length >= 0) title.Versions = parts1[2].Split(',').Select(s => s.Trim()).ToList();

                return title;
            }
            catch (Exception e) {
                Toolbelt.AppendLog($"{e.Message}\n{e.StackTrace}");
            }

            return null;
        }

        private static void SetCodes(Title _title)
        {
            var title = Titles.Find(x => x.Contains(_title.Id.ToUpper()));
            if (string.IsNullOrEmpty(title)) return;

            var parts2 = title.Split('|');
            if (parts2.Length < 0) return;
            
            var pcode = Helper.XmlGetStringByTag(_title.MetaLocation, "product_code") ?? "0000";
            var ccode = Helper.XmlGetStringByTag(_title.MetaLocation, "company_code") ?? "01";

            _title.ImageCode = pcode.Substring(pcode.Length - 4) + ccode.Substring(ccode.Length - 2);
            _title.ProductCode = Helper.XmlGetStringByTag(_title.MetaLocation, "product_code");
            _title.CDN = title.Contains("|Yes");
        }

        public static async Task<string> FindImage(string titleId, string metaFile)
        {
            var str = Titles.Find(x => x.Contains(titleId.ToUpper()));
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            var strs = str.Split('|');
            if (strs.Length < 3 || !strs[2].Contains('-'))
                return string.Empty;

            var pCode = strs[2].Substring(6);

            string imageCode;
            if (!string.IsNullOrEmpty(metaFile)) {
                var cCode = Helper.XmlGetStringByTag(metaFile, "company_code") ?? "01";
                imageCode = pCode.Substring(pCode.Length - 4) + cCode.Substring(cCode.Length - 2);
            }
            else {
                imageCode = pCode.Substring(pCode.Length - 4) + "01";
            }

            var cacheDir = Path.Combine(Settings.ConfigFolder, "cache");
            var cachedFile = Path.Combine(cacheDir, $"{imageCode}.jpg");

            if (!Directory.Exists(cacheDir))
                Directory.CreateDirectory(cacheDir);

            var langCodes = "US,EN,FR,DE,ES,IT,RU,JA,NL,SE,DK,NO,FI".Split(',').ToList();

            foreach (var langCode in langCodes)
            {
                if (File.Exists(cachedFile))
                    return cachedFile;

                try {
                    var url = @"http://" + $@"art.gametdb.com/wiiu/coverHQ/{langCode}/{imageCode}.jpg";
                    File.WriteAllBytes(cachedFile, await WebClient.DownloadDataAsync(url));
                }
                catch {
                    // ignored
                }
            }

            return string.Empty;
        }

        public Task OrganizeTitles()
        {
            return Task.Run(() => {
                foreach (var value in Values) {
                    var fromLocation = value.FolderLocation;
                    var toLocation = Path.Combine(Settings.TitleDirectory, value.ToString());

                    if (!Directory.Exists(fromLocation) || Directory.Exists(toLocation)) continue;

                    Directory.Move(fromLocation, toLocation);
                    value.FolderLocation = toLocation;
                    value.MetaLocation = value.MetaLocation.Replace(fromLocation, toLocation);
                }
            });
        }
    }
}