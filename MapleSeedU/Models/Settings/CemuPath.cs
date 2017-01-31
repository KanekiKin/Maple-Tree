// Project: MapleSeedU
// File: CemuPath.cs
// Created By: Tsumes <github@tsumes.com>
// Created On: 01/30/2017 6:40 PM

using System.IO;
using System.Windows.Forms;

namespace MapleSeedU.Models.Settings
{
    public class CemuPath : ConfigEntry
    {
        public CemuPath() : base("CemuPath")
        {
        }

        protected override string SetValue()
        {
            return SetFilePath(@"Cemu Executable", @"Cemu Executable (cemu.exe) | cemu.exe");
        }

        private string SetFilePath(string title, string filter)
        {
            var diaglog = new OpenFileDialog { Title = title, Filter = filter };

            var result = diaglog.ShowDialog();

            if (result == DialogResult.OK)
                _configEntry.Value = Path.GetFullPath(diaglog.FileName);

            return _configEntry.Value;
        }
    }
}