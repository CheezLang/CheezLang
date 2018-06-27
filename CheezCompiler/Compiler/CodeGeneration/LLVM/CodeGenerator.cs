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

        private LLVMContextRef context;
        private Module module;
        private LLVMTargetDataRef targetData;

        // <arch><sub>-<vendor>-<sys>-<abi>
        // arch = x86_64, i386, arm, thumb, mips, etc.
        private string targetTriple;

        private Dictionary<CheezType, LLVMTypeRef> typeMap = new Dictionary<CheezType, LLVMTypeRef>();
        private Dictionary<object, LLVMValueRef> valueMap = new Dictionary<object, LLVMValueRef>();

        public bool GenerateCode(Workspace workspace, string targetFile)
        {
            try
            {
                this.workspace = workspace;
                this.targetFile = targetFile;

                //context = new Context();
                module = new Module("test-module");
                context = module.GetModuleContext();
                targetTriple = "i386-pc-win32";
                module.SetTarget(targetTriple);
                targetData = module.GetTargetData();
                

                // generate code
                {
                    GenerateTypes();

                    GenerateFunctions();

                    GenerateMainFunction();
                }
                
                // debug
                module.PrintToFile(targetFile + ".ll");

                // emit machine code to object file
                TargetExt.InitializeX86Target();
                {
                    var targetMachine = TargetMachineExt.FromTriple(targetTriple);
                    var objFile = targetFile + ".obj";
                    var dir = Path.GetDirectoryName(Path.GetFullPath(targetFile));
                    objFile = Path.Combine(dir, Path.GetFileName(objFile));

                    
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
            return Linker.Link(workspace, targetFile, libraryIncludeDirectories, libraries, subsystem, errorHandler);
        }

        ///////////////////////////////////////

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
                case StringType _:
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

                case StringType _:
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

            for (uint i = 0; i < function.Parameters.Count; i++)
            {
                var param = function.Parameters[(int)i];
                var llvmParam = lfunc.GetParam(i);

                if (!CanPassByValue(param.Type))
                {
                    //var att = LLVM.CreateStringAttribute(context, "sret", 4, "", 0);
                    var att = LLVM.CreateEnumAttribute(context, AttributeKind.ByVal.ToUInt(), 0);
                    LLVM.AddAttributeAtIndex(lfunc, (LLVMAttributeIndex)(i + 1), att);

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

            if (function.Body == null)
                return lfunc;

            // generate body
            {
                var builder = new IRBuilder();
                
                var bbParams = lfunc.AppendBasicBlock("params");
                var bbLocals = lfunc.AppendBasicBlock("locals");
                var bbTemps = lfunc.AppendBasicBlock("temps");
                var bbBody = lfunc.AppendBasicBlock("body");

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
                foreach (var l in function.LocalVariables)
                {
                    valueMap[l] = builder.CreateAlloca(CheezTypeToLLVMType(l.Type), l.Name?.Name ?? "");
                }
                builder.CreateBr(bbTemps);

                // temp values
                builder.PositionBuilderAtEnd(bbTemps);
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
                    var next = bb.GetNextBasicBlock();
                    bb.RemoveBasicBlockFromParent();
                    bb = next;
                    continue;
                }

                bb = bb.GetNextBasicBlock();
            }

            //
            if (lfunc.VerifyFunction(LLVMVerifierFailureAction.LLVMPrintMessageAction))
            {
                Console.Error.WriteLine($"in function {lfunc}");
            }

            return lfunc;
        }
    }
}
