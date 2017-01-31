// Project: MapleSeedU
// File: IConfigEntry.cs
// Created By: Tsumes <github@tsumes.com>
// Created On: 01/30/2017 8:30 PM

using MapleSeedU.Models.Tools;

namespace MapleSeedU.Models.Settings
{
    public abstract class ConfigEntry
    {
        protected ConfigEntry(string name)
        {
            _configEntry = new ConfigurationEntry(Name = name);
        }

        protected ConfigurationEntry _configEntry { get; }

        public string GetValue()
        {
            if (string.IsNullOrEmpty(_configEntry.Value) 
                || !_configEntry.Value.FileOrDirectoryExists())
                return SetValue();
            return _configEntry.Value;
        }

        protected abstract string SetValue();

        protected string SetStringValue(string value)
        {
            return _configEntry.Value = value;
        }

        public void ResetValue()
        {
            _configEntry.DeleteKey(Name);
        }

        private string Name { get; }
    }
}