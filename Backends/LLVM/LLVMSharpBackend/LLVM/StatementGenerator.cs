using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Types;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;
using LLVMSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Cheez.CodeGeneration.LLVMCodeGen
{
    public partial class LLVMCodeGenerator
    {
        private void GenerateFunctionHeader(AstFuncExpr function)
        {
            if (function.IsMacroFunction)
                return;

            var name = "";

            if (function.ImplBlock != null)
            {
                name += function.ImplBlock.TargetType + ".";
                if (function.ImplBlock.Trait != null)
                    name += function.ImplBlock.Trait + ".";
            }

            name += function.Name;

            if (function.PolymorphicTypes != null && function.PolymorphicTypes.Count > 0)
                name += "." + string.Join(".", function.PolymorphicTypes.Select(p => $"{p.Key}.{p.Value}"));
            if (function.ConstParameters != null && function.ConstParameters.Count > 0)
                name += "." + string.Join(".", function.ConstParameters.Select(p => $"{p.Key}.{p.Value.type}.{p.Value.value}"));

            if (function.Body != null)
                name += ".che";

            var linkname = function.GetDirective("linkname");
            if (linkname != null)
            {
                name = linkname.Arguments[0].Value as string;
            }

            LLVMTypeRef ltype = FuncTypeToLLVMType(function.FunctionType);
            var lfunc = module.AddFunction(name, ltype);

            // :temporary
            if (function.Body != null)
                lfunc.SetLinkage(LLVMLinkage.LLVMInternalLinkage);

            if (function.HasDirective("extern"))
            {
                lfunc.SetLinkage(LLVMLinkage.LLVMExternalLinkage);
            }

            // TODO

            if (function.HasDirective("noinline"))
            {
                lfunc.AddFunctionAttribute(context, LLVMAttributeKind.NoInline);
            }
            lfunc.AddFunctionAttribute(context, LLVMAttributeKind.NoUnwind);

            var ccDir = function.GetDirective("stdcall");
            if (ccDir != null)
            {
                LLVM.SetFunctionCallConv(lfunc, (int)LLVMCallConv.LLVMX86StdcallCallConv);
            }

            valueMap[function] = lfunc;
        }

        private void GenerateFunctionImplementation(AstFuncExpr function)
        {
            if (function.Body == null || function.IsMacroFunction)
                return;

            currentFunction = function;

            keepTrackOfStackTrace = !function.HasDirective("nostacktrace") && enableStackTrace;

            var lfunc = valueMap[function];
            currentLLVMFunction = lfunc;

            // generate body
            {
                var builder = new IRBuilder();
                this.builder = builder;
                
                var bbParams = lfunc.AppendBasicBlock("locals");
                //var bbTemps = lfunc.AppendBasicBlock("temps");
                var bbBody = lfunc.AppendBasicBlock("body");

                // allocate space for parameters and return values on stack
                builder.PositionBuilderAtEnd(bbParams);

                PushStackTrace(function);

                for (int i = 0; i < function.Parameters.Count; i++)
                {
                    var param = function.Parameters[i];
                    var p = lfunc.GetParam((uint)i);
                    var ptype = LLVM.TypeOf(p);
                    p = builder.CreateAlloca(ptype, $"p_{param.Name?.Name}");
                    valueMap[param] = p;
                }

                foreach (var c in function.ConstScope.Symbols)
                {
                    if (c.Value is ConstSymbol s && !s.Type.IsComptimeOnly)
                    {
                        var val = CheezValueToLLVMValue(s.Type, s.Value);
                        var cnst = builder.CreateAlloca(CheezTypeToLLVMType(s.Type), $"c_");
                        builder.CreateStore(val, cnst);
                        valueMap[s] = cnst;
                    }
                }

                if (function.ReturnTypeExpr != null)
                {
                    var ptype = CheezTypeToLLVMType(function.ReturnTypeExpr.Type);
                    var p = builder.CreateAlloca(ptype, $"ret_{function.ReturnTypeExpr.Name?.Name}");
                    valueMap[function.ReturnTypeExpr] = p;
                }

                // store params and rets in local variables
                for (int i = 0; i < function.Parameters.Count; i++)
                {
                    var param = function.Parameters[i];
                    var p = lfunc.GetParam((uint)i);
                    builder.CreateStore(p, valueMap[param]);
                }

                // temp values
                //builder.PositionBuilderAtEnd(bbTemps);
                builder.CreateBr(bbBody);

                // body
                builder.PositionBuilderAtEnd(bbBody);
                GenerateExpression(function.Body, false);

                // ret if void
                if (function.ReturnTypeExpr == null && !function.Body.GetFlag(ExprFlags.Returns))
                {
                    PopStackTrace();
                    builder.CreateRetVoid();
                }
                builder.Dispose();
            }

            // remove empty basic blocks
            var bb = lfunc.GetFirstBasicBlock();
            while (bb.Pointer.ToInt64() != 0)
            {
                var first = bb.GetFirstInstruction();

                if (bb.GetBasicBlockTerminator().Pointer.ToInt64() == 0)
                {
                    var b = new IRBuilder();
                    b.PositionBuilderAtEnd(bb);
                    b.CreateUnreachable();
                    b.Dispose();
                }

                bb = bb.GetNextBasicBlock();
            }

            // TODO
            //if (lfunc.VerifyFunction(LLVMVerifierFailureAction.LLVMPrintMessageAction))
            //{
            //    Console.Error.WriteLine($"in function {lfunc}");
            //}

            currentFunction = null;
        }

        private void GenerateStatement(AstStatement stmt)
        {
            switch (stmt)
            {
                case AstReturnStmt ret: GenerateReturnStatement(ret); break;
                case AstExprStmt expr: GenerateExprStatement(expr); break;
                case AstVariableDecl decl: GenerateVariableDecl(decl); break;
                case AstAssignment ass: GenerateAssignment(ass); break;
                case AstWhileStmt whl: GenerateWhile(whl); break;
                case AstUsingStmt use:
                    if (use.Value.Type is StructType)
                        GenerateExpression(use.Value, true);
                    break;

                //default: throw new NotImplementedException();
            }
        }

        private void GenerateWhile(AstWhileStmt whl)
        {
            if (whl.PreActions != null)
                foreach (var pre in whl.PreActions)
                    GenerateStatement(pre);

            var bbCond = LLVM.AppendBasicBlock(currentLLVMFunction, "_loop_cond");
            var bbBody = LLVM.AppendBasicBlock(currentLLVMFunction, "_loop_body");
            var bbPost = LLVM.AppendBasicBlock(currentLLVMFunction, "_loop_post");
            var bbEnd = LLVM.AppendBasicBlock(currentLLVMFunction, "_loop_end");

            loopEndMap[whl] = bbEnd;
            loopPostActionMap[whl] = bbPost;

            builder.CreateBr(bbCond);

            builder.PositionBuilderAtEnd(bbCond);
            var cond = GenerateExpression(whl.Condition, true);
            builder.CreateCondBr(cond, bbBody, bbEnd);

            builder.PositionBuilderAtEnd(bbBody);
            GenerateExpression(whl.Body, false);

            builder.CreateBr(bbPost);
            builder.PositionBuilderAtEnd(bbPost);

            if (whl.PostAction != null)
                GenerateStatement(whl.PostAction);

            builder.CreateBr(bbCond);

            builder.PositionBuilderAtEnd(bbEnd);
        }

        private void GenerateAssignmentValue(AstAssignment ass)
        {
            if (ass.SubAssignments?.Count > 0)
            {
                foreach (var sub in ass.SubAssignments)
                    GenerateAssignmentValue(sub);
                return;
            }

            var v = GenerateExpression(ass.Value, true);
            valueMap[ass] = v;
        }

        private void GenerateAssignmentStore(AstAssignment ass)
        {
            if (ass.SubAssignments?.Count > 0)
            {
                foreach (var sub in ass.SubAssignments)
                    GenerateAssignmentStore(sub);
                return;
            }

            if (ass.OnlyGenerateValue)
                return;
            var v = valueMap[ass];

            bool deref = ass.Pattern.Type is ReferenceType;
            var ptr = GenerateExpression(ass.Pattern, deref);

            builder.CreateStore(v, ptr);
        }

        private void GenerateAssignment(AstAssignment ass)
        {
            if (ass.Destructions != null)
            {
                foreach (var dest in ass.Destructions)
                {
                    GenerateExpression(dest, false);
                }
            }

            GenerateAssignmentValue(ass);
            GenerateAssignmentStore(ass);
        }

        private void GenerateExprStatement(AstExprStmt expr)
        {
            GenerateExpression(expr.Expr, false);
            if (expr.Destructions != null)
            {
                foreach (var dest in expr.Destructions)
                {
                    GenerateExpression(dest, false);
                }
            }
        }

        private void InitGlobalVariable(AstVariableDecl decl, HashSet<AstVariableDecl> visited)
        {
            if (visited.Contains(decl))
                return;

            if (decl.Dependencies != null)
            {
                foreach (var dep in decl.Dependencies)
                {
                    if (dep is AstVariableDecl v)
                        InitGlobalVariable(v, visited);
                }
            }

            // create vars
            var type = CheezTypeToLLVMType(decl.Type);

            var varPtr = module.AddGlobal(type, decl.Name.Name);
            varPtr.SetLinkage(LLVMLinkage.LLVMInternalLinkage);
            if (decl.HasDirective("thread_local"))
                varPtr.SetThreadLocal(true);
            //varPtr.SetLinkage(LLVMLinkage.LLVMExternalLinkage);// TODO?

            var dExtern = decl.GetDirective("extern");
            if (dExtern != null) varPtr.SetLinkage(LLVMLinkage.LLVMExternalLinkage);

            LLVMValueRef initializer = LLVM.ConstNull(CheezTypeToLLVMType(decl.Type));

            varPtr.SetInitializer(initializer);
            valueMap[decl] = varPtr;

            // do initialization TODO: other patterns
            var x = GenerateExpression(decl.Initializer, true);
            builder.CreateStore(x, varPtr);

            visited.Add(decl);
        }

        private void GenerateVariableDecl(AstVariableDecl decl)
        {
            if (decl.Type.IsComptimeOnly)
                return;

            var varPtr = CreateLocalVariable(decl.Type, decl.Name.Name);
            valueMap[decl] = varPtr;

            if (decl.Initializer != null)
            {
                var x = GenerateExpression(decl.Initializer, true);
                builder.CreateStore(x, varPtr);
            }
        }

        private void GenerateReturnStatement(AstReturnStmt ret)
        {
            if (ret.ReturnValue != null)
            {
                var return_var = valueMap[currentFunction.ReturnTypeExpr];
                var retval = GenerateExpression(ret.ReturnValue, true);

                // dtors
                if (ret.Destructions != null)
                {
                    foreach (var dest in ret.Destructions)
                    {
                        GenerateExpression(dest, false);
                    }
                }

                PopStackTrace();
                builder.CreateRet(retval);
            }
            else if (currentFunction.ReturnTypeExpr != null)
            {
                var retVal = valueMap[currentFunction.ReturnTypeExpr];
                retVal = builder.CreateLoad(retVal, "");

                // dtors
                if (ret.Destructions != null)
                {
                    foreach (var dest in ret.Destructions)
                    {
                        GenerateExpression(dest, false);
                    }
                }

                PopStackTrace();
                builder.CreateRet(retVal);
            }
            else
            {
                // dtors
                if (ret.Destructions != null)
                {
                    foreach (var dest in ret.Destructions)
                    {
                        GenerateExpression(dest, false);
                    }
                }

                PopStackTrace();
                builder.CreateRetVoid();
            }
        }
    }
}
