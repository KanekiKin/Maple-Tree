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
        string ICommandItem.Text {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        string ICommandItem.ToolTip {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        System.Windows.Input.ICommand ICommandItem.Command {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }
    }
}