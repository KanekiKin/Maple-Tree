// Project: MapleSeedU
// File: LibraryPath.cs
// Created By: Tsumes <github@tsumes.com>
// Created On: 01/30/2017 6:40 PM

using System.IO;
using System.Windows.Forms;

namespace MapleSeedU.Models.Settings
{
    public class LibraryPath : ConfigEntry
    {
        public LibraryPath() : base("LibraryPath")
        {
        }

        protected override string SetValue()
        {
            return SetDirectoryPath(@"Cemu Library Path (Root folder of Wii U Games)");
        }

        private string SetDirectoryPath(string description)
        {
            var diaglog = new FolderBrowserDialog { Description = description };
            var result = diaglog.ShowDialog();

            if (result == DialogResult.OK)
                _configEntry.Value = Path.GetFullPath(diaglog.SelectedPath);

            return _configEntry.Value;
        }
    }
}