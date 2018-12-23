using Cheez.Compiler.Ast;
using Cheez.Compiler.Visitor;
using LLVMSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Cheez.Compiler.CodeGeneration.LLVMCodeGen
{
    public class LLVMCodeGeneratorNew : VisitorBase<LLVMValueRef, object>, ICodeGenerator
    {
        private Workspace workspace;
        private string targetFile;
        private string intDir;
        private string outDir;

        private bool emitDebugInfo = false;

        private LLVMContextRef context;
        private Module module;
        private LLVMTargetDataRef targetData;

        // <arch><sub>-<vendor>-<sys>-<abi>
        // arch = x86_64, i386, arm, thumb, mips, etc.
        private string targetTriple;

        private Dictionary<CheezType, LLVMTypeRef> typeMap = new Dictionary<CheezType, LLVMTypeRef>();
        private Dictionary<object, LLVMValueRef> valueMap = new Dictionary<object, LLVMValueRef>();

        // intrinsics
        private LLVMValueRef memcpy32;
        private LLVMValueRef memcpy64;

        //
        private LLVMTypeRef pointerType;

        // context
        private AstFunctionDecl currentFunction;
        private LLVMValueRef currentLLVMFunction;
        private IRBuilder builder;
        private Dictionary<object, LLVMValueRef> returnValuePointer = new Dictionary<object, LLVMValueRef>();
        private LLVMBasicBlockRef currentTempBasicBlock;

        private void RunOptimizations(uint level)
        {
            module.PrintToFile(Path.Combine(intDir, targetFile + ".debug.ll"));

            var pmBuilder = LLVM.PassManagerBuilderCreate();
            LLVM.PassManagerBuilderSetOptLevel(pmBuilder, level);

            var funcPM = module.CreateFunctionPassManagerForModule();
            LLVM.PassManagerBuilderPopulateFunctionPassManager(pmBuilder, funcPM);
            bool r = LLVM.InitializeFunctionPassManager(funcPM);

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
            r = LLVM.FinalizeFunctionPassManager(funcPM);

            Console.WriteLine($"[LLVM] {modifiedFunctions} functions where modified during optimization.");

            var modPM = LLVM.CreatePassManager();
            LLVM.PassManagerBuilderPopulateModulePassManager(pmBuilder, modPM);
            r = LLVM.RunPassManager(modPM, module.GetModuleRef());
            if (!r) Console.WriteLine($"[LLVM] Module was not modified during optimization.");


            // verify module
            {
                module.VerifyModule(LLVMVerifierFailureAction.LLVMPrintMessageAction, out string llvmErrors);
                if (!string.IsNullOrWhiteSpace(llvmErrors))
                    Console.Error.WriteLine($"[LLVM-validate-module] {llvmErrors}");
            }
        }

        private void RunOptimizationsCustom()
        {
            module.PrintToFile(Path.Combine(intDir, targetFile + ".debug.ll"));

            var funcPM = module.CreateFunctionPassManagerForModule();
            LLVM.AddCFGSimplificationPass(funcPM);

            bool r = false;
            //bool r = LLVM.InitializeFunctionPassManager(funcPM); // needed?

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
            r = LLVM.FinalizeFunctionPassManager(funcPM);

            Console.WriteLine($"[LLVM] {modifiedFunctions} functions where modified during optimization.");

            var modPM = LLVM.CreatePassManager();
            r = LLVM.RunPassManager(modPM, module.GetModuleRef());
            if (!r) Console.WriteLine($"[LLVM] Module was not modified during optimization.");

            // verify module
            {
                module.VerifyModule(LLVMVerifierFailureAction.LLVMPrintMessageAction, out string llvmErrors);
                if (!string.IsNullOrWhiteSpace(llvmErrors))
                    Console.Error.WriteLine($"[LLVM-validate-module] {llvmErrors}");
            }
        }

        public bool GenerateCode(Workspace workspace, string intDir, string outDir, string targetFile, bool optimize)
        {
            try
            {
                this.workspace = workspace;
                this.intDir = intDir ?? "";
                this.outDir = outDir ?? "";
                this.targetFile = targetFile;
                this.emitDebugInfo = !optimize;

                module = new Module("test-module");
                context = module.GetModuleContext();
                targetTriple = "i386-pc-win32";
                module.SetTarget(targetTriple);
                targetData = module.GetTargetData();

                pointerType = LLVM.PointerType(LLVM.Int8Type(), 0);
                // generate code
                {
                    GenerateTypes();

                    GenerateIntrinsicDeclarations();

                    GenerateFunctions();

                    GenerateMainFunction();
                }
                
                // generate int dir
                if (!string.IsNullOrWhiteSpace(intDir) && !Directory.Exists(intDir))
                    Directory.CreateDirectory(intDir);

                // run optimizations
                if (optimize)
                {
                    Console.WriteLine("[LLVM] Running optimizations...");
                    RunOptimizations(3);
                    //RunOptimizationsCustom();
                    Console.WriteLine("[LLVM] Done.");
                }



                // create .ll file
                module.PrintToFile(Path.Combine(intDir, targetFile + ".ll"));

                // emit machine code to object file
                TargetExt.InitializeX86Target();
                {
                    var objFile = Path.Combine(intDir, targetFile + ".obj");

                    var targetMachine = TargetMachineExt.FromTriple(targetTriple);
                    targetMachine.EmitToFile(module, objFile);

                    targetMachine.Dispose();
                }

                module.DisposeModule();

                return true;
            }
            catch (Exception e)
            {
                workspace.ReportError(e.Message);
                return false;
            }
        }

        public bool CompileCode(IEnumerable<string> libraryIncludeDirectories, IEnumerable<string> libraries, string subsystem, IErrorHandler errorHandler)
        {
            if (!string.IsNullOrWhiteSpace(outDir) && !Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);

            string objFile = Path.Combine(intDir, targetFile + ".obj");
            string exeFile = Path.Combine(outDir, targetFile);

            return LLVMLinker.Link(workspace, exeFile, objFile, libraryIncludeDirectories, libraries, subsystem, errorHandler);
        }

        ///////////////////////////////////////
        private void GenerateIntrinsicDeclarations()
        {
            memcpy32 = GenerateIntrinsicDeclaration("llvm.memcpy.p0i8.p0i8.i32", LLVM.VoidType(),
                LLVM.PointerType(LLVM.Int8Type(), 0),
                LLVM.PointerType(LLVM.Int8Type(), 0),
                LLVM.Int32Type(),
                LLVM.Int1Type());

            memcpy64 = GenerateIntrinsicDeclaration("llvm.memcpy.p0i8.p0i8.i64", LLVM.VoidType(),
                LLVM.PointerType(LLVM.Int8Type(), 0),
                LLVM.PointerType(LLVM.Int8Type(), 0),
                LLVM.Int64Type(),
                LLVM.Int1Type());
        }

        private LLVMValueRef GenerateIntrinsicDeclaration(string name, LLVMTypeRef retType, params LLVMTypeRef[] paramTypes)
        {
            var ltype = LLVM.FunctionType(retType, paramTypes, false);
            var lfunc = module.AddFunction(name, ltype);
            return lfunc;
        }

        private void GenerateMainFunction()
        {
            var ltype = LLVM.FunctionType(LLVM.Int32Type(), new LLVMTypeRef[0], false);
            var lfunc = module.AddFunction("main", ltype);
            var entry = lfunc.AppendBasicBlock("entry");

            var builder = new IRBuilder(context);
            builder.PositionBuilderAtEnd(entry);

            { // call main function
                var cheezMain = valueMap[workspace.MainFunction];
                if (workspace.MainFunction.ReturnType == VoidType.Intance)
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
        }

        private void GenerateTypes()
        {
            foreach (var t in workspace.GlobalScope.TypeDeclarations)
            {
                if (t is AstStructDecl s)
                {
                    var llvmType = LLVM.StructCreateNamed(context, $"struct.{s.Name.Name}");
                    var memTypes = s.Members.Select(m => CheezTypeToLLVMType(m.Type)).ToArray();
                    llvmType.StructSetBody(memTypes, false);
                    typeMap[s.Type] = llvmType;
                }
            }
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

        private bool CanPassByValue(CheezType ct)
        {
            switch (ct)
            {
                case IntType _:
                case FloatType _:
                case PointerType _:
                case BoolType _:
                case CStringType _:
                case VoidType _:
                    return true;

                default:
                    return false;
            }
        }

        private LLVMTypeRef ParamTypeToLLVMType(CheezType ct)
        {
            var t = CheezTypeToLLVMType(ct);
            if (!CanPassByValue(ct))
                t = LLVM.PointerType(t, 0);
            return t;
        }

        private LLVMTypeRef CheezTypeToLLVMType(CheezType ct, bool functionPointer = true)
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
                    if (p.TargetType == VoidType.Intance)
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
                        if (!CanPassByValue(f.ReturnType) && !(f.ReturnType is VoidType))
                        {
                            paramTypes.Add(LLVM.PointerType(returnType, 0));
                            returnType = LLVM.VoidType();
                        }
                        foreach (var p in f.ParameterTypes)
                        {
                            var pt = CheezTypeToLLVMType(p);
                            if (!CanPassByValue(p))
                                pt = LLVM.PointerType(pt, 0);

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
                        return typeMap[s];
                        //var memTypes = s.Declaration.Members.Select(m => CheezTypeToLLVMType(m.Type)).ToArray();
                        //return LLVM.StructType(memTypes, false);
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        private LLVMValueRef GetTempValue(CheezType exprType)
        {
            var builder = LLVM.CreateBuilder();
            
            var brInst = currentTempBasicBlock.GetLastInstruction();
            LLVM.PositionBuilderBefore(builder, brInst);

            var type = CheezTypeToLLVMType(exprType);
            var result = LLVM.BuildAlloca(builder, type, "");

            LLVM.DisposeBuilder(builder);

            return result;
        }
        /////////////////////////////////////////////////////////////



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

            var ltype = CheezTypeToLLVMType(function.Type, false);
            var lfunc = module.AddFunction(name, ltype);

            lfunc.AddFunctionAttribute(context, AttributeKind.NoInline);
            lfunc.AddFunctionAttribute(context, AttributeKind.NoUnwind);
            //lfunc.AddFunctionAttribute(context, AttributeKind.OptimizeNone);

            int paramOffset = 0;

            if (!CanPassByValue(function.ReturnType))
            {
                lfunc.AddFunctionParamAttribute(context, 0, AttributeKind.StructRet);
                paramOffset = 1;
                returnValuePointer[function] = lfunc.GetParam(0);
            }
            
            for (int i = 0; i < function.Parameters.Count; i++)
            {
                var param = function.Parameters[i];
                var llvmParam = lfunc.GetParam((uint)(i + paramOffset));

                if (!CanPassByValue(param.Type))
                {
                    //var att = LLVM.CreateStringAttribute(context, "sret", 4, "", 0);
                    lfunc.AddFunctionParamAttribute(context, i + paramOffset, AttributeKind.ByVal);
                }
            }

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

        public override LLVMValueRef VisitFunctionDeclaration(AstFunctionDecl function, object data = null)
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

                // allocate space for parameters on stack
                builder.PositionBuilderAtEnd(bbParams);
                for (int i = 0; i < function.Parameters.Count; i++)
                {
                    var param = function.Parameters[i];
                    var p = lfunc.GetParam((uint)i);
                    p = builder.CreateAlloca(ParamTypeToLLVMType(param.Type), $"p_{param.Name}");
                    valueMap[param] = p;
                }
                // store params in local variables
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
                function.Body.Accept(this);

                // ret if void
                if (function.ReturnType == VoidType.Intance)
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

        public override LLVMValueRef VisitBlockStatement(AstBlockStmt block, object data = null)
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

        public override LLVMValueRef VisitVariableDeclaration(AstVariableDecl variable, object data = null)
        {
            if (variable.IsConstant)
                return default;

            if (variable.GetFlag(StmtFlags.GlobalScope))
                {
                throw new NotImplementedException();
                }
                else
                {
                var ptr = CreateLocalVariable(variable);
                if (variable.Initializer != null)
                {

                    var val = variable.Initializer.Accept(this);
                    CastIfAny(variable.Type, variable.Initializer.Type, ref val);
                    return builder.CreateStore(val, ptr);
                }
                return default;
            }
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

        public override LLVMValueRef VisitNumberExpression(AstNumberExpr num, object data = null)
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

        #region Helper
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

            var bb = currentLLVMFunction.GetFirstBasicBlock();
            var brInst = bb.GetLastInstruction();
            LLVM.PositionBuilderBefore(builder, brInst);

            var type = CheezTypeToLLVMType(exprType);
            var result = LLVM.BuildAlloca(builder, type, "");
            var alignment = targetData.AlignmentOfType(type);
            LLVM.SetAlignment(result, alignment);

            LLVM.DisposeBuilder(builder);

            return result;
        }

        private void CastIfAny(CheezType targetType, CheezType sourceType, ref LLVMValueRef value)
        {
            if (targetType == CheezType.Any && sourceType != CheezType.Any)
            {
                var type = CheezTypeToLLVMType(targetType);
                if (sourceType is IntType)
                    value = builder.CreateIntCast(value, type, "");
                else if (sourceType is BoolType)
                    value = builder.CreateZExtOrBitCast(value, type, "");
                else if (sourceType is PointerType || sourceType is CStringType || sourceType is ArrayType)
                    value = builder.CreatePtrToInt(value, type, "");
                else
                    throw new NotImplementedException("any cast");
            }
        }
        #endregion
    }
}
