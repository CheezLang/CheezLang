using LLVMSharp;
using System;
using System.Runtime.InteropServices;

namespace Cheez.CodeGeneration.LLVMCodeGen
{
    public static class BuilderExt
    {
        public static LLVMValueRef CallIntrinsic(this IRBuilder self, LLVMValueRef intr, params LLVMValueRef[] args)
        {
            return self.CreateCall(intr, args, "");
        }
    }

    public static class BasicBlockExt
    {
        public static bool IsNull(this LLVMBasicBlockRef self)
        {
            return self.Pointer.ToInt64() == 0;
        }
    }

    public static class TypeExt
    {
        public static LLVMTypeRef GetPointerTo(this LLVMTypeRef self)
        {
            return LLVM.PointerType(self, 0);
        }
    }

    public static class ValueExt
    {
        public static void AddFunctionAttribute(this LLVMValueRef self, LLVMContextRef context, LLVMAttributeKind kind, uint value = 0)
        {
            var att = LLVM.CreateEnumAttribute(context, kind.ToUInt(), value);
            LLVM.AddAttributeAtIndex(self, LLVMAttributeIndex.LLVMAttributeFunctionIndex, att);
        }

        public static void AddFunctionReturnAttribute(this LLVMValueRef self, LLVMContextRef context, LLVMAttributeKind kind, uint value = 0)
        {
            var att = LLVM.CreateEnumAttribute(context, kind.ToUInt(), value);
            LLVM.AddAttributeAtIndex(self, LLVMAttributeIndex.LLVMAttributeReturnIndex, att);
        }

        public static void AddFunctionParamAttribute(this LLVMValueRef self, LLVMContextRef context, int param, LLVMAttributeKind kind, uint value = 0)
        {
            var att = LLVM.CreateEnumAttribute(context, kind.ToUInt(), value);
            LLVM.AddAttributeAtIndex(self, (LLVMAttributeIndex)(param + 1), att);
        }

        public static void AddCallAttribute(this LLVMValueRef self, LLVMContextRef context, int arg, LLVMAttributeKind kind, uint value = 0)
        {
            var att = LLVM.CreateEnumAttribute(context, kind.ToUInt(), value);
            LLVM.AddCallSiteAttribute(self, (LLVMAttributeIndex)(arg + 1), att);
        }
    }

    public static class ModuleExt
    {
        public static LLVMModuleRef GetModuleRef(this Module self)
        {
            var type = typeof(Module);
            var field = type.GetField("instance", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var val = field.GetValue(self);
            return (LLVMModuleRef)val;
        }

        public static void PrintToFile(this Module self, string filename)
        {
            if (LLVM.PrintModuleToFile(self.GetModuleRef(), filename, out string errors))
            {
                throw new Exception($"Failed to print module '{self.GetIdentifier()}' to file '{filename}': {errors}");
            }
        }

        public static string GetIdentifier(this Module self)
        {
            var id = LLVM.GetModuleIdentifier(self.GetModuleRef(), out var len);
            return id;
        }

        public static LLVMTargetDataRef GetTargetData(this Module self)
        {
            var data = LLVM.GetModuleDataLayout(self.GetModuleRef());
            return data;
        }

        public static bool VerifyModule(this Module self, LLVMVerifierFailureAction Action, out string OutMessage)
        {
            return LLVM.VerifyModule(self.GetModuleRef(), LLVMVerifierFailureAction.LLVMPrintMessageAction, out OutMessage);
        }
    }

    public static class TargetDataExt
    {
        public static ulong SizeOfTypeInBits(this LLVMTargetDataRef self, LLVMTypeRef type)
        {
            return LLVM.SizeOfTypeInBits(self, type);
        }


        public static uint AlignmentOfType(this LLVMTargetDataRef self, LLVMTypeRef type)
        {
            return LLVM.ABIAlignmentOfType(self, type);
        }
    }

    public static class TargetExt
    {
        public static LLVMTargetRef FromTriple(string triple)
        {
            LLVMTargetRef res;
            if (LLVM.GetTargetFromTriple(triple, out res, out string error))
            {
                throw new Exception($"Failed to create target from triple '{triple}': {error}");
            }

            return res;
        }

        public static void InitializeX86Target()
        {
            try
            {
                LLVM.InitializeX86TargetMC();
                LLVM.InitializeX86Target();
                LLVM.InitializeX86TargetInfo();
                LLVM.InitializeX86AsmParser();
                LLVM.InitializeX86AsmPrinter();
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to initialize X86 target: {e.Message}");
            }
        }
    }

    public static class TargetMachineExt
    {
        public static LLVMTargetMachineRef FromTriple(string triple)
        {
            var res = LLVM.CreateTargetMachine(
                TargetExt.FromTriple(triple),
                triple,
                "generic",
                "",
                LLVMCodeGenOptLevel.LLVMCodeGenLevelNone,
                LLVMRelocMode.LLVMRelocDefault,
                LLVMCodeModel.LLVMCodeModelDefault
                );

            return res;
        }

        public static void EmitToFile(this LLVMTargetMachineRef self, Module module, string filename)
        {
            try
            {
                var filenameMarshaled = Marshal.StringToCoTaskMemAnsi(filename);
                if (LLVM.TargetMachineEmitToFile(
                    self, module.GetModuleRef(), filenameMarshaled, LLVMCodeGenFileType.LLVMObjectFile, out string error))
                {
                    throw new Exception(error);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to emit module '{module}' to file '{filename}': {e.Message}");
            }
        }

        public static void Dispose(this LLVMTargetMachineRef self)
        {
            LLVM.DisposeTargetMachine(self);
        }
    }
}
