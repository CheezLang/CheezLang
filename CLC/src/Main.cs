using Cheez.Compiler.CodeGeneration;
using Cheez.Compiler;
using System.Diagnostics;
using System.IO;
using System;
using System.Text;
using CommandLine;
using System.Collections.Generic;

namespace CheezCLI
{
    class Prog
    {
        class CompilerOptions
        {
            [Value(0, Min = 1)]
            public IEnumerable<string> Files { get; set; }
        }

        public static int Main(string[] args)
        {
            //var opts = new CompilerOptions()
            //{
            //    Files = new List<string> { "test.che", "another.che" },
            //    TestOption = "loll"
            //};

            //var s = Parser.Default.FormatCommandLine(opts);
            //Console.WriteLine(s);
            //return;

            Console.OutputEncoding = Encoding.UTF8;

            var argsParser = Parser.Default;
            //argsParser.Settings.HelpWriter = CommandLine.Text.HelpText.DefaultParsingErrorsHandler()

            return argsParser.ParseArguments<CompilerOptions>(args)
                .MapResult(
                    options => Run(options),
                    _ => 1);
        }

        static int Run(CompilerOptions options)
        {
            var stopwatch = Stopwatch.StartNew();
            
            var compiler = new Compiler();
            foreach (var file in options.Files)
            {
                var result = compiler.AddFile(file, workspace: compiler.DefaultWorkspace);
                
                if (result.HasErrors)
                {
                    foreach (var e in result.Errors)
                    {
                        Console.WriteLine(e.Message);
                    }
                    return 2;
                }
            }
            
            compiler.DefaultWorkspace.CompileAll();

            if (compiler.HasErrors)
                return 3;
            //compiler.CompileAll();

            var ourCompileTime = stopwatch.Elapsed;
            System.Console.WriteLine($"Our compile time  : {ourCompileTime}");

            // print code
            //var printer = new AstPrinter();
            //foreach (var s in file.Statements)
            //{
            //    System.Console.WriteLine(s.Accept(printer));
            //}

            // generate code
            System.Console.WriteLine();

            stopwatch.Restart();

            bool clangOk = GenerateAndCompileCode(compiler.DefaultWorkspace);

            var clangTime = stopwatch.Elapsed;
            System.Console.WriteLine();
            System.Console.WriteLine($"Compilation finished in {ourCompileTime + clangTime}.");
            System.Console.WriteLine($"Clang compile time: {clangTime}");

            if (clangOk)
            {
                System.Console.WriteLine();
                System.Console.WriteLine($"Running code:");
                System.Console.WriteLine("=======================================");
                var testProc = StartProcess(@"gen\test.exe", workingDirectory: "gen", stdout: (s, e) => System.Console.WriteLine(e.Data));
                testProc.WaitForExit();
            }

            return 0;
        }

        private static bool GenerateAndCompileCode(Workspace workspace)
        {
            foreach (string f in Directory.EnumerateFiles("gen"))
            {
                string extension = Path.GetExtension(f);
                if (/*extension == ".cpp" || */extension == ".exe")
                    File.Delete(f);
            }

            CppCodeGenerator generator = new CppCodeGenerator();
            string code = generator.GenerateCode(workspace);
            File.WriteAllText("gen/code.cpp", code);

            // run clang
            var clang = StartProcess(@"D:\Program Files\LLVM\bin\clang++.exe", "-O0 -o test.exe code.cpp", "gen", stderr: Process_ErrorDataReceived);
            clang.WaitForExit();

            //var clangOutput = process.StandardOutput.ReadToEnd();

            System.Console.WriteLine($"Clang finished compiling with exit code {clang.ExitCode}");
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
            System.Console.WriteLine($"[CLANG] {e.Data}");
        }
    }
}
