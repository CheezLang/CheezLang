using Cheez.Compiler.Ast;
using LLVMSharp;
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
                    var llvmType = LLVM.StructCreateNamed(context, $"struct.{s.Name.Name}");
                    typeMap[s.Type] = llvmType;
                }
            }

            foreach (var t in workspace.GlobalScope.TypeDeclarations)
            {
                if (t is AstStructDecl s)
                {
                    var llvmType = typeMap[s.Type];
                    var memTypes = s.Members.Select(m => CheezTypeToLLVMType(m.Type)).ToArray();
                    llvmType.StructSetBody(memTypes, false);
                }
            }
        }

        private void GenerateMainFunction()
        {
            var ltype = LLVM.FunctionType(LLVM.Int32Type(), new LLVMTypeRef[0], false);
            var lfunc = module.AddFunction("main", ltype);
            var entry = lfunc.AppendBasicBlock("entry");

            builder = new IRBuilder(context);
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
                if (workspace.MainFunction.ReturnValues.Count == 0)
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

            if (lfunc.VerifyFunction(LLVMVerifierFailureAction.LLVMPrintMessageAction))
            {
                Console.Error.WriteLine($"in function {lfunc}");
            }

            builder.Dispose();
            builder = null;
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
                LLVM.SetFunctionCallConv(lfunc, (uint)LLVMCallConv.LLVMX86StdcallCallConv);
            }

            valueMap[function] = lfunc;
        }

        [DebuggerStepThrough]
        private void GenerateFunctionImplementation(AstFunctionDecl function)
        {
            function.Accept(this);
        }

        public override LLVMValueRef VisitFunctionDecl(AstFunctionDecl function, LLVMCodeGeneratorNewContext context = default)
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
                builder.PositionBuilderAtEnd(bbParams);
                for (int i = 0; i < function.Parameters.Count; i++)
                {
                    var param = function.Parameters[i];
                    var p = lfunc.GetParam((uint)i);
                    var ptype = LLVM.TypeOf(p);
                    p = builder.CreateAlloca(ptype, $"p_{param.Name.Name}");
                    valueMap[param] = p;
                }

                if (function.ReturnValues.Count > 1)
                {
                    int offset = function.Parameters.Count;
                    for (int i = 0; i < function.ReturnValues.Count; i++)
                    {
                        var param = function.ReturnValues[i];
                        var p = lfunc.GetParam((uint)(i + offset));
                        var ptype = LLVM.TypeOf(p);
                        p = builder.CreateAlloca(ptype, $"ret_{param.Name?.Name ?? i.ToString()}");
                        valueMap[param] = p;
                    }
                }

                // store params and rets in local variables
                for (int i = 0; i < function.Parameters.Count; i++)
                {
                    var param = function.Parameters[i];
                    var p = lfunc.GetParam((uint)i);
                    builder.CreateStore(p, valueMap[param]);
                }

                if (function.ReturnValues.Count > 1)
                {
                    int offset = function.Parameters.Count;
                    for (int i = 0; i < function.ReturnValues.Count; i++)
                    {
                        var param = function.ReturnValues[i];
                        var p = lfunc.GetParam((uint)(i + offset));
                        builder.CreateStore(p, valueMap[param]);
                    }
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
                function.Body.Accept(this);

                // ret if void
                if (function.ReturnValues.Count == 0)
                    builder.CreateRetVoid();
                builder.Dispose();
            }

            // remove empty basic blocks
            var bb = LLVM.GetFirstBasicBlock(lfunc);
            while (bb.Pointer.ToInt64() != 0)
            {
                var first = bb.GetFirstInstruction();

                if (bb.GetBasicBlockTerminator().Pointer == IntPtr.Zero)
                {
                    var b = new IRBuilder();
                    b.PositionBuilderAtEnd(bb);
                    b.CreateUnreachable();
                    b.Dispose();
                }

                bb = bb.GetNextBasicBlock();
            }

            //
            if (lfunc.VerifyFunction(LLVMVerifierFailureAction.LLVMPrintMessageAction))
            {
                Console.Error.WriteLine($"in function {lfunc}");
            }

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
                LLVMValueRef varPtr;
                {
                    var type = CheezTypeToLLVMType(decl.Type);

                    varPtr = module.AddGlobal(type, v.Name.Name);
                    LLVM.SetLinkage(varPtr, LLVMLinkage.LLVMInternalLinkage);

                    var dExtern = decl.GetDirective("extern");
                    if (dExtern != null) LLVM.SetLinkage(varPtr, LLVMLinkage.LLVMExternalLinkage);

                    LLVM.SetInitializer(varPtr, GetDefaultLLVMValue(decl.Type));
                    valueMap[decl] = varPtr;
                }

                // do initialization
                if (decl.Initializer != null)
                {
                    var val = decl.Initializer.Accept(this);
                    builder.CreateStore(val, varPtr);
                }
            }

            visited.Add(decl);
        }

        public override LLVMValueRef VisitBlockStmt(AstBlockStmt block, LLVMCodeGeneratorNewContext context = default)
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

        public override LLVMValueRef VisitVariableDecl(AstVariableDecl variable, LLVMCodeGeneratorNewContext context = default)
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

        public override LLVMValueRef VisitCharLiteralExpr(AstCharLiteral expr, LLVMCodeGeneratorNewContext context = default)
        {
            var ch = expr.CharValue;
            var val = LLVM.ConstInt(LLVM.Int8Type(), ch, true);
            return val;
        }

        public override LLVMValueRef VisitStringLiteralExpr(AstStringLiteral expr, LLVMCodeGeneratorNewContext context = default)
        {
            throw new NotImplementedException();
        }

        public override LLVMValueRef VisitNumberExpr(AstNumberExpr num, LLVMCodeGeneratorNewContext context = default)
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

        public override LLVMValueRef VisitIdExpr(AstIdExpr expr, LLVMCodeGeneratorNewContext context = default)
        {
            var v = valueMap[expr.Symbol];
            return builder.CreateLoad(v, "");
        }
    }
}
