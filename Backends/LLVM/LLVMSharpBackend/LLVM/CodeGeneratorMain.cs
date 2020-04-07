using Cheez.Ast.Statements;
using Cheez.Types;
using Cheez.Types.Complex;
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
        private bool genDebugInfo = false;

        //
        private Workspace workspace;
        private string targetFile;
        private string intDir;
        private string outDir;

        private bool emitDebugInfo = false;
        private bool dontEmitUnusedDeclarations = true;
        
        private Module module;
        private LLVMContextRef context;
        private LLVMTargetDataRef targetData;

        // <arch><sub>-<vendor>-<sys>-<abi>
        // arch = x86_64, i386, arm, thumb, mips, etc.
        private string targetTriple;

        private Dictionary<CheezType, LLVMTypeRef> typeMap = new Dictionary<CheezType, LLVMTypeRef>();
        private Dictionary<object, LLVMValueRef> valueMap = new Dictionary<object, LLVMValueRef>();
        private Dictionary<AstWhileStmt, LLVMBasicBlockRef> loopBodyMap = new Dictionary<AstWhileStmt, LLVMBasicBlockRef>();
        private Dictionary<IBreakable, LLVMBasicBlockRef> breakTargetMap = new Dictionary<IBreakable, LLVMBasicBlockRef>();

        // destructors
        private Dictionary<CheezType, LLVMValueRef> mDestructorMap = new Dictionary<CheezType, LLVMValueRef>();

        // stack trace
        private bool enableStackTrace = false;
        private bool keepTrackOfStackTrace = false;
        private LLVMTypeRef stackTraceType;
        private LLVMValueRef stackTraceTop;

        // vtable stuff
        private bool checkForNullTraitObjects = false;
        private Dictionary<CheezType, LLVMTypeRef> vtableTypes = new Dictionary<CheezType, LLVMTypeRef>();
        private Dictionary<CheezType, (LLVMValueRef type_info, LLVMValueRef vtable)> typeInfoTable = new Dictionary<CheezType, (LLVMValueRef type_info, LLVMValueRef vtable)>();
        private Dictionary<object, int> vtableIndices = new Dictionary<object, int>();
        private Dictionary<object, ulong> vtableOffsets = new Dictionary<object, ulong>();
        private Dictionary<AstImplBlock, LLVMValueRef> vtableMap = new Dictionary<AstImplBlock, LLVMValueRef>();

        // c lib
        private LLVMValueRef exit;
        private LLVMValueRef printf;
        private LLVMValueRef sprintf;
        private LLVMValueRef puts;
        private LLVMValueRef exitThread;

        //
        private LLVMTypeRef voidPointerType;
        private int pointerSize = 8;

        // rtti stuff
        private CheezType sTypeInfoAttribute;
        private TraitType sTypeInfo;
        private CheezType sTypeInfoInt;
        private CheezType sTypeInfoVoid;
        private CheezType sTypeInfoFloat;
        private CheezType sTypeInfoBool;
        private CheezType sTypeInfoChar;
        private CheezType sTypeInfoString;
        private CheezType sTypeInfoPointer;
        private CheezType sTypeInfoReference;
        private CheezType sTypeInfoSlice;
        private CheezType sTypeInfoArray;
        private CheezType sTypeInfoTuple;
        private CheezType sTypeInfoFunction;
        private CheezType sTypeInfoAny;
        private CheezType sTypeInfoStruct;
        private CheezType sTypeInfoStructMember;
        private CheezType sTypeInfoTupleMember;
        private CheezType sTypeInfoEnum;
        private CheezType sTypeInfoEnumMember;
        private CheezType sTypeInfoTrait;
        private CheezType sTypeInfoTraitFunction;
        private CheezType sTypeInfoTraitImpl;
        private CheezType sTypeInfoType;

        private LLVMTypeRef rttiTypeInfoAttribute;
        private LLVMTypeRef rttiTypeInfoPtr;
        private LLVMTypeRef rttiTypeInfoRef;
        private LLVMTypeRef rttiTypeInfoInt;
        private LLVMTypeRef rttiTypeInfoVoid;
        private LLVMTypeRef rttiTypeInfoFloat;
        private LLVMTypeRef rttiTypeInfoBool;
        private LLVMTypeRef rttiTypeInfoChar;
        private LLVMTypeRef rttiTypeInfoString;
        private LLVMTypeRef rttiTypeInfoPointer;
        private LLVMTypeRef rttiTypeInfoReference;
        private LLVMTypeRef rttiTypeInfoSlice;
        private LLVMTypeRef rttiTypeInfoArray;
        private LLVMTypeRef rttiTypeInfoTuple;
        private LLVMTypeRef rttiTypeInfoFunction;
        private LLVMTypeRef rttiTypeInfoAny;
        private LLVMTypeRef rttiTypeInfoStruct;
        private LLVMTypeRef rttiTypeInfoStructMember;
        private LLVMTypeRef rttiTypeInfoStructMemberInitializer;
        private LLVMTypeRef rttiTypeInfoTupleMember;
        private LLVMTypeRef rttiTypeInfoEnum;
        private LLVMTypeRef rttiTypeInfoEnumMember;
        private LLVMTypeRef rttiTypeInfoTrait;
        private LLVMTypeRef rttiTypeInfoTraitFunction;
        private LLVMTypeRef rttiTypeInfoTraitImpl;
        private LLVMTypeRef rttiTypeInfoType;


        // context
        private AstFuncExpr currentFunction;
        private LLVMValueRef currentLLVMFunction;
        private IRBuilder builder;
        private LLVMBuilderRef rawBuilder;

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
            this.workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
            this.intDir = Path.GetFullPath(intDir ?? "");
            this.outDir = Path.GetFullPath(outDir ?? "");
            this.targetFile = targetFile;
            this.emitDebugInfo = !optimize;

            this.targetTriple = GetTargetTriple(workspace.TargetArch);

            module = new Module("test-module");
            module.SetTarget(targetTriple);
            module.SetDataLayout("e-i64:64-n8:16:32:64");

            LLVM.EnablePrettyStackTrace();

            context = module.GetModuleContext();
            targetData = module.GetTargetData();

            rawBuilder = LLVM.CreateBuilder();

            voidPointerType = LLVM.Int8Type().GetPointerTo();

            //{ // check attributes
            //    for (int i = 1; i < 100; i++)
            //    {
            //        var func = module.AddFunction($"attribute.{i}", LLVM.FunctionType(LLVM.VoidType(), Array.Empty<LLVMTypeRef>(), false));
            //        func.AddFunctionAttribute(context, (LLVMAttributeKind)i);
            //        func.Dump();
            //    }
            //}
            
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
                InitTypeInfoLLVMTypes();
                GenerateVTables();
                GenerateTypeInfos();

                // create declarations
                foreach (var function in workspace.Functions)
                {
                    if (!function.IsGeneric)
                    {
                        bool forceEmitCode = false;
                        if (function.TraitFunction != null)
                            forceEmitCode = true;
                        GenerateFunctionHeader(function, forceEmitCode);
                    }
                }

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
                foreach (var function in workspace.Functions)
                {

                    if (!function.IsGeneric)
                    {
                        bool forceEmitCode = false;
                        if (function.TraitFunction != null)
                            forceEmitCode = true;
                        GenerateFunctionImplementation(function, forceEmitCode);
                    }
                }

                FinishStructMemberInitializers();
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

        public bool CompileCode(IEnumerable<string> libraryIncludeDirectories, IEnumerable<string> libraries, string subsystem, IErrorHandler errorHandler, bool printLikerArgs)
        {
            if (!string.IsNullOrWhiteSpace(outDir) && !Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);

            string objFile = Path.Combine(intDir, targetFile + OS.ObjectFileExtension);
            string exeFile = Path.Combine(outDir, targetFile);


            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return LLVMLinker.Link(workspace, exeFile, objFile, libraryIncludeDirectories, libraries, subsystem, errorHandler, printLikerArgs);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return ClangLinker.Link(workspace, exeFile, objFile, libraries);
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

            var ltype = LLVM.FunctionType(returnType, Array.Empty<LLVMTypeRef>(), false);
            var lfunc = module.AddFunction(mainFuncName, ltype);
            var entry = lfunc.AppendBasicBlock("entry");
            var main = lfunc.AppendBasicBlock("main");

            currentLLVMFunction = lfunc;

            builder = new IRBuilder();
            builder.PositionBuilderAtEnd(entry);

            SetTypeInfos();

            {
                var visited = new HashSet<AstVariableDecl>();

                // init global variables
                foreach (var gv in workspace.Variables)
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
                    builder.CreateCall(cheezMain, Array.Empty<LLVMValueRef>(), "");
                    builder.CreateRet(LLVM.ConstInt(returnType, 0, false));
                }
                else
                {
                    var exitCode = builder.CreateCall(cheezMain, Array.Empty<LLVMValueRef>(), "exitCode");
                    builder.CreateRet(exitCode);
                }
            }

            builder.Dispose();
        }
    }
}
