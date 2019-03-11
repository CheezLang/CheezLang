using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Types;
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
        private void GenerateFunctions()
        {
            // create declarations
            foreach (var function in workspace.GlobalScope.FunctionDeclarations)
            {
                GenerateFunctionHeader(function);
            }

            // create implementations
            foreach (var f in workspace.GlobalScope.FunctionDeclarations)
            {
                GenerateFunctionImplementation(f);
            }
        }

        private void GenerateFunctionHeader(AstFunctionDecl function)
        {
            var name = function.Name.Name;
            if (function.PolymorphicTypes != null && function.PolymorphicTypes.Count > 0)
                name += "." + string.Join(".", function.PolymorphicTypes.Select(p => $"{p.Key}.{p.Value}"));
            if (function.ConstParameters != null && function.ConstParameters.Count > 0)
                name += "." + string.Join(".", function.ConstParameters.Select(p => $"{p.Key}.{p.Value.type}.{p.Value.value}"));

            var linkname = function.GetDirective("linkname");
            if (linkname != null)
            {
                name = linkname.Arguments[0].Value as string;
            }

            var ltype = CheezTypeToLLVMType(function.Type);
            var lfunc = module.AddFunction(name, ltype);

            // TODO
            lfunc.AddFunctionAttribute(context, LLVMAttributeKind.NoInline);
            lfunc.AddFunctionAttribute(context, LLVMAttributeKind.NoUnwind);

            var ccDir = function.GetDirective("stdcall");
            if (ccDir != null)
            {
                LLVM.SetFunctionCallConv(lfunc, (int)LLVMCallConv.LLVMX86StdcallCallConv);
            }

            valueMap[function] = lfunc;
        }

        private void GenerateFunctionImplementation(AstFunctionDecl function)
        {
            if (function.Body == null)
                return;

            currentFunction = function;

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
                    if (c.Value is ConstSymbol s && s.Type != CheezType.Type)
                    {
                        var val = CheezValueToLLVMValue(s.Type, s.Value);
                        var cnst = builder.CreateAlloca(CheezTypeToLLVMType(s.Type), $"c_");
                        builder.CreateStore(val, cnst);
                        valueMap[s] = cnst;
                    }
                }

                if (function.ReturnValue != null)
                {
                    var ptype = CheezTypeToLLVMType(function.ReturnValue.Type);
                    var p = builder.CreateAlloca(ptype, $"ret_{function.ReturnValue.Name?.Name}");
                    valueMap[function.ReturnValue] = p;
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
                GenerateExpression(function.Body, null, false);

                // ret if void
                if (function.ReturnValue == null)
                    builder.CreateRetVoid();
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
                default: throw new NotImplementedException();
            }

        }

        private void GenerateWhile(AstWhileStmt whl)
        {
            if (whl.PreAction != null)
                GenerateStatement(whl.PreAction);

            var bbCond = LLVM.AppendBasicBlock(currentLLVMFunction, "_cond");
            var bbBody = LLVM.AppendBasicBlock(currentLLVMFunction, "_body");
            var bbEnd = LLVM.AppendBasicBlock(currentLLVMFunction, "_end");

            builder.CreateBr(bbCond);

            builder.PositionBuilderAtEnd(bbCond);
            var cond = GenerateExpressionHelper(whl.Condition, null, true);
            builder.CreateCondBr(cond.Value, bbBody, bbEnd);

            builder.PositionBuilderAtEnd(bbBody);
            GenerateExpression(whl.Body, null, false);

            if (whl.PostAction != null)
                GenerateStatement(whl.PostAction);
            builder.CreateBr(bbCond);

            builder.PositionBuilderAtEnd(bbEnd);
        }

        private void GenerateAssignment(AstAssignment ass)
        {
            if (ass.SubAssignments?.Count > 0)
            {
                foreach (var sub in ass.SubAssignments)
                {
                    GenerateAssignment(sub);
                }
                return;
            }

            var ptr = GenerateExpression(ass.Pattern, null, false);
            GenerateExpressionHelper(ass.Value, ptr, true);
        }

        private void GenerateExprStatement(AstExprStmt expr)
        {
            GenerateExpression(expr.Expr, null, false);
        }

        private void InitGlobalVariable(AstVariableDecl decl, HashSet<AstVariableDecl> visited)
        {
            if (visited.Contains(decl))
                return;

            if (decl.Dependencies != null)
            {
                foreach (var dep in decl.Dependencies)
                {
                    InitGlobalVariable(dep.VarDeclaration, visited);
                }
            }

            bool generateInitializer = false;

            // create vars
            foreach (var v in decl.SubDeclarations)
            {
                var type = CheezTypeToLLVMType(v.Type);

                var varPtr = module.AddGlobal(type, v.Name.Name);
                varPtr.SetLinkage(LLVMLinkage.LLVMInternalLinkage);
                varPtr.SetLinkage(LLVMLinkage.LLVMExternalLinkage);

                var dExtern = decl.GetDirective("extern");
                if (dExtern != null) varPtr.SetLinkage(LLVMLinkage.LLVMExternalLinkage);

                LLVMValueRef initializer = default;
                if (v.Value != null)
                    initializer = CheezValueToLLVMValue(v.Type, v.Value);
                else
                    initializer = GetDefaultLLVMValue(v.Type);

                varPtr.SetInitializer(initializer);
                valueMap[v] = varPtr;

                if (v.Value == null)
                    generateInitializer = true;
            }

            // do initialization TODO: other patterns
            if (decl.Initializer != null && generateInitializer)
            {
                // assign to single variables
                foreach (var v in decl.SubDeclarations)
                {
                    var varPtr = valueMap[v];
                    var val = GenerateExpressionHelper(v.Initializer, varPtr, true);
                }
            }

            visited.Add(decl);
        }

        public void GenerateVariableDecl(AstVariableDecl decl)
        {
            foreach (var v in decl.SubDeclarations)
            {
                var varPtr = CreateLocalVariable(v.Type, v.Name.Name);
                valueMap[v] = varPtr;

                if (v.Initializer != null)
                {
                    GenerateExpressionHelper(v.Initializer, varPtr, true);
                }
            }
        }

        public void GenerateReturnStatement(AstReturnStmt ret)
        {
            if (ret.ReturnValue != null)
            {
                var return_var = valueMap[currentFunction.ReturnValue];
                var retval = GenerateExpression(ret.ReturnValue, return_var, true);
                if (retval != null)
                {
                    builder.CreateStore(retval.Value, return_var);
                }

                retval = builder.CreateLoad(return_var, "");
                builder.CreateRet(retval.Value);
            }
            else if (currentFunction.ReturnValue != null)
            {
                var retVal = valueMap[currentFunction.ReturnValue];
                retVal = builder.CreateLoad(retVal, "");
                builder.CreateRet(retVal);
            }
            else
            {
                builder.CreateRetVoid();
            }
        }
    }
}
