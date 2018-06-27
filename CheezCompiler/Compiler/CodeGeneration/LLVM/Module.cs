using LLVMSharp;
using System;

namespace Cheez.Compiler.CodeGeneration.LLVMCodeGen
{
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
    }
}
