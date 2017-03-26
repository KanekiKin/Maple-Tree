// Project: MapleCake
// File: MapleContext.cs
// Updated By: Jared
// 

using System.Collections.Generic;
using System.IO;
using System.Linq;
using MapleCake.Models.ContextMenu;
using MapleCake.Models.Interfaces;
using MapleCake.ViewModels;
using MapleLib;
using MapleLib.Structs;

namespace MapleCake.Models
{
    public static class MapleContext
    {
        private static MapleButtons Click => MainWindowViewModel.Instance.Click;

        private static Title SelectedItem => MainWindowViewModel.Instance.SelectedItem;
        
        public static List<ICommandItem> CreateMenu()
        {
            if (SelectedItem == null)
                return null;

            var vers = string.Join(", ", SelectedItem.Versions.ToArray());

            var items = new List<ICommandItem>
            {
                new SimpleCommandItem {Text = SelectedItem.TitleID, ToolTip = vers },
                new SeparatorCommandItem()
            };

            if (SelectedItem.Versions.Any())
                items.Add(new SimpleCommandItem {Text = "[+] Update", ToolTip = "Add Update", Command = Click.AddUpdate});

            var patchDir = Path.Combine(Settings.BasePatchDir, SelectedItem.Lower8Digits);
            var patchMeta = Path.Combine(patchDir, "meta", "meta.xml");

            if (File.Exists(patchMeta))
                items.Add(new SimpleCommandItem { Text = "[-] Update", ToolTip = "Remove Update", Command = Click.RemoveUpdate });

            return items;
        }
    }
}