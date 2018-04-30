using LLVMSharp;
using System;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //AustinHarris.JsonRpc.se
        }

        static int Add(int a, int b)
        {
            LLVMModuleRef module = LLVM.ModuleCreateWithName("test-module");
            LLVMBuilderRef builder = LLVM.CreateBuilder();

            var funcType = LLVM.FunctionType(LLVM.Int32Type(), new LLVMTypeRef[] { LLVM.Int32Type(), LLVM.Int32Type() }, false);
            var func = LLVM.AddFunction(module, "iadd", funcType);
            LLVM.PositionBuilderAtEnd(builder, LLVM.AppendBasicBlock(func, "entry"));
            
            var left = func.GetParam(0);
            var right = func.GetParam(1);
            var sum = LLVM.BuildAdd(builder, left, right, "result");

            LLVM.BuildRet(builder, sum);
            LLVM.VerifyFunction(func, LLVMVerifierFailureAction.LLVMPrintMessageAction);

            LLVMExecutionEngineRef exe;
            if (LLVM.CreateExecutionEngineForModule(out exe, module, out string error).Value != 0)
            {
                throw new Exception(error);
            }
            var args = new LLVMGenericValueRef[2];
            args[0] = LLVM.CreateGenericValueOfInt(LLVM.Int32Type(), (ulong)a, new LLVMBool(1));
            args[1] = LLVM.CreateGenericValueOfInt(LLVM.Int32Type(), (ulong)b, new LLVMBool(1));
            var result = LLVM.RunFunction(exe, func, args);
            return (int)LLVM.GenericValueToInt(result, new LLVMBool(1));

            
        }
    }
}
