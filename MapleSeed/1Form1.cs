// Project: MapleSeed
// File: 1Form1.cs
// Updated By: Jared
// 

#region usings

using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using MapleLib;
using MapleLib.Common;
using MapleLib.Network;
using MapleLib.Network.Web;
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

        private static List<string> Library { get; set; }

        private void RegisterEvents()
        {
            TextLog.MesgLog.NewLogEntryEventHandler += MesgLog_NewLogEntryEventHandler;
            TextLog.ChatLog.NewLogEntryEventHandler += ChatLog_NewLogEntryEventHandler;
            TextLog.StatusLog.NewLogEntryEventHandler += StatusLog_NewLogEntryEventHandler;
            WebClient.DownloadProgressChangedEvent += Network_DownloadProgressChangedEvent;
            AppUpdate.Instance.ProgressChangedEventHandler += Instance_ProgressChangedEventHandler;

            Toolkit.GlobalTimer.Elapsed += GlobalTimer_Elapsed;
            GlobalTimer_Elapsed(null, null);
        }

        private void ReadLibrary()
        {
            if (Toolbelt.Settings == null) return;

            var dir = Toolbelt.Settings.TitleDirectory;
            if (dir.IsNullOrEmpty()) return;

            Library = new List<string>(Directory.GetFileSystemEntries(dir));
            foreach (var item in Library) {
                var fi = new FileInfo(item);

                if (fi.Attributes.HasFlag(FileAttributes.Directory)) {
                    if (!File.Exists(Path.Combine(item, "meta", "meta.xml")))
                        continue;

                    if (!titleList.Items.Contains(fi.Name))
                        ListBoxAddItem(fi.Name);
                }
                else if (fi.Attributes.HasFlag(FileAttributes.Archive)) {
                    if (fi.Extension != ".wud" && fi.Extension != ".wux") continue;
                    if (!titleList.Items.Contains(fi.Name))
                        ListBoxAddItem(fi.Name);
                }
            }

            var cache = new object[titleList.Items.Count];
            titleList.Items.CopyTo(cache, 0);

            foreach (var item in cache) {
                var path = Path.Combine(dir, item.ToString());
                if (!Directory.Exists(path) && !File.Exists(path))
                    titleList.Invoke(new Action(() => titleList.Items.Remove(item)));
            }
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

            RegisterEvents();

            titleList.Enabled = false;
            await Database.Initialize();
            titleList.Enabled = true;

            RegisterDefaults();

            CheckUpdate();

            AppendLog($"Game Directory [{Toolbelt.Settings.TitleDirectory}]");
            AppendChat(@"Welcome to Maple Tree.");
            AppendChat(@"Enter /help for a list of possible commands.");

            Discord.UpdateUserlist(userList);

            await Discord.Connect();
        }

        private void GlobalTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try {
                username.Invoke(new Action(() => username.Text = Discord.Nickname));

                ReadLibrary();
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

        private void ListBoxAddItem(object obj)
        {
            if (titleList.InvokeRequired)
                titleList.BeginInvoke(new Action(() => { titleList.Items.Add(obj); }));
            else
                titleList.Items.Add(obj);
        }

        private void titleList_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.Graphics.DrawString(titleList.Items[e.Index].ToString(), titleList.Font, Brushes.Black, e.Bounds);
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

        private async void updateBtn_Click(object sender, EventArgs e)
        {
            try {
                if (
                    MessageBox.Show(@"This action will download update content files!", @"Confirm Update",
                        MessageBoxButtons.OKCancel) == DialogResult.OK) {
                    updateBtn.Enabled = false;
                    foreach (var item in titleList.SelectedItems) {
                        var fullPath = item as string;
                        if (fullPath.IsNullOrEmpty()) continue;

                        // ReSharper disable once AssignNullToNotNullAttribute
                        fullPath = Path.Combine(Toolbelt.Settings.TitleDirectory, fullPath);

                        if (Database.TitleDbObject == null) continue;
                        var title = Database.Find(new FileInfo(fullPath).Name);
                        await Database.UpdateGame(title.Id, fullPath, "Patch", titleVersion.Text);
                    }
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }

            updateBtn.Enabled = true;
            titleList.Enabled = true;
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            playBtn_Click(null, null);
        }

        private void fullScreen_CheckedChanged(object sender, EventArgs e)
        {
            Toolbelt.Settings.FullScreenMode = fullScreen.Checked;
        }

        private void username_TextChanged(object sender, EventArgs e)
        {
            Settings.Instance.Username = username.Text;
        }

        private void playBtn_Click(object sender, EventArgs e)
        {
            var title = titleList.SelectedItem as string;
            if (!Toolbelt.LaunchCemu(title)) return;
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
                    var fullPath = Path.Combine(Settings.Instance.TitleDirectory, title.ToString());
                    if (title.Id.IsNullOrEmpty()) return false;
                    Invoke(new Action(async () => await Database.UpdateGame(title.Id, fullPath, "eShop/Application")));
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
            Toolbelt.Settings.TitleDirectory = titleDir.Text;
        }

        private void cemuDir_TextChanged(object sender, EventArgs e)
        {
            Toolbelt.Settings.CemuDirectory = cemuDir.Text;
        }

        private void serverHub_TextChanged(object sender, EventArgs e)
        {
            Toolbelt.Settings.Hub = serverHub.Text;
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

        private async void dlcBtn_Click(object sender, EventArgs e)
        {
            try {
                if (MessageBox.Show(@"This action will overwrite pre-existing files!",
                        @"Confirm DLC Download", MessageBoxButtons.OKCancel) == DialogResult.OK) {
                    dlcBtn.Enabled = false;

                    foreach (var item in titleList.SelectedItems) {
                        var fullPath = item as string;
                        if (fullPath.IsNullOrEmpty()) continue;

                        // ReSharper disable once AssignNullToNotNullAttribute
                        fullPath = Path.Combine(Toolbelt.Settings.TitleDirectory, fullPath);

                        if (Database.TitleDbObject == null) continue;
                        var title = Database.Find(new FileInfo(fullPath).Name);
                        await Database.UpdateGame(title.Id, fullPath, "DLC");
                    }
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }

            dlcBtn.Enabled = true;
        }

        private void storeEncCont_CheckedChanged(object sender, EventArgs e)
        {
            Toolbelt.Settings.StoreEncryptedContent = storeEncCont.Checked;
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

                    var fullPath = item as string;
                    if (fullPath.IsNullOrEmpty()) continue;

                    // ReSharper disable once AssignNullToNotNullAttribute
                    fullPath = Path.Combine(Toolbelt.Settings.TitleDirectory, fullPath);
                    var title = Database.Find(Path.GetFileName(fullPath));

                    if (Directory.Exists(Path.Combine(fullPath, "code")))
                        Directory.Delete(Path.Combine(fullPath, "code"), true);

                    if (Directory.Exists(Path.Combine(fullPath, "content")))
                        Directory.Delete(Path.Combine(fullPath, "content"), true);

                    await Database.UpdateGame(title.Id, fullPath, "eShop/Application");
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }

            cleanTitleBtn.Enabled = true;
        }

        private void titleList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = titleList.SelectedItem as string;
            var title = Database.Find(item);
            if (string.IsNullOrEmpty(title?.Lower8Digits)) return;

            dlcBtn.Enabled = Database.HasDLC(title.Id);
            updateBtn.Enabled = Database.HasUpdates(title.Id);

            var compare = StringComparison.CurrentCultureIgnoreCase;
            var results = Database.TitleDbObject.Where(t => string.Equals(t.Lower8Digits, title.Lower8Digits, compare));
            results = new List<Title>(results);

            var titleUpdates = results.ToArray();
            if (!titleUpdates.Any()) return;

            var updatesStr = titleUpdates[0].Versions.Aggregate(string.Empty,
                (current, update) => current + $"| v{update.Trim()} ");
            TextLog.StatusLog.WriteLog($"{titleUpdates[0].Lower8Digits} | Available Updates: {updatesStr}", Color.Green);
        }
    }
}