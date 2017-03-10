// Project: MapleSeed
// File: 1Form1.cs
// Updated By: Jared
// 

#region usings

using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Lidgren.Network;
using MapleLib;
using MapleLib.Common;
using MapleLib.Enums;
using MapleLib.Network;
using MapleLib.Network.Events;
using MapleLib.Properties;
using MapleLib.Structs;
using ProtoBuf;
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
            MapleServer.Init();
        }

        private static bool IsLive { get; set; } = true;

        private static List<string> Library { get; set; }

        private static MapleClient Client { get; set; }

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
                    if (fi.Extension == ".wud" || fi.Extension == ".wux")
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

        private void RegisterMapleClient()
        {
            if (Client != null) return;
            Client = MapleClient.Create();
            Client.OnMessageReceived += ClientOnMessageReceived;
            Client.OnConnected += ClientOnConnected;
            Client.NetClient.Start();
        }

        private void RegisterDefaults()
        {
            fullScreen.Checked = Settings.Instance.FullScreenMode;
            cemu173Patch.Checked = Settings.Instance.Cemu173Patch;

            username.Text = Settings.Instance.Username;
            if (Settings.Instance.Username.IsNullOrEmpty())
                username.Text = Settings.Instance.Username = Toolkit.TempName();

            titleDir.Text = Settings.Instance.TitleDirectory;
            cemuDir.Text = Settings.Instance.CemuDirectory;
            serverHub.Text = Settings.Instance.Hub;

            discordEmail.Text = Settings.Instance.DiscordEmail;
            discordPass.Text = Settings.Instance.DiscordPass;
        }

        private void CheckUpdate()
        {
            if (AppUpdate.Instance.UpdateAvailable)
            {
                var curVersion = AppUpdate.Instance.Ad.CurrentVersion.ToString();
                var version = AppUpdate.Instance.Ad.CheckForDetailedUpdate().AvailableVersion.ToString();

                TextLog.ChatLog.WriteLog($"Current Version: {curVersion}", Color.Chocolate);
                TextLog.ChatLog.WriteLog($"Latest Version: {version}", Color.Chocolate);
                TextLog.ChatLog.WriteLog("Update via the Settings tab.", Color.Chocolate);
            }
            else
            {
                if (AppUpdate.Instance.Ad == null) return;
                var version = AppUpdate.Instance.Ad.CurrentVersion.ToString();
                TextLog.ChatLog.WriteLog($"Current Version: {version}", Color.Green);
            }
        }

        private void UpdateUIModes()
        {
            if (Client == null) return;

            if (Client.NetClient.ConnectionsCount <= 0 && IsLive) {
                Client.Start(Toolbelt.Settings.Hub);
                shareBtn.Enabled = false;
                myUploads.Enabled = false;
                sendChat.Enabled = false;
                username.Enabled = false;
            }
            else {
                shareBtn.Enabled = true;
                myUploads.Enabled = true;
                sendChat.Enabled = true;
                username.Enabled = true;
            }

            connectBtn.BackgroundImage = Client.NetClient.ConnectionStatus == NetConnectionStatus.Connected
                ? Resources.Green_Light.ToBitmap()
                : Resources.Red_Light.ToBitmap();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            MinimumSize = MaximumSize = Size;

            await Database.Initialize();

            RegisterEvents();

            RegisterDefaults();

            RegisterMapleClient();

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
                if (InvokeRequired)
                    Invoke(new Action(UpdateUIModes));
                else UpdateUIModes();

                ReadLibrary();

                if (!Discord.Connected)
                    Client?.Send("", MessageType.Userlist);
            }
            catch (Exception ex) {
                AppendLog(ex.StackTrace);
            }
        }

        private void ClientOnConnected(object sender, EventArgs e)
        {
            Client.UserData = new UserData
            {
                Username = Settings.Instance.Username,
                Serial = Settings.Instance.Serial
            };
            Client.Send(Client.UserData, MessageType.ModUserData);

            GlobalTimer_Elapsed(null, null);

            Toolbelt.AppendLog($"Connected to Hub [{Toolbelt.Settings.Hub}]", Color.DarkGreen);
        }

        private void ClientOnMessageReceived(object sender, OnMessageReceivedEventArgs e)
        {
            var header = e.Header;

            switch (header.Type) {
                case MessageType.Userlist:
                    HandleUserList(header.Data);
                    break;
                case MessageType.ChatMessage:
                    HandleChatMessage(header.Data);
                    break;
                case MessageType.ModUserData:
                    UpdateUsername(e.Header.Data);
                    break;
                case MessageType.StorageUpload:
                    ConfirmStorageUpload(e.Header);
                    break;
                case MessageType.ShaderData:
                    HandleShaderData(e.Header);
                    break;
                case MessageType.ReceiveFile:
                    break;
                case MessageType.RequestDownload:
                    HandleRequestDownload(e.Header);
                    break;
                case MessageType.RequestSearch:
                    HandleRequestSearch(e.Header.Data);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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

        private void MesgLog_NewLogEntryEventHandler(object sender, NewLogEntryEvent e)
        {
            if (richTextBox1.InvokeRequired) {
                richTextBox1.BeginInvoke(new Action(() => {
                    richTextBox1.AppendText(e.Entry, e.Color);
                    richTextBox1.ScrollToCaret();
                }));
            }
            else {
                richTextBox1.AppendText(e.Entry, e.Color);
                richTextBox1.ScrollToCaret();
            }
        }

        private void ChatLog_NewLogEntryEventHandler(object sender, NewLogEntryEvent e)
        {
            if (chatbox.InvokeRequired) {
                chatbox.BeginInvoke(new Action(() => {
                    chatbox.AppendText(e.Entry, e.Color);
                    chatbox.ScrollToCaret();
                }));
            }
            else {
                chatbox.AppendText(e.Entry, e.Color);
                chatbox.ScrollToCaret();
            }
        }

        private void StatusLog_NewLogEntryEventHandler(object sender, NewLogEntryEvent e)
        {
            if (status.InvokeRequired) {
                status.BeginInvoke(new Action(() => {
                    status.Text = e.Entry;
                    status.ForeColor = e.Color;
                }));
            }
            else {
                status.Text = e.Entry;
                status.ForeColor = e.Color;
            }
        }

        private void HandleChatMessage(byte[] data)
        {
            AppendChat($"{Encoding.UTF8.GetString(data)}", Color.MediumSlateBlue);
        }

        private void HandleRequestDownload(MessageHeader header)
        {
            try {
                using (var ms = new MemoryStream(header.Data)) {
                    var sd = Serializer.Deserialize<StorageData>(ms);

                    var saveTo = sd.Shader
                        ? Path.Combine(Toolbelt.Settings.CemuDirectory, "shaderCache", "transferable", sd.Name)
                        : Path.Combine(Toolbelt.Settings.CemuDirectory, "graphicPacks", sd.Name, "rules.txt");

                    File.WriteAllBytes(saveTo, sd.Data);
                    AppendLog($"[{sd.Name}] Download Complete.");
                }
            }
            catch (Exception e) {
                MessageBox.Show(e.Message + Environment.NewLine + e.StackTrace);
            }
        }

        private void HandleRequestSearch(byte[] data)
        {
            List<StorageData> list;
            using (var ms = new MemoryStream(data)) {
                list = Serializer.Deserialize<List<StorageData>>(ms);
            }
            dataGrid1.Invoke(new Action(() => {
                dataGrid1.DataSource = list;
                dataGrid1.Columns.Remove("Data");
                var dataGridViewColumn = dataGrid1.Columns["Serial"];
                if (dataGridViewColumn != null) dataGridViewColumn.HeaderText = @"Owner";
            }));
        }

        private void HandleShaderData(MessageHeader eHeader)
        {
            try {
                using (var ms = new MemoryStream(eHeader.Data)) {
                    var list = Serializer.Deserialize<List<StorageData>>(ms);
                    dataGrid1.Invoke(new Action(() => {
                        dataGrid1.DataSource = list;
                        dataGrid1.Columns.Remove("Data");
                        var dataGridViewColumn = dataGrid1.Columns["Serial"];
                        if (dataGridViewColumn != null) dataGridViewColumn.HeaderText = @"Owner";
                    }));
                }
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        private void HandleUserList(byte[] data)
        {
            return;
            List<string> userlist;
            using (var ms = new MemoryStream(data)) {
                userlist = Serializer.Deserialize<List<string>>(ms);
            }

            userList.Invoke(new Action(() => {
                userList.Items.Clear();

                foreach (var name in userlist)
                    if (!string.IsNullOrEmpty(name))
                        userList.Items.Add(name);
            }));
        }

        private void ListBoxAddItem(object obj)
        {
            if (titleList.InvokeRequired)
                titleList.BeginInvoke(new Action(() => { titleList.Items.Add(obj); }));
            else
                titleList.Items.Add(obj);
        }

        private void connectBtn_Click(object sender, EventArgs e)
        {
            if (!IsLive) Client.Start(Toolbelt.Settings.Hub);
            else Client.Stop();
            IsLive = !IsLive;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Client.IsRunning) Client.Stop();
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
                    MessageBox.Show(@"This action will overwrite pre-existing files!", @"Confirm Update",
                        MessageBoxButtons.OKCancel) == DialogResult.OK) {
                    updateBtn.Enabled = false;
                    foreach (var item in titleList.SelectedItems) {
                        var fullPath = item as string;
                        if (fullPath.IsNullOrEmpty()) continue;

                        // ReSharper disable once AssignNullToNotNullAttribute
                        fullPath = Path.Combine(Toolbelt.Settings.TitleDirectory, fullPath);

                        if (Toolbelt.Database == null) continue;
                        var title = Database.Find(new FileInfo(fullPath).Name);
                        await Toolbelt.Database.UpdateGame(title.TitleID, fullPath);
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

        private void fullTitle_CheckedChanged(object sender, EventArgs e)
        {
            Toolbelt.Settings.DownloadFullTitle = fullTitle.Checked;
            updateBtn.Text = fullTitle.Checked ? "Download" : "Update";
        }

        private void fullScreen_CheckedChanged(object sender, EventArgs e)
        {
            Toolbelt.Settings.FullScreenMode = fullScreen.Checked;
        }

        private void username_TextChanged(object sender, EventArgs e)
        {
            Settings.Instance.Username = username.Text;

            if (Client?.UserData == null) return;
            Client.UserData.Username = username.Text;
            Client.Send(Client.UserData, MessageType.ModUserData);
            Client.Send("", MessageType.Userlist);
        }

        private void UpdateUsername(byte[] data)
        {
            UserData ud;
            using (var ms = new MemoryStream(data)) {
                ud = Serializer.Deserialize<UserData>(ms);
            }
            Client.UserData.Username = ud.Username;
            Settings.Instance.Username = ud.Username;
            username.Invoke(new Action(() => { username.Text = ud.Username; }));
        }

        private void shareBtn_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                CheckFileExists = true,
                Filter = @"Tansferable Data |*.bin;rules.txt",
                InitialDirectory = Toolbelt.Settings.CemuDirectory,
                Multiselect = true
            };
            var result = ofd.ShowDialog();
            if (!File.Exists(ofd.FileName))
                return;

            if (result != DialogResult.OK)
                return;

            var file = Path.GetFullPath(ofd.FileName);

            if (string.IsNullOrWhiteSpace(file))
                return;

            if (new FileInfo(file).Extension == ".txt") {
                var folder = Path.GetDirectoryName(file);
                folder = Path.GetFileName(folder);
                Storage.Upload(Client, file, folder, Toolbelt.Serial, false);
            }
            else {
                if (ofd.FileNames.Length > 0)
                    foreach (var ofdFile in ofd.FileNames)
                        Storage.Upload(Client, ofdFile, Path.GetFileName(ofdFile), Toolbelt.Settings.Serial, true);
                else Storage.Upload(Client, file, Path.GetFileName(file), Toolbelt.Settings.Serial, true);
            }
        }

        private void ConfirmStorageUpload(MessageHeader header)
        {
            using (var ms = new MemoryStream(header.Data)) {
                var sd = Serializer.Deserialize<StorageData>(ms);
                if (sd.Length <= 0) return;

                AppendLog(!sd.Shader
                    ? $"Graphic Pack, {sd.Name} has been uploaded!"
                    : $"Transferable Shader, {sd.Name} has been uploaded!");
            }
        }

        private void playBtn_Click(object sender, EventArgs e)
        {
            var wiiuTitle = titleList.SelectedItem as string;
            if (!Toolbelt.LaunchCemu(wiiuTitle)) return;

            var title = Path.GetFileNameWithoutExtension(wiiuTitle);
            if (title == null || title.Length <= 1) return;
            var msg = $"[{Client.UserData.Username}] Has started playing {title}!";
            Client.Send(msg, MessageType.ChatMessage);
            AppendLog(msg);
        }

        private void refreshBtn_Click(object sender, EventArgs e)
        {
            Client.Send(Toolbelt.Serial, MessageType.ShaderData);
        }

        private void dataGrid1_DoubleClick(object sender, EventArgs e)
        {
            if (dataGrid1.SelectedRows.Count <= 0) return;
            var row = (StorageData) dataGrid1.SelectedRows[0].DataBoundItem;
            AppendLog($"[{row.Name}] Requesting download from storage server.");
            Client.Send(row, MessageType.RequestDownload);
        }

        private void search_TextChanged(object sender, EventArgs e)
        {
            if (search.Text.Length > 2)
                Client.Send(search.Text, MessageType.RequestSearch);
        }

        private async void sendChat_Click(object sender, EventArgs e)
        {
            if (chatInput.Text.IsNullOrEmpty()) return;
            var text = chatInput.Text;
            chatInput.Text = string.Empty;

            if (await CheckForCommandInput(text)) return;

            if (Discord.Connected) {
                Discord.SendMessage(text);
            }
        }

        private async Task<bool> CheckForCommandInput(string s)
        {
            if (s.StartsWith("/dl")) {
                var titleId = s.Substring(3).Trim();
                var title = Database.FindByTitleId(titleId);
                var fullPath = Path.Combine(Settings.Instance.TitleDirectory, title.ToString());
                if (title.TitleID.IsNullOrEmpty()) return false;
                await Toolbelt.Database.UpdateGame(titleId, fullPath, false);
                return true;
            }
            if (s.StartsWith("/find")) {
                var titleStr = s.Substring(5).Trim();
                var titles = Database.FindTitles(titleStr);
                foreach (var title in titles)
                    AppendChat($"{title}, TitleID: {title.TitleID}, [{title.ContentType}]\n");
                return true;
            }
            if (s.StartsWith("/channel"))
            {
                var channel = s.Substring(8).Trim();
                Discord.SetChannel(channel);
                return true;
            }
            if (s.StartsWith("/chlist")) {
                var str = Discord.GetChannelList().Aggregate(string.Empty, (current, channel) => current + $"|{channel}|");
                TextLog.ChatLog.WriteLog(str);
                return true;
            }
            if (s.StartsWith("/clear")) {
                chatbox.Text = string.Empty;
                return true;
            }
            if (s.StartsWith("/help")) {
                AppendChat("------------------------------------------");
                AppendChat("/dl <title id> - Download the specified title ID from NUS.");
                AppendChat("/find <title name> <region(optional)> - Searches for Title ID based on Title Name.");
                AppendChat("/clear - Clears the current chat log.");
                AppendChat("------------------Discord-----------------");
                AppendChat("/channel <channel name> - Switch your currently active Discord channel.");
                AppendChat("/chlist - Returns a list of available channels.");
                AppendChat("------------------------------------------");
                return true;
            }

            return false;
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
    }
}