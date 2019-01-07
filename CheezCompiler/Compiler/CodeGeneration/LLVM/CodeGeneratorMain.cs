using Cheez.Compiler.Ast;
using Cheez.Compiler.Visitor;
using LLVMCS;
using System;
using System.Collections.Generic;
using System.IO;

namespace Cheez.Compiler.CodeGeneration.LLVMCodeGen
{
    public struct LLVMCodeGeneratorNewContext
    {
        public List<ValueRef> Targets;
    }

    public partial class LLVMCodeGeneratorNew : VisitorBase<ValueRef, LLVMCodeGeneratorNewContext>, ICodeGenerator
    {

        private Workspace workspace;
        private string targetFile;
        private string intDir;
        private string outDir;

        private bool emitDebugInfo = false;
        
        private Module module;
        private DataLayout targetData;

        // <arch><sub>-<vendor>-<sys>-<abi>
        // arch = x86_64, i386, arm, thumb, mips, etc.
        private string targetTriple;

        private Dictionary<CheezType, TypeRef> typeMap = new Dictionary<CheezType, TypeRef>();
        private Dictionary<object, ValueRef> valueMap = new Dictionary<object, ValueRef>();

        // intrinsics
        private ValueRef memcpy32;
        private ValueRef memcpy64;

        //
        private TypeRef pointerType;
        private int pointerSize = 4;

        // context
        private AstFunctionDecl currentFunction;
        private ValueRef currentLLVMFunction;
        private IRBuilder builder;
        private Dictionary<object, ValueRef> returnValuePointer = new Dictionary<object, ValueRef>();
        private BasicBlockRef currentTempBasicBlock;

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
            targetTriple = "i386-pc-win32";
            module.SetTargetTriple(targetTriple);
            targetData = new DataLayout(module);

            
            pointerType = TypeRef.GetIntType(8).GetPointerTo();
            // generate code
            {
                GenerateTypes();
                GenerateIntrinsicDeclarations();
                GenerateFunctions();
                GenerateMainFunction();
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
            Target.InitializeAll();
            {
                var objFile = Path.Combine(intDir, targetFile + ".obj");

                var target = Target.FromTargetTriple(targetTriple);
                var targetMachine = new TargetMachine(target, targetTriple);
                targetMachine.EmitModule(module, objFile);
                targetMachine.Dispose();
            }

            module.Dispose();
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
