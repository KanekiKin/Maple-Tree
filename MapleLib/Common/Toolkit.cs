// Project: MapleRoot
// File: Toolkit.cs
// Updated By: Jared
// 

#region usings

using System.IO;
using System.Management;
using System.Timers;

#endregion

namespace MapleLib.Common
{
    public static class Toolkit
    {
        static Toolkit()
        {
            if (GlobalTimer == null)
                GlobalTimer = new Timer();

            GlobalTimer.AutoReset = true;
            GlobalTimer.Interval = 500;
            GlobalTimer.Start();
        }

        public static Timer GlobalTimer { get; }

        public static string UniqueID()
        {
            var drive = DriveInfo.GetDrives()[0].ToString().Replace(":", "").Replace("\\", "");
            var dsk = new ManagementObject(@"win32_logicaldisk.deviceid=""" + drive + @":""");
            dsk.Get();
            var volumeSerial = dsk["VolumeSerialNumber"].ToString();
            return volumeSerial;
        }
    }
}