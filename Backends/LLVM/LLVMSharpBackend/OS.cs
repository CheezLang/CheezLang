using Cheez.Util;
using Microsoft.Win32;
using System.IO;
using System.Linq;
using System.Security.AccessControl;

namespace Cheez.CodeGeneration.OS
{
    public static class OS
    {
        public class CheezWindowsSdk
        {
            public string Version;
            public string Path;
            public string UcrtPath;
            public string UmPath;
        }

        public static string GetLatestSdkVersion(string sdkPath)
        {
            int v0 = 0, v1 = 0, v2 = 0, v3 = 0;
            string version = null;
            foreach (var path in Directory.EnumerateDirectories(sdkPath))
            {
                var v = path.Scan1(@"(\d+).(\d+).(\d+).(\d+)").Select(s => int.TryParse(s, out int r) ? r : 0).ToArray();

                if (v.Length != 4)
                    continue;

                if (v[0] == 10 && v[1] == 0 && v[2] == 10240 && v[3] == 0)
                {
                    // Microsoft released 26624 as 10240 accidentally.
                    // https://developer.microsoft.com/en-us/windows/downloads/sdk-archive
                    v[2] = 26624;
                }

                if ((v[0] > v0) || (v[1] > v1) || (v[2] > v2) || (v[3] > v3))
                {
                    v0 = v[0];
                    v1 = v[1];
                    v2 = v[2];
                    v3 = v[3];
                    version = $"{v[0]}.{v[1]}.{v[2]}.{v[3]}";
                }
            }

            return version;
        }

        public static CheezWindowsSdk FindWindowsSdk()
        {
            using (var localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default))
            using (var roots = localMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Kits\Installed Roots", RegistryRights.ReadKey))
            {
                var sdk = new CheezWindowsSdk();

                sdk.Path = roots.GetValue("KitsRoot10") as string;

                if (sdk.Path == null)
                    return null;

                sdk.Version = GetLatestSdkVersion(Path.Combine(sdk.Path, "bin"));
                if (sdk.Version == null)
                    return null;

                sdk.UcrtPath = Path.Combine(sdk.Path, "Lib", sdk.Version, "ucrt");
                if (!Directory.Exists(sdk.UcrtPath))
                    sdk.UcrtPath = null;

                sdk.UmPath = Path.Combine(sdk.Path, "Lib", sdk.Version, "um");
                if (!Directory.Exists(sdk.UmPath))
                    sdk.UmPath = null;

                return sdk;
            }
        }
    }
}
