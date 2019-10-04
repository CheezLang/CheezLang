﻿using Cheez.Ast.Statements;
using Cheez.Types;
using LLVMSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Cheez.CodeGeneration.LLVMCodeGen
{
    public partial class LLVMCodeGenerator : ICodeGenerator
    {
        // temp
        private bool genDebugInfo = true;

        //
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
        private Dictionary<AstWhileStmt, LLVMBasicBlockRef> loopEndMap = new Dictionary<AstWhileStmt, LLVMBasicBlockRef>();
        private Dictionary<AstWhileStmt, LLVMBasicBlockRef> loopPostActionMap = new Dictionary<AstWhileStmt, LLVMBasicBlockRef>();

        // destructors
        private Dictionary<CheezType, LLVMValueRef> mDestructorMap = new Dictionary<CheezType, LLVMValueRef>();

        // stack trace
        private bool enableStackTrace = false;
        private bool keepTrackOfStackTrace = false;
        private LLVMTypeRef stackTraceType;
        private LLVMValueRef stackTraceTop;

        // vtable stuff
        private bool checkForNullTraitObjects = true;
        private Dictionary<CheezType, LLVMTypeRef> vtableTypes = new Dictionary<CheezType, LLVMTypeRef>();
        private Dictionary<object, int> vtableIndices = new Dictionary<object, int>();
        private Dictionary<(CheezType, CheezType), LLVMValueRef> vtableMap = new Dictionary<(CheezType, CheezType), LLVMValueRef>();

        // intrinsics
        private LLVMValueRef memcpy32;
        private LLVMValueRef memcpy64;

        // c lib
        private LLVMValueRef exit;
        private LLVMValueRef printf;

        //
        private LLVMTypeRef pointerType;
        private int pointerSize = 4;

        // context
        private AstFunctionDecl currentFunction;
        private LLVMValueRef currentLLVMFunction;
        private IRBuilder builder;
        private LLVMBuilderRef rawBuilder;
        private Dictionary<object, LLVMValueRef> returnValuePointer = new Dictionary<object, LLVMValueRef>();
        private LLVMBasicBlockRef currentTempBasicBlock;

        public LLVMCodeGenerator(bool enableStackTrace)
        {
            this.enableStackTrace = enableStackTrace;
        }

        private LLVMBuilderRef GetRawBuilder()
        {
            LLVM.PositionBuilderAtEnd(rawBuilder, builder.GetInsertBlock());
            return rawBuilder;
        }

        private string GetTargetTriple(TargetArchitecture arch) {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                switch (arch)
                {
                    case TargetArchitecture.X64:
                        return "x86_64-pc-windows-gnu";

                    case TargetArchitecture.X86:
                        return "i386-pc-win32";
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                switch (arch)
                {
                    case TargetArchitecture.X64:
                        return "x86_64-pc-linux-gnu";

                    case TargetArchitecture.X86:
                        return "i386-pc-linux-gnu";
                }
            }
            throw new NotImplementedException();
        }

        public bool GenerateCode(Workspace workspace, string intDir, string outDir, string targetFile, bool optimize, bool outputIntermediateFile)
        {
            this.workspace = workspace;
            this.intDir = Path.GetFullPath(intDir ?? "");
            this.outDir = Path.GetFullPath(outDir ?? "");
            this.targetFile = targetFile;
            this.emitDebugInfo = !optimize;

            this.targetTriple = GetTargetTriple(workspace.TargetArch);

            module = new Module("test-module");
            module.SetTarget(targetTriple);


            LLVM.EnablePrettyStackTrace();

            context = module.GetModuleContext();
            targetData = module.GetTargetData();

            rawBuilder = LLVM.CreateBuilder();

            pointerType = LLVM.Int8Type().GetPointerTo();
            var pointerSize = pointerType.SizeOf();
            var size = LLVM.SizeOfTypeInBits(targetData, pointerType);
            
            // create dibuilder for debug info
            if (genDebugInfo)
            {
                //dibuilder = new DIBuilder(module);
                //var file = dibuilder.CreateFile("test.che", "examples/test.che");
                //var cu = dibuilder.CreateCompileUnit(file, "Cheez Compiler", false);

                //dibuilder.FinalizeBuilder();
                //dibuilder.DisposeBuilder();

                //var dibuilder = LLVM.NewDIBuilder(module.GetModuleRef());
                //LLVM.DIBuilderCreateCompileUnit(dibuilder, Dwarf.LANG_C, "test.che", ".", "Cheez Compiler", 0, "", 0);
            }

            // generate code
            {
                GenerateIntrinsicDeclarations();

                GenerateVTables();

                // create declarations
                foreach (var function in workspace.mFunctions)
                    if (!function.IsGeneric)
                        GenerateFunctionHeader(function);

                foreach (var t in workspace.TypesWithDestructor)
                {
                    GetDestructor(t);
                }

                //foreach (var i in workspace.mImpls)
                //    foreach (var function in i.SubScope.FunctionDeclarations)
                //        if (!function.IsGeneric)
                //            GenerateFunctionHeader(function);

                CreateCLibFunctions();
                SetupStackTraceStuff();

                SetVTables();

                GenerateMainFunction();

                // create implementations
                foreach (var function in workspace.mFunctions)
                    if (!function.IsGeneric)
                        GenerateFunctionImplementation(function);

                // generate destructors
                GenerateDestructors();

                //foreach (var i in workspace.mImpls)
                //    foreach (var function in i.SubScope.FunctionDeclarations)
                //        if (!function.IsGeneric)
                //            GenerateFunctionImplementation(function);
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
                var objFile = Path.Combine(intDir, $"{targetFile}{OS.ObjectFileExtension}");
                targetMachine.EmitToFile(module, objFile);
            }

            module.DisposeModule();
            return true;
        }

        public bool CompileCode(IEnumerable<string> libraryIncludeDirectories, IEnumerable<string> libraries, string subsystem, IErrorHandler errorHandler)
        {
            if (!string.IsNullOrWhiteSpace(outDir) && !Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);

            string objFile = Path.Combine(intDir, targetFile + OS.ObjectFileExtension);
            string exeFile = Path.Combine(outDir, targetFile);


            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return LLVMLinker.Link(workspace, exeFile, objFile, libraryIncludeDirectories, libraries, subsystem, errorHandler);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return ClangLinker.Link(workspace, exeFile, objFile, libraryIncludeDirectories, libraries, subsystem, errorHandler);
            else
                throw new NotImplementedException();
        }

        private (int, bool) RunOptimizations(uint level)
        {
            module.PrintToFile(Path.Combine(intDir, targetFile + ".debug.ll"));

            var pmBuilder = LLVM.PassManagerBuilderCreate();
            LLVM.PassManagerBuilderSetOptLevel(pmBuilder, level);
            LLVM.PassManagerBuilderUseInlinerWithThreshold(pmBuilder, 100);

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

            // optimize module
            bool r = false;
            var modPM = LLVM.CreatePassManager();
            LLVM.PassManagerBuilderPopulateModulePassManager(pmBuilder, modPM);
            r = LLVM.RunPassManager(modPM, module.GetModuleRef());


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
            string mainFuncName = null;
            LLVMTypeRef returnType = LLVM.VoidType();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                switch (workspace.TargetArch)
                {
                    case TargetArchitecture.X86:
                        mainFuncName = "main";
                        returnType = LLVM.Int32Type();
                        break;
                    case TargetArchitecture.X64:
                        mainFuncName = "WinMain";
                        returnType = LLVM.Int64Type();
                        break;
                }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                switch (workspace.TargetArch)
                {
                    case TargetArchitecture.X86:
                        mainFuncName = "main";
                        returnType = LLVM.Int32Type();
                        break;
                    case TargetArchitecture.X64:
                        mainFuncName = "main";
                        returnType = LLVM.Int32Type();
                        break;
                }

            var ltype = LLVM.FunctionType(returnType, new LLVMTypeRef[0], false);
            var lfunc = module.AddFunction(mainFuncName, ltype);
            var entry = lfunc.AppendBasicBlock("entry");
            var main = lfunc.AppendBasicBlock("main");

            currentLLVMFunction = lfunc;

            builder = new IRBuilder();
            builder.PositionBuilderAtEnd(entry);

            {
                var visited = new HashSet<AstVariableDecl>();

                // init global variables
                foreach (var gv in workspace.mVariables)
                {
                    InitGlobalVariable(gv, visited);
                }
            }

            builder.CreateBr(main);
            builder.PositionBuilderAtEnd(main);

            { // call main function
                var cheezMain = valueMap[workspace.MainFunction];
                if (workspace.MainFunction.ReturnTypeExpr == null)
                {
                    builder.CreateCall(cheezMain, new LLVMValueRef[0], "");
                    builder.CreateRet(LLVM.ConstInt(returnType, 0, false));
                }
                else
                {
                    var exitCode = builder.CreateCall(cheezMain, new LLVMValueRef[0], "exitCode");
                    builder.CreateRet(exitCode);
                }
            }

            builder.Dispose();
        }
    }
}
