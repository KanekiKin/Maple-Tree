﻿// Project: MapleLib
// File: TitleEntry.cs
// Updated By: Jared
// 

using System.Collections.Generic;
using MapleLib.Common;

namespace MapleLib.Structs
{
    public class Title
    {
        public string Id { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public bool CDN { get; set; }
        public string Upper8Digits => Id.Length >= 8 ? Id.Substring(0, 8).ToUpper() : string.Empty;
        public string Lower8Digits => Id.Length >= 8 ? Id.Substring(8).ToUpper() : string.Empty;
        public List<string> Versions { get; set; } = new List<string>();
        public string ContentType
        {
            get
            {
                var header = Id.Substring(0, 8).ToUpper();

                switch (header)
                {
                    case "00050010":
                    case "0005001B":
                        return "System Application";

                    case "00050000":
                        return "eShop/Application";

                    case "00050002":
                        return "Demo";

                    case "0005000E":
                        return "Patch";

                    case "0005000C":
                        return "DLC";
                }

                return "Unknown";
            }
        }

        public override string ToString()
        {
            return Toolbelt.RIC($"{Name}") + $" ({Region})";
        }
    }
}