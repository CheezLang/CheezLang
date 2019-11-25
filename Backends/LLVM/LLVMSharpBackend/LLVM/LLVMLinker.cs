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
    public class VsWhere
    {
        public string instanceId { get; set; }
        public string installDate { get; set; }
        public string installationName { get; set; }
        public string installationPath { get; set; }
        public string installationVersion { get; set; }
        public string isPrerelease { get; set; }
        public string displayName { get; set; }
        public string description { get; set; }
        public string enginePath { get; set; }
        public string channelId { get; set; }
        public string channelPath { get; set; }
        public string channelUri { get; set; }
        public string releaseNotes { get; set; }
        public string thirdPartyNotices { get; set; }
    }

#pragma warning disable CA1060 // Move pinvokes to native methods class
    public static class LLVMLinker
#pragma warning restore CA1060 // Move pinvokes to native methods class
    {
        [DllImport(@"D:\Programming\CheezLang\LLVMLinker.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static bool llvm_link_coff(string[] argv, int argc);

        [DllImport("LLVMLinker", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static bool llvm_link_elf(string[] argv, int argc);

        public static bool Link(Workspace workspace, string targetFile, string objFile, IEnumerable<string> libraryIncludeDirectories, IEnumerable<string> libraries, string subsystem, IErrorHandler errorHandler, bool printLinkerArgs)
        {
            if (workspace is null)
                throw new ArgumentNullException(nameof(workspace));
            if (libraryIncludeDirectories is null)
                throw new ArgumentNullException(nameof(libraryIncludeDirectories));
            if (errorHandler is null)
                throw new ArgumentNullException(nameof(errorHandler));

            string target = null;
            switch (workspace.TargetArch)
            {
                case TargetArchitecture.X86: target = "x86"; break;
                case TargetArchitecture.X64: target = "x64"; break;
            }

            foreach (var f in workspace.Files)
            {
                libraries = libraries.Concat(f.Libraries);
            }
            libraries = libraries.Distinct();

            var filename = Path.GetFileNameWithoutExtension(targetFile + ".x");
            var dir = Path.GetDirectoryName(Path.GetFullPath(targetFile));

            filename = Path.Combine(dir, filename);

            var lldArgs = new List<string>();
            //lldArgs.Add("lld");
            lldArgs.Add($"/out:{filename}{OS.ExecutableFileExtension}");

            // library paths

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)){
                var winSdk = OS.FindWindowsSdk();
                if (winSdk == null)
                {
                    errorHandler.ReportError("Couldn't find windows sdk");
                    return false;
                }

                var msvcLibPath = FindVisualStudioLibraryDirectory();
                if (msvcLibPath == null)
                {
                    errorHandler.ReportError("Couldn't find Visual Studio library directory");
                    return false;
                }
                if (winSdk.UcrtPath != null)
                    lldArgs.Add($@"-libpath:{winSdk.UcrtPath}\{target}");

                if (winSdk.UmPath != null)
                    lldArgs.Add($@"-libpath:{winSdk.UmPath}\{target}");
                if (msvcLibPath != null)
                    lldArgs.Add($@"-libpath:{msvcLibPath}\{target}");
            }

            var exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            lldArgs.Add($"-libpath:{exePath}");

            foreach (var linc in libraryIncludeDirectories)
            {
                lldArgs.Add($@"-libpath:{linc}");
            }

            lldArgs.Add($@"-libpath:{Environment.CurrentDirectory}\lib"); // @hack
            lldArgs.Add($@"-libpath:{exePath}\lib");

            // other options
            switch (workspace.TargetArch)
            {
                case TargetArchitecture.X86: lldArgs.Add("/entry:mainCRTStartup"); break;
                case TargetArchitecture.X64: lldArgs.Add("/entry:WinMainCRTStartup"); break;
            }
            lldArgs.Add($"/machine:{target}");
            lldArgs.Add($"/subsystem:{subsystem}");

            // runtime
            if (workspace.TargetArch == TargetArchitecture.X86)
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
            lldArgs.Add("shlwapi.lib");

            foreach (var linc in libraries)
            {
                lldArgs.Add(linc);
            }

            // generated object files
            lldArgs.Add(objFile);

            if (printLinkerArgs)
                Console.WriteLine("[LINKER] " + string.Join(" ", lldArgs));

            var process = Utilities.StartProcess("lld-link", lldArgs,
                            stdout: (s, e) => { if (e.Data != null) Console.WriteLine(e.Data); },
                            stderr: (s, e) => { if (e.Data != null) Console.Error.WriteLine(e.Data); });
            process.WaitForExit();
            var result = process.ExitCode == 0;
            //var result = llvm_link_coff(lldArgs.ToArray(), lldArgs.Count);
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


        private static (int, string) FindVSInstallDirWithVsWhere(int skipLatest = 0)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                var programFilesX86 = Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%");
                var p = Utilities.StartProcess($@"{programFilesX86}\Microsoft Visual Studio\Installer\vswhere.exe", "-nologo -format json", stdout:
                    (sender, e) =>
                    {
                        sb.AppendLine(e.Data);
                    });
                p.WaitForExit();

                var versions = Newtonsoft.Json.JsonConvert.DeserializeObject<VsWhere[]>(sb.ToString());

                if (versions?.Length == 0)
                    return (-1, null);

                if (versions.Length <= skipLatest)
                    skipLatest = versions.Length - 1;

                var latest = versions.Skip(Math.Min(skipLatest, versions.Length - 1)).First();
                System.Console.WriteLine($"vs version: {latest.installationVersion}");

                var v = latest.installationVersion.Scan1(@"(\d+)\.(\d+)\.(\d+)\.(\d+)").Select(s => int.TryParse(s, out int i) ? i : 0).First();
                var dir = latest.installationPath;

                return (v, dir);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
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
                // visual studio 2017
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

                    // visual studio 2019
                case 16:
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
