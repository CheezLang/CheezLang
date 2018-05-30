using Cheez.Compiler.CodeGeneration;
using Cheez.Compiler;
using System.Diagnostics;
using System.IO;
using System;
using System.Text;
using CommandLine;
using System.Collections.Generic;
using System.Linq;
using Cheez.Compiler.Visitor;

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

            [Option("print-ast", Default = false)]
            public bool PrintAst { get; set; }

            [Option("no-code", Default = false)]
            public bool DontEmitCode { get; set; }

            [Option("no-errors", Default = false)]
            public bool NoErrors { get; set; }
        }

        public static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            var argsParser = Parser.Default;


            var stopwatch = Stopwatch.StartNew();
            var exitCode = argsParser.ParseArguments<CompilerOptions>(args)
                .MapResult(
                    options => Run(options),
                    _ => 1);


            var ourCompileTime = stopwatch.Elapsed;

            Console.WriteLine($"Compilation finished in {ourCompileTime}");

            return exitCode;
        }

        static int Run(CompilerOptions options)
        {
            if (options.OutName == null)
                options.OutName = Path.GetFileNameWithoutExtension(options.Files.First());

            Console.WriteLine(Parser.Default.FormatCommandLine(options));


            IErrorHandler errorHandler = new ConsoleErrorHandler();
            if (options.NoErrors)
            {
                errorHandler = new SilentErrorHandler();
            }

            var compiler = new Compiler(errorHandler);
            foreach (var file in options.Files)
            {
                var v = compiler.AddFile(file);
                if (v == null)
                    return 4;
            }

            //if (!errorHandler.HasErrors)
                compiler.DefaultWorkspace.CompileAll();

            if (options.PrintAst)
            {
                var printer = new AstPrinter();
                printer.PrintWorkspace(compiler.DefaultWorkspace, Console.Out);
            }

            if (errorHandler.HasErrors)
                return 3;

            if (options.DontEmitCode)
                return 0;

            // generate code
            bool codeGenOk = GenerateAndCompileCode(options, compiler.DefaultWorkspace);

            if (options.RunCode && codeGenOk)
            {
                Console.WriteLine($"Running code:");
                Console.WriteLine("=======================================");
                var testProc = Util.StartProcess(
                    Path.Combine(options.OutPath, options.OutName + ".exe"),
                    "",
                    workingDirectory: options.OutPath, 
                    stdout: (s, e) => System.Console.WriteLine(e.Data),
                    stderr: (s, e) => System.Console.Error.WriteLine(e.Data));
                testProc.WaitForExit();
                Console.WriteLine("=======================================");
                Console.WriteLine("Program exited with code " + testProc.ExitCode);
            }

            return 0;
        }

        private static bool GenerateAndCompileCode(CompilerOptions options, Workspace workspace)
        {
            if (!string.IsNullOrWhiteSpace(options.OutPath) && !Directory.Exists(options.OutPath))
                Directory.CreateDirectory(options.OutPath);
            string filePath = Path.Combine(options.OutPath, options.OutName);

            ICodeGenerator generator = new LLVMCodeGenerator();
            bool success = generator.GenerateCode(workspace, filePath);
            if (!success)
                return false;
            
            if (true)
                return generator.CompileCode();
            return success;
        }

        
    }
}
