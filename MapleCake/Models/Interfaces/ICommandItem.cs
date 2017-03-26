// Project: MapleCake
// File: ICommandItem.cs
// Updated By: Jared
// 

using System.Windows.Input;

namespace MapleCake.Models.Interfaces
{
    public interface ICommandItem
    {
        string Text { get; set; }
        string ToolTip { get; set; }
        ICommand Command { get; set; }
    }
}