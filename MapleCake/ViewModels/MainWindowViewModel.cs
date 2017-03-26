// Project: MapleUI
// File: MainWindowViewModel.cs
// Updated By: Jared
// 

using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using MapleCake.Models;
using MapleLib;
using MapleLib.Collections;
using MapleLib.Common;
using MapleLib.Network.Web;
using MapleLib.Structs;
using Application = System.Windows.Application;
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
            if (Instance == null)
                Instance = this;

            Init();
        }

        public static MainWindowViewModel Instance { get; private set; }

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

        public Title SelectedItem {
            get { return _selectedItem; }
            set {
                _selectedItem = value;
                SetBackgroundImg(_selectedItem);
                RaisePropertyChangedEvent("SelectedItem");
            }
        }

        public MapleDictionary TitleList => Database.TitleDb;

        public MapleButtons Click { get; set; } = new MapleButtons();

        private void Init()
        {
            SetTitle($"Maple Seed {Settings.Version}");

            SetDefaults();

            InitSettings();

            RegisterEvents();

            new DispatcherTimer(TimeSpan.Zero, DispatcherPriority.ApplicationIdle, OnLoadComplete,
                Application.Current.Dispatcher);
        }

        private void SetTitle(string title)
        {
            Name = title;
            RaisePropertyChangedEvent("Name");
        }

        private void SetDefaults()
        {
            LogBox = string.Empty;
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

        private async void OnLoadComplete(object sender, EventArgs e)
        {
            (sender as DispatcherTimer)?.Stop();

            CheckUpdate();

            await TitleList.Init();
        }

        private static void CheckUpdate()
        {
            AutoUpdaterDotNET.AutoUpdater.Start("https://s3.amazonaws.com/mapletree/mapleseed.xml", "MapleSeed");
            TextLog.MesgLog.WriteLog($"Current Version: {Settings.Version}", Color.Green);
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