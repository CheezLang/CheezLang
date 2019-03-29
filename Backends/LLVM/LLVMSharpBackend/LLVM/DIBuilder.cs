using LLVMSharp;
using System;
using System.Runtime.InteropServices;

namespace Cheez.CodeGeneration.LLVMCodeGen
{
    public class DIBuilder
    {
        [DllImport("LLVMWrapper", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static IntPtr dibuilder_create_file(IntPtr dibuilder, string filename, string directory);

        [DllImport("LLVMWrapper", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static IntPtr dibuilder_create_compile_unit(IntPtr dibuilder, IntPtr file, string producer, bool isOptimized);
        
        private LLVMDIBuilderRef builderRef;

        public DIBuilder(Module module)
        {
            builderRef = LLVM.NewDIBuilder(module.GetModuleRef());
        }

        public void FinalizeBuilder()
        {
            LLVM.DIBuilderFinalize(builderRef);
        }

        public LLVMMetadataRef CreateFile(string filename, string directory)
        {
            return new LLVMMetadataRef(dibuilder_create_file(builderRef.Pointer, filename, directory));
        }

        public LLVMMetadataRef CreateCompileUnit(LLVMMetadataRef file, string producer, bool isOptimized)
        {
            return new LLVMMetadataRef(dibuilder_create_compile_unit(builderRef.Pointer, file.Pointer, producer, isOptimized));
        }
    }
}
