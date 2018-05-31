using Cheez.Compiler.Ast;
using Cheez.Compiler.Visitor;
using LLVMSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Cheez.Compiler.CodeGeneration
{

    public class LLVMCodeGeneratorData
    {
        public LLVMBuilderRef Builder { get; set; }
        public bool Deref { get; set; } = true;
        public LLVMValueRef Function { get; set; }
        public LLVMBasicBlockRef BasicBlock { get; set; }

        public LLVMCodeGeneratorData Clone(LLVMBuilderRef? Builder = null, bool? Deref = null, LLVMValueRef? Function = null, LLVMBasicBlockRef? BasicBlock = null)
        {
            return new LLVMCodeGeneratorData
            {
                Builder = Builder ?? this.Builder,
                Deref = Deref ?? this.Deref,
                Function = Function ?? this.Function,
                BasicBlock = BasicBlock ?? this.BasicBlock
            };
        }
    }

    public class LLVMCodeGenerator : VisitorBase<LLVMValueRef, LLVMCodeGeneratorData>, ICodeGenerator
    {
        private string targetFile;

        private LLVMModuleRef module;
        private Workspace workspace;

        private Dictionary<AstFunctionDecl, LLVMValueRef> functionMap = new Dictionary<AstFunctionDecl, LLVMValueRef>();
        private Dictionary<object, LLVMValueRef> valueMap = new Dictionary<object, LLVMValueRef>();

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

            // generate all types
            GenerateAllTypes();

            // generate global variables
            GenerateGlobalVariables();

            // generate functions
            GenerateFunctions();
            GenerateMainFunction();

            // verify module
            {
                LLVM.VerifyModule(module, LLVMVerifierFailureAction.LLVMPrintMessageAction, out string llvmErrors);
                if (!string.IsNullOrWhiteSpace(llvmErrors))
                    Console.Error.WriteLine($"[LLVM-validate-module] {llvmErrors}");
            }

            // generate file
            if (true) {
                LLVM.PrintModuleToFile(module, $"{targetFile}.ll", out string llvmErrors);
                if (!string.IsNullOrWhiteSpace(llvmErrors))
                    Console.Error.WriteLine($"[LLVM-validate-print] {llvmErrors}");
            }

            // run code
            if (false)
            {
                var res = LLVM.CreateExecutionEngineForModule(out var exe, module, out var llvmErrors);
                System.Console.Error.WriteLine(llvmErrors);
                if (res.Value == 0)
                {
                    var main = functionMap[workspace.MainFunction];
                    int result = LLVM.RunFunctionAsMain(exe, main, 0, new string[0], new string[0]);
                }
            }

            {
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
                    LLVMCodeGenOptLevel.LLVMCodeGenLevelNone, 
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

        public bool CompileCode()
        {
            var filename = Path.GetFileNameWithoutExtension(targetFile + ".x");
            var dir = Path.GetDirectoryName(Path.GetFullPath(targetFile));

            //// compile .ll to .obj
            //var llc = Util.StartProcess(
            //    "llc.exe",
            //    new List<string>()
            //    {
            //        "-O0",
            //        "-filetype=obj",
            //        "-o",
            //        $"{filename}.obj",
            //        $"{filename}.ll"
            //    },
            //    dir,
            //    CreateHandler("llc.exe", Console.Out),
            //    CreateHandler("llc.exe - ERROR", Console.Error)
            //    );
            //llc.WaitForExit();
            //if (llc.ExitCode != 0)
            //    return false;

            // link .obj to .exe
            var link = Util.StartProcess(
                @"lld-link.exe",
                new List<string>
                {
                    $"/out:{filename}.exe",
                    @"-libpath:C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\VC\Tools\MSVC\14.12.25827\lib\x86",
                    @"-libpath:C:\Program Files (x86)\Windows Kits\10\Lib\10.0.16299.0\ucrt\x86",
                    @"-libpath:C:\Program Files (x86)\Windows Kits\10\Lib\10.0.16299.0\um\x86",
                    @"-libpath:D:\llvm\build\Debug\lib",
                    "/entry:mainCRTStartup",
                    "/machine:X86",
                    "/subsystem:console",

                    "libucrtd.lib",
                    //"libvcruntimed.lib",
                    "libcmtd.lib",
                    //"msvcrtd.lib", //

                    "kernel32.lib",
                    "user32.lib",
                    "gdi32.lib",
                    "winspool.lib",
                    "comdlg32.lib",
                    "advapi32.lib",
                    "shell32.lib",
                    "ole32.lib",
                    "oleaut32.lib",
                    "uuid.lib",
                    "odbc32.lib",
                    "odbccp32.lib",

                    "legacy_stdio_definitions.lib",
                    "legacy_stdio_wide_specifiers.lib",

                    "libclang.lib",

                    $"{filename}.obj"
                },
                dir,
                stdout: CreateHandler("link.exe", Console.Out),
                stderr: CreateHandler("link.exe - ERROR", Console.Error)
                );
            link.WaitForExit();
            if (link.ExitCode != 0)
                return false;

            Console.WriteLine($"Generated {filename}.exe");

            return true;
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

        private void GenerateMainFunction()
        {
            var ltype = LLVM.FunctionType(LLVM.Int32Type(), new LLVMTypeRef[0], false);
            var lfunc = LLVM.AddFunction(module, "main", ltype);

            LLVMBuilderRef builder = LLVM.CreateBuilder();
            LLVM.PositionBuilderAtEnd(builder, LLVM.AppendBasicBlock(lfunc, "entry"));

            var cheezMain = functionMap[workspace.MainFunction];

            // initialize global variables
            {
                var d = new LLVMCodeGeneratorData
                {
                    Builder = builder
                };
                foreach (var g in workspace.GlobalScope.VariableDeclarations)
                {
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

            LLVM.VerifyFunction(lfunc, LLVMVerifierFailureAction.LLVMPrintMessageAction);
            LLVM.DisposeBuilder(builder);
        }

        #region Utility
        private LLVMTypeRef CheezTypeToLLVMType(CheezType ct)
        {
            switch (ct)
            {
                case BoolType b:
                    return LLVM.Int1Type();

                case IntType i:
                    return LLVM.IntType((uint)i.SizeInBytes * 8);

                case StringType _:
                    return LLVM.PointerType(LLVM.Int8Type(), 0);

                case PointerType p:
                    if (p.TargetType == VoidType.Intance)
                        return LLVM.PointerType(LLVM.Int8Type(), 0);
                    return LLVM.PointerType(CheezTypeToLLVMType(p.TargetType), 0);

                case VoidType _:
                    return LLVM.VoidType();

                case FunctionType f:
                    {
                        var returnType = CheezTypeToLLVMType(f.ReturnType);
                        var paramTypes = f.ParameterTypes.Select(t => CheezTypeToLLVMType(t)).ToArray();
                        return LLVM.FunctionType(returnType, paramTypes, f.VarArgs);
                    }

                case StructType s:
                    {
                        var memTypes = s.Declaration.Members.Select(m => CheezTypeToLLVMType(m.Type)).ToArray();
                        return LLVM.StructType(memTypes, false);
                    }

                default: return default;
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
                var varargs = function.GetDirective("varargs");
                if (varargs != null)
                    (function.Type as FunctionType).VarArgs = true;

                var ltype = CheezTypeToLLVMType(function.Type);
                var lfunc = LLVM.AddFunction(module, function.Name.Name, ltype);

                var ccDir = function.GetDirective("stdcall");
                if (ccDir != null)
                {
                    LLVM.SetFunctionCallConv(lfunc, (uint)LLVMCallConv.LLVMX86StdcallCallConv);
                }

                functionMap[function] = lfunc;
            }

            // create implementations
            foreach (var f in workspace.GlobalScope.FunctionDeclarations)
            {
                f.Accept(this, null);
            }
        }

        //private LLVMValueRef CastIfString(LLVMValueRef val, LLVMBuilderRef builder)
        //{

        //}

        private LLVMValueRef GetDefaultLLVMValue(CheezType type)
        {
            switch (type)
            {
                case PointerType p:
                    return LLVM.ConstIntToPtr(LLVM.ConstInt(LLVM.IntType(32), 0, false), CheezTypeToLLVMType(type));

                case StringType s:
                    return LLVM.ConstIntToPtr(LLVM.ConstInt(LLVM.IntType(32), 0, false), LLVM.PointerType(LLVM.Int8Type(), 0));

                case IntType i:
                    return LLVM.ConstInt(CheezTypeToLLVMType(type), 0, false);

                default:
                    throw new NotImplementedException();
            }
        }

        private LLVMValueRef AllocVar(LLVMBuilderRef builder, CheezType type, string name = "")
        {
            var llvmType = CheezTypeToLLVMType(type);
            if (type is ArrayType a)
            {
                llvmType = CheezTypeToLLVMType(PointerType.GetPointerType(a.TargetType));
                var val = LLVM.BuildAlloca(builder, llvmType, name);
                return val;
            }
            else if (type is StructType s)
            {
                llvmType = CheezTypeToLLVMType(s);
                var val = LLVM.BuildAlloca(builder, llvmType, name);
                return val;
            }
            else
            {
                var val = LLVM.BuildAlloca(builder, llvmType, name);
                return val;
            }
        }

        #endregion

        #region Statements

        public override LLVMValueRef VisitTypeDeclaration(AstTypeDecl type, LLVMCodeGeneratorData data = null)
        {
            var ct = type.Type;
            var llvmType = CheezTypeToLLVMType(ct);

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
                if (variable.Initializer != null)
                {
                    var ptr = valueMap[variable];
                    var val = variable.Initializer.Accept(this, data);
                    return LLVM.BuildStore(data.Builder, val, ptr);
                }
                return default;
            }
        }

        public override LLVMValueRef VisitAssignment(AstAssignment ass, LLVMCodeGeneratorData data = null)
        {
            var left = ass.Target.Accept(this, data.Clone(Deref: false));
            var right = ass.Value.Accept(this, data);
            return LLVM.BuildStore(data.Builder, right, left);
        }

        public override LLVMValueRef VisitFunctionDeclaration(AstFunctionDecl function, LLVMCodeGeneratorData data = null)
        {
            var lfunc = functionMap[function];

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
                {
                    foreach (var l in function.LocalVariables)
                    {
                        valueMap[l] = AllocVar(builder, l.Type, l.Name?.Name ?? "");
                    }
                }


                // store params in local variables
                for (int i = 0; i < function.Parameters.Count; i++)
                {
                    var param = function.Parameters[i];
                    var p = lfunc.GetParam((uint)i);
                    LLVM.BuildStore(builder, p, valueMap[param]);
                }

                // body
                {
                    var d = new LLVMCodeGeneratorData { Builder = builder, BasicBlock = entry, Function = lfunc };
                    //foreach (var s in function.Statements)
                    //{
                    //    s.Accept(this, d);
                    //}
                    function.Body.Accept(this, d);
                }

                if (function.ReturnType == VoidType.Intance)
                    LLVM.BuildRetVoid(builder);
                LLVM.DisposeBuilder(builder);
            }

            LLVM.VerifyFunction(lfunc, LLVMVerifierFailureAction.LLVMPrintMessageAction);

            return lfunc;
        }

        public override LLVMValueRef VisitReturnStatement(AstReturnStmt ret, LLVMCodeGeneratorData data = null)
        {
            var prevBB = data.BasicBlock;
            var nextBB = prevBB;

            if (!ret.GetFlag(StmtFlags.IsLastStatementInBlock))
                nextBB = LLVM.AppendBasicBlock(data.Function, "ret");

            LLVMValueRef? retInts = null;
            if (ret.ReturnValue != null)
            {
                var retVal = ret.ReturnValue.Accept(this, data);
                retInts = LLVM.BuildRet(data.Builder, retVal);
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

            // Condition
            var bbCondition = LLVM.AppendBasicBlock(data.Function, "while_condition");
            LLVM.BuildBr(data.Builder, bbCondition);
            LLVM.PositionBuilderAtEnd(data.Builder, bbCondition);

            data.BasicBlock = bbCondition;
            var cond = ws.Condition.Accept(this, data);
            var bbConditionEnd = data.BasicBlock;

            // body
            var bbBody = LLVM.AppendBasicBlock(data.Function, "while_body");
            LLVM.PositionBuilderAtEnd(data.Builder, bbBody);

            data.BasicBlock = bbBody;
            ws.Body.Accept(this, data);
            LLVM.BuildBr(data.Builder, bbCondition);

            //
            var bbEnd = LLVM.AppendBasicBlock(data.Function, "while_end");
            LLVM.PositionBuilderAtEnd(data.Builder, bbEnd);
            data.BasicBlock = bbEnd;

            // connect condition with body and end
            LLVM.PositionBuilderAtEnd(data.Builder, bbConditionEnd);
            LLVM.BuildCondBr(data.Builder, cond, bbBody, bbEnd);

            LLVM.PositionBuilderAtEnd(data.Builder, bbEnd);

            return default;
        }

        public override LLVMValueRef VisitIfStatement(AstIfStmt ifs, LLVMCodeGeneratorData data = null)
        {
            var bbPrev = data.BasicBlock;

            // Condition
            var cond = ifs.Condition.Accept(this, data);
            bbPrev = data.BasicBlock;

            // if body
            var bbIfBody = LLVM.AppendBasicBlock(data.Function, "if_body");
            LLVM.PositionBuilderAtEnd(data.Builder, bbIfBody);
            data.BasicBlock = bbIfBody;
            ifs.IfCase.Accept(this, data);
            var bbIfBodyEnd = data.BasicBlock;

            // else body
            var bbElseBody = LLVM.AppendBasicBlock(data.Function, "else_body");
            var bbElseBodyEnd = bbElseBody;
            LLVM.PositionBuilderAtEnd(data.Builder, bbElseBody);
            if (ifs.ElseCase != null)
            {
                data.BasicBlock = bbElseBody;
                ifs.ElseCase.Accept(this, data);
                bbElseBodyEnd = data.BasicBlock;
            }

            //
            var bbEnd = LLVM.AppendBasicBlock(data.Function, "if_end");

            // connect basic blocks
            LLVM.PositionBuilderAtEnd(data.Builder, bbPrev);
            LLVM.BuildCondBr(data.Builder, cond, bbIfBody, bbElseBody);

            if (!ifs.IfCase.GetFlag(StmtFlags.Returns))
            {
                LLVM.PositionBuilderAtEnd(data.Builder, bbIfBodyEnd);
                LLVM.BuildBr(data.Builder, bbEnd);
            }

            if (!(ifs.ElseCase?.GetFlag(StmtFlags.Returns) ?? false))
            {
                LLVM.PositionBuilderAtEnd(data.Builder, bbElseBodyEnd);
                LLVM.BuildBr(data.Builder, bbEnd);
            }

            LLVM.PositionBuilderAtEnd(data.Builder, bbEnd);
            data.BasicBlock = bbEnd;

            return default;
        }

        public override LLVMValueRef VisitBlockStatement(AstBlockStmt block, LLVMCodeGeneratorData data = null)
        {
            foreach (var s in block.Statements)
            {
                s.Accept(this, data);
            }

            return default;
        }

        #endregion

        #region Expressions

        public override LLVMValueRef VisitDotExpression(AstDotExpr dot, LLVMCodeGeneratorData data = null)
        {
            if (dot.IsDoubleColon)
            {
                throw new NotImplementedException();
            }
            else
            {
                if (dot.Left.Type is StructType s)
                {
                    var left = dot.Left.Accept(this, data.Clone(Deref: false));
                    var index = (uint)s.GetIndexOfMember(dot.Right);
                    var elemPtr = LLVM.BuildStructGEP(data.Builder, left, index, dot.ToString());

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
            var sub = add.SubExpression.Accept(this, data.Clone(Deref: false));
            return sub;
        }

        public override LLVMValueRef VisitCastExpression(AstCastExpr cast, LLVMCodeGeneratorData data = null)
        {
            var sub = cast.SubExpression.Accept(this, data);

            if (cast.Type is PointerType)
            {
                var type = CheezTypeToLLVMType(cast.Type);
                if (cast.SubExpression.Type is PointerType)
                {
                    return LLVM.BuildPointerCast(data.Builder, sub, type, "");
                }
                else if (cast.SubExpression.Type is IntType i)
                {
                    return LLVM.BuildIntToPtr(data.Builder, sub, type, "");
                }
                else if (cast.SubExpression.Type is StringType s)
                {
                    return LLVM.BuildPointerCast(data.Builder, sub, type, "");
                }
            }
            else if (cast.Type is StringType s)
            {
                var type = CheezTypeToLLVMType(s.ToPointerType());

                if (cast.SubExpression.Type is PointerType)
                {
                    return LLVM.BuildPointerCast(data.Builder, sub, type, "");
                }
                else if (cast.SubExpression.Type is IntType)
                {
                    return LLVM.BuildIntToPtr(data.Builder, sub, type, "");
                }
            }
            else if (cast.Type is ArrayType a)
            {
                var type = CheezTypeToLLVMType(a.ToPointerType());
                return LLVM.BuildPointerCast(data.Builder, sub, type, "");
            }
            else if (cast.Type is IntType tt)
            {
                var type = CheezTypeToLLVMType(cast.Type);
                if (cast.SubExpression.Type is IntType || cast.SubExpression.Type is BoolType)
                {
                    return LLVM.BuildIntCast(data.Builder, sub, type, "");
                }
                else if (cast.SubExpression.Type is PointerType)
                {
                    return LLVM.BuildPtrToInt(data.Builder, sub, type, "");
                }
            }
            else if (cast.Type is BoolType)
            {
                var type = CheezTypeToLLVMType(cast.Type);
                if (cast.SubExpression.Type is IntType i)
                {
                    return LLVM.BuildIntCast(data.Builder, sub, type, "");
                }
            }

            throw new NotImplementedException($"Cast from {cast.SubExpression.Type} to {cast.Type} is not implemented yet");
        }

        public override LLVMValueRef VisitIdentifierExpression(AstIdentifierExpr ident, LLVMCodeGeneratorData data = null)
        {
            var s = ident.Symbol;
            var v = valueMap[s];

            if (data.Deref)
                return LLVM.BuildLoad(data.Builder, v, "");

            return v;
        }

        public override LLVMValueRef VisitCallExpression(AstCallExpr call, LLVMCodeGeneratorData data = null)
        {
            LLVMValueRef func;
            if (call.Function is AstIdentifierExpr i)
            {
                func = LLVM.GetNamedFunction(module, i.Name);
            }
            else if (call.Function is AstFunctionExpression afe)
            {
                func = LLVM.GetNamedFunction(module, afe.Declaration.Name.Name);
            }
            else
            {
                throw new NotImplementedException();
            }

            var args = call.Arguments.Select(a => a.Accept(this, data)).ToArray();

            var res = LLVM.BuildCall(data.Builder, func, args, "");
            LLVM.SetInstructionCallConv(res, LLVM.GetFunctionCallConv(func));
            return res;
        }

        public override LLVMValueRef VisitStringLiteral(AstStringLiteral str, LLVMCodeGeneratorData data = null)
        {
            var lstr = LLVM.BuildGlobalString(data.Builder, str.Value, "");
            var val = LLVM.BuildPointerCast(data.Builder, lstr, CheezTypeToLLVMType(StringType.Instance), "");
            return val;
        }

        public override LLVMValueRef VisitNumberExpression(AstNumberExpr num, LLVMCodeGeneratorData data = null)
        {
            if (num.Type is IntType i)
            {
                var llvmType = CheezTypeToLLVMType(i);
                var val = num.Data.ToUlong();
                return LLVM.ConstInt(llvmType, val, false);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override LLVMValueRef VisitBoolExpression(AstBoolExpr bo, LLVMCodeGeneratorData data = null)
        {
            return LLVM.ConstInt(LLVM.Int1Type(), bo.Value ? 1ul : 0ul, false);
        }

        public override LLVMValueRef VisitArrayAccessExpression(AstArrayAccessExpr arr, LLVMCodeGeneratorData data = null)
        {
            var sub = arr.SubExpression.Accept(this, data.Clone(Deref: true)); // @Todo: Deref: !data.Deref / true
            var ind = arr.Indexer.Accept(this, data.Clone(Deref: true));

            var dataLayout = LLVM.GetModuleDataLayout(module);
            var pointerSize = LLVM.PointerSize(dataLayout);

            var castType = LLVM.IntType(pointerSize * 8);

            var subCasted = LLVM.BuildPtrToInt(data.Builder, sub, castType, "");
            var indCasted = LLVM.BuildIntCast(data.Builder, ind, castType, "");

            var add = LLVM.BuildAdd(data.Builder, subCasted, indCasted, "");

            var result = LLVM.BuildIntToPtr(data.Builder, add, LLVM.TypeOf(sub), "");

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
                    return LLVM.BuildNeg(data.Builder, sub, "");

                default:
                    throw new NotImplementedException();
            }
        }

        public override LLVMValueRef VisitBinaryExpression(AstBinaryExpr bin, LLVMCodeGeneratorData data = null)
        {

            if (bin.Type is IntType i)
            {
                var left = bin.Left.Accept(this, data);
                var right = bin.Right.Accept(this, data);

                switch (bin.Operator)
                {
                    case "+":
                        return LLVM.BuildAdd(data.Builder, left, right, "");

                    case "-":
                        return LLVM.BuildSub(data.Builder, left, right, "");

                    case "*":
                        return LLVM.BuildMul(data.Builder, left, right, "");

                    case "/":
                        if (i.Signed)
                            return LLVM.BuildSDiv(data.Builder, left, right, "");
                        else
                            return LLVM.BuildUDiv(data.Builder, left, right, "");

                    case "%":
                        if (i.Signed)
                            return LLVM.BuildSRem(data.Builder, left, right, "");
                        else
                            return LLVM.BuildURem(data.Builder, left, right, "");


                    default:
                        throw new NotImplementedException();
                }
            }
            else if (bin.Type is FloatType)
            {
                var left = bin.Left.Accept(this, data);
                var right = bin.Right.Accept(this, data);

                switch (bin.Operator)
                {
                    case "+":
                        return LLVM.BuildFAdd(data.Builder, left, right, "");

                    case "-":
                        return LLVM.BuildFSub(data.Builder, left, right, "");

                    case "*":
                        return LLVM.BuildFMul(data.Builder, left, right, "");

                    case "/":
                        return LLVM.BuildFDiv(data.Builder, left, right, "");

                    default:
                        throw new NotImplementedException();
                }
            }
            else if (bin.Type is BoolType b)
            {
                if (bin.Left.Type is IntType ii)
                {
                    var left = bin.Left.Accept(this, data);
                    var right = bin.Right.Accept(this, data);
                    switch (bin.Operator)
                    {
                        case "!=":
                            return LLVM.BuildICmp(data.Builder, LLVMIntPredicate.LLVMIntNE, left, right, "");

                        case "==":
                            return LLVM.BuildICmp(data.Builder, LLVMIntPredicate.LLVMIntEQ, left, right, "");

                        case "<":
                            if (ii.Signed)
                                return LLVM.BuildICmp(data.Builder, LLVMIntPredicate.LLVMIntSLT, left, right, "");
                            else
                                return LLVM.BuildICmp(data.Builder, LLVMIntPredicate.LLVMIntULT, left, right, "");

                        case ">":
                            if (ii.Signed)
                                return LLVM.BuildICmp(data.Builder, LLVMIntPredicate.LLVMIntSGT, left, right, "");
                            else
                                return LLVM.BuildICmp(data.Builder, LLVMIntPredicate.LLVMIntUGT, left, right, "");

                        case "<=":
                            if (ii.Signed)
                                return LLVM.BuildICmp(data.Builder, LLVMIntPredicate.LLVMIntSLE, left, right, "");
                            else
                                return LLVM.BuildICmp(data.Builder, LLVMIntPredicate.LLVMIntULE, left, right, "");

                        case ">=":
                            if (ii.Signed)
                                return LLVM.BuildICmp(data.Builder, LLVMIntPredicate.LLVMIntSGE, left, right, "");
                            else
                                return LLVM.BuildICmp(data.Builder, LLVMIntPredicate.LLVMIntUGE, left, right, "");

                        default:
                            throw new NotImplementedException();
                    }
                }
                else if (bin.Left.Type is BoolType)
                {
                    var left = bin.Left.Accept(this, data);
                    switch (bin.Operator)
                    {
                        case "and":
                            {
                                var tempVar = valueMap[bin];
                                LLVM.BuildStore(data.Builder, left, tempVar);

                                // right
                                var bbSecond = LLVM.AppendBasicBlock(data.Function, "and_rhs");
                                LLVM.PositionBuilderAtEnd(data.Builder, bbSecond);
                                var right = bin.Right.Accept(this, data);
                                LLVM.BuildStore(data.Builder, right, tempVar);

                                var bbEnd = LLVM.AppendBasicBlock(data.Function, "and_end");
                                LLVM.BuildBr(data.Builder, bbEnd);

                                // 
                                LLVM.PositionBuilderAtEnd(data.Builder, data.BasicBlock);
                                LLVM.BuildCondBr(data.Builder, left, bbSecond, bbEnd);

                                //
                                LLVM.PositionBuilderAtEnd(data.Builder, bbEnd);
                                tempVar = LLVM.BuildLoad(data.Builder, tempVar, bin.ToString());
                                data.BasicBlock = bbEnd;
                                return tempVar;
                            }
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            throw new NotImplementedException();
        }

        #endregion
    }
}
