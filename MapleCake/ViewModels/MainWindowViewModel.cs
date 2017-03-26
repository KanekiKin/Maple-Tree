// Project: MapleUI
// File: MainWindowViewModel.cs
// Updated By: Jared
// 

using System;
using System.Deployment.Application;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using MapleLib;
using MapleLib.Collections;
using MapleLib.Common;
using MapleLib.Network;
using MapleLib.Network.Web;
using MapleLib.Structs;
using Application = System.Windows.Application;
using DownloadProgressChangedEventArgs = System.Net.DownloadProgressChangedEventArgs;
using WebClient = MapleLib.Network.Web.WebClient;

namespace MapleCake.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _contextVisibility = "Visible";
        private Title _selectedItem;
        private string _titleId = "0005000010144F00";

        public MainWindowViewModel()
        {
            if (Update.IsAvailable())
                Update.StartProcedure();

            Init();
        }

        public string Name { get; set; }

        public string Status { get; set; }

        public string BackgroundImage { get; set; }

        public string ContextVisibility {
            get { return _contextVisibility; }
            set {
                _contextVisibility = value;
                RaisePropertyChangedEvent("ContextVisibility");
            }
        }

        public string TitleID {
            get { return _titleId; }
            set { titleIdTextChanged(_titleId = value); }
        }

        public int ProgressValue { get; set; }

        public bool DownloadCommandEnabled { get; set; } = true;

        public string LogBox { get; set; }

        public MapleDictionary TitleList => Database.TitleDb;

        public Title SelectedItem {
            get { return _selectedItem; }
            set {
                _selectedItem = value;
                SetBackgroundImg(_selectedItem);
                RaisePropertyChangedEvent("SelectedItem");
            }
        }

        public ICommand DownloadCommand => new CommandHandler(DownloadButton);

        public ICommand LaunchCemuCommand => new CommandHandler(LaunchCemuButton);

        public ICommand AddUpdateCommand => new CommandHandler(AddUpdateButton);

        private void LaunchCemuButton()
        {
            if (SelectedItem == null) return;

            if (!Toolbelt.LaunchCemu(SelectedItem.MetaLocation)) return;
            TextLog.MesgLog.WriteLog($"Now Playing: {SelectedItem.Name}");
        }

        private async void DownloadButton()
        {
            if (string.IsNullOrEmpty(TitleID))
                return;

            var title = await MapleDictionary.BuildTitle(TitleID, string.Empty, true);
            if (title == null)
                return;

            DownloadCommandEnabled = false;
            RaisePropertyChangedEvent("DownloadCommandEnabled");

            await Database.DownloadTitle(title, title.FolderLocation, title.ContentType, "0");

            await Database.TitleDb.BuildDatabase(false);

            DownloadCommandEnabled = true;
            RaisePropertyChangedEvent("DownloadCommandEnabled");
        }

        private async void AddUpdateButton()
        {
            int ver;
            var version = int.TryParse("0", out ver) ? ver.ToString() : "0";
            await DownloadContentClick("Patch", version);
        }

        private void Init()
        {
            SetTitle($"Maple Seed {Update.CurrentVersion}");

            SetDefaults();

            CheckUpdate();

            InitSettings();

            RegisterEvents();

            new DispatcherTimer(TimeSpan.Zero, DispatcherPriority.ApplicationIdle, OnLoadComplete,
                Application.Current.Dispatcher);
        }

        private async void OnLoadComplete(object sender, EventArgs e)
        {
            (sender as DispatcherTimer)?.Stop();

            await TitleList.Init();
        }

        private void SetDefaults()
        {
            LogBox = string.Empty;
        }

        private void SetTitle(string title)
        {
            Name = title;
            RaisePropertyChangedEvent("Name");
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
                    System.Windows.Forms.Application.Exit();
                }

                Settings.TitleDirectory = fbd.SelectedPath;
            }
        }

        private void RegisterEvents()
        {
            TitleList.OnAddTitle += MapleDictionaryOnAddTitle;
            TitleList.CompletedInit += MapleDictionaryOnCompletedInit;

            TextLog.MesgLog.NewLogEntryEventHandler += MesgLogOnNewLogEntryEventHandler;
            TextLog.StatusLog.NewLogEntryEventHandler += StatusLogOnNewLogEntryEventHandler;
            WebClient.DownloadProgressChangedEvent += WebClientOnDownloadProgressChangedEvent;
            AppUpdate.Instance.ProgressChangedEventHandler += InstanceOnProgressChangedEventHandler;
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

        private async void SetBackgroundImg(Title title)
        {
            if (string.IsNullOrEmpty(title.Image))
                await Task.Run(() => MapleDictionary.FindImage(title));

            BackgroundImage = title.Image;
            RaisePropertyChangedEvent("BackgroundImage");
        }

        private void titleIdTextChanged(string tid)
        {
            if (tid.Length != 16)
                return;

            var title =
                MapleDictionary.JsonObj.Find(
                    x => string.Equals(x.TitleID, tid, StringComparison.CurrentCultureIgnoreCase));
            if (title == null) return;

            SelectedItem = title;
            RaisePropertyChangedEvent("TitleID");
        }

        private async Task DownloadContentClick(string contentType, string version = "0")
        {
            try {
                if (SelectedItem == null)
                    return;

                switch (contentType) {
                    case "DLC":
                        foreach (var _dlc in SelectedItem.DLC)
                            await _dlc.DownloadContent();
                        break;

                    case "Patch":
                        if (!SelectedItem.Versions.Any()) {
                            MessageBox.Show($@"Update for {SelectedItem.Name} is not available");
                            break;
                        }

                        await SelectedItem.DownloadUpdate(version);
                        break;

                    case "eShop/Application":
                        await SelectedItem.DownloadContent(version);
                        break;
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        private void InstanceOnProgressChangedEventHandler(object sender, AppUpdate.ProgressChangedEventArgs e) {}

        private void WebClientOnDownloadProgressChangedEvent(object sender, DownloadProgressChangedEventArgs e)
        {
            ProgressValue = e?.ProgressPercentage ?? 0;
            RaisePropertyChangedEvent("ProgressValue");
        }

        private void StatusLogOnNewLogEntryEventHandler(object sender, NewLogEntryEvent newLogEntryEvent)
        {
            Status = newLogEntryEvent.Entry;
            RaisePropertyChangedEvent("Status");
        }

        private void MesgLogOnNewLogEntryEventHandler(object sender, NewLogEntryEvent newLogEntryEvent)
        {
            LogBox += Status = newLogEntryEvent.Entry;
            RaisePropertyChangedEvent("LogBox");
            RaisePropertyChangedEvent("Status");
        }

        private void MapleDictionaryOnCompletedInit(object sender, EventArgs eventArgs)
        {
            TextLog.MesgLog.WriteLog($"Game Directory [{Settings.TitleDirectory}]");
        }

        private void MapleDictionaryOnAddTitle(object sender, Title title)
        {
            if (TitleList.Contains(title))
                return;

            TitleList.AddOnUI(title);

            if (SelectedItem != null) return;
            SelectedItem = title;
        }
    }
}