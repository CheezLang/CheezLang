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
using Cheez.Compiler.CodeGeneration.LLVMCodeGen;

namespace CheezCLI
{
    public enum SubSystem
    {
        windows,
        console
    }

    public class CompilerOptions
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

        [Option("print-ast-file", Default = null)]
        public string PrintAstFile { get; set; }

        [Option("no-code", Default = false)]
        public bool DontEmitCode { get; set; }

        [Option("no-errors", Default = false)]
        public bool NoErrors { get; set; }

        [Option("ld")]
        public IEnumerable<string> LibraryIncludeDirectories { get; set; }

        [Option("libs")]
        public IEnumerable<string> Libraries { get; set; }

        [Option("subsystem", Default = SubSystem.console)]
        public SubSystem SubSystem { get; set; }
    }

    class Prog
    {
        class CompilationResult
        {
            public int ExitCode;
            public TimeSpan? LexAndParse;
            public TimeSpan? SemanticAnalysis;
            public TimeSpan? FrontEnd;
            public TimeSpan? BackEnd;
            public TimeSpan? Execution;
        }

        public static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            var argsParser = Parser.Default;


            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var result = argsParser.ParseArguments<CompilerOptions>(args)
                .MapResult(
                    options => Run(options),
                    _ => new CompilationResult { ExitCode = -1 });


            var ourCompileTime = stopwatch.Elapsed;

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("-------------------------------------");
            Console.WriteLine($"Total Compilation Time: {ourCompileTime:mm\\:ss\\.fffffff}");
            Console.WriteLine($"              Frontend: {result.FrontEnd:mm\\:ss\\.fffffff}");
            if (result.LexAndParse != null)
                Console.WriteLine($"    Lexing and Parsing: {result.LexAndParse:mm\\:ss\\.fffffff}");
            if (result.SemanticAnalysis != null)
                Console.WriteLine($"     Semantic Analysis: {result.SemanticAnalysis:mm\\:ss\\.fffffff}");
            if (result.BackEnd != null)
            {
                Console.WriteLine();
                Console.WriteLine($"               Backend: {result.BackEnd:mm\\:ss\\.fffffff}");
            }
            if (result.Execution != null)
            {
                Console.WriteLine();
                Console.WriteLine($"        Execution time: {result.Execution:mm\\:ss\\.fffffff}");
            }

            return result.ExitCode;
        }

        static CompilationResult Run(CompilerOptions options)
        {
            var result = new CompilationResult();

            if (options.OutName == null)
                options.OutName = Path.GetFileNameWithoutExtension(options.Files.First());

            Console.WriteLine(Parser.Default.FormatCommandLine(options));


            IErrorHandler errorHandler = new ConsoleErrorHandler();
            if (options.NoErrors)
            {
                errorHandler = new SilentErrorHandler();
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var compiler = new Compiler(errorHandler);
            foreach (var file in options.Files)
            {
                var v = compiler.AddFile(file);
                if (v == null)
                {
                    result.ExitCode = 4;
                }
            }

            result.LexAndParse = stopwatch.Elapsed;
            stopwatch.Restart();

            //if (!errorHandler.HasErrors)
            compiler.DefaultWorkspace.CompileAll();

            result.SemanticAnalysis = stopwatch.Elapsed;
            result.FrontEnd = result.LexAndParse + result.SemanticAnalysis;

            if (options.PrintAst)
            {
                var printer = new AstPrinter();
                printer.PrintWorkspace(compiler.DefaultWorkspace, Console.Out);
            }
            if (options.PrintAstFile != null)
            {
                var printer = new AstPrinter();
                using (var file = File.Open(options.PrintAstFile, FileMode.Create))
                using (var writer = new StreamWriter(file))
                {
                    printer.PrintWorkspace(compiler.DefaultWorkspace, writer);
                }
            }

            if (errorHandler.HasErrors)
            {
                result.ExitCode = 3;
                return result;
            }

            if (!options.DontEmitCode)
            {
                // generate code
                stopwatch.Restart();
                bool codeGenOk = GenerateAndCompileCode(options, compiler.DefaultWorkspace, errorHandler);
                result.BackEnd = stopwatch.Elapsed;

                stopwatch.Restart();
                if (options.RunCode && codeGenOk)
                {
                    Console.WriteLine($"Running code:");
                    Console.WriteLine("=====================================");
                    var testProc = Util.StartProcess(
                        Path.Combine(options.OutPath, options.OutName + ".exe"),
                        "",
                        workingDirectory: options.OutPath,
                        stdout: (s, e) => System.Console.WriteLine(e.Data),
                        stderr: (s, e) => System.Console.Error.WriteLine(e.Data));
                    testProc.WaitForExit();
                    Console.WriteLine("=====================================");
                    Console.WriteLine("Program exited with code " + testProc.ExitCode);
                    result.Execution = stopwatch.Elapsed;
                }
            }

            return result;
        }

        private static bool GenerateAndCompileCode(CompilerOptions options, Workspace workspace, IErrorHandler errorHandler)
        {
            if (!string.IsNullOrWhiteSpace(options.OutPath) && !Directory.Exists(options.OutPath))
                Directory.CreateDirectory(options.OutPath);
            string filePath = Path.Combine(options.OutPath, options.OutName);

            ICodeGenerator generator = new LLVMCodeGeneratorNew();
            bool success = generator.GenerateCode(workspace, filePath);
            if (!success)
                return false;

            return generator.CompileCode(options.LibraryIncludeDirectories, options.Libraries, options.SubSystem.ToString(), errorHandler);
        }


    }
}
