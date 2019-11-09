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
    public static class ClangLinker
    {
        public static bool Link(Workspace workspace, string targetFile, string objFile, IEnumerable<string> libraries)
        {
            if (workspace == null)
                throw new ArgumentNullException(nameof(workspace));

            foreach (var f in workspace.Files)
            {
                libraries = libraries.Concat(f.Libraries);
            }
            libraries = libraries.Distinct();

            var filename = Path.GetFileNameWithoutExtension(targetFile + ".x");
            var dir = Path.GetDirectoryName(Path.GetFullPath(targetFile));

            filename = Path.Combine(dir, filename);

            var lldArgs = new List<string>();
            lldArgs.Add($"-o{filename}{OS.ExecutableFileExtension}");

            // library paths

            // var exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            // lldArgs.Add($"-libpath:{exePath}");

            // foreach (var linc in libraryIncludeDirectories)
            // {
            //     lldArgs.Add($@"-libpath:{linc}");
            // }

            // lldArgs.Add($@"-libpath:{Environment.CurrentDirectory}\lib"); // @hack
            // lldArgs.Add($@"-libpath:{exePath}\lib");

            // // other options
            // switch (workspace.TargetArch)
            // {
            //     case TargetArchitecture.X86: lldArgs.Add("/entry:mainCRTStartup"); break;
            //     case TargetArchitecture.X64: lldArgs.Add("/entry:WinMainCRTStartup"); break;
            // }
            // lldArgs.Add($"/machine:{target}");
            // lldArgs.Add($"/subsystem:{subsystem}");

            // // runtime
            // if (workspace.TargetArch == TargetArchitecture.X86)
            //     lldArgs.Add("clang_rt.builtins-i386.lib");

            // lldArgs.Add("libclang.lib");

            foreach (var linc in libraries)
            {
                lldArgs.Add(linc);
            }

            // generated object files
            lldArgs.Add(objFile);

            // Console.WriteLine("[LINKER] " + string.Join(" ", lldArgs));

            var process = Utilities.StartProcess("clang", lldArgs,
                            stdout: (s, e) => { if (e.Data != null) Console.WriteLine(e.Data); },
                            stderr: (s, e) => { if (e.Data != null) Console.Error.WriteLine(e.Data); });
            process.WaitForExit();
            var result = process.ExitCode == 0;
            if (result)
            {
                Console.WriteLine($"Generated {filename}{OS.ExecutableFileExtension}");
            }
            else
            {
                Console.WriteLine($"Failed to link");
            }

            return result;
        }
    }
}
