using MapleLib.Common;

namespace MapleLib.Structs
{
    public class WiiUTitle
    {
        public string TitleID { get; set; }
        public string TitleKey { get; set; }
        public string Name { get; set; }
        public string Region { get; set; }
        public string Ticket { get; set; }
        public string Lower8Digits => TitleID?.Substring(8).ToUpper();

        public string ContentType {
            get {
                var header = TitleID.Substring(0, 8).ToUpper();

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