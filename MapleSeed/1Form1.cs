// Project: MapleSeed
// File: 1Form1.cs
// Updated By: Jared
// 

#region usings

using System;
using System.Deployment.Application;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using MapleLib;
using MapleLib.Collections;
using MapleLib.Common;
using MapleLib.Network;
using MapleLib.Network.Web;
using MapleLib.Properties;
using MapleLib.Structs;
using DownloadProgressChangedEventArgs = System.Net.DownloadProgressChangedEventArgs;
using WebClient = MapleLib.Network.Web.WebClient;

#endregion

namespace MapleSeed
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void RegisterEvents()
        {
            MapleLoadiine.OnAddTitle += MapleLoadiine_OnAddTitle;

            TextLog.MesgLog.NewLogEntryEventHandler += MesgLog_NewLogEntryEventHandler;
            TextLog.ChatLog.NewLogEntryEventHandler += ChatLog_NewLogEntryEventHandler;
            TextLog.StatusLog.NewLogEntryEventHandler += StatusLog_NewLogEntryEventHandler;
            WebClient.DownloadProgressChangedEvent += Network_DownloadProgressChangedEvent;
            AppUpdate.Instance.ProgressChangedEventHandler += Instance_ProgressChangedEventHandler;

            Toolkit.GlobalTimer.Elapsed += GlobalTimer_Elapsed;
            GlobalTimer_Elapsed(null, null);
        }

        private void RegisterDefaults()
        {
            fullScreen.Checked = Settings.Instance.FullScreenMode;
            cemu173Patch.Checked = Settings.Instance.Cemu173Patch;
            storeEncCont.Checked = Settings.Instance.StoreEncryptedContent;

            titleDir.Text = Settings.Instance.TitleDirectory;
            cemuDir.Text = Settings.Instance.CemuDirectory;
            serverHub.Text = Settings.Instance.Hub;

            discordEmail.Text = Settings.Instance.DiscordEmail;
            discordPass.Text = Settings.Instance.DiscordPass;

            if (!ApplicationDeployment.IsNetworkDeployed) return;
            var ver = ApplicationDeployment.CurrentDeployment?.CurrentVersion;
            if (ver != null) Text = $@"Maple Seed - Version: {ver}";
        }

        private static void CheckUpdate()
        {
            if (!ApplicationDeployment.IsNetworkDeployed) return;
            var curVersion = AppUpdate.Instance.Ad.CurrentVersion.ToString();

            if (AppUpdate.Instance.UpdateAvailable) {
                TextLog.MesgLog.WriteLog($"Current Version: {curVersion}", Color.Chocolate);

                var version = AppUpdate.Instance.Ad.CheckForDetailedUpdate().AvailableVersion.ToString();
                TextLog.MesgLog.WriteLog($"Latest Version: {version}", Color.Chocolate);
                TextLog.MesgLog.WriteLog("Update via the Settings tab.", Color.Chocolate);
            }

            if (!AppUpdate.Instance.UpdateAvailable)
                TextLog.ChatLog.WriteLog($"Current Version: {curVersion}", Color.Green);
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            MinimumSize = MaximumSize = Size;

            MessageBox.Show(Resources.EdgeBuildNotice, @"MapleSeed - Edge Build");

            RegisterEvents();

            TextLog.MesgLog.WriteLog("[Database] Populating Title Library", Color.DarkViolet);

            await Database.TitleDb.Init(Settings.Instance.TitleDirectory);

            var random = new Random().Next(Database.TitleDb.Values.Count);
            await SetCurrentImage(Database.TitleDb.Values.ToArray()[random]);

            RegisterDefaults();
            
            CheckUpdate();

            AppendLog($"Game Directory [{Settings.Instance.TitleDirectory}]");
            AppendChat(@"Welcome to Maple Tree.");
            AppendChat(@"Enter /help for a list of possible commands.");

            Discord.UpdateUserlist(userList);

            await Discord.Connect();
        }

        private void MapleLoadiine_OnAddTitle(object sender, Title e)
        {
            ListBoxAddItem(e);
        }

        private void ListBoxAddItem(object obj)
        {
            var title = obj as Title;

            if (titleList.InvokeRequired) {
                titleList.Invoke(new Action(() => {
                    titleList.Items.Add(title ?? obj);
                }));
            }
            else {
                titleList.Items.Add(title ?? obj);
            }
        }

        private void GlobalTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try {
                username.Invoke(new Action(() => username.Text = Discord.Nickname));
            }
            catch (Exception ex) {
                AppendLog(ex.StackTrace);
            }
        }

        private void UpdateProgressBar(int _progress, long _toReceive, long _received)
        {
            try {
                Invoke(new Action(() => { progressBar.Value = _progress; }));

                var toReceive = Toolbelt.SizeSuffix(_toReceive);
                var received = Toolbelt.SizeSuffix(_received);

                progressOverlay.Invoke(new Action(() => { progressOverlay.Text = $@"{received} / {toReceive}"; }));
            }
            catch (Exception ex) {
                TextLog.MesgLog.WriteError($"{ex.Message}\n{ex.StackTrace}");
                TextLog.StatusLog.WriteError($"{ex.Message}\n{ex.StackTrace}");
            }
        }

        private void Instance_ProgressChangedEventHandler(object sender, AppUpdate.ProgressChangedEventArgs e)
        {
            UpdateProgressBar(e?.ProgressPercentage ?? 0, e?.TotalBytesReceived ?? 0, e?.BytesReceived ?? 0);
        }

        private void Network_DownloadProgressChangedEvent(object sender, DownloadProgressChangedEventArgs e)
        {
            UpdateProgressBar(e?.ProgressPercentage ?? 0, e?.TotalBytesToReceive ?? 0, e?.BytesReceived ?? 0);
        }

        private void ChatLog_NewLogEntryEventHandler(object sender, NewLogEntryEvent e)
        {
            chatbox.AppendText(e.Entry, e.Color);
        }

        private void MesgLog_NewLogEntryEventHandler(object sender, NewLogEntryEvent e)
        {
            richTextBox1.AppendText(e.Entry, e.Color);
        }

        private void StatusLog_NewLogEntryEventHandler(object sender, NewLogEntryEvent e)
        {
            if (status.InvokeRequired) {
                status.Invoke(new Action(() => {
                    status.Text = e.Entry;
                    status.ForeColor = e.Color;
                }));
            }
            else {
                status.Text = e.Entry;
                status.ForeColor = e.Color;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (var proc in Process.GetProcessesByName("CDecrypt"))
                try {
                    proc.Kill();
                }
                catch {
                    // ignored
                }

            Application.Exit();
        }

        private void AppendChat(string msg, Color color = default(Color))
        {
            TextLog.ChatLog.WriteLog(msg, color);
        }

        private void AppendLog(string msg, Color color = default(Color))
        {
            TextLog.MesgLog.WriteLog(msg, color);
        }

        private async Task DownloadContentClick(Control btn, string message, string contentType, string version = "0")
        {
            btn.Enabled = false;

            try {
                if (MessageBox.Show(message, @"Confirm Content Download", MessageBoxButtons.OKCancel) == DialogResult.OK) {

                    foreach (var item in titleList.CheckedItems) {
                        var title = item as Title;
                        if (title == null) continue;

                        switch (contentType) {
                            case "DLC":
                                foreach (var _dlc in title.DLC)
                                    await _dlc.DownloadContent();
                                break;

                            case "Patch":
                                await title.DownloadUpdate(version);
                                break;

                            case "eShop/Application":
                                await title.DownloadContent(version);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }

            btn.Enabled = true;
            titleList.Enabled = true;
        }

        private async void dlcBtn_Click(object sender, EventArgs e)
        {
            await DownloadContentClick(dlcBtn, @"This action may overwrite current DLC files!", "DLC");
        }

        private async void updateBtn_Click(object sender, EventArgs e)
        {
            int ver;
            var version = int.TryParse(titleVersion.Text, out ver) ? ver.ToString() : "0";
            await DownloadContentClick(updateBtn, @"This action will update content files!", "Patch", version);
        }

        private void titleList_DoubleClick(object sender, EventArgs e)
        {
            playBtn_Click(null, null);
        }

        private void fullScreen_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Instance.FullScreenMode = fullScreen.Checked;
        }

        private void username_TextChanged(object sender, EventArgs e)
        {
            Settings.Instance.Username = username.Text;
        }

        private void playBtn_Click(object sender, EventArgs e)
        {
            var title = titleList.SelectedItem as Title;
            if (!Toolbelt.LaunchCemu(title?.Location)) return;
            TextLog.MesgLog.WriteLog($"[{Settings.Instance.Username}] Has started playing {title}!");
        }

        private async void sendChat_Click(object sender, EventArgs e)
        {
            if (chatInput.Text.IsNullOrEmpty()) return;
            var text = chatInput.Text;
            chatInput.Text = string.Empty;

            if (await CheckForCommandInput(text)) return;

            if (Discord.Connected) Discord.SendMessage(text);
        }

        private async Task<bool> CheckForCommandInput(string s)
        {
            return await Task.Run(() => {
                if (s.StartsWith("/dl")) {
                    var titleId = s.Substring(3).Trim();
                    var title = Database.FindByTitleId(titleId);
                    if (title.Id.IsNullOrEmpty()) return false;
                    Invoke(new Action(async () => await title.DownloadContent()));
                    return true;
                }

                if (s.StartsWith("/find")) {
                    var titleStr = s.Substring(5).Trim();
                    var titles = Database.FindTitles(titleStr);
                    foreach (var title in titles)
                        TextLog.MesgLog.WriteLog($"{title}, TitleID: {title.Id}, [{title.ContentType}]", Color.Green);
                    return true;
                }

                if (s.StartsWith("/channel")) {
                    var channel = s.Substring(8).Trim();
                    Discord.SetChannel(channel);
                    return true;
                }

                if (s.StartsWith("/chlist")) {
                    var str = Discord.GetChannelList()
                        .Aggregate(string.Empty, (current, channel) => current + $" | {channel}");
                    TextLog.ChatLog.WriteLog(str);
                    return true;
                }

                if (s.StartsWith("/clear")) {
                    chatbox.Text = string.Empty;
                    return true;
                }

                if (s.StartsWith("/help")) {
                    TextLog.MesgLog.WriteLog("------------------------------------------");
                    TextLog.MesgLog.WriteLog("/dl <title id> - Download the specified title ID from NUS.");
                    TextLog.MesgLog.WriteLog("/find <title name> <region(optional)> - Searches for Title ID based on Title Name.");
                    TextLog.MesgLog.WriteLog("/clear - Clears the current chat log.");
                    TextLog.MesgLog.WriteLog("------------------Discord-----------------");
                    TextLog.MesgLog.WriteLog("/channel <channel name> - Switch your currently active Discord channel.");
                    TextLog.MesgLog.WriteLog("/chlist - Returns a list of available channels.");
                    TextLog.MesgLog.WriteLog("------------------------------------------");
                    return true;
                }

                return false;
            });
        }

        private void titleDir_TextChanged(object sender, EventArgs e)
        {
            Settings.Instance.TitleDirectory = titleDir.Text;
        }

        private void cemuDir_TextChanged(object sender, EventArgs e)
        {
            Settings.Instance.CemuDirectory = cemuDir.Text;
        }

        private void serverHub_TextChanged(object sender, EventArgs e)
        {
            Settings.Instance.Hub = serverHub.Text;
        }

        private void checkUpdateBtn_Click(object sender, EventArgs e)
        {
            try {
                if (!AppUpdate.Instance.UpdateAvailable) return;

                var result = MessageBox.Show(@"Would you like to update?", @"Update Available!", MessageBoxButtons.YesNo);
                if (result != DialogResult.Yes) return;

                AppUpdate.Instance.Update();
            }
            catch (DeploymentDownloadException dde) {
                System.Windows.MessageBox.Show(
                    "Cannot install the latest version of the application.\n\nPlease check your network connection, or try again later. Error: " +
                    dde);
            }
        }

        private void cemu173Patch_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Instance.Cemu173Patch = cemu173Patch.Checked;
        }

        private void discordEmail_TextChanged(object sender, EventArgs e)
        {
            Settings.Instance.DiscordEmail = discordEmail.Text;
        }

        private void discordPass_TextChanged(object sender, EventArgs e)
        {
            Settings.Instance.DiscordPass = discordPass.Text;
        }

        private async void discordConnect_Click(object sender, EventArgs e)
        {
            if (!Discord.Connected)
                await Discord.Connect();
        }

        private void storeEncCont_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Instance.StoreEncryptedContent = storeEncCont.Checked;
        }

        private async void cleanTitleBtn_Click(object sender, EventArgs e)
        {
            try {
                cleanTitleBtn.Enabled = false;
                var result = DialogResult.Cancel;

                foreach (var item in titleList.SelectedItems) {
                    if (result != DialogResult.OK) {
                        var msg =
                            $"WARNING!! WARNING!! WARNING!!\n\nThis task will delete your '{item}' directory and redownload the base title content!";
                        result = MessageBox.Show(msg, $@"Reinstall {item}", MessageBoxButtons.OKCancel);

                        if (result != DialogResult.OK)
                            continue;
                    }

                    var title = item as Title;

                    // ReSharper disable once AssignNullToNotNullAttribute
                    var fullPath = Path.GetDirectoryName(Path.GetDirectoryName(title?.Location));
                    if (string.IsNullOrEmpty(fullPath))
                        return;

                    if (Directory.Exists(Path.Combine(fullPath, "code")))
                        Directory.Delete(Path.Combine(fullPath, "code"), true);

                    if (Directory.Exists(Path.Combine(fullPath, "content")))
                        Directory.Delete(Path.Combine(fullPath, "content"), true);

                    title.Location = fullPath;
                    await title.DownloadContent();
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }

            cleanTitleBtn.Enabled = true;
        }
        
        private async void titleList_SelectedValueChanged(object sender, EventArgs e)
        {
            var title = titleList.SelectedItem as Title;
            if (string.IsNullOrEmpty(title?.Lower8Digits)) {
                dlcBtn.Enabled = updateBtn.Enabled = false;
                return;
            }

            dlcBtn.Enabled = title.DLC.Count > 0;
            updateBtn.Enabled = title.Versions.Count > 0;

            var updatesStr = title.Versions.Aggregate(string.Empty,
                (current, update) => current + $"| v{update.Trim()} ");

            await SetCurrentImage(title);

            TextLog.StatusLog.WriteLog($"{title.Lower8Digits} | Available Updates: {updatesStr}", Color.Green);
        }

        private async Task SetCurrentImage(Title title)
        {
            try {
                var cache = Path.Combine("cache", $"{title.ImageCode}.jpg");
                if (!Directory.Exists(Path.Combine("cache")))
                    Directory.CreateDirectory(Path.Combine("cache"));

                if (!File.Exists(cache)) {
                    var url = @"http://" + $@"art.gametdb.com/wiiu/coverHQ/US/{title.ImageCode}.jpg";
                    File.WriteAllBytes(cache, await WebClient.DownloadData(url));
                }

                pictureBox1.ImageLocation = cache;
            }
            catch (Exception e) {
                TextLog.MesgLog.AddHistory(e.Message);
                pictureBox1.ImageLocation = string.Empty;
            }
        }

        private async void newdlbtn_Click(object sender, EventArgs e)
        {
            var dir = Path.Combine(Settings.Instance.TitleDirectory);
            if (string.IsNullOrEmpty(titleIdTextBox.Text))
                return;

            var title = await MapleLoadiine.BuildTitle(titleIdTextBox.Text, string.Empty, true);
            if (title == null)
                return;

            await Database.DownloadTitle(title, Path.Combine(dir, Toolbelt.RIC(title.Name)), "eShop/Application", "0");

            await Database.TitleDb.BuildDatabase(true);
        }

        private async void titleIdTextBox_TextChanged(object sender, EventArgs e)
        {
            if (titleIdTextBox.Text.Length != 16)
                return;

            var title = await MapleLoadiine.BuildTitle(titleIdTextBox.Text, string.Empty, true);
            if (title == null)
                return;

            //await SetCurrentImage(title);
        }
    }
}