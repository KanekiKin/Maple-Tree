// Project: MapleUI
// File: MainWindowViewModel.cs
// Updated By: Jared
// 

using System;
using System.Deployment.Application;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using MapleCake.Properties;
using MapleLib;
using MapleLib.Collections;
using MapleLib.Common;
using MapleLib.Network.Web;
using MapleLib.Structs;
using Application = System.Windows.Application;
using DownloadProgressChangedEventArgs = System.Net.DownloadProgressChangedEventArgs;
using MessageBox = System.Windows.Forms.MessageBox;
using Settings = MapleLib.Settings;
using WebClient = MapleLib.Network.Web.WebClient;

namespace MapleCake.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private Title _selectedItem;
        private string _titleId = "0005000010144F00";

        public MainWindowViewModel()
        {
            Init();
        }

        private short Build => 1;

        public string Name { get; set; }

        public string Status { get; set; }

        public string BackgroundImage { get; set; }

        public string TitleID {
            get { return _titleId; }
            set { titleIdTextChanged(_titleId = value); }
        }

        public int ProgressValue { get; set; }

        public bool DownloadCommandEnabled { get; set; } = true;

        public MapleDictionary TitleList { get; set; }

        public Title SelectedItem {
            get { return _selectedItem; }
            set {
                _selectedItem = value;
                SetBackgroundImg(_selectedItem);
                RaisePropertyChangedEvent("SelectedItem");
            }
        }

        public ICommand DownloadCommand => new CommandHandler(DownloadButton);

        private void Init()
        {
            SetTitle($"Maple Seed 2.1.0.{Build}");

            CheckUpdate();

            InitSettings();

            SetDefaults();

            RegisterEvents();

            var timer = new DispatcherTimer(TimeSpan.Zero, DispatcherPriority.ApplicationIdle, OnLoadComplete,
                Application.Current.Dispatcher);
        }

        private async void OnLoadComplete(object sender, EventArgs e)
        {
            (sender as DispatcherTimer)?.Stop();

            await TitleList.Init(Settings.TitleDirectory);
        }

        private void SetDefaults()
        {
            TitleList = new MapleDictionary();
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

        private async void DownloadButton()
        {
            if (string.IsNullOrEmpty(TitleID))
                return;

            var title = await MapleDictionary.BuildTitle(TitleID, string.Empty, true);
            if (title == null)
                return;

            var str = string.Format(Resources.ConfirmDownload, title.Name);
            var result = MessageBox.Show(str, Resources.Confirmation, MessageBoxButtons.YesNo);

            if (result != DialogResult.Yes)
                return;

            DownloadCommandEnabled = false;
            RaisePropertyChangedEvent("DownloadCommandEnabled");

            await Database.DownloadTitle(title, title.FolderLocation, title.ContentType, "0");

            await Database.TitleDb.BuildDatabase();

            DownloadCommandEnabled = true;
            RaisePropertyChangedEvent("DownloadCommandEnabled");
        }

        private void titleIdTextChanged(string tid)
        {
            if (tid.Length != 16)
                return;

            var title = MapleDictionary.JsonObj.Find(x => string.Equals(x.TitleID, tid, StringComparison.CurrentCultureIgnoreCase));
            if (title == null) return;

            SelectedItem = title;
            RaisePropertyChangedEvent("TitleID");
        }

        private void InstanceOnProgressChangedEventHandler(object sender, AppUpdate.ProgressChangedEventArgs e) {}

        private void WebClientOnDownloadProgressChangedEvent(object sender, DownloadProgressChangedEventArgs e)
        {
            ProgressValue = e.ProgressPercentage;
            RaisePropertyChangedEvent("ProgressValue");
        }

        private void StatusLogOnNewLogEntryEventHandler(object sender, NewLogEntryEvent newLogEntryEvent)
        {
            Status = newLogEntryEvent.Entry;
            RaisePropertyChangedEvent("Status");
        }

        private void MesgLogOnNewLogEntryEventHandler(object sender, NewLogEntryEvent newLogEntryEvent)
        {
            Status = newLogEntryEvent.Entry;
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