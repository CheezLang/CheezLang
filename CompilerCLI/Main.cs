using Cheez.CodeGeneration;
using System.Diagnostics;
using System.IO;
using System;
using System.Text;
using CommandLine;
using System.Collections.Generic;
using System.Linq;
using Cheez.Visitors;
using Cheez.Util;
using Cheez.CodeGeneration.LLVMCodeGen;
using Cheez;
using Cheez.Ast.Expressions;
using Cheez.Ast;
using System.Reflection;

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
        public string OutDir { get; set; }

        [Option("int", Default = null, HelpText = "Intermediate directory: --int <directory>")]
        public string IntDir { get; set; }

        [Option('n', "name", HelpText = "Name of the executable generated: <name>")]
        public string OutName { get; set; }

        [Option("print-ast-raw", Default = null, HelpText = "Print the raw abstract syntax tree to a file: --print-ast-raw <filepath>")]
        public string PrintRawAst { get; set; }

        [Option("print-ast-analysed", Default = null, HelpText = "Print the analysed abstract syntax tree to a file: --print-ast-analysed <filepath>")]
        public string PrintAnalysedAst { get; set; }

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

        [Option("emit-llvm-ir", Default = false, HelpText = "Output .ll file containing LLVM IR: --emit-llvm-ir")]
        public bool EmitLLVMIR { get; set; }

        [Option("time", Default = false, HelpText = "Print how long the compilation takes: --time")]
        public bool PrintTime { get; set; }

        [Option("test", Default = false, HelpText = "Run the program as a test.")]
        public bool RunAsTest { get; set; }

        [Option("trace-stack", Default = false, HelpText = "Enable stacktrace (potentially big impact on performance): --trace-stack")]
        public bool EnableStackTrace { get; set; }

        [Option("error-source", Default = false, HelpText = "When reporting an error, print the the line which contains the error")]
        public bool PrintSourceInErrorMessage { get; set; }

        [Option("preload", Default = null, HelpText = "Path to a .che file used to import by default")]
        public string Preload { get; set; }

        [Option("print-linker-args", Default = false, HelpText = "Print arguments passed to linker")]
        public bool PrintLinkerArguments { get; set; }


        [Option("language-server-tcp", Default = false)]
        public bool LaunchLanguageServerTcp { get; set; }

        [Option("port", Default = 5007)]
        public int Port { get; set; }

        [Option("language-server-console", Default = false)]
        public bool LaunchLanguageServerConsole { get; set; }
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
                    options =>
                    {
                        if (options.LaunchLanguageServerTcp)
                        {
                            CheezLanguageServer.CheezLanguageServerLauncher.RunLanguageServerOverTcp(options.Port);
                            return new CompilationResult { ExitCode = 0 };
                        }
                        else if (options.LaunchLanguageServerConsole)
                        {
                            CheezLanguageServer.CheezLanguageServerLauncher.RunLanguageServerOverStdInOut();
                            return new CompilationResult { ExitCode = 0 };
                        }
                        else
                        {
                            return Run(SetDefaults(options));
                        }
                    },
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

        private static CompilerOptions SetDefaults(CompilerOptions options)
        {
            if (options.OutDir != null) options.OutDir = Path.GetFullPath(options.OutDir);
            else options.OutDir = Path.GetFullPath(".");

            if (options.IntDir != null) options.IntDir = Path.GetFullPath(options.IntDir);
            else options.IntDir = Path.Combine(options.OutDir, "int");

            if (options.PrintRawAst != null) options.PrintRawAst = Path.GetFullPath(options.PrintRawAst);
            if (options.PrintAnalysedAst != null) options.PrintAnalysedAst = Path.GetFullPath(options.PrintAnalysedAst);

            if (options.Stdlib != null) options.Stdlib = Path.GetFullPath(options.Stdlib);
            return options;
        }

        static CompilationResult Run(CompilerOptions options)
        {
            var result = new CompilationResult();
            result.PrintTime = options.PrintTime;

            if (options.OutName == null)
                options.OutName = Path.GetFileNameWithoutExtension(options.Files.First());

            Console.WriteLine(Parser.Default.FormatCommandLine(options));

            IErrorHandler errorHandler = new ConsoleErrorHandler(0, 0, options.PrintSourceInErrorMessage);
            if (options.NoErrors)
            {
                errorHandler = new SilentErrorHandler();
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var compiler = new CheezCompiler(errorHandler, options.Stdlib, options.Preload);
            foreach (string mod in options.Modules)
                compiler.ModulePaths.Add(mod);

            // load additional module paths from modules.txt if existent
            {
                string exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                string modulesFile = Path.Combine(exePath, "modules.txt");
                if (File.Exists(modulesFile))
                {
                    foreach (var modulePath in File.ReadAllLines(modulesFile))
                    {
                        if (!string.IsNullOrWhiteSpace(modulePath))
                            compiler.ModulePaths.Add(modulePath);
                    }
                }
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

            if (options.PrintRawAst != null)
            {
                var dir = Path.GetDirectoryName(options.PrintRawAst);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                using (var file = File.Open(options.PrintRawAst, FileMode.Create))
                using (var writer = new StreamWriter(file))
                {
                    var printer = new RawAstPrinter(writer);
                    printer.PrintWorkspace(compiler.DefaultWorkspace);
                }
            }

            stopwatch.Restart();
            compiler.DefaultWorkspace.CompileAll();
            result.SemanticAnalysis = stopwatch.Elapsed;
            result.FrontEnd = result.LexAndParse + result.SemanticAnalysis;

            if (options.PrintAnalysedAst != null)
            {
                var dir = Path.GetDirectoryName(options.PrintAnalysedAst);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                var printer = new AnalysedAstPrinter();
                using (var file = File.Open(options.PrintAnalysedAst, FileMode.Create))
                using (var writer = new StreamWriter(file))
                {
                    printer.PrintWorkspace(compiler.DefaultWorkspace, writer);
                }
            }

            System.Console.WriteLine($"Global variables: {compiler.DefaultWorkspace.Variables.Count()}");
            System.Console.WriteLine($"           Impls: {compiler.DefaultWorkspace.Impls.Count()}");
            System.Console.WriteLine($"       Functions: {compiler.DefaultWorkspace.Functions.Count()}");
            System.Console.WriteLine($"         Structs: {compiler.DefaultWorkspace.Structs.Count()}");
            System.Console.WriteLine($"           Enums: {compiler.DefaultWorkspace.Enums.Count()}");
            System.Console.WriteLine($"          Traits: {compiler.DefaultWorkspace.Traits.Count()}");

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

                if (!codeGenOk)
                {
                    result.ExitCode = 4;
                    return result;
                }

                if (options.RunCode && codeGenOk)
                {
                    if (options.RunAsTest)
                    {
                        stopwatch.Restart();
                        string[] StringLiteralToString(AstExpression e) => (e as AstStringLiteral).StringValue.Split('\n');
                        IEnumerable<string> DirectiveToStrings(AstDirective d) => d.Arguments.SelectMany(StringLiteralToString);
                        string[] expectedOutputs = compiler.TestOutputs.Select(DirectiveToStrings).SelectMany(x => x).ToArray();
                        int currentExpectedOutput = 0;
                        int linesFailed = 0;

                        var testProc = Utilities.StartProcess(
                            Path.Combine(options.OutDir, options.OutName + ".exe"),
                            "",
                            workingDirectory: options.OutDir,
                            stdout: (s, e) => {
                                if (e.Data != null)
                                {
                                    var expectedOutput = currentExpectedOutput < expectedOutputs.Length ? expectedOutputs[currentExpectedOutput] : "";
                                    if (expectedOutput != e.Data)
                                    {
                                        Console.WriteLine($"[TEST] ({currentExpectedOutput}) Expected: '{expectedOutput}', got: '{e.Data}'");
                                        linesFailed++;
                                    }
                                    currentExpectedOutput++;
                                }
                            },
                            stderr: (s, e) => { if (e.Data != null) Console.Error.WriteLine(e.Data); });
                        testProc.WaitForExit();
                        result.Execution = stopwatch.Elapsed;
                        
                        if (linesFailed == 0 && currentExpectedOutput == expectedOutputs.Length)
                        {
                            Console.WriteLine($"[TEST] {options.OutName} ok");
                        }
                        else
                        {
                            if (linesFailed > 0)
                            {
                                Console.WriteLine($"[TEST] {linesFailed} error(s).");
                            }
                            if (currentExpectedOutput != expectedOutputs.Length)
                            {
                                Console.WriteLine($"[TEST] Not enough output.");
                            }

                            result.ExitCode = 69;
                            return result;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"\nRunning code:");
                        Console.WriteLine("=====================================");
                        stopwatch.Restart();
                        var testProc = Utilities.StartProcess(
                            Path.Combine(options.OutDir, options.OutName + ".exe"),
                            "",
                            workingDirectory: options.OutDir,
                            stdout: (s, e) => { if (e.Data != null) Console.WriteLine(e.Data); },
                            stderr: (s, e) => { if (e.Data != null) Console.Error.WriteLine(e.Data); });
                        //var testProc = Utilities.StartProcess(
                        //    Path.Combine(options.OutDir, options.OutName + ".exe"),
                        //    "",
                        //    workingDirectory: options.OutDir,
                        //    useShellExecute: true,
                        //    createNoWindow: true);
                        testProc.WaitForExit();
                        result.Execution = stopwatch.Elapsed;
                        Console.WriteLine("=====================================");
                        Console.WriteLine("Program exited with code " + testProc.ExitCode);
                    }
                }
            }

            return result;
        }

        private static bool GenerateAndCompileCode(CompilerOptions options, Workspace workspace, IErrorHandler errorHandler)
        {
            using var generator = new LLVMCodeGenerator(options.EnableStackTrace);
            bool success = generator.GenerateCode(workspace, options.IntDir, options.OutDir, options.OutName, options.Optimize, options.EmitLLVMIR);
            if (!success)
                return false;

            return generator.CompileCode(options.LibraryIncludeDirectories, options.Libraries, options.SubSystem.ToString(), errorHandler, options.PrintLinkerArguments);
        }
    }
}
