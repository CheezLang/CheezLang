using Cheez.Compiler.CodeGeneration;
using Cheez.Compiler;
using System.Diagnostics;
using System.IO;
using System;
using System.Text;
using CommandLine;
using System.Collections.Generic;
using System.Linq;

namespace CheezCLI
{
    class Prog
    {
        class CompilerOptions
        {
            [Option('r', "run", HelpText = "Specifies whether the code should be run immediatly", Default = false, Required = false, Hidden = false, MetaValue = "STRING", SetName = "run")]
            public bool RunCode { get; set; }

            [Value(0, Min = 1)]
            public IEnumerable<string> Files { get; set; }

            [Option('o', "out", Default = "")]
            public string OutPath { get; set; }

            [Option('n', "name")]
            public string OutName { get; set; }
        }

        public static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            var argsParser = Parser.Default;
            return argsParser.ParseArguments<CompilerOptions>(args)
                .MapResult(
                    options => Run(options),
                    _ => 1);
        }

        static int Run(CompilerOptions options)
        {
            if (options.OutName == null)
                options.OutName = Path.GetFileNameWithoutExtension(options.Files.First());

            Console.WriteLine(Parser.Default.FormatCommandLine(options));

            var stopwatch = Stopwatch.StartNew();

            var errorHandler = new ConsoleErrorHandler();

            var compiler = new Compiler(errorHandler);
            foreach (var file in options.Files)
            {
                compiler.AddFile(file);
            }

            compiler.DefaultWorkspace.CompileAll();

            var ourCompileTime = stopwatch.Elapsed;
            Console.WriteLine($"Compilation finished in {ourCompileTime}");

            if (errorHandler.HasErrors)
                return 3;


            // generate code
            Console.WriteLine();

            stopwatch.Restart();

            bool clangOk = GenerateAndCompileCode(options, compiler.DefaultWorkspace);

            var clangTime = stopwatch.Elapsed;
            Console.WriteLine($"Clang compile time: {clangTime}");

            if (options.RunCode && clangOk)
            {
                Console.WriteLine();
                Console.WriteLine($"Running code:");
                Console.WriteLine("=======================================");
                var testProc = StartProcess(Path.Combine(options.OutPath, options.OutName + ".exe"), workingDirectory: options.OutPath, stdout: (s, e) => System.Console.WriteLine(e.Data));
                testProc.WaitForExit();
            }

            return 0;
        }

        private static bool GenerateAndCompileCode(CompilerOptions options,  Workspace workspace)
        {
            string filePath = Path.Combine(options.OutPath, options.OutName);

            CppCodeGenerator generator = new CppCodeGenerator();
            string code = generator.GenerateCode(workspace);
            File.WriteAllText(filePath + ".cpp", code);

            // run clang
            var clang = StartProcess(@"D:\Program Files\LLVM\bin\clang++.exe", $"-O0 -o {options.OutName}.exe {options.OutName}.cpp", options.OutPath, stderr: Process_ErrorDataReceived);
            clang.WaitForExit();

            //var clangOutput = process.StandardOutput.ReadToEnd();

            Console.WriteLine($"Clang finished compiling with exit code {clang.ExitCode}");
            return clang.ExitCode == 0;
        }

        private static Process StartProcess(string exe, string args = null, string workingDirectory = null, DataReceivedEventHandler stdout = null, DataReceivedEventHandler stderr = null)
        {
            var process = new Process();
            process.StartInfo.FileName = exe;
            if (workingDirectory != null)
                process.StartInfo.WorkingDirectory = workingDirectory;
            if (args != null)
                process.StartInfo.Arguments = args;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            if (stdout != null)
            {
                process.StartInfo.RedirectStandardOutput = true;
                process.OutputDataReceived += stdout;
            }

            if (stderr != null)
            {
                process.StartInfo.RedirectStandardError = true;
                process.ErrorDataReceived += stderr;
            }

            process.Start();

            if (stdout != null)
                process.BeginOutputReadLine();
            if (stderr != null)
                process.BeginErrorReadLine();

            return process;
        }

        private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Data))
                return;
            Console.WriteLine($"[CLANG] {e.Data}");
        }
    }
}
