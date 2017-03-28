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
        public MainWindowViewModel()
        {
            if (Config == null)
                Config = new ViewModelConfig(this);

            if (Instance == null)
                Instance = this;

            Init();
        }

        public static MainWindowViewModel Instance { get; private set; }

        public MapleButtons Click { get; set; } = new MapleButtons();

        public ViewModelConfig Config { get; }

        private void Init()
        {
            SetTitle($"MapleSeed {Settings.Version}");

            SetDefaults();

            InitSettings();

            RegisterEvents();

            new DispatcherTimer(TimeSpan.Zero, DispatcherPriority.ApplicationIdle, OnLoadComplete,
                Application.Current.Dispatcher);
        }

        private void SetTitle(string title)
        {
            Config.Name = title;
            Config.RaisePropertyChangedEvent("Name");
        }

        private void SetDefaults()
        {
            Config.LogBox = string.Empty;
        }

        private void InitSettings()
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
            Config.TitleList.AddTitleEvent += MapleDictionaryOnAddTitle;
            Config.TitleList.CompletedInitEvent += MapleDictionaryOnCompletedInit;

            TextLog.MesgLog.NewLogEntryEventHandler += MesgLogOnNewLogEntryEventHandler;
            TextLog.StatusLog.NewLogEntryEventHandler += StatusLogOnNewLogEntryEventHandler;
            WebClient.DownloadProgressChangedEvent += WebClientOnDownloadProgressChangedEvent;
            AppUpdate.Instance.ProgressChangedEventHandler += InstanceOnProgressChangedEventHandler;
        }

        private static void CheckUpdate()
        {
            AutoUpdaterDotNET.AutoUpdater.Start("https://s3.amazonaws.com/mapletree/mapleseed.xml", "MapleSeed");
            TextLog.MesgLog.WriteLog($"Current Version: {Settings.Version}", Color.Green);
        }

        public async void SetBackgroundImg(Title title)
        {
            if (string.IsNullOrEmpty(title.Image))
                await Task.Run(() => MapleDictionary.FindImage(title));

            Config.BackgroundImage = title.Image;
            Config.RaisePropertyChangedEvent("BackgroundImage");
        }

        public void titleIdTextChanged(string tid)
        {
            if (tid.Length != 16)
                return;

            var title =
                MapleDictionary.JsonObj.Find(
                    x => string.Equals(x.TitleID, tid, StringComparison.CurrentCultureIgnoreCase));
            if (title == null) return;

            Config.SelectedItem = title;
            RaisePropertyChangedEvent("TitleID");
        }

        public void WriteVersions(Title title)
        {
            var result = string.Join(", ", title.Versions.ToArray());
            TextLog.StatusLog.WriteLog($"Versions: {result}");
        }

        private async void OnLoadComplete(object sender, EventArgs e)
        {
            (sender as DispatcherTimer)?.Stop();

            CheckUpdate();

            await Config.TitleList.Init();
        }

        private void InstanceOnProgressChangedEventHandler(object sender, AppUpdate.ProgressChangedEventArgs e) {}

        private void WebClientOnDownloadProgressChangedEvent(object sender, DownloadProgressChangedEventArgs e)
        {
            Config.ProgressValue = e?.ProgressPercentage ?? 0;
            Config.RaisePropertyChangedEvent("ProgressValue");
        }

        private void StatusLogOnNewLogEntryEventHandler(object sender, NewLogEntryEvent newLogEntryEvent)
        {
            Config.Status = newLogEntryEvent.Entry;
            Config.RaisePropertyChangedEvent("Status");
        }

        private void MesgLogOnNewLogEntryEventHandler(object sender, NewLogEntryEvent newLogEntryEvent)
        {
            Config.LogBox += Config.Status = newLogEntryEvent.Entry;
            Config.RaisePropertyChangedEvent("LogBox");
            Config.RaisePropertyChangedEvent("Status");
        }

        private void MapleDictionaryOnCompletedInit(object sender, EventArgs eventArgs)
        {
            TextLog.MesgLog.WriteLog($"Game Directory [{Settings.TitleDirectory}]");
        }

        private void MapleDictionaryOnAddTitle(object sender, Title title)
        {
            if (Config.TitleList.Contains(title))
                return;

            Config.TitleList.AddOnUI(title);

            if (Config.SelectedItem != null) return;
            Config.SelectedItem = title;
        }
    }
}