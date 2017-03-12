// Project: MapleLib
// File: TitleEntry.cs
// Updated By: Jared
// 

using System.Collections.Generic;
using MapleLib.Common;

namespace MapleLib.Structs
{
    public class TitleUpdate
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<string> Versions { get; set; } = new List<string>();
        public string Region { get; set; } = string.Empty;
        public string Lower8Digits => Id.Substring(8).ToUpper();
        
        public override string ToString()
        {
            return Toolbelt.RIC($"{Name}") + $" ({Region})";
        }
    }
}