using LLVMSharp;
using System;
using System.Runtime.InteropServices;

namespace Cheez.Compiler.CodeGeneration.LLVMCodeGen
{
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
            var filenameMarshaled = Marshal.StringToCoTaskMemAnsi(filename);
            if (LLVM.TargetMachineEmitToFile(
                self, module.GetModuleRef(), filenameMarshaled, LLVMCodeGenFileType.LLVMObjectFile, out string error))
            {
                throw new Exception($"Failed to emit module '{module}' to file '{filename}': {error}");
            }
        }

        public static void Dispose(this LLVMTargetMachineRef self)
        {
            LLVM.DisposeTargetMachine(self);
        }
    }
}
