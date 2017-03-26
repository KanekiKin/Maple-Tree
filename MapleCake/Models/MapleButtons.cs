// Project: MapleCake
// File: MapleButtons.cs
// Updated By: Jared
// 

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using MapleCake.ViewModels;
using MapleLib;
using MapleLib.Collections;
using MapleLib.Common;
using MapleLib.Properties;
using MapleLib.Structs;

namespace MapleCake.Models
{
    public class MapleButtons
    {
        private static Title SelectedItem => MainWindowViewModel.Instance.SelectedItem;

        private static string TitleID => MainWindowViewModel.Instance.TitleID;

        public ICommand LaunchCemu => new CommandHandler(LaunchCemuButton);
        public ICommand Download => new CommandHandler(DownloadButton);
        public ICommand AddUpdate => new CommandHandler(AddUpdateButton);
        public ICommand RemoveUpdate => new CommandHandler(RemoveUpdateButton);
        public ICommand TitleIdToClipboard => new CommandHandler(TitleIdToClipboardButton);

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

            MainWindowViewModel.Instance.DownloadCommandEnabled = false;
            RaisePropertyChangedEvent("DownloadCommandEnabled");

            await Database.DownloadTitle(title, title.FolderLocation, title.ContentType, "0");

            await Database.TitleDb.BuildDatabase(false);

            MainWindowViewModel.Instance.DownloadCommandEnabled = true;
            RaisePropertyChangedEvent("DownloadCommandEnabled");
        }

        private async void AddUpdateButton()
        {
            int ver;
            var version = int.TryParse("0", out ver) ? ver.ToString() : "0";
            await DownloadContentClick("Patch", version);
        }

        private async void RemoveUpdateButton()
        {
            if (SelectedItem == null) return;

            await Task.Run(() => {
                var updatePath = Path.Combine(Settings.BasePatchDir, SelectedItem.Lower8Digits);
                var result = MessageBox.Show(string.Format(Resources.ActionWillDeleteAllContent, updatePath),
                    Resources.PleaseConfirmAction, MessageBoxButtons.OKCancel);

                if (result != DialogResult.OK)
                    return;

                SelectedItem.DeleteUpdateContent();
            });
        }

        private static void TitleIdToClipboardButton()
        {
            Clipboard.SetText(SelectedItem.TitleID);
        }

        private static async Task DownloadContentClick(string contentType, string version = "0")
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

        private static void RaisePropertyChangedEvent(string propertyName)
        {
            MainWindowViewModel.Instance.RaisePropertyChangedEvent(propertyName);
        }
    }
}