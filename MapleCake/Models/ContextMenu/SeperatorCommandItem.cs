// Project: MapleCake
// File: SeperatorCommandItem.cs
// Updated By: Jared
// 

using System;
using MapleCake.Models.Interfaces;

namespace MapleCake.Models.ContextMenu
{
    public class SeparatorCommandItem : ICommandItem
    {
        string ICommandItem.Text { get; set; } = string.Empty;

        string ICommandItem.ToolTip { get; set; }

        System.Windows.Input.ICommand ICommandItem.Command { get; set; }
    }
}