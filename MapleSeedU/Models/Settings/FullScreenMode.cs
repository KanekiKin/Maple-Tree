// Project: MapleSeedU
// File: FullScreen.cs
// Created By: Tsumes <github@tsumes.com>
// Created On: 01/30/2017 9:08 PM

using System;
using MapleSeedU.ViewModels;

namespace MapleSeedU.Models.Settings
{
    public class FullScreenMode : ConfigEntry
    {
        public FullScreenMode() : base("FullScreen")
        {
        }

        public bool Value => _configEntry.Value == "True";

        protected override string SetValue()
        {
            if (MainWindowViewModel.Instance != null)
                return SetStringValue(MainWindowViewModel.Instance.FullScreen.ToString());

            return SetStringValue(false.ToString());
        }

        internal void SetValue(bool value)
        {
            if (MainWindowViewModel.Instance != null)
                SetStringValue(value.ToString());
        }
    }
}