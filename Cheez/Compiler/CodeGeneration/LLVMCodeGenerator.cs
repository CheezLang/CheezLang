using Cheez.Compiler.Ast;
using Cheez.Compiler.Visitor;
using LLVMSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Cheez.Compiler.CodeGeneration
{

    public class LLVMCodeGeneratorData
    {
        public LLVMBuilderRef Builder { get; set; }
    }

    public class LLVMCodeGenerator : VisitorBase<LLVMValueRef, LLVMCodeGeneratorData>, ICodeGenerator
    {
        private LLVMModuleRef module;
        private Workspace workspace;

        private Dictionary<AstFunctionDecl, LLVMValueRef> functionMap = new Dictionary<AstFunctionDecl, LLVMValueRef>();
        private Dictionary<AstExpression, LLVMValueRef> expressionMap = new Dictionary<AstExpression, LLVMValueRef>();

        private LLVMValueRef llvmPrintf;

        public bool GenerateCode(Workspace workspace, string targetFile)
        {
            const string targetTriple = "i686-pc-win32";

            module = LLVM.ModuleCreateWithName("test");
            this.workspace = workspace;
            
            // <arch><sub>-<vendor>-<sys>-<abi>
            LLVM.SetTarget(module, targetTriple);

            // generate functions
            {
                var ltype = LLVM.FunctionType(LLVM.VoidType(), new LLVMTypeRef[] {
                    LLVM.PointerType(LLVM.Int8Type(), 0)
                }, false);
                llvmPrintf = LLVM.AddFunction(module, "printf", ltype);
                
                LLVM.VerifyFunction(llvmPrintf, LLVMVerifierFailureAction.LLVMPrintMessageAction);
            }
            GenerateFunctions();
            GenerateMainFunction();

            // verify module
            {
                LLVM.VerifyModule(module, LLVMVerifierFailureAction.LLVMPrintMessageAction, out string llvmErrors);
                System.Console.Error.WriteLine(llvmErrors);
            }

            // generate file
            {
                LLVM.PrintModuleToFile(module, $"{targetFile}.ll", out string llvmErrors);
                System.Console.Error.WriteLine(llvmErrors);
            }

            // run code
            if (false) {
                var res = LLVM.CreateExecutionEngineForModule(out var exe, module, out var llvmErrors);
                System.Console.Error.WriteLine(llvmErrors);
                if (res.Value == 0)
                {
                    var main = functionMap[workspace.MainFunction];
                    int result = LLVM.RunFunctionAsMain(exe, main, 0, new string[0], new string[0]);
                }
            }

            // compile code to exe
            {
                LLVM.InitializeAllTargetInfos();
                LLVM.InitializeAllTargets();
                LLVM.InitializeAllTargetMCs();
                if (LLVM.GetTargetFromTriple(targetTriple, out var target, out string llvmErrors).Value == 0)
                {
                    var targetMachine = LLVM.CreateTargetMachine(target, targetTriple, "", "", LLVMCodeGenOptLevel.LLVMCodeGenLevelNone, LLVMRelocMode.LLVMRelocDefault, LLVMCodeModel.LLVMCodeModelDefault);
                    var filenameStr = $"{targetFile}.exe";
                    var filename = Marshal.StringToCoTaskMemAnsi(filenameStr);
                    if (LLVM.TargetMachineEmitToFile(targetMachine, module, filename, LLVMCodeGenFileType.LLVMAssemblyFile, out string llvmErrors2).Value == 0)
                    {

                    }
                    else
                    {
                        Console.WriteLine(llvmErrors2);
                    }
                }
                else
                {
                    Console.WriteLine(llvmErrors);
                }
            }
            //{                
            //    LLVM.GetDefaultTargetTriple()
            //    if (LLVM.GetTargetFromTriple(targetTriple, out var target, out string llvmErrors).Value == 0)
            //    {

            //    }
            //    else
            //    {
            //        Console.WriteLine(llvmErrors);
            //    }
            //}

            // cleanup
            LLVM.DisposeModule(module);
            return true;
        }

        private void GenerateMainFunction()
        {
            var ltype = LLVM.FunctionType(LLVM.VoidType(), new LLVMTypeRef[0], false);
            var lfunc = LLVM.AddFunction(module, "main", ltype);

            LLVMBuilderRef builder = LLVM.CreateBuilder();
            LLVM.PositionBuilderAtEnd(builder, LLVM.AppendBasicBlock(lfunc, "entry"));

            var cheezMain = functionMap[workspace.MainFunction];

            {
                LLVM.BuildCall(builder, cheezMain, new LLVMValueRef[0], "");
                LLVM.BuildRetVoid(builder);
            }

            LLVM.VerifyFunction(lfunc, LLVMVerifierFailureAction.LLVMPrintMessageAction);
            LLVM.DisposeBuilder(builder);
        }

        #region Utility
        private LLVMTypeRef CheezTypeToLLVMType(CheezType ct)
        {
            switch (ct)
            {
                case IntType i:
                    return LLVM.IntType((uint)i.SizeInBytes * 8);

                case StringType _:
                    return LLVM.PointerType(LLVM.Int8Type(), 0);

                case PointerType p:
                    return LLVM.PointerType(CheezTypeToLLVMType(p.TargetType), 0);

                case VoidType _:
                    return LLVM.VoidType();

                case FunctionType f:
                    {
                        var returnType = CheezTypeToLLVMType(f.ReturnType);
                        var paramTypes = f.ParameterTypes.Select(t => CheezTypeToLLVMType(t)).ToArray();
                        return LLVM.FunctionType(returnType, paramTypes, false);
                    }

                default: return default;
            }
        }

        [DebuggerStepThrough]
        private void GenerateFunctions()
        {
            // create declarations
            foreach (var function in workspace.GlobalScope.FunctionDeclarations)
            {
                var ltype = CheezTypeToLLVMType(function.Type);
                var lfunc = LLVM.AddFunction(module, function.Name, ltype);
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

        #endregion

        #region Statements

        public override LLVMValueRef VisitFunctionDeclaration(AstFunctionDecl function, LLVMCodeGeneratorData data = null)
        {
            var lfunc = functionMap[function];

            if (function.HasImplementation)
            {
                LLVMBuilderRef builder = LLVM.CreateBuilder();
                LLVM.PositionBuilderAtEnd(builder, LLVM.AppendBasicBlock(lfunc, "entry"));

                // body
                {
                    foreach (var s in function.Statements)
                    {
                        s.Accept(this, new LLVMCodeGeneratorData { Builder = builder });
                    }
                }

                if (function.ReturnType == VoidType.Intance)
                    LLVM.BuildRetVoid(builder);
                LLVM.DisposeBuilder(builder);
            }

            LLVM.VerifyFunction(lfunc, LLVMVerifierFailureAction.LLVMPrintMessageAction);
            
            return lfunc;
        }

        public override LLVMValueRef VisitExpressionStatement(AstExprStmt stmt, LLVMCodeGeneratorData data = null)
        {
            return stmt.Expr.Accept(this, data);
        }

        #endregion

        #region Expressions

        public override LLVMValueRef VisitIdentifierExpression(AstIdentifierExpr ident, LLVMCodeGeneratorData data = null)
        {
            return base.VisitIdentifierExpression(ident, data);
        }

        public override LLVMValueRef VisitCallExpression(AstCallExpr call, LLVMCodeGeneratorData data = null)
        {
            LLVMValueRef func;
            if (call.Function is AstIdentifierExpr i)
            {
                func = LLVM.GetNamedFunction(module, i.Name);
            }
            else
            {
                throw new NotImplementedException();
            }

            var args = call.Arguments.Select(a => a.Accept(this, data)).ToArray();

            var res = LLVM.BuildCall(data.Builder, func, args, "");
            return res;
        }

        public override LLVMValueRef VisitPrintStatement(AstPrintStmt print, LLVMCodeGeneratorData data = null)
        {
            var val = print.Expressions.First().Accept(this, data);
            var casted = LLVM.BuildPointerCast(data.Builder, val, LLVM.PointerType(LLVM.Int8Type(), 0), "");
            LLVM.BuildCall(data.Builder, llvmPrintf, new LLVMValueRef[] { casted }, "");
            return val;
        }

        public override LLVMValueRef VisitStringLiteral(AstStringLiteral str, LLVMCodeGeneratorData data = null)
        {
            var lstr = LLVM.BuildGlobalString(data.Builder, str.Value, "");
            var val = LLVM.BuildPointerCast(data.Builder, lstr, CheezTypeToLLVMType(StringType.Instance), "");
            return val;
        }

        #endregion
    }
}
