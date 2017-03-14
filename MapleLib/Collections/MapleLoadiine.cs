// Project: MapleLib
// File: MapleLoadiine.cs
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
    public class MapleLoadiine : Dictionary<string, Title>
    {
        private static List<WiiUTitle> _jsonObj;
        private static readonly List<string> Updates = new List<string>(Resources.updates.Split('\n'));
        private static readonly List<string> Titles = new List<string>(Resources.titles.Split('\n'));

        private string _baseDir { get; set; }

        private List<string> _directories { get; set; }

        public async Task<MapleLoadiine> Init(string baseDir)
        {
            if (string.IsNullOrEmpty(baseDir))
                throw new Exception("MapleLoadiine.Init(baseDir) cannot be null");

            _baseDir = baseDir;

            await BuildDatabase();

            return this;
        }

        public void Add(Title title)
        {
            Add(title.Id, title);
            OnAddTitle?.Invoke(this, title);
        }

        public static event EventHandler<Title> OnAddTitle;

        public async Task BuildDatabase(bool notice = true)
        {
            await Task.Run(() => {
                _directories = Directory.GetDirectories(_baseDir).ToList();

                foreach (var loadiineDirectory in _directories) BuildTitleList(loadiineDirectory);

                if (notice)
                    Toolbelt.AppendLog($"[Database] [+] Loadiine Titles: {Count}", Color.DarkViolet);
            });

            await Task.Run(() => {
                BuildDLCList();
                var count = Values.Sum(title => title.DLC.Count);

                if (notice)
                    Toolbelt.AppendLog($"[Database] [+] DLC Titles: {count}", Color.DarkViolet);
            });
        }

        private void BuildDLCList()
        {
            foreach (var value in Values) {
                var id = value.Lower8Digits;
                var wtitles = _jsonObj.Where(x => x.Lower8Digits == id && x.ContentType == "DLC");

                foreach (var wiiUTitle in wtitles)
                    value.DLC.Add(new Title
                    {
                        Id = wiiUTitle.TitleID.ToUpper(),
                        Key = wiiUTitle.TitleKey,
                        Name = wiiUTitle.Name,
                        Region = wiiUTitle.Region,
                        WTKTicket = wiiUTitle.Ticket == "1",
                        Location = Path.Combine(Title.BasePatchDir, id, "aoc")
                    });
            }
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
                        throw new Exception("MapleLoadiine.Init.titleId cannot return null");

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

                        var data = await WebClient.DownloadData("https://wiiu.titlekeys.com" + "/json");

                        if (data.Length <= 0)
                            throw new WebException("[Database] Unable to download Wii U title database.");

                        File.WriteAllBytes(jsonFile, data);
                    }

                    var json = Encoding.UTF8.GetString(File.ReadAllBytes(jsonFile));
                    _jsonObj = JsonConvert.DeserializeObject<List<WiiUTitle>>(json);
                }

                WiiUTitle jtitle;
                if ((jtitle = _jsonObj?.Find(x => x.TitleID.ToUpper() == titleId.ToUpper())) == null)
                    throw new Exception("MapleLoadiine.BuildTitleList.jtitle cannot return null");

                var _title = new Title
                {
                    Id = titleId,
                    Location = location,
                    Key = jtitle.TitleKey,
                    Name = jtitle.Name,
                    Region = jtitle.Region,
                    WTKTicket = jtitle.Ticket == "1"
                };

                if (newTitle)
                    return _title;

                var title = Titles.Find(x => x.Contains(_title.Id.ToUpper()));
                if (!string.IsNullOrEmpty(title)) {
                    var parts2 = title.Split('|');
                    if (parts2.Length >= 0) {
                        var pcode = Helper.XmlGetStringByTag(location, "product_code") ?? "0000";
                        var ccode = Helper.XmlGetStringByTag(location, "company_code") ?? "00";

                        _title.ImageCode = pcode.Substring(pcode.Length - 4) + ccode.Substring(ccode.Length - 2);
                        _title.ProductCode = $"{Helper.XmlGetStringByTag(location, "product_code")}";
                        _title.CDN = title.Contains("|Yes");
                    }
                }

                var update = Updates.Find(x => x.Contains(_title.Lower8Digits));
                if (string.IsNullOrEmpty(update)) return _title;
                var parts1 = update.Split('|');
                if (parts1.Length >= 0) _title.Versions = parts1[2].Split(',').Select(s => s.Trim()).ToList();

                return _title;
            }
            catch (Exception e) {
                Toolbelt.AppendLog($"{e.Message}\n{e.StackTrace}");
            }

            return null;
        }
    }
}