// Project: MapleSeed
// File: Network.cs
// Updated By: Jared
// 

#region usings

using System;
using System.Net;
using System.Threading.Tasks;

#endregion

namespace MapleLib.Network.Web
{
    public static class WebClient
    {
        private const string WII_USER_AGENT = "wii libnup/1.0";

        public static event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChangedEvent;

        public static async Task DownloadFileAsync(string url, string saveTo)
        {
            var wc = new System.Net.WebClient { Headers = {[HttpRequestHeader.UserAgent] = WII_USER_AGENT } };
            wc.DownloadProgressChanged += DownloadProgressChanged;

            await wc.DownloadFileTaskAsync(new Uri(url), saveTo);
            while (wc.IsBusy) await Task.Delay(100);
            wc.Dispose();
        }

        public static byte[] DownloadData(string url)
        {
            using (var wc = new System.Net.WebClient()) {
                wc.Headers[HttpRequestHeader.UserAgent] = WII_USER_AGENT;
                wc.DownloadProgressChanged += DownloadProgressChanged;
                return wc.DownloadData(url);
            }
        }

        public static async Task<byte[]> DownloadDataAsync(string url)
        {
            using (var wc = new System.Net.WebClient())
            {
                wc.Headers[HttpRequestHeader.UserAgent] = WII_USER_AGENT;
                wc.DownloadProgressChanged += DownloadProgressChanged;
                return await wc.DownloadDataTaskAsync(new Uri(url));
            }
        }

        public static void ResetDownloadProgressChanged()
        {
            DownloadProgressChangedEvent?.Invoke(null, null);
        }

        private static void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            DownloadProgressChangedEvent?.Invoke(sender, e);
        }
    }
}