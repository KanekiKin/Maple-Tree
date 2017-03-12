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
        public string Title_ID { get; set; } = string.Empty;
        public string Title_Name { get; set; } = string.Empty;
        public List<string> Versions { get; set; } = new List<string>();
        public string Region { get; set; } = string.Empty;
        public string Lower8Digits => Title_ID.Substring(8).ToUpper();
        
        public override string ToString()
        {
            return Toolbelt.RIC($"{Title_Name}") + $" ({Region})";
        }
    }
}