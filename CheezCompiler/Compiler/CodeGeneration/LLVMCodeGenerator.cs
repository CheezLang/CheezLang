using Cheez.Compiler.Ast;
using Cheez.Compiler.Visitor;
using LLVMSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Text;
using Cheez.Compiler.CodeGeneration.LLVMCodeGen;

namespace Cheez.Compiler.CodeGeneration
{
    class VsWhere
    {
#pragma warning disable 0649 // #warning directive
        public string instanceId;
        public string installDate;
        public string installationName;
        public string installationPath;
        public string installationVersion;
        public string isPrerelease;
        public string displayName;
        public string description;
        public string enginePath;
        public string channelId;
        public string channelPath;
        public string channelUri;
        public string releaseNotes;
        public string thirdPartyNotices;
#pragma warning restore CS0649 // #warning directive
    }

    static class LinkerUtil
    {
        private static (int, string) FindVSInstallDirWithVsWhere()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                var programFilesX86 = Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%");
                var p = Util.StartProcess($@"{programFilesX86}\Microsoft Visual Studio\Installer\vswhere.exe", "-nologo -latest -format json", stdout:
                    (sender, e) =>
                    {
                        sb.AppendLine(e.Data);
                    });
                p.WaitForExit();

                var versions = Newtonsoft.Json.JsonConvert.DeserializeObject<VsWhere[]>(sb.ToString());

                if (versions?.Length == 0)
                    return (-1, null);

                var latest = versions[0];

                var v = latest.installationVersion.Scan1(@"(\d+)\.(\d+)\.(\d+)\.(\d+)").Select(s => int.TryParse(s, out int i) ? i : 0).First();
                var dir = latest.installationPath;

