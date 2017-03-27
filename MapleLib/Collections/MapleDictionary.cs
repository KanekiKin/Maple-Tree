// Project: MapleLib
// File: MapleDictionary.cs
// Updated By: Jared
// 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MapleLib.Common;
using MapleLib.Properties;
using MapleLib.Structs;
using Newtonsoft.Json;
using WebClient = MapleLib.Network.Web.WebClient;

namespace MapleLib.Collections
{
    public class MapleDictionary : BindingList<Title>
    {
        public MapleDictionary(string baseDir)
        {
            BaseDir = baseDir;
        }

        public static List<Title> JsonObj { get; private set; }
        private static List<string> Updates => new List<string>(Resources.updates.Split('\n'));
        private static List<string> Titles => new List<string>(Resources.titles.Split('\n'));

        private string BaseDir { get; }

        private List<string> Directories { get; set; }

        public event EventHandler<Title> OnAddTitle;
        public event EventHandler<EventArgs> CompletedInit;

        private void EventAdd(Title value)
        {
            OnAddTitle?.Invoke(this, value);
        }

        public async Task<MapleDictionary> Init()
        {
            return await Task.Run(async () => {
                if (string.IsNullOrEmpty(BaseDir))
                    throw new Exception("MapleDictionary.Init(baseDir) cannot be null");

                await BuildDatabase();

                CompletedInit?.Invoke(this, EventArgs.Empty);
                return this;
            });
        }

        public async Task BuildDatabase(bool notice = true)
        {
            await LoadTitles(notice);

            await LoadAddOns(notice);
        }

        private async Task LoadTitles(bool notice = true)
        {
            if (notice)
                Toolbelt.AppendLog("Loading Titles...", Color.DarkViolet);

            Directories = Directory.GetDirectories(BaseDir, "*", SearchOption.AllDirectories).ToList();
            await Task.Run(() => Directories.ForEach(LoadTitle));

            if (notice)
                Toolbelt.AppendLog($"Loaded Titles: {Count}", Color.DarkViolet);
        }

        private async Task LoadAddOns(bool notice = true)
        {
            if (notice)
                Toolbelt.AppendLog("Loading DLC...", Color.DarkViolet);

            await Task.Run(() => this.ToList().ForEach(LoadAddOn));

            if (notice)
                Toolbelt.AppendLog($"Loaded DLC: {this.Sum(title => title.DLC.Count)}", Color.DarkViolet);
        }

        private async void LoadTitle(string path)
        {
            var fileSystemEntries = Directory.GetFiles(path, "meta.xml").ToList();

            if (!fileSystemEntries.Any())
                return;

            try {
                var fileSystemEntry = fileSystemEntries[0];
                var titleId = Helper.XmlGetStringByTag(fileSystemEntry, "title_id");

                if (string.IsNullOrEmpty(titleId))
                    throw new Exception("MapleDictionary.Init.titleId cannot return null");

                var title = await BuildTitle(titleId, fileSystemEntry);
                if (title == null || title.ContentType != "eShop/Application") {
                    TextLog.MesgLog.AddHistory($"Not a valid eShop/Application - TitleID: {titleId}");
                    return;
                }

                FindImage(title);
                EventAdd(title);
            }
            catch (Exception e) {
                MessageBox.Show($"{e.Message}\n{e.StackTrace}");
            }
        }

        private static void LoadAddOn(Title value)
        {
            var id = value.Lower8Digits;
            var titles = JsonObj.Where(x => x.Lower8Digits == id && x.ContentType == "DLC");

            value.DLC.Clear();
            foreach (var title in titles)
                value.DLC.Add(new Title
                {
                    TitleID = title.TitleID.ToUpper(),
                    TitleKey = title.TitleKey,
                    Name = title.Name,
                    Region = title.Region,
                    Ticket = title.Ticket,
                    FolderLocation = Path.Combine(Settings.BasePatchDir, id, "aoc")
                });
        }

