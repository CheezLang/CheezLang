using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
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
            var varargs = function.GetDirective("varargs");
            if (varargs != null)
            {
                function.FunctionType.VarArgs = true;
            }

            var name = function.Name.Name;
            if (function.IsPolyInstance)
            {
                name += ".";
                name += string.Join(".", function.PolymorphicTypes.Select(p => $"{p.Key}.{p.Value}"));
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

        [DebuggerStepThrough]
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
                
                var bbParams = lfunc.AppendBasicBlock("params");
                var bbLocals = lfunc.AppendBasicBlock("locals");
                //var bbTemps = lfunc.AppendBasicBlock("temps");
                var bbBody = lfunc.AppendBasicBlock("body");

                currentTempBasicBlock = bbLocals;

                // allocate space for parameters and return values on stack
                builder.PositionBuilderAtEnd(bbParams);
                for (int i = 0; i < function.Parameters.Count; i++)
                {
                    var param = function.Parameters[i];
                    var p = lfunc.GetParam((uint)i);
                    var ptype = LLVM.TypeOf(p);
                    p = builder.CreateAlloca(ptype, $"p_{param.Name.Name}");
                    valueMap[param] = p;
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

                builder.CreateBr(bbLocals);

                // allocate space for local variables
                builder.PositionBuilderAtEnd(bbLocals);
                //foreach (var l in function.LocalVariables)
                //{
                //    valueMap[l] = builder.CreateAlloca(CheezTypeToLLVMType(l.Type), l.Name?.Name ?? "");
                //}
                //builder.CreateBr(bbTemps);

                // temp values
                //builder.PositionBuilderAtEnd(bbTemps);
                builder.CreateBr(bbBody);

                // body
                builder.PositionBuilderAtEnd(bbBody);
                GenerateBlockStmt(function.Body);

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

        private void GenerateBlockStmt(AstBlockStmt block)
        {
            foreach (var s in block.Statements)
            {
                GenerateStatement(s);
            }

            for (int i = block.DeferredStatements.Count - 1; i >= 0; i--)
            {
                GenerateStatement(block.DeferredStatements[i]);
            }
        }

        private void GenerateStatement(AstStatement stmt)
        {
            switch (stmt)
            {
                case AstReturnStmt ret: GenerateReturnStatement(ret); break;
                default: throw new NotImplementedException();
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

                var uiae = LLVM.TypeOf(varPtr);

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
                    var val = GenerateExpression(varPtr, v.Initializer, varPtr, true);
                }
            }

            visited.Add(decl);
        }

        public LLVMValueRef VisitVariableDecl(AstVariableDecl variable)
        {
            // TODO
            return default;
            if (variable.IsConstant)
                return default;

            //if (variable.GetFlag(StmtFlags.GlobalScope))
            //    {
            //    throw new NotImplementedException();
            //    }
            //    else
            //    {
            //    var ptr = CreateLocalVariable(variable);
            //    if (variable.Initializer != null)
            //    {

            //        var val = variable.Initializer.Accept(this);
            //        CastIfAny(variable.Type, variable.Initializer.Type, ref val);
            //        return builder.CreateStore(val, ptr);
            //    }
            //    return default;
            //}
        }

        public void GenerateReturnStatement(AstReturnStmt ret)
        {
            if (ret.ReturnValue != null)
            {
                var return_var = valueMap[currentFunction.ReturnValue];
                var retval = GenerateExpression(ret.ReturnValue, return_var, false);
                if (retval != null)
                {
                    builder.CreateStore(retval.Value, return_var);
                }

                retval = builder.CreateLoad(return_var, "");
                builder.CreateRet(retval.Value);
            }
            else
            {
                builder.CreateRetVoid();
            }
        }
    }
}
