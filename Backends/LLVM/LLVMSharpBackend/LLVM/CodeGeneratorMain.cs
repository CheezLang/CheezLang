using Cheez.Ast.Statements;
using Cheez.Compiler;
using Cheez.Compiler.CodeGeneration;
using Cheez.Types;
using LLVMSharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace Cheez.CodeGeneration.LLVMCodeGen
{
    public struct LLVMCodeGeneratorNewContext
    {
        public List<LLVMValueRef> Targets;
    }

    public partial class LLVMCodeGenerator : ICodeGenerator
    {

        private Workspace workspace;
        private string targetFile;
        private string intDir;
        private string outDir;

        private bool emitDebugInfo = false;
        
        private Module module;
        private LLVMContextRef context;
        private LLVMTargetDataRef targetData;

        // <arch><sub>-<vendor>-<sys>-<abi>
        // arch = x86_64, i386, arm, thumb, mips, etc.
        private string targetTriple;

        private Dictionary<CheezType, LLVMTypeRef> typeMap = new Dictionary<CheezType, LLVMTypeRef>();
        private Dictionary<object, LLVMValueRef> valueMap = new Dictionary<object, LLVMValueRef>();

        // intrinsics
        private LLVMValueRef memcpy32;
        private LLVMValueRef memcpy64;

        //
        private LLVMTypeRef pointerType;
        private int pointerSize = 4;

        // context
        private AstFunctionDecl currentFunction;
        private LLVMValueRef currentLLVMFunction;
        private IRBuilder builder;
        private Dictionary<object, LLVMValueRef> returnValuePointer = new Dictionary<object, LLVMValueRef>();
        private LLVMBasicBlockRef currentTempBasicBlock;

        //private void RunOptimizations(uint level)
        //{
        //    module.PrintToFile(Path.Combine(intDir, targetFile + ".debug.ll"));

        //    var pmBuilder = LLVM.PassManagerBuilderCreate();
        //    LLVM.PassManagerBuilderSetOptLevel(pmBuilder, level);

        //    var funcPM = module.CreateFunctionPassManagerForModule();
        //    LLVM.PassManagerBuilderPopulateFunctionPassManager(pmBuilder, funcPM);
        //    bool r = LLVM.InitializeFunctionPassManager(funcPM);

        //    // optimize functions
        //    var func = module.GetFirstFunction();

        //    int modifiedFunctions = 0;
        //    while (func.Pointer != IntPtr.Zero)
        //    {
        //        if (!func.IsDeclaration())
        //        {
        //            var modified = LLVM.RunFunctionPassManager(funcPM, func);
        //            if (modified) modifiedFunctions++;
        //        }
        //        func = func.GetNextFunction();
        //    }
        //    r = LLVM.FinalizeFunctionPassManager(funcPM);

        //    Console.WriteLine($"[LLVM] {modifiedFunctions} functions where modified during optimization.");

        //    var modPM = LLVM.CreatePassManager();
        //    LLVM.PassManagerBuilderPopulateModulePassManager(pmBuilder, modPM);
        //    r = LLVM.RunPassManager(modPM, module.GetModuleRef());
        //    if (!r) Console.WriteLine($"[LLVM] Module was not modified during optimization.");


        //    // verify module
        //    {
        //        module.VerifyModule(LLVMVerifierFailureAction.LLVMPrintMessageAction, out string llvmErrors);
        //        if (!string.IsNullOrWhiteSpace(llvmErrors))
        //            Console.Error.WriteLine($"[LLVM-validate-module] {llvmErrors}");
        //    }
        //}

        //private void RunOptimizationsCustom()
        //{
        //    module.PrintToFile(Path.Combine(intDir, targetFile + ".debug.ll"));

        //    var funcPM = module.CreateFunctionPassManagerForModule();
        //    LLVM.AddCFGSimplificationPass(funcPM);

        //    bool r = false;
        //    //bool r = LLVM.InitializeFunctionPassManager(funcPM); // needed?

        //    // optimize functions
        //    var func = module.GetFirstFunction();

        //    int modifiedFunctions = 0;
        //    while (func.Pointer != IntPtr.Zero)
        //    {
        //        if (!func.IsDeclaration())
        //        {
        //            var modified = LLVM.RunFunctionPassManager(funcPM, func);
        //            if (modified) modifiedFunctions++;
        //        }
        //        func = func.GetNextFunction();
        //    }
        //    r = LLVM.FinalizeFunctionPassManager(funcPM);

        //    Console.WriteLine($"[LLVM] {modifiedFunctions} functions where modified during optimization.");

        //    var modPM = LLVM.CreatePassManager();
        //    r = LLVM.RunPassManager(modPM, module.GetModuleRef());
        //    if (!r) Console.WriteLine($"[LLVM] Module was not modified during optimization.");

        //    // verify module
        //    {
        //        module.VerifyModule(LLVMVerifierFailureAction.LLVMPrintMessageAction, out string llvmErrors);
        //        if (!string.IsNullOrWhiteSpace(llvmErrors))
        //            Console.Error.WriteLine($"[LLVM-validate-module] {llvmErrors}");
        //    }
        //}

        public bool GenerateCode(Workspace workspace, string intDir, string outDir, string targetFile, bool optimize, bool outputIntermediateFile)
        {
            this.workspace = workspace;
            this.intDir = Path.GetFullPath(intDir ?? "");
            this.outDir = Path.GetFullPath(outDir ?? "");
            this.targetFile = targetFile;
            this.emitDebugInfo = !optimize;

            module = new Module("test-module");
            context = module.GetModuleContext();
            targetTriple = "i386-pc-win32";
            module.SetTarget(targetTriple);
            targetData = module.GetTargetData();
            
            pointerType = LLVM.Int8Type().GetPointerTo();
            // generate code
            {
                //GenerateTypes();
                //GenerateIntrinsicDeclarations();
                //GenerateFunctions();
                //GenerateMainFunction();
            }

            // generate int dir
            if (!string.IsNullOrWhiteSpace(intDir) && !Directory.Exists(intDir))
                Directory.CreateDirectory(intDir);

            // run optimizations
            if (optimize)
            {
                Console.WriteLine("[LLVM] Running optimizations...");
                //RunOptimizations(3);
                //RunOptimizationsCustom();
                Console.WriteLine("[LLVM] Done.");
            }

            // create .ll file
            if (outputIntermediateFile)
            {
                module.PrintToFile(Path.Combine(intDir, targetFile + ".ll"));
            }

            // emit machine code to object file
            {
                TargetExt.InitializeX86Target();
                var targetMachine = TargetMachineExt.FromTriple(targetTriple);
                var objFile = Path.Combine(intDir, targetFile + ".obj");
                targetMachine.EmitToFile(module, objFile);
            }

            module.DisposeModule();
            return true;
        }

        public bool CompileCode(IEnumerable<string> libraryIncludeDirectories, IEnumerable<string> libraries, string subsystem, IErrorHandler errorHandler)
        {
            if (!string.IsNullOrWhiteSpace(outDir) && !Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);

            string objFile = Path.Combine(intDir, targetFile + ".obj");
            string exeFile = Path.Combine(outDir, targetFile);

            return LLVMLinker.Link(workspace, exeFile, objFile, libraryIncludeDirectories, libraries, subsystem, errorHandler);
        }

    }
}
