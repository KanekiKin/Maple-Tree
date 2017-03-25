using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Windows;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using MapleLib.Common;
using MapleLib.Network.Web;

namespace MapleLib.Network
{
    public static class Update
    {
        private const string BucketName = "mapletree";
        private const string KeyName = "update.bin";
        private const string UpdateUrl = "https://s3.amazonaws.com/mapletree/version.txt";

        private static readonly string _accessKey;
        private static readonly string _secretKey;
        private static readonly string _updateFile = Path.Combine(Settings.ConfigDirectory, KeyName);

        public static readonly string CurrentVersion =
            Assembly.GetExecutingAssembly().GetName().Version.ToString();

        static Update()
        {
            _accessKey = "AKIAJTDWPYC3Y2TVPFYA";
            _secretKey = "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";
        }

        public static bool IsAvailable()
        {
            var version = WebClient.DownloadString(UpdateUrl);
            return CurrentVersion != version;
        }

        public static void StartProcedure()
        {
            //DownloadUpdate();
            
            //await Toolbelt.StartProcess(newLoc, "", "");
        }

        public static void DownloadUpdate()
        {
            var credentials = new BasicAWSCredentials(_accessKey, _secretKey);

            IAmazonS3 client;
            using (client = new AmazonS3Client(credentials, RegionEndpoint.USEast1)) {
                var request = new GetObjectRequest {BucketName = BucketName, Key = KeyName};

                using (var response = client.GetObject(request)) {
                    response.WriteResponseStreamToFile(_updateFile);
                }
            }
        }
    }
}