        public static async void FindImage(Title title)
        {
            var titleId = "00050000" + title.TitleID.Substring(8);
            var str = Titles.Find(x => x.Contains(titleId.ToUpper()));
            if (string.IsNullOrEmpty(str))
                return;

            var strs = str.Split('|');
            if (strs.Length < 3 || !strs[2].Contains('-'))
                return;

            var pCode = strs[2].Substring(6);

            string imageCode;
            if (!string.IsNullOrEmpty(title.MetaLocation) && File.Exists(title.MetaLocation)) {
                var cCode = Helper.XmlGetStringByTag(title.MetaLocation, "company_code") ?? "01";
                imageCode = pCode.Substring(pCode.Length - 4) + cCode.Substring(cCode.Length - 2);
            }
            else {
                imageCode = pCode.Substring(pCode.Length - 4) + "01";
            }

            var cacheDir = Path.Combine(Settings.ConfigDirectory, "cache");
            var cachedFile = Path.Combine(cacheDir, $"{imageCode}.jpg");

            if (!Directory.Exists(cacheDir))
                Directory.CreateDirectory(cacheDir);

            var langCodes = "US,EN,FR,DE,ES,IT,RU,JA,NL,SE,DK,NO,FI".Split(',').ToList();

            await Task.Run(() => {
                foreach (var langCode in langCodes) {
                    if (File.Exists(cachedFile)) {
                        title.Image = cachedFile;
                        break;
                    }

                    try {
                        var url = $"http://art.gametdb.com/wiiu/coverHQ/{langCode}/{imageCode}.jpg";

                        if (WebClient.UrlExists(url)) {
                            title.Image = cachedFile;
                            var data = WebClient.DownloadData(url);
                            File.WriteAllBytes(title.Image, data);
                        }
                    }
                    catch {
                        // ignored
                    }
                }
            });
        }

        public static async Task<Title> BuildTitle(string titleId, string location, bool newTitle = false)
        {
            try {
                if (JsonObj == null)
                    JsonObj = await LoadJsonTitles();

                Title title;
                if (
                    (title =
                        JsonObj?.Find(x => string.Equals(x.TitleID, titleId, StringComparison.CurrentCultureIgnoreCase))) ==
                    null)
                    throw new Exception("MapleDictionary.BuildTitleList.jtitle cannot return null");

                title.Name = Toolbelt.RIC(title.Name);

                var folder = newTitle
                    ? Path.Combine(Settings.TitleDirectory, Toolbelt.RIC(title.ToString()))
                    : Path.GetDirectoryName(Path.GetDirectoryName(location));

                if (!Directory.Exists(folder) && !newTitle)
                    throw new Exception("MapleDictionary.BuildTitleList.FolderLocation is not valid");

                title.MetaLocation = Path.Combine(title.FolderLocation = folder, "meta", "meta.xml");

                if (newTitle) {
                    FindImage(title);
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
            if (_title == null) throw new ArgumentNullException(nameof(_title));
            var title = Titles.Find(x => x.Contains(_title.TitleID.ToUpper()));
            if (string.IsNullOrEmpty(title)) return;

            var parts2 = title.Split('|');
            if (parts2.Length < 0) return;

            var pcode = Helper.XmlGetStringByTag(_title.MetaLocation, "product_code") ?? "0000";
            var ccode = Helper.XmlGetStringByTag(_title.MetaLocation, "company_code") ?? "01";

            _title.ImageCode = pcode.Substring(pcode.Length - 4) + ccode.Substring(ccode.Length - 2);
            _title.ProductCode = Helper.XmlGetStringByTag(_title.MetaLocation, "product_code");
            _title.CDN = title.Contains("|Yes");
        }

        private static async Task<List<Title>> LoadJsonTitles()
        {
            var jsonFile = Path.Combine(Settings.ConfigDirectory, "titlekeys.json");

            if (!File.Exists(jsonFile)) {
                var data = await WebClient.DownloadDataAsync("https://wiiu.titlekeys.com" + "/json");
                if (data.Length <= 0)
                    throw new WebException("[Database] Unable to download Wii U title database.");

                File.WriteAllBytes(jsonFile, data);
            }

            var json = Encoding.UTF8.GetString(File.ReadAllBytes(jsonFile));
            return JsonConvert.DeserializeObject<List<Title>>(json);
        }

        public Task OrganizeTitles()
        {
            return Task.Run(() => {
                foreach (var value in this) {
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