// Project: MapleSeed
// File: 1Form1.cs
// Updated By: Jared
// 

#region usings

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
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
using WebClient = MapleLib.Network.WebClient;

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

        private async void Form1_Load(object sender, EventArgs e)
        {
            TextLog.MesgLog.NewLogEntryEventHandler += MesgLog_NewLogEntryEventHandler;
            TextLog.ChatLog.NewLogEntryEventHandler += ChatLog_NewLogEntryEventHandler;
            TextLog.StatusLog.NewLogEntryEventHandler += StatusLog_NewLogEntryEventHandler;
            WebClient.DownloadProgressChangedEvent += Network_DownloadProgressChangedEvent;

            await Database.Initialize();

            if (Client == null) {
                Client = MapleClient.Create();
                Client.OnMessageReceived += ClientOnMessageReceived;
                Client.OnConnected += ClientOnConnected;
                Client.NetClient.Start();
            }

            MinimumSize = MaximumSize = Size;
            Text += Toolbelt.Version;
            Text += $@" - Serial {Toolbelt.Settings.Serial}";

            ReadLibrary();

            fullScreen.Checked = Settings.Instance.FullScreenMode;

            username.Text = Settings.Instance.Username;
            if (Settings.Instance.Username.IsNullOrEmpty())
                username.Text = Settings.Instance.Username = Toolkit.TempName();

            Toolkit.GlobalTimer.Elapsed += GlobalTimer_Elapsed;
            GlobalTimer_Elapsed(null, null);

            chatbox.Text += @"Welcome to Maple Tree." + Environment.NewLine;
            chatbox.Text += @"Enter /help for a list of poossible commands.";
            chatbox.Text += Environment.NewLine;
            AppendLog($"Game Directory [{Toolbelt.Settings.TitleDirectory}]");

            titleDir.Text = Toolbelt.Settings.TitleDirectory;
            cemuDir.Text = Toolbelt.Settings.CemuDirectory;
            serverHub.Text = Toolbelt.Settings.Hub;
        }

        private void ReadLibrary()
        {
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

        private void UpdateUIModes()
        {
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

        private void GlobalTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try {
                if (InvokeRequired)
                    Invoke(new Action(UpdateUIModes));
                else UpdateUIModes();

                ReadLibrary();
                Client.Send("", MessageType.Userlist);
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

        private void Network_DownloadProgressChangedEvent(object sender, DownloadProgressChangedEventArgs e)
        {
            try {
                Invoke(new Action(() => { progressBar.Value = e?.ProgressPercentage ?? 0; }));

                var received = Toolbelt.SizeSuffix(e?.BytesReceived ?? 0);
                var toReceive = Toolbelt.SizeSuffix(e?.TotalBytesToReceive ?? 0);

                progressOverlay.Invoke(new Action(() => { progressOverlay.Text = $@"{received} / {toReceive}"; }));
            }
            catch (Exception ex) {
                TextLog.MesgLog.WriteError($"{ex.Message}\n{ex.StackTrace}");
                TextLog.StatusLog.WriteError($"{ex.Message}\n{ex.StackTrace}");
            }
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
            var msg = Encoding.UTF8.GetString(data);
            AppendChat(msg);
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
            TextLog.ChatLog.WriteLog(msg + '\n', color);
        }

        private void AppendLog(string msg, Color color = default(Color))
        {
            TextLog.MesgLog.WriteLog(msg, color);
        }

        public void SetStatus(string msg, Color color = default(Color))
        {
            TextLog.StatusLog.WriteLog(msg);
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

            if (Client.UserData == null) return;
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
            var msg = $"[{DateTime.UtcNow:T}][{Client.UserData.Username}] Has started playing {title}!";
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

            if (await CheckForCommandInput(chatInput.Text)) {
                chatInput.Text = string.Empty;
            }
            else {
                if (Client.NetClient.ServerConnection == null) return;
                Client.Send($"[{DateTime.UtcNow:T}][{username.Text}]: {chatInput.Text}", MessageType.ChatMessage);
                chatInput.Text = string.Empty;
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
                    AppendChat($"{title}, TitleID: {title.TitleID}, [{title.GetTypeAttribute}]\n");
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
    }
}