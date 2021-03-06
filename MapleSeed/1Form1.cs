﻿// Project: MapleSeed
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
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using MapleLib;
using MapleLib.Collections;
using MapleLib.Common;
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
            Database.TitleDb.AddItemEvent += OnAddTitleEvent;

            TextLog.MesgLog.NewLogEntryEventHandler += MesgLog_NewLogEntryEventHandler;
            TextLog.StatusLog.NewLogEntryEventHandler += StatusLog_NewLogEntryEventHandler;
            WebClient.DownloadProgressChangedEvent += Network_DownloadProgressChangedEvent;
            AppUpdate.Instance.ProgressChangedEventHandler += Instance_ProgressChangedEventHandler;
        }

        private void RegisterDefaults()
        {
            fullScreen.Checked = Settings.FullScreenMode;
            cemu173Patch.Checked = Settings.Cemu173Patch;
            storeEncCont.Checked = Settings.StoreEncryptedContent;

            titleDir.Text = Settings.TitleDirectory;
            cemuDir.Text = Settings.CemuDirectory;
            serverHub.Text = Settings.Hub;

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

        private static void InitSettings()
        {
            if (string.IsNullOrEmpty(Settings.CemuDirectory) ||
                !File.Exists(Path.Combine(Settings.CemuDirectory, "cemu.exe"))) {
                var ofd = new OpenFileDialog
                {
                    CheckFileExists = true,
                    Filter = @"Cemu Excutable |cemu.exe"
                };

                var result = ofd.ShowDialog();
                if (string.IsNullOrWhiteSpace(ofd.FileName) || result != DialogResult.OK) {
                    MessageBox.Show(@"Cemu Directory is required to launch titles.");
                    Settings.CemuDirectory = string.Empty;
                }

                Settings.CemuDirectory = Path.GetDirectoryName(ofd.FileName);
            }

            if (string.IsNullOrEmpty(Settings.TitleDirectory) || !Directory.Exists(Settings.TitleDirectory)) {
                var fbd = new FolderBrowserDialog
                {
                    Description = @"Cemu Title Directory" + Environment.NewLine + @"(Where you store games)"
                };

                var result = fbd.ShowDialog();
                if (string.IsNullOrWhiteSpace(fbd.SelectedPath) || result == DialogResult.Cancel) {
                    MessageBox.Show(@"Title Directory is required. Shutting down.");
                    Application.Exit();
                }

                Settings.TitleDirectory = fbd.SelectedPath;
            }
        }

        private void SetCurrentImage(eShopTitle title)
        {
            title.Image();
            pictureBox1.ImageLocation = title.ImageLocation;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            Database.Load();

            Text = $@"MapleSeed {Assembly.GetEntryAssembly().GetName().Version}";
            MinimumSize = MaximumSize = Size;

            Controls.Cast<Control>().ToList().ForEach(x => x.Enabled = false);

            InitSettings();

            RegisterEvents();

            await Database.LoadLibrary(Settings.TitleDirectory);

            RegisterDefaults();

            CheckUpdate();

            AppendLog($"Game Directory [{Settings.TitleDirectory}]");
            AppendLog(@"Welcome to Maple Tree.");
            AppendLog(@"Enter /help for a list of possible commands.");

            Controls.Cast<Control>().ToList().ForEach(x => x.Enabled = true);
        }

        private void OnAddTitleEvent(object sender, AddItemEventArgs<Title> e)
        {
            var title = e.item;
            if (title == null) return;

            if (pictureBox1.ImageLocation.IsNullOrEmpty())
                SetCurrentImage(title);

            titleList.BeginInvoke(new Action(() => { titleList.Items.Add(title); }));
        }

        private void UpdateProgressBar(int percent, long _toReceive, long _received)
        {
            if (percent <= 0 || _toReceive <= 0 || _received <= 0)
                return;

            try {
                Invoke(new Action(() => progressBar.Value = percent));

                var toReceive = Toolbelt.SizeSuffix(_toReceive);
                var received = Toolbelt.SizeSuffix(_received);

                progressOverlay.Invoke(new Action(() => { progressOverlay.Text = $@"{received} / {toReceive}"; }));
            }
            catch (Exception ex) {
                TextLog.MesgLog.WriteError($"{ex.Message}\n{ex.StackTrace}");
            }
        }

        private void AppendLog(string msg, Color color = default(Color))
        {
            TextLog.MesgLog.WriteLog(msg, color);
        }

        private async Task DownloadContentClick(Control btn, string contentType, string version = "0")
        {
            btn.Enabled = false;

            try {
                foreach (var item in titleList.SelectedItems) {
                    var title = item as Title;
                    if (title == null) continue;

                    switch (contentType) {
                        case "DLC":
                            await title.DownloadDLC();
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
            catch (Exception ex) {
                MessageBox.Show($@"{ex.Message}\n{ex.StackTrace}");
            }

            btn.Enabled = true;
        }

        private void Instance_ProgressChangedEventHandler(object sender, AppUpdate.ProgressChangedEventArgs e)
        {
            if (e == null) return;
            UpdateProgressBar(e.ProgressPercentage, e.TotalBytesReceived, e.BytesReceived);
        }

        private void Network_DownloadProgressChangedEvent(object sender, DownloadProgressChangedEventArgs e)
        {
            if (e == null) return;
            UpdateProgressBar(e.ProgressPercentage, e.TotalBytesToReceive, e.BytesReceived);
        }

        private void MesgLog_NewLogEntryEventHandler(object sender, NewLogEntryEvent e)
        {
            outputTextbox.AppendText(e.Entry, e.Color);
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

        private void playBtn_Click(object sender, EventArgs e)
        {
            var title = titleList.SelectedItem as Title;
            if (title == null) return;

            if (!Toolbelt.LaunchCemu(title.MetaLocation)) return;
            TextLog.MesgLog.WriteLog($"Started playing {title.Name}");
        }

        private async void dlcBtn_Click(object sender, EventArgs e)
        {
            await DownloadContentClick(dlcBtn, "DLC");
        }

        private async void updateBtn_Click(object sender, EventArgs e)
        {
            int ver;
            var version = int.TryParse(titleVersion.Text, out ver) ? ver.ToString() : "0";
            await DownloadContentClick(updateBtn, "Patch", version);
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
                    if (title == null) continue;

                    // ReSharper disable once AssignNullToNotNullAttribute
                    var fullPath = Path.GetFullPath(title.FolderLocation);
                    if (string.IsNullOrEmpty(fullPath))
                        return;

                    if (Directory.Exists(Path.Combine(fullPath, "code")))
                        Directory.Delete(Path.Combine(fullPath, "code"), true);

                    if (Directory.Exists(Path.Combine(fullPath, "content")))
                        Directory.Delete(Path.Combine(fullPath, "content"), true);

                    await title.DownloadContent();
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }

            cleanTitleBtn.Enabled = true;
        }

        private async void newdlbtn_Click(object sender, EventArgs e)
        {
            var titleID = titleIdTextBox.Text;
            if (string.IsNullOrEmpty(titleID)) return;

            Title title;
            if ((title = Database.SearchById(titleID)) == null) return;
            title.FolderLocation = Path.Combine(Settings.TitleDirectory, $"{title}");

            await title.DownloadContent(titleVersion.Text);

            Database.TitleDb.Add(title);
        }

        private void organizeBtn_Click(object sender, EventArgs e)
        {
            foreach (var value in Database.TitleDb)
                AppendLog(Path.Combine(Settings.TitleDirectory, value.ToString()));

            var result = MessageBox.Show(Resources.OrganizeBtn_Click_, Resources.PleaseConfirmAction,
                MessageBoxButtons.OKCancel);

            if (result == DialogResult.OK)
                Database.TitleDb.OrganizeTitles();
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

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            playBtn_Click(null, null);
        }

        private void clearCache_Click(object sender, EventArgs e)
        {
            var deleteDir = Path.Combine(Settings.ConfigDirectory, "cache");
            var result = MessageBox.Show(string.Format(Resources.WillDeleteContents, deleteDir),
                Resources.PleaseConfirmAction, MessageBoxButtons.OKCancel);

            if (result == DialogResult.OK)
                Directory.Delete(deleteDir, true);
        }

        private void fullScreen_CheckedChanged(object sender, EventArgs e)
        {
            Settings.FullScreenMode = fullScreen.Checked;
        }

        private void titleDir_TextChanged(object sender, EventArgs e)
        {
            Settings.TitleDirectory = titleDir.Text;
        }

        private void cemuDir_TextChanged(object sender, EventArgs e)
        {
            Settings.CemuDirectory = cemuDir.Text;
        }

        private void serverHub_TextChanged(object sender, EventArgs e)
        {
            Settings.Hub = serverHub.Text;
        }

        private void cemu173Patch_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Cemu173Patch = cemu173Patch.Checked;
        }

        private void storeEncCont_CheckedChanged(object sender, EventArgs e)
        {
            Settings.StoreEncryptedContent = storeEncCont.Checked;
        }

        private void titleIdTextBox_TextChanged(object sender, EventArgs e)
        {
            if (titleIdTextBox.Text.Length != 16)
                return;

            var title = Database.SearchById(titleIdTextBox.Text);
            if (title == null) return;

            titleName.Text = Toolbelt.RIC(title.Name);

            SetCurrentImage(title);
        }

        private void titleList_SelectedValueChanged(object sender, EventArgs e)
        {
            var title = titleList.SelectedItem as Title;
            if (string.IsNullOrEmpty(title?.ID)) return;

            titleName.Text = title.Name;

            //dlcBtn.Enabled = title.DLC.Count > 0;
            //updateBtn.Enabled = title.Versions.Count > 0;

            var updatesStr = title.Versions.Aggregate(string.Empty,
                (current, update) => current + $"| v{update} ");

            SetCurrentImage(title);

            var tvs = title.GetTitleVersion();
            var l8d = title.Lower8Digits();
            TextLog.StatusLog.WriteLog($"{l8d} | Current Update: v{tvs} | Available Updates: {updatesStr}");
        }

        private void titleList_MouseUp(object sender, MouseEventArgs e)
        {
            var location = titleList.IndexFromPoint(e.Location);
            if (e.Button != MouseButtons.Right)
                return;

            titleList.SelectedIndex = location;
            if (titleList.SelectedItems.Count <= 0)
                return;

            var title = titleList.SelectedItems[0] as Title;
            if (title == null)
                return;

            nameToolStripTextBox1.Text = title.ID;

            var titlePath = Path.Combine(Settings.BasePatchDir, title.Lower8Digits());

            installDLCToolStripMenuItem.Enabled = title.HasDLC;
            uninstallDLCToolStripMenuItem.Enabled = File.Exists(Path.Combine(titlePath, "aoc", "meta", "meta.xml"));

            installUpdateToolStripMenuItem.Enabled = title.Versions.Any();
            uninstallUpdateToolStripMenuItem.Enabled = File.Exists(Path.Combine(titlePath, "meta", "meta.xml"));

            titeListMenuStrip1.Show(MousePosition);
        }

        private async void installDLCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await DownloadContentClick(dlcBtn, "DLC");
        }

        private void uninstallDLCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (titleList.SelectedItems.Count <= 0) return;

            foreach (var titleListSelectedItem in titleList.SelectedItems) {
                var title = titleListSelectedItem as Title;
                if (title == null) continue;

                var updatePath = Path.Combine(Settings.BasePatchDir, title.Lower8Digits());

                var result = MessageBox.Show(string.Format(Resources.ActionWillDeleteAllContent, updatePath),
                    Resources.PleaseConfirmAction, MessageBoxButtons.OKCancel);

                if (result != DialogResult.OK)
                    return;

                title.DeleteAddOnContent();
            }
        }

        private async void installUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int ver;
            var version = int.TryParse(titleVersion.Text, out ver) ? ver.ToString() : "0";
            await DownloadContentClick(updateBtn, "Patch", version);
        }

        private void uninstallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (titleList.SelectedItems.Count <= 0) return;

            foreach (var titleListSelectedItem in titleList.SelectedItems) {
                var title = titleListSelectedItem as Title;
                if (title == null) continue;

                var updatePath = Path.Combine(Settings.BasePatchDir, title.Lower8Digits());

                var result = MessageBox.Show(string.Format(Resources.ActionWillDeleteAllContent, updatePath),
                    Resources.PleaseConfirmAction, MessageBoxButtons.OKCancel);

                if (result != DialogResult.OK)
                    return;

                title.DeleteUpdateContent();
            }
        }

        private void deleteTitleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (titleList.SelectedItems.Count <= 0) return;
            var titleListSelectedItem = titleList.SelectedItems[0];
            var title = titleListSelectedItem as Title;
            if (title == null) return;

            var updatePath = Path.GetFullPath(title.FolderLocation);

            var result = MessageBox.Show(string.Format(Resources.ActionWillDeleteAllContent, updatePath),
                Resources.PleaseConfirmAction, MessageBoxButtons.OKCancel);

            if (result != DialogResult.OK) return;

            title.DeleteContent();

            titleList.Items.Remove(title);
        }
    }
}