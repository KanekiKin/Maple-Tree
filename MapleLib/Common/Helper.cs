// Project: MapleSeed-UI
// File: Helper.cs
// Updated By: Jared
// 

using System;
using System.IO;
using System.Reflection;

namespace MapleLib.Common
{
    public static class Helper
    {
        public static void ResolveAssembly()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
                var resourceName = new AssemblyName(args.Name).Name + ".dll";
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)) {
                    if (stream == null) return null;
                    var assemblyData = new byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            };
        }

        public static bool IsFolder(string file)
        {
            return Directory.Exists(file);
        }

        public static string EncryptStr(string text)
        {
            return Crypto.Encrypt(text);
        }

        public static string DecryptStr(string text)
        {
            return Crypto.Decrypt(text);
        }
    }
}