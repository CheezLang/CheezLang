using Cheez.Ast.Statements;
using Cheez.Types;
using LLVMSharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace Cheez.CodeGeneration.LLVMCodeGen
{
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

        public bool GenerateCode(Workspace workspace, string intDir, string outDir, string targetFile, bool optimize, bool outputIntermediateFile)
        {
            this.workspace = workspace;
            this.intDir = Path.GetFullPath(intDir ?? "");
            this.outDir = Path.GetFullPath(outDir ?? "");
            this.targetFile = targetFile;
            this.emitDebugInfo = !optimize;

            targetTriple = "i386-pc-win32";

            module = new Module("test-module");
            module.SetTarget(targetTriple);

            context = module.GetModuleContext();
            targetData = module.GetTargetData();

            pointerType = LLVM.Int8Type().GetPointerTo();
            // generate code
            {
                GenerateIntrinsicDeclarations();
                GenerateFunctions();
                GenerateMainFunction();
            }

            // verify module
            {
                if (module.VerifyModule(LLVMVerifierFailureAction.LLVMReturnStatusAction, out string message))
                {
                    Console.Error.WriteLine($"[LLVM-validate-module] {message}");
                }
            }

            // generate int dir
            if (!string.IsNullOrWhiteSpace(intDir) && !Directory.Exists(intDir))
                Directory.CreateDirectory(intDir);

            // run optimizations
            if (optimize) // TODO
            {
                Console.WriteLine("[LLVM] Running optimizations...");
                var (modifiedFunctions, modifiedModule) = RunOptimizations(3);

                Console.WriteLine($"[LLVM] {modifiedFunctions} functions where modified during optimization.");
                if (!modifiedModule) Console.WriteLine($"[LLVM] Module was not modified during optimization.");
                Console.WriteLine("[LLVM] Optimizations done.");
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

        private (int, bool) RunOptimizations(uint level)
        {
            module.PrintToFile(Path.Combine(intDir, targetFile + ".debug.ll"));

            var pmBuilder = LLVM.PassManagerBuilderCreate();
            LLVM.PassManagerBuilderSetOptLevel(pmBuilder, level);

            var funcPM = module.CreateFunctionPassManagerForModule();
            LLVM.PassManagerBuilderPopulateFunctionPassManager(pmBuilder, funcPM);
            LLVM.InitializeFunctionPassManager(funcPM);

            // optimize functions
            var func = module.GetFirstFunction();

            int modifiedFunctions = 0;
            while (func.Pointer != IntPtr.Zero)
            {
                if (!func.IsDeclaration())
                {
                    var modified = LLVM.RunFunctionPassManager(funcPM, func);
                    if (modified) modifiedFunctions++;
                }
                func = func.GetNextFunction();
            }
            LLVM.FinalizeFunctionPassManager(funcPM);

            var modPM = LLVM.CreatePassManager();
            LLVM.PassManagerBuilderPopulateModulePassManager(pmBuilder, modPM);
            var r = LLVM.RunPassManager(modPM, module.GetModuleRef());
            return (modifiedFunctions, r);
        }

        private (int, bool) RunOptimizationsCustom()
        {
            module.PrintToFile(Path.Combine(intDir, targetFile + ".debug.ll"));

            var funcPM = module.CreateFunctionPassManagerForModule();
            LLVM.AddCFGSimplificationPass(funcPM);

            // optimize functions
            var func = module.GetFirstFunction();

            int modifiedFunctions = 0;
            while (func.Pointer != IntPtr.Zero)
            {
                if (!func.IsDeclaration())
                {
                    var modified = LLVM.RunFunctionPassManager(funcPM, func);
                    if (modified) modifiedFunctions++;
                }
                func = func.GetNextFunction();
            }
            LLVM.FinalizeFunctionPassManager(funcPM);

            var modPM = LLVM.CreatePassManager();
            var r = LLVM.RunPassManager(modPM, module.GetModuleRef());
            return (modifiedFunctions, r);
        }

        private void GenerateMainFunction()
        {
            var ltype = LLVM.FunctionType(LLVM.Int32Type(), new LLVMTypeRef[0], false);
            var lfunc = module.AddFunction("main", ltype);
            var entry = lfunc.AppendBasicBlock("entry");

            builder = new IRBuilder();
            builder.PositionBuilderAtEnd(entry);

            {
                var visited = new HashSet<AstVariableDecl>();

                // init global variables
                foreach (var gv in workspace.GlobalScope.VariableDeclarations)
                {
                    InitGlobalVariable(gv, visited);
                }
            }

            { // call main function
                var cheezMain = valueMap[workspace.MainFunction];
                if (workspace.MainFunction.ReturnValue == null)
                {
                    builder.CreateCall(cheezMain, new LLVMValueRef[0], "");
                    builder.CreateRet(LLVM.ConstInt(LLVM.Int32Type(), 0, false));
                }
                else
                {
                    var exitCode = builder.CreateCall(cheezMain, new LLVMValueRef[0], "exitCode");
                    builder.CreateRet(exitCode);
                }
            }


            //if (LLVM.VerifyFunction(lfunc, LLVMVerifierFailureAction.LLVMPrintMessageAction))
            //{
            //    Console.Error.WriteLine($"in function {lfunc}");
            //}

            builder.Dispose();
        }
    }
}
