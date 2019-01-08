using Cheez.Compiler;
using Cheez.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Cheez.CodeGeneration.LLVMCodeGen
{
    class VsWhere
    {
#pragma warning disable 0649 // #warning directive
        public string instanceId;
        public string installDate;
        public string installationName;
        public string installationPath;
        public string installationVersion;
        public string isPrerelease;
        public string displayName;
        public string description;
        public string enginePath;
        public string channelId;
        public string channelPath;
        public string channelUri;
        public string releaseNotes;
        public string thirdPartyNotices;
#pragma warning restore CS0649 // #warning directive
    }

    public class LLVMLinker
    {
        [DllImport("LLVMLinker", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static bool llvm_link_coff(string[] argv, int argc);

        [DllImport("LLVMLinker", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static bool llvm_link_elf(string[] argv, int argc);

        public static bool Link(Workspace workspace, string targetFile, string objFile, IEnumerable<string> libraryIncludeDirectories, IEnumerable<string> libraries, string subsystem, IErrorHandler errorHandler)
        {
            foreach (var f in workspace.Files)
            {
                libraries = libraries.Concat(f.Libraries);
            }
            libraries = libraries.Distinct();

            var winSdk = OS.OS.FindWindowsSdk();
            if (winSdk == null)
            {
                errorHandler.ReportError("Couldn't find windows sdk");
                return false;
            }

            var msvcLibPath = FindVisualStudioLibraryDirectory();
            if (winSdk == null)
            {
                errorHandler.ReportError("Couldn't find Visual Studio library directory");
                return false;
            }

            var filename = Path.GetFileNameWithoutExtension(targetFile + ".x");
            var dir = Path.GetDirectoryName(Path.GetFullPath(targetFile));

            filename = Path.Combine(dir, filename);

            var lldArgs = new List<string>();
            lldArgs.Add("lld");
            lldArgs.Add($"/out:{filename}.exe");

            // library paths
            if (winSdk.UcrtPath != null)
                lldArgs.Add($@"-libpath:{winSdk.UcrtPath}\x86");

            if (winSdk.UmPath != null)
                lldArgs.Add($@"-libpath:{winSdk.UmPath}\x86");

            var exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            lldArgs.Add($"-libpath:{exePath}");

            if (msvcLibPath != null)
                lldArgs.Add($@"-libpath:{msvcLibPath}\x86");

            foreach (var linc in libraryIncludeDirectories)
            {
                lldArgs.Add($@"-libpath:{linc}");
            }

            lldArgs.Add($@"-libpath:{Environment.CurrentDirectory}\lib"); // @hack
            lldArgs.Add($@"-libpath:{exePath}\lib");

            // other options
            lldArgs.Add("/entry:mainCRTStartup");
            lldArgs.Add("/machine:X86");
            lldArgs.Add($"/subsystem:{subsystem}");

            // runtime
            lldArgs.Add("clang_rt.builtins-i386.lib");

            // windows and c libs
            lldArgs.Add("libucrtd.lib");
            lldArgs.Add("libcmtd.lib");

            lldArgs.Add("kernel32.lib");
            lldArgs.Add("user32.lib");
            lldArgs.Add("gdi32.lib");
            lldArgs.Add("winspool.lib");
            lldArgs.Add("comdlg32.lib");
            lldArgs.Add("advapi32.lib");
            lldArgs.Add("shell32.lib");
            lldArgs.Add("ole32.lib");
            lldArgs.Add("oleaut32.lib");
            lldArgs.Add("uuid.lib");
            lldArgs.Add("odbc32.lib");
            lldArgs.Add("odbccp32.lib");

            lldArgs.Add("legacy_stdio_definitions.lib");
            lldArgs.Add("legacy_stdio_wide_specifiers.lib");
            lldArgs.Add("libclang.lib");
            lldArgs.Add("libvcruntimed.lib");
            lldArgs.Add("msvcrtd.lib");

            foreach (var linc in libraries)
            {
                lldArgs.Add(linc);
            }

            // generated object files
            lldArgs.Add(objFile);

            var result = llvm_link_coff(lldArgs.ToArray(), lldArgs.Count);
            if (result)
            {
                Console.WriteLine($"Generated {filename}.exe");
            }
            else
            {
                Console.WriteLine($"Failed to link");
            }

            return result;
        }


        private static (int, string) FindVSInstallDirWithVsWhere()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                var programFilesX86 = Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%");
                var p = Utilities.StartProcess($@"{programFilesX86}\Microsoft Visual Studio\Installer\vswhere.exe", "-nologo -latest -format json", stdout:
                    (sender, e) =>
                    {
                        sb.AppendLine(e.Data);
                    });
                p.WaitForExit();

                var versions = Newtonsoft.Json.JsonConvert.DeserializeObject<VsWhere[]>(sb.ToString());

                if (versions?.Length == 0)
                    return (-1, null);

                var latest = versions[0];

                var v = latest.installationVersion.Scan1(@"(\d+)\.(\d+)\.(\d+)\.(\d+)").Select(s => int.TryParse(s, out int i) ? i : 0).First();
                var dir = latest.installationPath;

                return (v, dir);
            }
            catch (Exception)
            {
                return (-1, null);
            }
        }

        private static (int, string) FindVSInstallDirWithRegistry()
        {
            return (-1, null);
        }

        private static (int, string) FindVSInstallDirWithPath()
        {
            return (-1, null);
        }

        private static string FindVSLibDir(int version, string installDir)
        {
            switch (version)
            {
                case 15:
                    {
                        var MSVC = Path.Combine(installDir, "VC", "Tools", "MSVC");
                        if (!Directory.Exists(MSVC))
                            return null;

                        SortedList<int[], string> versions = new SortedList<int[], string>(Comparer<int[]>.Create((a, b) =>
                        {
                            for (int i = 0; i < a.Length && i < b.Length; i++)
                            {
                                if (a[i] > b[i])
                                    return -1;
                                else if (a[i] < b[i])
                                    return 1;
                            }

                            return 0;
                        }));

                        foreach (var sub in Directory.EnumerateDirectories(MSVC))
                        {
                            var name = sub.Substring(sub.LastIndexOfAny(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }) + 1);
                            var v = name.Scan1(@"(\d+)\.(\d+)\.(\d+)").Select(s => int.TryParse(s, out int i) ? i : 0).ToArray();

                            if (v.Length != 3)
                                continue;

                            versions.Add(v, sub);
                        }

                        foreach (var kv in versions)
                        {
                            var libDir = Path.Combine(kv.Value, "lib");

                            if (!Directory.Exists(libDir))
                                continue;

                            return libDir;
                        }

                        return null;
                    }

                default:
                    return null;
            }
        }

        public static string FindVisualStudioLibraryDirectory()
        {
            var (version, dir) = FindVSInstallDirWithVsWhere();

            if (dir == null)
                (version, dir) = FindVSInstallDirWithRegistry();

            if (dir == null)
                (version, dir) = FindVSInstallDirWithPath();

            if (dir == null)
                return null;

            return FindVSLibDir(version, dir);
        }
    }
}
