using Cheez.Compiler.Ast;
using LLVMCS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Cheez.Compiler.CodeGeneration.LLVMCodeGen
{
    public partial class LLVMCodeGeneratorNew
    {
        private void GenerateTypes()
        {
            foreach (var t in workspace.GlobalScope.TypeDeclarations)
            {
                if (t is AstStructDecl s)
                {
                    var llvmType = context.GetNamedStruct($"struct.{s.Name.Name}");
                    typeMap[s.Type] = llvmType;
                }
            }

            foreach (var t in workspace.GlobalScope.TypeDeclarations)
            {
                if (t is AstStructDecl s)
                {
                    var llvmType = typeMap[s.Type];
                    var memTypes = s.Members.Select(m => CheezTypeToLLVMType(m.Type)).ToArray();
                    context.SetStructBody(llvmType, memTypes);
                }
            }
        }

        private void GenerateMainFunction()
        {
            var ltype = context.GetFunctionType(context.GetIntType(32));
            var lfunc = module.AddFunction("main", ltype);
            var entry = lfunc.AppendBasicBlock("entry");

            builder = new IRBuilder(context);
            builder.PositionAtEnd(entry);

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
                    builder.Call(cheezMain);
                    builder.Ret(LLVM.ConstUInt(context.GetIntType(32), 0));
                }
                else
                {
                    var exitCode = builder.Call("exitCode", cheezMain);
                    builder.Ret(exitCode);
                }
            }

            if (lfunc.VerifyFunction(LLVMVerifierFailureAction.LLVMPrintMessageAction))
            {
                Console.Error.WriteLine($"in function {lfunc}");
            }

            builder.Dispose();
        }

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

            lfunc.AddFunctionAttribute(context, AttributeKind.NoInline);
            lfunc.AddFunctionAttribute(context, AttributeKind.NoUnwind);

            var ccDir = function.GetDirective("stdcall");
            if (ccDir != null)
            {
                lfunc.SetCallConv(LLVMCallConv.LLVMX86StdcallCallConv);
            }

            valueMap[function] = lfunc;
        }

        [DebuggerStepThrough]
        private void GenerateFunctionImplementation(AstFunctionDecl function)
        {
            function.Accept(this);
        }

        public override ValueRef VisitFunctionDecl(AstFunctionDecl function, LLVMCodeGeneratorNewContext context = default)
        {
            var lfunc = valueMap[function];
            currentLLVMFunction = lfunc;

            if (function.Body == null)
                return lfunc;

            currentFunction = function;

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
                builder.PositionAtEnd(bbParams);
                for (int i = 0; i < function.Parameters.Count; i++)
                {
                    var param = function.Parameters[i];
                    var p = lfunc.GetParam(i);
                    var ptype = p.Type();
                    p = builder.Alloca(ptype, $"p_{param.Name.Name}");
                    valueMap[param] = p;
                }

                if (function.ReturnValue != null)
                {
                    var ptype = CheezTypeToLLVMType(function.ReturnValue.Type);
                    var p = builder.Alloca(ptype, $"ret_{function.ReturnValue.Name?.Name}");
                    valueMap[function.ReturnValue] = p;
                }

                // store params and rets in local variables
                for (int i = 0; i < function.Parameters.Count; i++)
                {
                    var param = function.Parameters[i];
                    var p = lfunc.GetParam(i);
                    builder.Store(p, valueMap[param]);
                }

                builder.Br(bbLocals);

                // allocate space for local variables
                builder.PositionAtEnd(bbLocals);
                //foreach (var l in function.LocalVariables)
                //{
                //    valueMap[l] = builder.CreateAlloca(CheezTypeToLLVMType(l.Type), l.Name?.Name ?? "");
                //}
                //builder.CreateBr(bbTemps);

                // temp values
                //builder.PositionBuilderAtEnd(bbTemps);
                builder.Br(bbBody);

                // body
                builder.PositionAtEnd(bbBody);
                function.Body.Accept(this);

                // ret if void
                if (function.ReturnValue == null)
                    builder.RetVoid();
                builder.Dispose();
            }

            // remove empty basic blocks
            
            var bb = lfunc.GetFirstBasicBlock();
            while (!bb.IsNull())
            {
                var first = bb.GetFirstInstruction();

                if (bb.GetTerminator().IsNull())
                {
                    var b = new IRBuilder();
                    b.PositionAtEnd(bb);
                    b.Unreachable();
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

            return lfunc;
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

            foreach (var v in decl.SubDeclarations)
            {
                // create var
                ValueRef varPtr;
                {
                    var type = CheezTypeToLLVMType(decl.Type);

                    varPtr = module.AddGlobal(type, v.Name.Name);
                    varPtr.SetLinkage(LLVMLinkage.LLVMInternalLinkage);

                    var dExtern = decl.GetDirective("extern");
                    if (dExtern != null) varPtr.SetLinkage(LLVMLinkage.LLVMExternalLinkage);

                    varPtr.SetInitializer(GetDefaultLLVMValue(decl.Type));
                    valueMap[v] = varPtr;
                }

                // do initialization
                //if (decl.Initializer != null)
                //{
                //    var val = decl.Initializer.Accept(this);
                //    builder.CreateStore(val, varPtr);
                //}
            }

            visited.Add(decl);
        }

        public override ValueRef VisitBlockStmt(AstBlockStmt block, LLVMCodeGeneratorNewContext data = default)
        {
            foreach (var s in block.Statements)
            {
                s.Accept(this);
            }

            for (int i = block.DeferredStatements.Count - 1; i >= 0; i--)
            {
                block.DeferredStatements[i].Accept(this);
            }

            return default;
        }

        public override ValueRef VisitVariableDecl(AstVariableDecl variable, LLVMCodeGeneratorNewContext data = default)
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

        //public override LLVMValueRef VisitReturnStatement(AstReturnStmt ret, object data = null)
        //{
        //    if (ret.ReturnValue != null)
        //    {
        //        var retval = ret.ReturnValue.Accept(this);

        //        if (CanPassByValue(ret.ReturnValue.Type))
        //        {
        //            return builder.CreateRet(retval);
        //        }
        //        else
        //        {
        //            var type = CheezTypeToLLVMType(ret.ReturnValue.Type);
        //            ulong size = targetData.SizeOfTypeInBits(type);
        //            var sizeInBytes = LLVM.ConstInt(LLVM.Int32Type(), size, false);
        //            var retPtr = returnValuePointer[currentFunction];

        //            var dst = builder.CreatePointerCast(retPtr, pointerType, "");
        //            var src = builder.CreatePointerCast(retval, pointerType, "");


        //            var call = builder.CallIntrinsic(memcpy32, dst, src, sizeInBytes, LLVM.ConstInt(LLVM.Int1Type(), 0, false));
        //            return builder.CreateRetVoid();
        //        }
        //    }
        //    else
        //    {
        //        return builder.CreateRetVoid();
        //    }
        //}

        //public override LLVMValueRef VisitStructValueExpression(AstStructValueExpr str, object data = null)
        //{
        //    var value = GetTempValue(str.Type);

        //    var llvmType = CheezTypeToLLVMType(str.Type);

        //    foreach (var m in str.MemberInitializers)
        //    {
        //        var v = m.Value.Accept(this, data);
        //        var memberPtr = builder.CreateStructGEP(value, (uint)m.Index, "");
        //        var s = builder.CreateStore(v, memberPtr);
        //    }

        //    return value;
        //}

        public override ValueRef VisitCharLiteralExpr(AstCharLiteral expr, LLVMCodeGeneratorNewContext data = default)
        {
            var ch = expr.CharValue;
            var val = LLVM.ConstInt(context.GetIntType(8), ch);
            return val;
        }

        public override ValueRef VisitStringLiteralExpr(AstStringLiteral expr, LLVMCodeGeneratorNewContext data = default)
        {
            throw new NotImplementedException();
        }

        public override ValueRef VisitNumberExpr(AstNumberExpr num, LLVMCodeGeneratorNewContext data = default)
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

        public override ValueRef VisitIdExpr(AstIdExpr expr, LLVMCodeGeneratorNewContext data = default)
        {
            var v = valueMap[expr.Symbol];
            return builder.Load(v);
        }
    }
}