                return (v, dir);
            }
            catch (Exception)
            {
                return (-1, null);
            }
        }

        private static (int, string) FindVSInstallDirWithRegistry()
        {
            return (-1, null);
        }

        private static (int, string) FindVSInstallDirWithPath()
        {
            return (-1, null);
        }

        private static string FindVSLibDir(int version, string installDir)
        {
            switch (version)
            {
                case 15:
                    {
                        var MSVC = Path.Combine(installDir, "VC", "Tools", "MSVC");
                        if (!Directory.Exists(MSVC))
                            return null;

                        SortedList<int[], string> versions = new SortedList<int[], string>(Comparer<int[]>.Create((a, b) =>
                        {
                            for (int i = 0; i < a.Length && i < b.Length; i++)
                            {
                                if (a[i] > b[i])
                                    return -1;
                                else if (a[i] < b[i])
                                    return 1;
                            }

                            return 0;
                        }));

                        foreach (var sub in Directory.EnumerateDirectories(MSVC))
                        {
                            var name = sub.Substring(sub.LastIndexOfAny(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }) + 1);
                            var v = name.Scan1(@"(\d+)\.(\d+)\.(\d+)").Select(s => int.TryParse(s, out int i) ? i : 0).ToArray();

                            if (v.Length != 3)
                                continue;

                            versions.Add(v, sub);
                        }

                        foreach (var kv in versions)
                        {
                            var libDir = Path.Combine(kv.Value, "lib");

                            if (!Directory.Exists(libDir))
                                continue;

                            return libDir;
                        }

                        return null;
                    }

                default:
                    return null;
            }
        }

        public static string FindVisualStudioLibraryDirectory()
        {
            var (version, dir) = FindVSInstallDirWithVsWhere();

            if (dir == null)
                (version, dir) = FindVSInstallDirWithRegistry();

            if (dir == null)
                (version, dir) = FindVSInstallDirWithPath();

            if (dir == null)
                return null;

            return FindVSLibDir(version, dir);
        }
    }

    public class LLVMCodeGeneratorData
    {
        public LLVMBuilderRef Builder { get; set; }
        public bool Deref { get; set; } = true;
        public LLVMValueRef LFunction { get; set; }
        public AstFunctionDecl Function { get; set; }
        public LLVMBasicBlockRef BasicBlock { get; set; }

        public LLVMBasicBlockRef LoopCondition { get; set; }
        public LLVMBasicBlockRef LoopEnd { get; set; }

        [DebuggerStepThrough]
        public LLVMCodeGeneratorData Clone(LLVMBuilderRef? Builder = null, bool? Deref = null, LLVMValueRef? Function = null, LLVMBasicBlockRef? BasicBlock = null, LLVMBasicBlockRef? LoopCondition = null, LLVMBasicBlockRef? LoopEnd = null)
        {
            return new LLVMCodeGeneratorData
            {
                Builder = Builder ?? this.Builder,
                Deref = Deref ?? this.Deref,
                LFunction = Function ?? this.LFunction,
                BasicBlock = BasicBlock ?? this.BasicBlock,
                LoopCondition = LoopCondition ?? this.LoopCondition,
                LoopEnd = LoopEnd ?? this.LoopEnd,
                Function = this.Function
            };
        }

        public void MoveBuilderTo(LLVMBasicBlockRef bb)
        {
            LLVM.PositionBuilderAtEnd(Builder, bb);
            BasicBlock = bb;
        }
    }

    public class LLVMCodeGenerator : VisitorBase<LLVMValueRef, LLVMCodeGeneratorData>, ICodeGenerator
    {
        private string targetFile;

        private LLVMModuleRef module;
        private LLVMContextRef context;
        private LLVMTargetDataRef moduleDataLayout;
        private Workspace workspace;

        private Dictionary<object, LLVMValueRef> valueMap = new Dictionary<object, LLVMValueRef>();
        private Dictionary<object, LLVMValueRef> vtableMap = new Dictionary<object, LLVMValueRef>();
        private Dictionary<object, LLVMTypeRef> typeMap = new Dictionary<object, LLVMTypeRef>();
        //private Dictionary<object, LLVMTypeRef> vtableTypeMap = new Dictionary<object, LLVMTypeRef>();
        private Dictionary<object, int> vtableIndices = new Dictionary<object, int>();

        private LLVMTypeRef vtableType;

        private LLVMValueRef currentFunction;

        private uint pointerSize = 4;

        // intrinsics
        private LLVMValueRef memcpy64;

        //
        [DllImport("Linker.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static bool llvm_link_coff(string[] argv, int argc);

        public bool GenerateCode(Workspace workspace, string targetFile)
        {
            this.targetFile = targetFile;

            // <arch><sub>-<vendor>-<sys>-<abi>
            // arch = x86_64, i386, arm, thumb, mips, etc.
            const string targetTriple = "i386-pc-win32";


            module = LLVM.ModuleCreateWithName("test");
            this.workspace = workspace;

            // <arch><sub>-<vendor>-<sys>-<abi>
            LLVM.SetTarget(module, targetTriple);
            moduleDataLayout = LLVM.GetModuleDataLayout(module);
            context = LLVM.GetModuleContext(module);

            //Console.WriteLine("data layout: " + LLVM.GetDataLayoutStr(module));

            // generate named types
            foreach (var t in workspace.GlobalScope.TypeDeclarations)
            {
                if (t is AstStructDecl s)
                {
                    var llvmType = LLVM.StructCreateNamed(context, $"struct.{s.Name.Name}");
                    var memTypes = s.Members.Select(m => CheezTypeToLLVMType(m.Type, voidPointer: true)).ToArray();
                    LLVM.StructSetBody(llvmType, memTypes, false);
                    typeMap[s] = llvmType;
                    typeMap[s.Type] = llvmType;
                }
            }

            //
            GenerateIntrinsicDeclarations();

            // generate vtable
            GenerateVTables();

            // generate all types
            GenerateAllTypes();

            // generate global variables
            GenerateGlobalVariables();

            // generate functions
            GenerateFunctions();

            GenerateMainFunction();

            // set vtable values
            SetVTables();

            // verify module
            {
                LLVM.VerifyModule(module, LLVMVerifierFailureAction.LLVMPrintMessageAction, out string llvmErrors);
                if (!string.IsNullOrWhiteSpace(llvmErrors))
                    Console.Error.WriteLine($"[LLVM-validate-module] {llvmErrors}");
            }

            // generate file
            if (true)
            {
                LLVM.PrintModuleToFile(module, $"{targetFile}.ll", out string llvmErrors);
                if (!string.IsNullOrWhiteSpace(llvmErrors))
                    Console.Error.WriteLine($"[PrintModuleToFile] {llvmErrors}");
            }

            {
                var optimize = false;
                if (optimize)
                {
                    var pmBuilder = LLVM.PassManagerBuilderCreate();
                    LLVM.PassManagerBuilderSetOptLevel(pmBuilder, 0);
                    LLVM.PassManagerBuilderUseInlinerWithThreshold(pmBuilder, 1);
                    LLVM.PassManagerBuilderSetSizeLevel(pmBuilder, 1);
                    LLVM.PassManagerBuilderSetDisableUnrollLoops(pmBuilder, false);
                    LLVM.PassManagerBuilderSetDisableUnitAtATime(pmBuilder, true);
                    LLVM.PassManagerBuilderSetDisableSimplifyLibCalls(pmBuilder, false);

                    var funcPM = LLVM.CreateFunctionPassManagerForModule(module);
                    LLVM.PassManagerBuilderPopulateFunctionPassManager(pmBuilder, funcPM);
                    bool r = LLVM.InitializeFunctionPassManager(funcPM);

                    // optimize functions
                    var func = LLVM.GetFirstFunction(module);
                    while (func.Pointer != IntPtr.Zero)
                    {
                        if (!func.IsDeclaration())
                        {
                            var res = LLVM.RunFunctionPassManager(funcPM, func);
                            if (!res)
                            {
                                workspace.ReportError("Failed to run llvm passes on function " + func.ToString());
                            }
                        }
                        func = func.GetNextFunction();
                    }
                    r = LLVM.FinalizeFunctionPassManager(funcPM);

                    var modPM = LLVM.CreatePassManager();
                    LLVM.PassManagerBuilderPopulateModulePassManager(pmBuilder, modPM);
                    r = LLVM.RunPassManager(modPM, module);

                    // verify module
                    {
                        LLVM.VerifyModule(module, LLVMVerifierFailureAction.LLVMPrintMessageAction, out string llvmErrors);
                        if (!string.IsNullOrWhiteSpace(llvmErrors))
                            Console.Error.WriteLine($"[LLVM-validate-module] {llvmErrors}");
                    }

                    if (true)
                    {
                        LLVM.PrintModuleToFile(module, $"{targetFile}.opt.ll", out string llvmErrors);
                        if (!string.IsNullOrWhiteSpace(llvmErrors))
                            Console.Error.WriteLine($"[PrintModuleToFile] {llvmErrors}");
                    }
                }


                LLVM.InitializeX86TargetMC();
                LLVM.InitializeX86Target();
                LLVM.InitializeX86TargetInfo();
                LLVM.InitializeX86AsmParser();
                LLVM.InitializeX86AsmPrinter();

                LLVMTargetRef target = default;
                if (LLVM.GetTargetFromTriple(targetTriple, out target, out var llvmTargetError))
                {
                    System.Console.Error.WriteLine(llvmTargetError);
                    return false;
                }
                var targetMachine = LLVM.CreateTargetMachine(
                    target,
                    targetTriple,
                    "generic",
                    "",
                    LLVMCodeGenOptLevel.LLVMCodeGenLevelAggressive,
                    LLVMRelocMode.LLVMRelocDefault,
                    LLVMCodeModel.LLVMCodeModelDefault);

                var objFile = targetFile + ".obj";
                var dir = Path.GetDirectoryName(Path.GetFullPath(targetFile));
                objFile = Path.Combine(dir, Path.GetFileName(objFile));
                var filename = Marshal.StringToCoTaskMemAnsi(objFile);
                var uiae = Marshal.PtrToStringAnsi(filename);

                if (LLVM.TargetMachineEmitToFile(targetMachine, module, filename, LLVMCodeGenFileType.LLVMObjectFile, out string llvmError))
                {
                    System.Console.Error.WriteLine(llvmError);
                    return false;
                }


            }

            // cleanup
            LLVM.DisposeModule(module);
            return true;
        }

        public bool CompileCode(IEnumerable<string> libraryIncludeDirectories, IEnumerable<string> libraries, string subsystem, IErrorHandler errorHandler)
        {
            foreach (var f in workspace.Files)
            {
                libraries = libraries.Concat(f.Libraries);
            }
            libraries = libraries.Distinct();

            var winSdk = OS.FindWindowsSdk();
            if (winSdk == null)
            {
                errorHandler.ReportError("Couldn't find windows sdk");
                return false;
            }

            var msvcLibPath = LinkerUtil.FindVisualStudioLibraryDirectory();
            if (winSdk == null)
            {
                errorHandler.ReportError("Couldn't find Visual Studio library directory");
                return false;
            }

            var filename = Path.GetFileNameWithoutExtension(targetFile + ".x");
            var dir = Path.GetDirectoryName(Path.GetFullPath(targetFile));

            filename = Path.Combine(dir, filename);

            var lldArgs = new List<string>();
            lldArgs.Add("lld");
            lldArgs.Add($"/out:{filename}.exe");

            // library paths
            if (winSdk.UcrtPath != null)
                lldArgs.Add($@"-libpath:{winSdk.UcrtPath}\x86");

            if (winSdk.UmPath != null)
                lldArgs.Add($@"-libpath:{winSdk.UmPath}\x86");

            var exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            lldArgs.Add($"-libpath:{exePath}");

            if (msvcLibPath != null)
                lldArgs.Add($@"-libpath:{msvcLibPath}\x86");

            foreach (var linc in libraryIncludeDirectories)
            {
                lldArgs.Add($@"-libpath:{linc}");
            }

            // @hack
            lldArgs.Add($@"-libpath:{Environment.CurrentDirectory}\CheezRuntimeLibrary\lib\x86");
            lldArgs.Add($@"-libpath:{Assembly.GetEntryAssembly().Location}\CheezRuntimeLibrary\lib\x86");
            lldArgs.Add($@"-libpath:D:\Program Files (x86)\LLVM\lib");

            // other options
            lldArgs.Add("/entry:mainCRTStartup");
            lldArgs.Add("/machine:X86");
            lldArgs.Add($"/subsystem:{subsystem}");

            // runtime
            //lldArgs.Add("cheez-rtd.obj");
            lldArgs.Add("clang_rt.builtins-i386.lib");

            // windows and c libs
            lldArgs.Add("libucrtd.lib");
            lldArgs.Add("libcmtd.lib");

            lldArgs.Add("kernel32.lib");
            lldArgs.Add("user32.lib");
            lldArgs.Add("gdi32.lib");
            lldArgs.Add("winspool.lib");
            lldArgs.Add("comdlg32.lib");
            lldArgs.Add("advapi32.lib");
            lldArgs.Add("shell32.lib");
            lldArgs.Add("ole32.lib");
            lldArgs.Add("oleaut32.lib");
            lldArgs.Add("uuid.lib");
            lldArgs.Add("odbc32.lib");
            lldArgs.Add("odbccp32.lib");

            lldArgs.Add("legacy_stdio_definitions.lib");
            lldArgs.Add("legacy_stdio_wide_specifiers.lib");
            lldArgs.Add("libclang.lib");
            lldArgs.Add("libvcruntimed.lib");
            lldArgs.Add("msvcrtd.lib");

            foreach (var linc in libraries)
            {
                lldArgs.Add(linc);
            }

            // generated object files
            lldArgs.Add($"{filename}.obj");

            var result = llvm_link_coff(lldArgs.ToArray(), lldArgs.Count);
            if (result)
            {
                Console.WriteLine($"Generated {filename}.exe");
            }

            return result;
        }

        private DataReceivedEventHandler CreateHandler(string name, TextWriter writer)
        {
            return (l, a) =>
            {
                if (!string.IsNullOrWhiteSpace(a.Data))
                    writer.WriteLine($"[{name}] {a.Data}");
            };
        }

        private void GenerateGlobalVariables()
        {
            var builder = LLVM.CreateBuilder();
            foreach (var v in workspace.GlobalScope.VariableDeclarations)
            {
                v.Accept(this, new LLVMCodeGeneratorData
                {
                    Builder = builder
                });
            }

            LLVM.DisposeBuilder(builder);
        }



        ///////////////////////////////////////
        private void GenerateIntrinsicDeclarations()
        {
            memcpy64 = GenerateIntrinsicDeclaration("llvm.memcpy.p0i8.p0i8.i64", LLVM.VoidType(),
                LLVM.PointerType(LLVM.Int8Type(), 0),
                LLVM.PointerType(LLVM.Int8Type(), 0),
                LLVM.Int64Type());
        }

        private LLVMValueRef GenerateIntrinsicDeclaration(string name, LLVMTypeRef retType, params LLVMTypeRef[] paramTypes)
        {
            var ltype = LLVM.FunctionType(retType, paramTypes, false);
            var lfunc = LLVM.AddFunction(module, name, ltype);

            return lfunc;
        }
        //
        private void GenerateMainFunction()
        {
            var ltype = LLVM.FunctionType(LLVM.Int32Type(), new LLVMTypeRef[0], false);
            var lfunc = LLVM.AddFunction(module, "main", ltype);
            currentFunction = lfunc;

            LLVMBuilderRef builder = LLVM.CreateBuilder();
            LLVM.PositionBuilderAtEnd(builder, LLVM.AppendBasicBlock(lfunc, "entry"));

            var cheezMain = valueMap[workspace.MainFunction];

            // initialize global variables
            {
                var d = new LLVMCodeGeneratorData
                {
                    Builder = builder
                };

                //foreach (var str in workspace.GlobalScope.TypeDeclarations)
                //{
                //    if (str is AstStructDecl @struct && @struct.Traits.Count > 0)
                //    {
                //        var vtable = vtableMap[@struct];
                //        foreach (var impl in @struct.Implementations)
                //        {
                //            foreach (var func in impl.FunctionInstances)
                //            {
                //                var llvmFunc = valueMap[func];
                //                var vtableIndex = vtableIndices[func];

                //                var ptr = LLVM.BuildStructGEP(builder, vtable, (uint)vtableIndex, "");
                //                var r = LLVM.BuildStore(builder, llvmFunc, ptr);

                //            }
                //        }
                //    }
                //}

                foreach (var g in workspace.GlobalScope.VariableDeclarations)
                {
                    if (g.IsConstant)
                        continue;
                    if (g.Initializer != null)
                    {
                        var val = g.Initializer.Accept(this, d);
                        var ptr = valueMap[g];
                        LLVM.BuildStore(builder, val, ptr);
                    }
                }
            }

            {
                if (workspace.MainFunction.ReturnType == VoidType.Intance)
                {
                    LLVM.BuildCall(builder, cheezMain, new LLVMValueRef[0], "");
                    LLVM.BuildRet(builder, LLVM.ConstInt(LLVM.Int32Type(), 0, false));
                }
                else
                {
                    var exitCode = LLVM.BuildCall(builder, cheezMain, new LLVMValueRef[0], "exitCode");
                    LLVM.BuildRet(builder, exitCode);
                }
            }


            bool v = LLVM.VerifyFunction(lfunc, LLVMVerifierFailureAction.LLVMPrintMessageAction);
            if (v)
            {
                Console.Error.WriteLine($"in function {lfunc}");
            }
            LLVM.DisposeBuilder(builder);
        }

        #region Utility

        private void SetVTables()
        {
            var vfuncTypes = LLVM.GetStructElementTypes(vtableType);
            var vfuncCount = vfuncTypes.Length;

            foreach (var kv in workspace.TypeTraitMap)
            {
                var type = kv.Key;
                var traits = kv.Value;

                var functions = new LLVMValueRef[vfuncCount];
                for (int i = 0; i < functions.Length; i++)
                    functions[i] = LLVM.ConstNull(vfuncTypes[i]);
                foreach (var impl in workspace.GetTraitImplementations(type))
                {
                    foreach (var func in impl.FunctionInstances)
                    {
                        var traitFunc = func.TraitFunction;
                        var index = vtableIndices[traitFunc];
                        functions[index] = valueMap[func];
                    }
                }
                var defValue = LLVM.ConstNamedStruct(vtableType, functions);

                var vtable = vtableMap[type];
                LLVM.SetInitializer(vtable, defValue);
            }

            //foreach (var str in workspace.GlobalScope.TypeDeclarations)
            //{
            //    if (str is AstStructDecl @struct && @struct.Traits.Count > 0)
            //    {
            //        var functions = new LLVMValueRef[vfuncCount];
            //        for (int i = 0; i < functions.Length; i++)
            //            functions[i] = LLVM.ConstNull(vfuncTypes[i]);
            //        foreach (var impl in @struct.Implementations)
            //        {
            //            if (impl.Trait == null)
            //                continue;

            //            foreach (var func in impl.FunctionInstances)
            //            {
            //                var traitFunc = func.TraitFunction;
            //                var index = vtableIndices[traitFunc];
            //                functions[index] = valueMap[func];
            //            }
            //        }
            //        var defValue = LLVM.ConstNamedStruct(vtableType, functions);

            //        var vtable = vtableMap[@struct];
            //        LLVM.SetInitializer(vtable, defValue);
            //    }
            //}
        }

        private void GenerateVTables()
        {
            // create vtable type
            var vfuncs = new List<AstFunctionDecl>();
            foreach (var type in workspace.GlobalScope.TypeDeclarations)
            {
                if (type is AstTraitDeclaration trait)
                {
                    foreach (var func in trait.Functions)
                    {
                        if (func.IsGeneric)
                        {
                            throw new NotImplementedException();
                        }
                        else
                        {
                            vfuncs.Add(func);
                        }
                    }
                }
            }
            var temp = string.Join("\n", vfuncs);
            Console.WriteLine($"vtables: \n{temp}");

            var funcTypes = new List<LLVMTypeRef>();
            foreach (var func in vfuncs)
            {
                vtableIndices[func] = funcTypes.Count;

                var funcType = CheezTypeToLLVMType(func.Type, true);
                funcTypes.Add(funcType);
            }

            vtableType = LLVM.StructCreateNamed(context, "__vtable_type");
            LLVM.StructSetBody(vtableType, funcTypes.ToArray(), false);
            //vtableType = LLVM.StructType(funcTypes.ToArray(), false);

            foreach (var kv in workspace.TypeTraitMap)
            {
                var type = kv.Key;
                var traits = kv.Value;

                var vtable = LLVM.AddGlobal(module, vtableType, "__vtable_" + type.ToString());
                LLVM.SetLinkage(vtable, LLVMLinkage.LLVMInternalLinkage);
                vtableMap[type] = vtable;
            }

            //foreach (var str in workspace.GlobalScope.TypeDeclarations)
            //{
            //    if (str is AstStructDecl @struct && @struct.Traits.Count > 0)
            //    {
            //        var vtable = LLVM.AddGlobal(module, vtableType, "__vtable_" + @struct.Name.Name);
            //        LLVM.SetLinkage(vtable, LLVMLinkage.LLVMInternalLinkage);
            //        vtableMap[@struct] = vtable;
            //    }
            //}
        }

        private void CastIfAny(LLVMBuilderRef builder, CheezType targetType, CheezType sourceType, ref LLVMValueRef value)
        {
            if (targetType == CheezType.Any && sourceType != CheezType.Any)
            {
                var type = CheezTypeToLLVMType(targetType);
                if (sourceType is IntType)
                    value = LLVM.BuildIntCast(builder, value, type, "");
                else if (sourceType is BoolType)
                    value = LLVM.BuildZExtOrBitCast(builder, value, type, "");
                else if (sourceType is PointerType || sourceType is CStringType || sourceType is ArrayType)
                    value = LLVM.BuildPtrToInt(builder, value, type, "");
                else
                    throw new NotImplementedException("any cast");
            }
        }

        private LLVMValueRef CheezValueToLLVMValue(object value)
        {
            switch (value)
            {
                case int i:
                    return LLVM.ConstInt(LLVM.Int32Type(), (ulong)i, true);
            }

            throw new NotImplementedException();
        }

        //private LLVMTypeRef FunctionParamTypeToLLVMType(CheezType ct)
        //{
        //    if (CanPassByValue(ct))
        //        return CheezTypeToLLVMType(ct);
        //    else
        //        return LLVM.PointerType(CheezTypeToLLVMType(ct), 0);
        //}

        //private bool CanPassByValue(CheezType ct)
        //{
        //    switch (ct)
        //    {
        //        case IntType _:
        //        case FloatType _:
        //        case PointerType _:
        //        case BoolType _:
        //        case StringType _:
        //            return true;

        //        default:
        //            return false;
        //    }
        //}
        private LLVMTypeRef CheezTypeToLLVMType(CheezType ct, bool functionPointer = true, bool voidPointer = false)
        {
            return CheezTypeToLLVMTypeHelper(ct, functionPointer, voidPointer);
        }

        private LLVMTypeRef CheezTypeToLLVMTypeHelper(CheezType ct, bool functionPointer, bool voidPointer = false)
        {
            switch (ct)
            {
                case TraitType t:
                    return LLVM.StructType(new LLVMTypeRef[] {
                    LLVM.PointerType(LLVM.Int8Type(), 0),
                    LLVM.PointerType(LLVM.Int8Type(), 0)
                }, false);

                case AnyType a:
                    return LLVM.Int64Type();

                case BoolType b:
                    return LLVM.Int1Type();

                case IntType i:
                    return LLVM.IntType((uint)i.Size * 8);

                case FloatType f:
                    if (f.Size == 4)
                        return LLVM.FloatType();
                    else if (f.Size == 8)
                        return LLVM.DoubleType();
                    else
                        throw new NotImplementedException();

                case CharType c:
                    return LLVM.Int8Type();

                case CStringType _:
                    return LLVM.PointerType(LLVM.Int8Type(), 0);

                case PointerType p:
                    if (voidPointer || p.TargetType == VoidType.Intance)
                        return LLVM.PointerType(LLVM.Int8Type(), 0);
                    return LLVM.PointerType(CheezTypeToLLVMType(p.TargetType), 0);

                case ArrayType a:
                    return LLVM.ArrayType(CheezTypeToLLVMType(a.TargetType), (uint)a.Length);

                case SliceType s:
                    return LLVM.StructType(new LLVMTypeRef[]
                    {
                    LLVM.PointerType(CheezTypeToLLVMType(s.TargetType), 0),
                    LLVM.Int32Type()
                    }, false);


                case ReferenceType r:
                    return LLVM.PointerType(CheezTypeToLLVMType(r.TargetType), 0);

                case VoidType _:
                    return LLVM.VoidType();

                case FunctionType f:
                    {
                        var paramTypes = new List<LLVMTypeRef>();
                        var returnType = CheezTypeToLLVMType(f.ReturnType);
                        //if (!CanPassByValue(f.ReturnType) && !(f.ReturnType is VoidType))
                        //{
                        //    paramTypes.Add(LLVM.PointerType(returnType, 0));
                        //    returnType = LLVM.VoidType();
                        //}
                        foreach (var p in f.ParameterTypes)
                        {
                            var pt = CheezTypeToLLVMType(p);

                            paramTypes.Add(pt);
                        }

                        var func = LLVM.FunctionType(returnType, paramTypes.ToArray(), f.VarArgs);
                        if (functionPointer)
                            func = LLVM.PointerType(func, 0);
                        return func;
                    }

                case EnumType e:
                    {
                        return CheezTypeToLLVMType(e.MemberType);
                    }

                case StructType s:
                    {
                        var memTypes = s.Declaration.Members.Select(m => CheezTypeToLLVMType(m.Type, voidPointer: true)).ToArray();
                        return LLVM.StructType(memTypes, false);
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        private void PrintFunctionsInModule()
        {
            var f = LLVM.GetFirstFunction(module);

            Console.WriteLine($"Functions in module {module}");
            Console.WriteLine("==================");
            while (f.Pointer.ToInt64() != 0)
            {
                Console.WriteLine(f.ToString());
                f = f.GetNextFunction();
            }
            Console.WriteLine("==================");
        }

        private void GenerateAllTypes()
        {
            foreach (var td in workspace.GlobalScope.TypeDeclarations)
            {
                td.Accept(this, null);
            }
        }

        //[DebuggerStepThrough]
        private void GenerateFunctions()
        {
            // create declarations
            foreach (var function in workspace.GlobalScope.FunctionDeclarations)
            {
                GenerateFunctionHeader(function);
            }

            // impl functions
            //foreach (var impl in workspace.GlobalScope.ImplBlocks)
            //{
            //    foreach (var function in impl.SubScope.FunctionDeclarations)
            //    {
            //        GenerateFunctionHeader(function);
            //    }
            //}

            // create implementations
            foreach (var f in workspace.GlobalScope.FunctionDeclarations)
            {
                f.Accept(this, null);
            }
            //foreach (var impl in workspace.GlobalScope.ImplBlocks)
            //{
            //    foreach (var function in impl.SubScope.FunctionDeclarations)
            //    {
            //        function.Accept(this, null);
            //    }
            //}
        }

        private void GenerateFunctionHeader(AstFunctionDecl function)
        {
            var varargs = function.GetDirective("varargs");
            if (varargs != null)
            {
                (function.Type as FunctionType).VarArgs = true;
            }

            var name = function.Name.Name;
            if (function.IsPolyInstance)
            {
                name += ".";
                name += string.Join(".", function.PolymorphicTypes.Select(p => $"{p.Key}.{p.Value}"));
            }

            var ltype = CheezTypeToLLVMType(function.Type, false);
            var lfunc = LLVM.AddFunction(module, name, ltype);

            for (uint i = 0; i < function.Parameters.Count; i++)
            {
                var param = function.Parameters[(int)i];
                var llvmParam = lfunc.GetParam(i);

                //if (!CanPassByValue(param.Type))
                //{
                //    //var att = LLVM.CreateStringAttribute(context, "sret", 4, "", 0);
                //    var att = LLVM.CreateEnumAttribute(context, AttributeKind.ByVal.ToUInt(), 0);
                //    LLVM.AddAttributeAtIndex(lfunc, (LLVMAttributeIndex)(i + 1), att);

                //}
            }

            var ccDir = function.GetDirective("stdcall");
            if (ccDir != null)
            {
                LLVM.SetFunctionCallConv(lfunc, (uint)LLVMCallConv.LLVMX86StdcallCallConv);
            }

            valueMap[function] = lfunc;
        }

        //private LLVMValueRef CastIfString(LLVMValueRef val, LLVMBuilderRef builder)
        //{

        //}

        private LLVMValueRef GetDefaultLLVMValue(CheezType type)
        {
            switch (type)
            {
                case PointerType p:
                    return LLVM.ConstIntToPtr(LLVM.ConstInt(LLVM.IntType(pointerSize * 8), 0, false), CheezTypeToLLVMType(type));

                case CStringType s:
                    return LLVM.ConstIntToPtr(LLVM.ConstInt(LLVM.IntType(pointerSize * 8), 0, false), LLVM.PointerType(LLVM.Int8Type(), 0));

                case IntType i:
                    return LLVM.ConstInt(CheezTypeToLLVMType(type), 0, false);

                case FloatType f:
                    return LLVM.ConstReal(CheezTypeToLLVMType(type), 0.0);

                case CharType c:
                    return LLVM.ConstInt(CheezTypeToLLVMType(type), 0, false);

                case StructType p:
                    return LLVM.ConstStruct(p.Declaration.Members.Select(m => GetDefaultLLVMValue(m.Type)).ToArray(), false);

                case TraitType t:
                    return LLVM.ConstStruct(new LLVMValueRef[] {
                        LLVM.ConstPointerNull(LLVM.PointerType(LLVM.Int8Type(), 0)),
                        LLVM.ConstPointerNull(LLVM.PointerType(LLVM.Int8Type(), 0))
                    }, false);

                case AnyType a:
                    return LLVM.ConstInt(LLVM.Int64Type(), 0, false);

                default:
                    throw new NotImplementedException();
            }
        }

        private LLVMValueRef AllocVar(LLVMBuilderRef builder, CheezType type, string name = "")
        {
            var llvmType = CheezTypeToLLVMType(type);
            if (type is ArrayType a)
            {
                llvmType = CheezTypeToLLVMType(a);
                //LLVM.BuildArrayAlloca(builder, llvmType, )
                var val = LLVM.BuildAlloca(builder, llvmType, name);
                var alignment = moduleDataLayout.AlignmentOfType(llvmType);
                LLVM.SetAlignment(val, alignment);
                return val;
            }
            else
            {
                var val = LLVM.BuildAlloca(builder, llvmType, name);
                var alignment = moduleDataLayout.AlignmentOfType(llvmType);
                LLVM.SetAlignment(val, alignment);
                return val;
            }
        }

        private LLVMValueRef CreateLocalVariable(ITypedSymbol sym)
        {
            if (valueMap.ContainsKey(sym))
                return valueMap[sym];

            var t = CreateLocalVariable(sym.Type);
            valueMap[sym] = t;
            return t;
        }

        private LLVMValueRef CreateLocalVariable(CheezType exprType)
        {
            var builder = LLVM.CreateBuilder();

            var bb = currentFunction.GetFirstBasicBlock();
            var brInst = bb.GetLastInstruction();
            LLVM.PositionBuilderBefore(builder, brInst);

            var type = CheezTypeToLLVMType(exprType);
            var result = LLVM.BuildAlloca(builder, type, "");
            var alignment = moduleDataLayout.AlignmentOfType(type);
            LLVM.SetAlignment(result, alignment);

            LLVM.DisposeBuilder(builder);

            return result;
        }

        #endregion

        #region Statements

        public override LLVMValueRef VisitMatchStatement(AstMatchStmt match, LLVMCodeGeneratorData data = null)
        {
            var value = match.Value.Accept(this, data.Clone(Deref: true));

            var end = LLVM.AppendBasicBlock(data.LFunction, "match_end");

            var sw = LLVM.BuildSwitch(data.Builder, value, end, (uint)match.Cases.Count);
            foreach (var ca in match.Cases)
            {
                var cv = ca.Value.Accept(this, data.Clone(Deref: true));

                var cbb = LLVM.AppendBasicBlock(data.LFunction, "case " + ca.Value.ToString());
                LLVM.PositionBuilderAtEnd(data.Builder, cbb);
                data.BasicBlock = cbb;

                ca.Body.Accept(this, data);

                if (!ca.Body.GetFlag(StmtFlags.Returns))
                    LLVM.BuildBr(data.Builder, end);

                LLVM.AddCase(sw, cv, cbb);
            }

            LLVM.PositionBuilderAtEnd(data.Builder, end);
            data.BasicBlock = end;

            if (match.GetFlag(StmtFlags.Returns))
                LLVM.BuildUnreachable(data.Builder);

            return default;
        }

        public override LLVMValueRef VisitStructDeclaration(AstStructDecl type, LLVMCodeGeneratorData data = null)
        {
            //var ct = type.Type;
            //var llvmType = CheezTypeToLLVMType(ct);



            return default;
        }

        public override LLVMValueRef VisitVariableDeclaration(AstVariableDecl variable, LLVMCodeGeneratorData data = null)
        {
            if (variable.IsConstant)
                return default;

            if (variable.GetFlag(StmtFlags.GlobalScope))
            {
                var type = CheezTypeToLLVMType(variable.Type);
                var val = LLVM.AddGlobal(module, type, variable.Name.Name);
                LLVM.SetLinkage(val, LLVMLinkage.LLVMInternalLinkage);

                var dExtern = variable.GetDirective("extern");
                if (dExtern != null)
                {
                    LLVM.SetLinkage(val, LLVMLinkage.LLVMExternalLinkage);
                }

                LLVM.SetInitializer(val, GetDefaultLLVMValue(variable.Type));
                valueMap[variable] = val;
                return val;
            }
            else
            {
                var ptr = CreateLocalVariable(variable);
                if (variable.Initializer != null)
                {

                    var val = variable.Initializer.Accept(this, data.Clone(Deref: true));
                    CastIfAny(data.Builder, variable.Type, variable.Initializer.Type, ref val);
                    return LLVM.BuildStore(data.Builder, val, ptr);
                }
                return default;
            }
        }

        private bool xor(bool a, bool b)
        {
            return (a && !b) || (!a && b);
        }

        public override LLVMValueRef VisitAssignment(AstAssignment ass, LLVMCodeGeneratorData data = null)
        {
            var leftPtr = ass.Target.Accept(this, data.Clone(Deref: false));
            var right = ass.Value.Accept(this, data.Clone(Deref: true));

            if (ass.Operator != null)
            {
                var left = LLVM.BuildLoad(data.Builder, leftPtr, "");
                right = GenerateBinaryOperator(data.Builder, ass.Operator, ass.Target.Type, ass.Value.Type, left, right);
            }

            var targetType = LLVM.PointerType(CheezTypeToLLVMType(ass.Target.Type), 0);
            leftPtr = LLVM.BuildPointerCast(data.Builder, leftPtr, targetType, "");

            CastIfAny(data.Builder, ass.Target.Type, ass.Value.Type, ref right);

            return LLVM.BuildStore(data.Builder, right, leftPtr);
        }

        //public override LLVMValueRef VisitImplBlock(AstImplBlock impl, LLVMCodeGeneratorData data = null)
        //{
        //    foreach (var f in impl.Functions)
        //    {
        //        f.Accept(this, data);
        //    }

        //    return default;
        //}

        public override LLVMValueRef VisitFunctionDeclaration(AstFunctionDecl function, LLVMCodeGeneratorData data = null)
        {
            var lfunc = valueMap[function];
            currentFunction = lfunc;

            if (function.Body != null)
            {
                LLVMBuilderRef builder = LLVM.CreateBuilder();
                var entry = LLVM.AppendBasicBlock(lfunc, "entry");
                LLVM.PositionBuilderAtEnd(builder, entry);

                // allocate space for local variables on stack
                for (int i = 0; i < function.Parameters.Count; i++)
                {
                    var param = function.Parameters[i];
                    var p = lfunc.GetParam((uint)i);
                    p = AllocVar(builder, param.Type, $"p_{param.Name}");
                    valueMap[param] = p;
                }

                // create space for local variables
                //{
                //    foreach (var l in function.LocalVariables)
                //    {
                //        valueMap[l] = AllocVar(builder, l.Type, l.Name?.Name ?? "");
                //    }
                //}


                // store params in local variables
                for (int i = 0; i < function.Parameters.Count; i++)
                {
                    var param = function.Parameters[i];
                    var p = lfunc.GetParam((uint)i);
                    LLVM.BuildStore(builder, p, valueMap[param]);
                }

                var bodyBB = LLVM.AppendBasicBlock(lfunc, "body");
                LLVM.BuildBr(builder, bodyBB);
                LLVM.PositionBuilderAtEnd(builder, bodyBB);

                // body
                {
                    var d = new LLVMCodeGeneratorData { Builder = builder, BasicBlock = bodyBB, LFunction = lfunc, Function = function };
                    function.Body.Accept(this, d);
                }

                if (function.ReturnType == VoidType.Intance)
                    LLVM.BuildRetVoid(builder);
                LLVM.DisposeBuilder(builder);
            }

            //LLVM.ViewFunctionCFG(lfunc);
            var bb = LLVM.GetFirstBasicBlock(lfunc);
            while (bb.Pointer.ToInt64() != 0)
            {
                var inst = bb.GetFirstInstruction();

                if (bb.GetBasicBlockTerminator().Pointer == IntPtr.Zero)
                {
                    var next = bb.GetNextBasicBlock();
                    bb.RemoveBasicBlockFromParent();
                    bb = next;
                    continue;
                }

                bb = bb.GetNextBasicBlock();
            }

            bool v = LLVM.VerifyFunction(lfunc, LLVMVerifierFailureAction.LLVMPrintMessageAction);
            if (v)
            {
                Console.Error.WriteLine($"in function {lfunc}");
            }

            return lfunc;
        }

        public override LLVMValueRef VisitReturnStatement(AstReturnStmt ret, LLVMCodeGeneratorData data = null)
        {
            var prevBB = data.BasicBlock;
            var nextBB = prevBB;

            nextBB = LLVM.AppendBasicBlock(data.LFunction, "ret");

            var retVal = ret.ReturnValue?.Accept(this, data);

            foreach (var d in ret.DeferredStatements)
            {
                d.Accept(this, data);
            }

            LLVMValueRef? retInts = null;
            if (ret.ReturnValue != null)
            {
                retInts = LLVM.BuildRet(data.Builder, retVal.Value);
            }
            else
            {
                retInts = LLVM.BuildRetVoid(data.Builder);
            }

            LLVM.PositionBuilderAtEnd(data.Builder, nextBB);
            data.BasicBlock = nextBB;

            return retInts.Value;
        }

        public override LLVMValueRef VisitExpressionStatement(AstExprStmt stmt, LLVMCodeGeneratorData data = null)
        {
            return stmt.Expr.Accept(this, data);
        }

        public override LLVMValueRef VisitWhileStatement(AstWhileStmt ws, LLVMCodeGeneratorData data = null)
        {
            var bbCondition = LLVM.AppendBasicBlock(data.LFunction, "while_condition");
            var bbBody = LLVM.AppendBasicBlock(data.LFunction, "while_body");
            var bbEnd = LLVM.AppendBasicBlock(data.LFunction, "while_end");

            // pre statement
            if (ws.PreAction != null)
            {
                var temp = CreateLocalVariable(ws.PreAction);
                ws.PreAction.Accept(this, data);
            }

            // Condition
            LLVM.BuildBr(data.Builder, bbCondition);
            data.MoveBuilderTo(bbCondition);
            var cond = ws.Condition.Accept(this, data);
            var bbConditionEnd = data.BasicBlock;

            // body
            data.MoveBuilderTo(bbBody);
            ws.Body.Accept(this, data.Clone(LoopCondition: bbCondition, LoopEnd: bbEnd));

            // post action
            if (ws.PostAction != null)
            {
                ws.PostAction.Accept(this, data);
            }

            // go back to condition
            LLVM.BuildBr(data.Builder, bbCondition);

            //
            data.MoveBuilderTo(bbEnd);

            // connect condition with body and end
            data.MoveBuilderTo(bbConditionEnd);
            LLVM.BuildCondBr(data.Builder, cond, bbBody, bbEnd);

            //
            data.MoveBuilderTo(bbEnd);

            return default;
        }

        public override LLVMValueRef VisitBreakStatement(AstBreakStmt br, LLVMCodeGeneratorData data = null)
        {
            var prevBB = data.BasicBlock;
            var nextBB = prevBB;

            nextBB = LLVM.AppendBasicBlock(data.LFunction, "break");

            foreach (var d in br.DeferredStatements)
            {
                d.Accept(this, data);
            }

            LLVM.BuildBr(data.Builder, data.LoopEnd);

            data.MoveBuilderTo(nextBB);

            return default;
        }

        public override LLVMValueRef VisitContinueStatement(AstContinueStmt cont, LLVMCodeGeneratorData data = null)
        {
            var prevBB = data.BasicBlock;
            var nextBB = prevBB;

            nextBB = LLVM.AppendBasicBlock(data.LFunction, "continue");

            foreach (var d in cont.DeferredStatements)
            {
                d.Accept(this, data);
            }

            LLVM.BuildBr(data.Builder, data.LoopCondition);

            data.MoveBuilderTo(nextBB);

            return default;
        }

        public override LLVMValueRef VisitIfStatement(AstIfStmt ifs, LLVMCodeGeneratorData data = null)
        {
            LLVMBasicBlockRef? bbEnd = null;

            var bbIfCond = LLVM.AppendBasicBlock(data.LFunction, "if_cond");
            var bbIfBody = LLVM.AppendBasicBlock(data.LFunction, "if_body");

            if (!ifs.GetFlag(StmtFlags.Returns))
                bbEnd = LLVM.AppendBasicBlock(data.LFunction, "if_end");

            LLVMBasicBlockRef? bbElseBody = null;
            if (ifs.ElseCase != null)
                bbElseBody = LLVM.AppendBasicBlock(data.LFunction, "else_body");

            // pre action
            if (ifs.PreAction != null)
            {
                var temp = CreateLocalVariable(ifs.PreAction);
                ifs.PreAction.Accept(this, data);
            }
            LLVM.BuildBr(data.Builder, bbIfCond);

            // Condition
            LLVM.PositionBuilderAtEnd(data.Builder, bbIfCond);
            data.BasicBlock = bbIfCond;
            var cond = ifs.Condition.Accept(this, data);
            if (ifs.ElseCase != null)
                LLVM.BuildCondBr(data.Builder, cond, bbIfBody, bbElseBody.Value);
            else if (bbEnd != null)
                LLVM.BuildCondBr(data.Builder, cond, bbIfBody, bbEnd.Value);
            else
                LLVM.BuildBr(data.Builder, bbIfBody);


            // if body
            LLVM.PositionBuilderAtEnd(data.Builder, bbIfBody);
            data.BasicBlock = bbIfBody;
            ifs.IfCase.Accept(this, data);
            //if (!ifs.IfCase.GetFlag(StmtFlags.Returns))
            if (bbEnd != null)
                LLVM.BuildBr(data.Builder, bbEnd.Value);

            // else body
            if (ifs.ElseCase != null)
            {
                LLVM.PositionBuilderAtEnd(data.Builder, bbElseBody.Value);
                data.BasicBlock = bbElseBody.Value;
                ifs.ElseCase.Accept(this, data);

                if (bbEnd != null)
                    LLVM.BuildBr(data.Builder, bbEnd.Value);
            }

            //
            if (bbEnd != null)
            {
                LLVM.PositionBuilderAtEnd(data.Builder, bbEnd.Value);
                data.BasicBlock = bbEnd.Value;
            }

            return default;
        }

        public override LLVMValueRef VisitBlockStatement(AstBlockStmt block, LLVMCodeGeneratorData data = null)
        {
            foreach (var s in block.Statements)
            {
                s.Accept(this, data);
            }

            for (int i = block.DeferredStatements.Count - 1; i >= 0; i--)
            {
                block.DeferredStatements[i].Accept(this, data);
            }

            return default;
        }

        private LLVMValueRef GetStructMemberPointer(LLVMBuilderRef builder, LLVMValueRef pointer, uint member)
        {
            var type = pointer.TypeOf().GetElementType();

            if (type.TypeKind == LLVMTypeKind.LLVMArrayTypeKind)
            {
                return LLVM.BuildGEP(builder, pointer, new LLVMValueRef[]
                {
                    LLVM.ConstInt(LLVM.Int32Type(), 0, false),
                    LLVM.ConstInt(LLVM.Int32Type(), member, false)
                }, "");
            }

            return LLVM.BuildStructGEP(builder, pointer, member, "");

            var structType = pointer.TypeOf().GetElementType();
            var elementType = structType.GetStructElementTypes()[member];
            var offset = LLVM.OffsetOfElement(moduleDataLayout, structType, member);
            var elementPtrType = LLVM.PointerType(elementType, 0);

            var ptrAsInt = LLVM.BuildPtrToInt(builder, pointer, LLVM.Int64Type(), "");
            var offsetAsInt = LLVM.ConstInt(LLVM.Int64Type(), offset, false);

            var memberPtrAsInt = LLVM.BuildAdd(builder, ptrAsInt, offsetAsInt, "");

            var memberPtr = LLVM.BuildIntToPtr(builder, memberPtrAsInt, elementPtrType, "");
            return memberPtr;
        }

        private LLVMValueRef GetArrayAsPointer(LLVMBuilderRef builder, LLVMValueRef array)
        {
            var arrayType = array.TypeOf().GetElementType();
            var elementType = arrayType.GetElementType();
            var elementPtrType = LLVM.PointerType(elementType, 0);

            var ptr = LLVM.BuildPointerCast(builder, array, elementPtrType, "");
            return ptr;
        }

        #endregion

        #region Expressions

        public override LLVMValueRef VisitNullExpression(AstNullExpr nul, LLVMCodeGeneratorData data = null)
        {
            return LLVM.ConstNull(CheezTypeToLLVMType(nul.Type));
        }

        public override LLVMValueRef VisitStructValueExpression(AstStructValueExpr str, LLVMCodeGeneratorData data = null)
        {
            var value = CreateLocalVariable(str.Type);

            var llvmType = CheezTypeToLLVMType(str.Type);

            foreach (var m in str.MemberInitializers)
            {
                var v = m.Value.Accept(this, data.Clone(Deref: true));
                //var memberPtr = LLVM.BuildStructGEP(data.Builder, value, (uint)m.Index, "");
                var memberPtr = GetStructMemberPointer(data.Builder, value, (uint)m.Index);
                var castedMemberPtr = LLVM.BuildPointerCast(data.Builder, memberPtr, LLVM.PointerType(CheezTypeToLLVMType(m.Value.Type), 0), "");
                var s = LLVM.BuildStore(data.Builder, v, castedMemberPtr);
            }

            value = LLVM.BuildLoad(data.Builder, value, "");

            return value;
        }

        public override LLVMValueRef VisitDotExpression(AstDotExpr dot, LLVMCodeGeneratorData data = null)
        {
            if (dot.Left.Type is TraitType trait)
            {
                var ptr = dot.Left.Accept(this, data.Clone(Deref: false));
                //var vtablePtr = LLVM.BuildStructGEP(data.Builder, ptr, 0, "");
                var vtablePtr = GetStructMemberPointer(data.Builder, ptr, 0);
                vtablePtr = LLVM.BuildLoad(data.Builder, vtablePtr, "");
                vtablePtr = LLVM.BuildPointerCast(data.Builder, vtablePtr, LLVM.PointerType(vtableType, 0), "");

                var index = vtableIndices[dot.Value];
                //var func = LLVM.BuildStructGEP(data.Builder, vtablePtr, (uint)index, "");
                var func = GetStructMemberPointer(data.Builder, vtablePtr, (uint)index);
                func = LLVM.BuildLoad(data.Builder, func, "");
                return func;
            }

            if (dot.IsDoubleColon)
            {
                return valueMap[dot.Value];
            }
            else
            {
                if (dot.Left.Type is StructType s)
                {
                    var left = dot.Left.Accept(this, data.Clone(Deref: false));
                    var index = (uint)s.GetIndexOfMember(dot.Right);
                    var member = s.Declaration.Members[(int)index];

                    if (left.TypeOf().TypeKind == LLVMTypeKind.LLVMStructTypeKind)
                    {
                        var elemRaw = LLVM.BuildExtractValue(data.Builder, left, index, "");
                        //var elemType = LLVM.PointerType(CheezTypeToLLVMType(member.Type), 0);
                        //var elem = LLVM.BuildPointerCast(data.Builder, elemRaw, elemType, "");
                        return elemRaw;
                    }
                    else
                    {
                        var elemPtrRaw = GetStructMemberPointer(data.Builder, left, index);
                        var elemType = LLVM.PointerType(CheezTypeToLLVMType(member.Type), 0);
                        var elemPtr = LLVM.BuildPointerCast(data.Builder, elemPtrRaw, elemType, "");

                        if (data.Deref)
                        {
                            elemPtr = LLVM.BuildLoad(data.Builder, elemPtr, $"*{dot}");
                        }
                        return elemPtr;
                    }
                }
                else if (dot.Left.Type == CheezType.Type && dot.Left.Value is EnumType e)
                {
                    return CheezValueToLLVMValue(dot.Value);
                }
                else if (dot.Left.Type is SliceType slice)
                {
                    if (dot.Right == "length")
                    {
                        var left = dot.Left.Accept(this, data.Clone(Deref: false));
                        //var length = LLVM.BuildStructGEP(data.Builder, left, 1, "");
                        var length = GetStructMemberPointer(data.Builder, left, 1);
                        if (data.Deref)
                            length = LLVM.BuildLoad(data.Builder, length, "");
                        return length;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else if (dot.Left.Type is ArrayType arr)
                {
                    if (dot.Right == "length")
                    {
                        var length = LLVM.ConstInt(LLVM.Int32Type(), (ulong)arr.Length, false);
                        return length;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else if (dot.Left.Type is ReferenceType r && r.TargetType is StructType @struct)
                {
                    var left = dot.Left.Accept(this, data.Clone(Deref: false));
                    var index = (uint)@struct.GetIndexOfMember(dot.Right);
                    var member = @struct.Declaration.Members[(int)index];

                    left = LLVM.BuildLoad(data.Builder, left, "");
                    //var elemPtr = LLVM.BuildStructGEP(data.Builder, left, index, dot.ToString());
                    var elemPtrRaw = GetStructMemberPointer(data.Builder, left, index);
                    var elemType = LLVM.PointerType(CheezTypeToLLVMType(member.Type), 0);
                    var elemPtr = LLVM.BuildPointerCast(data.Builder, elemPtrRaw, elemType, "");

                    if (data.Deref)
                    {
                        elemPtr = LLVM.BuildLoad(data.Builder, elemPtr, $"*{dot}");
                    }
                    return elemPtr;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        public override LLVMValueRef VisitAddressOfExpression(AstAddressOfExpr add, LLVMCodeGeneratorData data = null)
        {
            LLVMValueRef sub;
            if (add.SubExpression.Type is ReferenceType)
                sub = add.SubExpression.Accept(this, data.Clone(Deref: true));
            else
                sub = add.SubExpression.Accept(this, data.Clone(Deref: false));
            return sub;
        }

        public override LLVMValueRef VisitCastExpression(AstCastExpr cast, LLVMCodeGeneratorData data = null)
        {

            if (cast.Type == cast.SubExpression.Type)
            {
                var sub = cast.SubExpression.Accept(this, data);
                return sub;
            }

            if (cast.Type is PointerType)
            {
                var type = CheezTypeToLLVMType(cast.Type);
                if (cast.SubExpression.Type is PointerType)
                    return LLVM.BuildPointerCast(data.Builder, cast.SubExpression.Accept(this, data.Clone(Deref: true)), type, "");
                else if (cast.SubExpression.Type is ArrayType)
                    return LLVM.BuildPointerCast(data.Builder, cast.SubExpression.Accept(this, data.Clone(Deref: true)), type, "");
                else if (cast.SubExpression.Type is IntType i)
                    return LLVM.BuildIntToPtr(data.Builder, cast.SubExpression.Accept(this, data.Clone(Deref: true)), type, "");
                else if (cast.SubExpression.Type is CStringType s)
                    return LLVM.BuildPointerCast(data.Builder, cast.SubExpression.Accept(this, data.Clone(Deref: true)), type, "");
                else if (cast.SubExpression.Type is AnyType)
                    return LLVM.BuildIntToPtr(data.Builder, cast.SubExpression.Accept(this, data.Clone(Deref: true)), type, "");
                else if (cast.SubExpression.Type is SliceType)
                {
                    var ptr = LLVM.BuildExtractValue(data.Builder, cast.SubExpression.Accept(this, data.Clone(Deref: true)), 0, "");
                    return LLVM.BuildPointerCast(data.Builder, ptr, type, "");
                }
                else if (cast.SubExpression.Type is TraitType trait)
                {
                    var sub = cast.SubExpression.Accept(this, data.Clone(Deref: false));
                    //var ptr = LLVM.BuildStructGEP(data.Builder, sub, 1, "");
                    var ptr = GetStructMemberPointer(data.Builder, sub, 1);
                    ptr = LLVM.BuildLoad(data.Builder, ptr, "");
                    ptr = LLVM.BuildPointerCast(data.Builder, ptr, type, "");
                    return ptr;
                }
            }
            else if (cast.Type is CStringType s)
            {
                var sub = cast.SubExpression.Accept(this, data);
                var type = CheezTypeToLLVMType(s.ToPointerType());

                if (cast.SubExpression.Type is PointerType)
                    return LLVM.BuildPointerCast(data.Builder, sub, type, "");
                else if (cast.SubExpression.Type is IntType)
                    return LLVM.BuildIntToPtr(data.Builder, sub, type, "");
                else if (cast.SubExpression.Type is AnyType)
                    return LLVM.BuildIntToPtr(data.Builder, sub, type, "");
            }
            // @Todo
            //else if (cast.Type is ArrayType a)
            //{
            //    var type = CheezTypeToLLVMType(a.ToPointerType());
            //    if (cast.SubExpression.Type is AnyType)
            //        return LLVM.BuildIntToPtr(data.Builder, sub, type, "");
            //    else if (cast.SubExpression.Type is PointerType)
            //        return LLVM.BuildPointerCast(data.Builder, sub, type, "");
            //    else if (cast.SubExpression.Type is IntType)
            //        return LLVM.BuildIntToPtr(data.Builder, sub, type, "");
            //    return LLVM.BuildPointerCast(data.Builder, sub, type, "");
            //}
            else if (cast.Type is IntType tt)
            {
                var sub = cast.SubExpression.Accept(this, data);
                var type = CheezTypeToLLVMType(cast.Type);
                if (cast.SubExpression.Type is IntType || cast.SubExpression.Type is BoolType)
                    return LLVM.BuildIntCast(data.Builder, sub, type, "");
                else if (cast.SubExpression.Type is PointerType)
                    return LLVM.BuildPtrToInt(data.Builder, sub, type, "");
                else if (cast.SubExpression.Type is AnyType)
                    return LLVM.BuildIntCast(data.Builder, sub, type, "");
                else if (cast.SubExpression.Type is EnumType)
                    return LLVM.BuildIntCast(data.Builder, sub, type, "");
                else if (cast.SubExpression.Type is CharType)
                    return LLVM.BuildIntCast(data.Builder, sub, type, "");
                else if (cast.SubExpression.Type is FloatType)
                {
                    if (tt.Signed)
                        return LLVM.BuildCast(data.Builder, LLVMOpcode.LLVMFPToSI, sub, type, "");
                    else
                        return LLVM.BuildCast(data.Builder, LLVMOpcode.LLVMFPToUI, sub, type, "");
                }
            }
            else if (cast.Type is FloatType f)
            {
                var sub = cast.SubExpression.Accept(this, data);
                var type = CheezTypeToLLVMType(cast.Type);
                if (cast.SubExpression.Type is FloatType)
                    return LLVM.BuildFPCast(data.Builder, sub, type, "");
                else if (cast.SubExpression.Type is IntType ttt)
                {
                    if (ttt.Signed)
                        return LLVM.BuildCast(data.Builder, LLVMOpcode.LLVMSIToFP, sub, type, "");
                    else
                        return LLVM.BuildCast(data.Builder, LLVMOpcode.LLVMUIToFP, sub, type, "");
                }
                else if (cast.SubExpression.Type is AnyType)
                {
                    if (f.Size == 8)
                        return LLVM.BuildBitCast(data.Builder, sub, type, "");
                    else
                    {
                        var t = LLVM.BuildIntCast(data.Builder, sub, LLVM.Int32Type(), "");
                        return LLVM.BuildBitCast(data.Builder, t, type, "");
                    }
                }
            }
            else if (cast.Type is CharType c)
            {
                var sub = cast.SubExpression.Accept(this, data);
                var type = CheezTypeToLLVMType(cast.Type);
                if (cast.SubExpression.Type is IntType)
                    return LLVM.BuildIntCast(data.Builder, sub, type, "");
                else if (cast.SubExpression.Type is AnyType)
                    return LLVM.BuildIntCast(data.Builder, sub, type, "");
            }
            else if (cast.Type is BoolType)
            {
                var sub = cast.SubExpression.Accept(this, data);
                var type = CheezTypeToLLVMType(cast.Type);
                if (cast.SubExpression.Type is IntType i)
                    return LLVM.BuildIntCast(data.Builder, sub, type, "");
                else if (cast.SubExpression.Type is AnyType)
                    return LLVM.BuildIntCast(data.Builder, sub, type, "");
            }
            else if (cast.Type is SliceType slice)
            {
                var sub = cast.SubExpression.Accept(this, data.Clone(Deref: false)); // TODO: verify Deref: true

                var type = CheezTypeToLLVMType(cast.Type);

                if (cast.SubExpression.Type is ArrayType arr)
                {
                    var temp = CreateLocalVariable(cast.Type);

                    //var dataPtr = LLVM.BuildStructGEP(data.Builder, temp, 0, "");
                    //var lenPtr = LLVM.BuildStructGEP(data.Builder, temp, 1, "");
                    var dataPtr = GetStructMemberPointer(data.Builder, temp, 0);
                    var lenPtr = GetStructMemberPointer(data.Builder, temp, 1);

                    //sub = LLVM.BuildGEP(data.Builder, sub, new LLVMValueRef[]
                    //{
                    //    LLVM.ConstInt(LLVM.Int32Type(), 0, false),
                    //    LLVM.ConstInt(LLVM.Int32Type(), 0, false)
                    //}, "");
                    sub = GetStructMemberPointer(data.Builder, sub, 0);
                    var d = LLVM.BuildStore(data.Builder, sub, dataPtr);
                    var len = LLVM.BuildStore(data.Builder, LLVM.ConstInt(LLVM.Int32Type(), (ulong)arr.Length, false), lenPtr);

                    var result = LLVM.BuildLoad(data.Builder, temp, "");
                    return result;
                }
                else if (cast.SubExpression.Type is PointerType ptr)
                {
                    if (cast.SubExpression.GetFlag(ExprFlags.IsLValue))
                        sub = LLVM.BuildLoad(data.Builder, sub, ""); // TODO: sometimes necessery?

                    var temp = CreateLocalVariable(cast.Type);
                    
                    var dataPtr = GetStructMemberPointer(data.Builder, temp, 0);
                    var lenPtr = GetStructMemberPointer(data.Builder, temp, 1);

                    var d = LLVM.BuildStore(data.Builder, sub, dataPtr);
                    var len = LLVM.BuildStore(data.Builder, LLVM.ConstInt(LLVM.Int32Type(), 0, false), lenPtr);

                    var result = LLVM.BuildLoad(data.Builder, temp, "");
                    return result;
                }
                else if (cast.SubExpression.Type == CheezType.CString)
                {
                    if (!cast.SubExpression.IsCompTimeValue || cast.SubExpression.Value == null)
                    {
                        throw new NotImplementedException("Can only cast constant strings to char slice");
                    }

                    int strLen = ((string)cast.SubExpression.Value).Length;

                    var temp = CreateLocalVariable(cast.Type);

                    var dataPtr = GetStructMemberPointer(data.Builder, temp, 0);
                    var lenPtr = GetStructMemberPointer(data.Builder, temp, 1);

                    var d = LLVM.BuildStore(data.Builder, sub, dataPtr);
                    var len = LLVM.BuildStore(data.Builder, LLVM.ConstInt(LLVM.Int32Type(), (ulong)strLen, false), lenPtr);

                    var result = LLVM.BuildLoad(data.Builder, temp, "");
                    return result;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else if (cast.Type is EnumType en)
            {
                var sub = cast.SubExpression.Accept(this, data);
                var type = CheezTypeToLLVMType(cast.Type);

                if (cast.SubExpression.Type is IntType || cast.SubExpression.Type is BoolType)
                    return LLVM.BuildIntCast(data.Builder, sub, type, "");
            }
            else if (cast.Type == CheezType.Any)
            {
                var sub = cast.SubExpression.Accept(this, data.Clone(Deref: true));
                CastIfAny(data.Builder, cast.Type, cast.SubExpression.Type, ref sub);
                return sub;
            }
            else if (cast.Type is TraitType trait)
            {
                var sub = cast.SubExpression.Accept(this, data.Clone(Deref: false));


                if (!cast.SubExpression.GetFlag(ExprFlags.IsLValue))
                {
                    var t = CreateLocalVariable(cast.SubExpression.Type);
                    var _ = LLVM.BuildStore(data.Builder, sub, t);
                    sub = t;
                }

                var temp = CreateLocalVariable(cast.Type);

                var type = cast.SubExpression.Type;

                if (cast.SubExpression.Type is PointerType ptr)
                {
                    sub = LLVM.BuildLoad(data.Builder, sub, "");

                    //var vtablePtr = LLVM.BuildStructGEP(data.Builder, temp, 0, "");
                    var vtablePtr = GetStructMemberPointer(data.Builder, temp, 0);
                    var vtable = vtableMap[ptr.TargetType];
                    vtable = LLVM.BuildPointerCast(data.Builder, vtable, LLVM.PointerType(LLVM.Int8Type(), 0), "");
                    LLVM.BuildStore(data.Builder, vtable, vtablePtr);

                    //var valuePtr = LLVM.BuildStructGEP(data.Builder, temp, 1, "");
                    var valuePtr = GetStructMemberPointer(data.Builder, temp, 1);
                    sub = LLVM.BuildPointerCast(data.Builder, sub, LLVM.PointerType(LLVM.Int8Type(), 0), "");
                    var _ = LLVM.BuildStore(data.Builder, sub, valuePtr);

                    temp = LLVM.BuildLoad(data.Builder, temp, "");
                    return temp;
                }
                else
                {
                    var vtablePtr = GetStructMemberPointer(data.Builder, temp, 0);
                    var vtable = vtableMap[type];
                    vtable = LLVM.BuildPointerCast(data.Builder, vtable, LLVM.PointerType(LLVM.Int8Type(), 0), "");
                    LLVM.BuildStore(data.Builder, vtable, vtablePtr);

                    //
                    var valuePtr = GetStructMemberPointer(data.Builder, temp, 1);
                    sub = LLVM.BuildPointerCast(data.Builder, sub, LLVM.PointerType(LLVM.Int8Type(), 0), "");
                    var _ = LLVM.BuildStore(data.Builder, sub, valuePtr);

                    temp = LLVM.BuildLoad(data.Builder, temp, "");
                    return temp;
                }
            }

            throw new NotImplementedException($"Cast from {cast.SubExpression.Type} to {cast.Type} is not implemented yet");
        }

        public override LLVMValueRef VisitIdentifierExpression(AstIdentifierExpr ident, LLVMCodeGeneratorData data = null)
        {
            var s = ident.Symbol;
            var v = valueMap[s];

            var func = v.IsAFunction();

            if (data.Deref && func.Pointer == IntPtr.Zero)
                return LLVM.BuildLoad(data.Builder, v, "");

            return v;
        }

        public override LLVMValueRef VisitCallExpression(AstCallExpr call, LLVMCodeGeneratorData data = null)
        {
            LLVMValueRef func;

            //func = call.Function.Accept(this, data.Clone(Deref: false));

            if (call.Function is AstIdentifierExpr id)
            {
                func = LLVM.GetNamedFunction(module, id.Name);
                if (func.Pointer == IntPtr.Zero)
                {
                    func = call.Function.Accept(this, data);
                }
            }
            else if (call.Function is AstFunctionExpression afe)
            {
                func = valueMap[afe.Declaration];
                //func = LLVM.GetNamedFunction(module, afe.Declaration.Name.Name);
            }
            else if (call.Function is AstStructExpression str)
            {
                return default;
            }
            else
            {
                func = call.Function.Accept(this, data);
            }

            //var funcDecl = call.Function.Value as AstFunctionDecl;
            var funcType = call.Function.Type as FunctionType;

            var args = new LLVMValueRef[call.Arguments.Count];
            for (int i = 0; i < funcType.ParameterTypes.Length; i++)
            {
                var param = funcType.ParameterTypes[i];
                var arg = call.Arguments[i];

                bool deref = true;
                if (param is ReferenceType && !(arg.Type is ReferenceType))
                    deref = false;

                var argVal = arg.Accept(this, data.Clone(Deref: deref));
                //if (CanPassByValue(param))
                //{
                args[i] = argVal;
                //}
                //else
                //{
                //    var temp = GetTempValue(param);
                //    var argVal2 = LLVM.BuildStore(data.Builder, argVal, temp);
                //    args[i] = temp;
                //}

                CastIfAny(data.Builder, param, arg.Type, ref args[i]);

                if (param == CheezType.Any && arg.Type != CheezType.Any)
                {
                    var type = CheezTypeToLLVMType(param);
                    if (arg.Type is IntType)
                        args[i] = LLVM.BuildIntCast(data.Builder, args[i], type, "");
                    else if (arg.Type is BoolType)
                        args[i] = LLVM.BuildZExtOrBitCast(data.Builder, args[i], type, "");
                    else if (arg.Type is PointerType || arg.Type is CStringType || arg.Type is ArrayType)
                        args[i] = LLVM.BuildPtrToInt(data.Builder, args[i], type, "");
                    else
                        throw new NotImplementedException("any cast");
                }
            }
            //var args = call.Arguments.Select(a => a.Accept(this, data)).ToArray();

            var res = LLVM.BuildCall(data.Builder, func, args, "");
            LLVM.SetInstructionCallConv(res, LLVM.GetFunctionCallConv(func));
            return res;
        }

        public override LLVMValueRef VisitStringLiteral(AstStringLiteral str, LLVMCodeGeneratorData data = null)
        {
            if (str.IsChar)
            {
                var ch = (char)str.Value;
                var val = LLVM.ConstInt(LLVM.Int8Type(), (ulong)ch, true);
                return val;
            }
            else
            {
                var lstr = LLVM.BuildGlobalString(data.Builder, str.StringValue, "");
                var val = LLVM.BuildPointerCast(data.Builder, lstr, CheezTypeToLLVMType(CStringType.Instance), "");
                return val;
            }
        }

        public override LLVMValueRef VisitNumberExpression(AstNumberExpr num, LLVMCodeGeneratorData data = null)
        {
            var llvmType = CheezTypeToLLVMType(num.Type);
            if (num.Type is IntType)
            {
                var val = num.Data.ToUlong();
                return LLVM.ConstInt(llvmType, val, false);
            }
            else
            {
                var val = num.Data.ToDouble();
                var result = LLVM.ConstReal(llvmType, val);
                return result;
            }
        }

        public override LLVMValueRef VisitBoolExpression(AstBoolExpr bo, LLVMCodeGeneratorData data = null)
        {
            return LLVM.ConstInt(LLVM.Int1Type(), bo.BoolValue ? 1ul : 0ul, false);
        }

        public override LLVMValueRef VisitArrayExpression(AstArrayExpression arr, LLVMCodeGeneratorData data = null)
        {
            var ptr = CreateLocalVariable(arr.Type);

            var targetType = (arr.Type as ArrayType).TargetType; // @Todo
            var llvmTargetType = CheezTypeToLLVMType(targetType);
            var typeSize = LLVM.ConstInt(LLVM.Int64Type(), moduleDataLayout.SizeOfTypeInBits(llvmTargetType) / 8, false);
            var llvmFalse = LLVM.ConstInt(LLVM.Int1Type(), 0, false);

            uint index = 0;
            foreach (var value in arr.Values)
            {
                var lv = value.Accept(this, data.Clone(Deref: true));

                CastIfAny(data.Builder, targetType, value.Type, ref lv);

                var vp = LLVM.BuildGEP(data.Builder, ptr, new LLVMValueRef[]
                {
                    LLVM.ConstInt(LLVM.Int32Type(), 0, new LLVMBool(0)),
                    LLVM.ConstInt(LLVM.Int32Type(), index, new LLVMBool(0))
                }, "");

                var store = LLVM.BuildStore(data.Builder, lv, vp);

                index++;
            }

            var val = ptr;
            if (data.Deref)
                val = LLVM.BuildLoad(data.Builder, ptr, "");

            return val;
        }

        public override LLVMValueRef VisitArrayAccessExpression(AstArrayAccessExpr arr, LLVMCodeGeneratorData data = null)
        {
            ulong dataSize = 1;
            var targetType = arr.SubExpression.Type;
            bool deref = true;
            switch (targetType)
            {
                case PointerType p:
                    dataSize = (ulong)p.TargetType.Size;
                    break;

                case ArrayType a:
                    dataSize = (ulong)a.TargetType.Size;
                    targetType = a.ToPointerType();
                    deref = false;
                    break;

                case SliceType s:
                    dataSize = (ulong)s.TargetType.Size;
                    targetType = s.ToPointerType();
                    deref = false;
                    break;
            }
            var targetLLVMType = CheezTypeToLLVMType(targetType);

            var sub = arr.SubExpression.Accept(this, data.Clone(Deref: deref)); // @Todo: Deref: !data.Deref / true
            var ind = arr.Indexer.Accept(this, data.Clone(Deref: true));
            
            var castType = LLVM.IntType(pointerSize * 8);

            LLVMValueRef subCasted = sub;

            if (arr.SubExpression.Type is SliceType)
            {
                //subCasted = LLVM.BuildStructGEP(data.Builder, subCasted, 0, "");
                subCasted = GetStructMemberPointer(data.Builder, subCasted, 0);
                subCasted = LLVM.BuildLoad(data.Builder, subCasted, "");
            }

            else if (!deref)
            {
                //subCasted = LLVM.BuildGEP(data.Builder, sub, new LLVMValueRef[]
                //{
                //    LLVM.ConstInt(LLVM.Int32Type(), 0, false)
                //}, "");
                subCasted = GetArrayAsPointer(data.Builder, sub);
                //subCasted = GetStructMemberPointer(data.Builder, sub, 0);
            }
            subCasted = LLVM.BuildPtrToInt(data.Builder, subCasted, castType, "");
            var indCasted = LLVM.BuildIntCast(data.Builder, ind, castType, "");

            if (dataSize != 1)
            {
                indCasted = LLVM.BuildMul(data.Builder, indCasted, LLVM.ConstInt(castType, dataSize, new LLVMBool(0)), "");
            }

            var add = LLVM.BuildAdd(data.Builder, subCasted, indCasted, "");

            var result = LLVM.BuildIntToPtr(data.Builder, add, targetLLVMType, "");

            if (data.Deref)
                return LLVM.BuildLoad(data.Builder, result, "");

            return result;
        }

        public override LLVMValueRef VisitDereferenceExpression(AstDereferenceExpr deref, LLVMCodeGeneratorData data = null)
        {
            var ptr = deref.SubExpression.Accept(this, data.Clone(Deref: true));

            var v = ptr;
            if (data.Deref)
                v = LLVM.BuildLoad(data.Builder, ptr, deref.ToString());
            return v;
        }

        public override LLVMValueRef VisitUnaryExpression(AstUnaryExpr bin, LLVMCodeGeneratorData data = null)
        {
            var sub = bin.SubExpr.Accept(this, data);

            switch (bin.Operator)
            {
                case "-":
                    if (bin.Type is IntType)
                        return LLVM.BuildNeg(data.Builder, sub, "");
                    else if (bin.Type is FloatType)
                        return LLVM.BuildFNeg(data.Builder, sub, "");
                    else
                        throw new NotImplementedException();

                case "!":
                    return LLVM.BuildNot(data.Builder, sub, "");

                default:
                    throw new NotImplementedException();
            }
        }

        public override LLVMValueRef VisitBinaryExpression(AstBinaryExpr bin, LLVMCodeGeneratorData data = null)
        {
            switch (bin.Operator)
            {
                case "and":
                    {
                        var bbAnd = LLVM.AppendBasicBlock(data.LFunction, "and");
                        var bbRhs = LLVM.AppendBasicBlock(data.LFunction, "and_rhs");
                        var bbEnd = LLVM.AppendBasicBlock(data.LFunction, "and_end");

                        LLVM.BuildBr(data.Builder, bbAnd);
                        data.MoveBuilderTo(bbAnd);

                        //
                        var tempVar = CreateLocalVariable(bin.Type);

                        //
                        var left = bin.Left.Accept(this, data.Clone(Deref: true));
                        LLVM.BuildStore(data.Builder, left, tempVar);
                        LLVM.BuildCondBr(data.Builder, left, bbRhs, bbEnd);

                        // rhs
                        data.MoveBuilderTo(bbRhs);
                        var right = bin.Right.Accept(this, data);
                        LLVM.BuildStore(data.Builder, right, tempVar);
                        LLVM.BuildBr(data.Builder, bbEnd);

                        //
                        data.MoveBuilderTo(bbEnd);
                        tempVar = LLVM.BuildLoad(data.Builder, tempVar, "");
                        return tempVar;
                    }

                case "or":
                    {
                        var bbOr = LLVM.AppendBasicBlock(data.LFunction, "or");
                        var bbRhs = LLVM.AppendBasicBlock(data.LFunction, "or_rhs");
                        var bbEnd = LLVM.AppendBasicBlock(data.LFunction, "or_end");

                        LLVM.BuildBr(data.Builder, bbOr);
                        data.MoveBuilderTo(bbOr);

                        //
                        var tempVar = CreateLocalVariable(bin.Type);

                        //
                        var left = bin.Left.Accept(this, data.Clone(Deref: true));
                        LLVM.BuildStore(data.Builder, left, tempVar);
                        LLVM.BuildCondBr(data.Builder, left, bbEnd, bbRhs);

                        // rhs
                        data.MoveBuilderTo(bbRhs);
                        var right = bin.Right.Accept(this, data);
                        LLVM.BuildStore(data.Builder, right, tempVar);
                        LLVM.BuildBr(data.Builder, bbEnd);

                        //
                        data.MoveBuilderTo(bbEnd);
                        tempVar = LLVM.BuildLoad(data.Builder, tempVar, "");
                        return tempVar;
                    }
            }
            {
                var left = bin.Left.Accept(this, data.Clone(Deref: true));
                var right = bin.Right.Accept(this, data.Clone(Deref: true));

                var result = GenerateBinaryOperator(data.Builder, bin.Operator, bin.Type, bin.Left.Type, left, right);
                return result;
            }
        }

        private LLVMValueRef GenerateBinaryOperator(LLVMBuilderRef builder, string op, CheezType targetType, CheezType argType, LLVMValueRef left, LLVMValueRef right)
        {
            if (targetType is IntType i)
            {
                switch (op)
                {
                    case "+":
                        return LLVM.BuildAdd(builder, left, right, "");

                    case "-":
                        return LLVM.BuildSub(builder, left, right, "");

                    case "*":
                        return LLVM.BuildMul(builder, left, right, "");

                    case "/":
                        if (i.Signed)
                            return LLVM.BuildSDiv(builder, left, right, "");
                        else
                            return LLVM.BuildUDiv(builder, left, right, "");

                    case "%":
                        if (i.Signed)
                            return LLVM.BuildSRem(builder, left, right, "");
                        else
                            return LLVM.BuildURem(builder, left, right, "");


                    default:
                        throw new NotImplementedException();
                }
            }
            else if (targetType is FloatType)
            {
                switch (op)
                {
                    case "+":
                        return LLVM.BuildFAdd(builder, left, right, "");

                    case "-":
                        return LLVM.BuildFSub(builder, left, right, "");

                    case "*":
                        return LLVM.BuildFMul(builder, left, right, "");

                    case "/":
                        return LLVM.BuildFDiv(builder, left, right, "");

                    case "%":
                        return LLVM.BuildFRem(builder, left, right, "");

                    default:
                        throw new NotImplementedException();
                }
            }
            else if (targetType is BoolType b)
            {
                if (argType is IntType ii)
                {
                    switch (op)
                    {
                        case "!=":
                            return LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntNE, left, right, "");

                        case "==":
                            return LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntEQ, left, right, "");

                        case "<":
                            if (ii.Signed)
                                return LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntSLT, left, right, "");
                            else
                                return LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntULT, left, right, "");

                        case ">":
                            if (ii.Signed)
                                return LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntSGT, left, right, "");
                            else
                                return LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntUGT, left, right, "");

                        case "<=":
                            if (ii.Signed)
                                return LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntSLE, left, right, "");
                            else
                                return LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntULE, left, right, "");

                        case ">=":
                            if (ii.Signed)
                                return LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntSGE, left, right, "");
                            else
                                return LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntUGE, left, right, "");

                        default:
                            throw new NotImplementedException();
                    }
                }
                else if (argType is CharType c)
                {
                    switch (op)
                    {
                        case "!=":
                            return LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntNE, left, right, "");

                        case "==":
                            return LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntEQ, left, right, "");

                        case "<":
                            return LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntSLT, left, right, "");

                        case ">":
                            return LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntSGT, left, right, "");

                        case "<=":
                            return LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntSLE, left, right, "");

                        case ">=":
                            return LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntSGE, left, right, "");

                        default:
                            throw new NotImplementedException();
                    }
                }
                else if (argType is PointerType)
                {
                    switch (op)
                    {
                        case "!=":
                            return LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntNE, left, right, "");

                        case "==":
                            return LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntEQ, left, right, "");

                        case "<":
                            return LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntULT, left, right, "");

                        case ">":
                            return LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntUGT, left, right, "");

                        case "<=":
                            return LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntULE, left, right, "");

                        case ">=":
                            return LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntUGE, left, right, "");

                        default:
                            throw new NotImplementedException();
                    }
                }
                else if (argType is FloatType)
                {
                    switch (op)
                    {
                        case "!=":
                            return LLVM.BuildFCmp(builder, LLVMRealPredicate.LLVMRealUNE, left, right, "");

                        case "==":
                            return LLVM.BuildFCmp(builder, LLVMRealPredicate.LLVMRealUEQ, left, right, "");

                        case "<":
                            return LLVM.BuildFCmp(builder, LLVMRealPredicate.LLVMRealULT, left, right, "");

                        case ">":
                            return LLVM.BuildFCmp(builder, LLVMRealPredicate.LLVMRealUGT, left, right, "");

                        case "<=":
                            return LLVM.BuildFCmp(builder, LLVMRealPredicate.LLVMRealULE, left, right, "");

                        case ">=":
                            return LLVM.BuildFCmp(builder, LLVMRealPredicate.LLVMRealUGE, left, right, "");

                        default:
                            throw new NotImplementedException();
                    }
                }
                else if (argType is EnumType e)
                {
                    switch (op)
                    {
                        case "!=":
                            return LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntNE, left, right, "");

                        case "==":
                            return LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntEQ, left, right, "");

                        case "<":
                            return LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntSLT, left, right, "");

                        case ">":
                            return LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntSGT, left, right, "");

                        case "<=":
                            return LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntSLE, left, right, "");

                        case ">=":
                            return LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntSGE, left, right, "");

                        default:
                            throw new NotImplementedException();
                    }
                }
                else if (argType is BoolType)
                {
                    switch (op)
                    {
                        case "!=":
                            {
                                return LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntNE, left, right, "");
                            }

                        case "==":
                            {
                                return LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntEQ, left, right, "");
                            }

                        default:
                            throw new NotImplementedException();
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else if (targetType is PointerType p)
            {
                switch (op)
                {
                    case "+":
                        return LLVM.BuildAdd(builder, left, right, "");

                    case "-":
                        return LLVM.BuildSub(builder, left, right, "");

                    case "*":
                        return LLVM.BuildMul(builder, left, right, "");

                    case "/":
                        return LLVM.BuildUDiv(builder, left, right, "");

                    case "%":
                        return LLVM.BuildURem(builder, left, right, "");


                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
