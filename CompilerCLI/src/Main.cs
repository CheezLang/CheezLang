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

        [Option('o', "out", Default = ".", HelpText = "Output directory: --out <directory>")]
        public string OutPath { get; set; }

        [Option("int", Default = "./int", HelpText = "Intermediate directory: --int <directory>")]
        public string IntPath { get; set; }

        [Option('n', "name", HelpText = "Name of the executable generated: <name>")]
        public string OutName { get; set; }

        [Option("print-ast-raw", Default = null, HelpText = "Print the raw abstract syntax tree to a file: --print-ast-raw <filepath>")]
        public string PrintRawAst { get; set; }

        [Option("print-ast-analyzed", Default = null, HelpText = "Print the analyzed abstract syntax tree to a file: --print-ast-analyzed <filepath>")]
        public string PrintAnalyzedAst { get; set; }

        [Option("no-code", Default = false, HelpText = "Don't generate exe")]
        public bool DontEmitCode { get; set; }

        [Option("no-errors", Default = false, HelpText = "Don't show error messages")]
        public bool NoErrors { get; set; }

        [Option("ld", HelpText = "Additional include directories: --ld [<path> [<path>]...]")]
        public IEnumerable<string> LibraryIncludeDirectories { get; set; }

        [Option("libs", HelpText = "Additional Libraries to link to: --libs [<path> [<path>]...]")]
        public IEnumerable<string> Libraries { get; set; }

        [Option("subsystem", Default = SubSystem.console, HelpText = "Sub system: --subsystem [windows|console]")]
        public SubSystem SubSystem { get; set; }

        [Option("modules", HelpText = "Additional modules: --modules [<name>:<path> [<name>:<path>]...]")]
        public IEnumerable<string> Modules { get; set; }

        [Option("stdlib", Default = null, HelpText = "Path to the standard library: --stdlib <path>")]
        public string Stdlib { get; set; }

        [Option("opt", Default = false, HelpText = "Perform optimizations: --opt")]
        public bool Optimize { get; set; }

        [Option("output-intermediate", Default = false, HelpText = "Output .ll file containing LLVM IR: --output-intermediate")]
        public bool OutputIntermediate { get; set; }

        [Option("time", Default = false, HelpText = "Print how long the compilation takes: --time")]
        public bool PrintTime { get; set; }

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
            public bool PrintTime = false;
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

            if (result.PrintTime)
            {
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
            }

            return result.ExitCode;
        }

        static CompilationResult Run(CompilerOptions options)
        {
            var result = new CompilationResult();
            result.PrintTime = options.PrintTime;

            if (options.OutName == null)
                options.OutName = Path.GetFileNameWithoutExtension(options.Files.First());

            //Console.WriteLine(Parser.Default.FormatCommandLine(options));

            IErrorHandler errorHandler = new ConsoleErrorHandler();
            if (options.NoErrors)
            {
                errorHandler = new SilentErrorHandler();
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var compiler = new Compiler(errorHandler, options.Stdlib);
            foreach (string mod in options.Modules)
            {
                var parts = mod.Split(':');
                if (parts.Length != 2)
                {
                    errorHandler.ReportError($"Invalid module option: {mod}");
                    continue;
                }

                compiler.ModulePaths[parts[0]] = parts[1];
            }

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

            if (options.PrintRawAst != null)
            {
                var printer = new RawAstPrinter();
                using (var file = File.Open(options.PrintRawAst, FileMode.Create))
                using (var writer = new StreamWriter(file))
                {
                    printer.PrintWorkspace(compiler.DefaultWorkspace, writer);
                }
            }

            compiler.DefaultWorkspace.CompileAll();

            result.SemanticAnalysis = stopwatch.Elapsed;
            result.FrontEnd = result.LexAndParse + result.SemanticAnalysis;

            if (options.PrintAnalyzedAst != null)
            {
                var printer = new AnalyzedAstPrinter();
                using (var file = File.Open(options.PrintAnalyzedAst, FileMode.Create))
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

            ICodeGenerator generator = new LLVMCodeGenerator();
            bool success = generator.GenerateCode(workspace, options.IntPath, options.OutPath, options.OutName, options.Optimize, options.OutputIntermediate);
            if (!success)
                return false;

            return generator.CompileCode(options.LibraryIncludeDirectories, options.Libraries, options.SubSystem.ToString(), errorHandler);
        }


    }
}
