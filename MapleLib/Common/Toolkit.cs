// Project: MapleRoot
// File: Toolkit.cs
// Updated By: Jared
// 

#region usings

using System;
using System.IO;
using System.Management;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Timer = System.Timers.Timer;

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

        public static void Sleep(int ms)
        {
            Thread.Sleep(ms);
        }

        public static string TempName()
        {
            return Environment.MachineName;
        }

        public static string UniqueID()
        {
            var drive = DriveInfo.GetDrives()[0].ToString().Replace(":", "").Replace("\\", "");
            var dsk = new ManagementObject(@"win32_logicaldisk.deviceid=""" + drive + @":""");
            dsk.Get();
            var volumeSerial = dsk["VolumeSerialNumber"].ToString();
            return volumeSerial;
        }

        public static byte[] ToBson<T>(T data)
        {
            using (var ms = new MemoryStream()) {
                using (var writer = new BsonWriter(ms)) {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(writer, data);
                    return ms.ToArray();
                }
            }
        }

        public static T FromBson<T>(string file)
        {
            using (var reader = new BsonReader(File.OpenRead(file))) {
                var serializer = new JsonSerializer();
                return serializer.Deserialize<T>(reader);
            }
        }

        public static T FromBson<T>(byte[] data)
        {
            using (var ms = new MemoryStream(data)) {
                using (var reader = new BsonReader(ms)) {
                    var serializer = new JsonSerializer();
                    return serializer.Deserialize<T>(reader);
                }
            }
        }
    }
}