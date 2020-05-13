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

        [DllImport("LLVMWrapper", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static IntPtr dibuilder_new(IntPtr module);

        [DllImport("LLVMWrapper", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void dibuilder_delete(IntPtr dibuilder);

        [DllImport("LLVMWrapper", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void dibuilder_finalize(IntPtr dibuilder);

        private IntPtr builderRef;

        public DIBuilder(Module module)
        {
            builderRef = dibuilder_new(module.GetModuleRef().Pointer);
        }

        public void DisposeBuilder()
        {
            dibuilder_delete(builderRef);
        }

        public void FinalizeBuilder()
        {
            dibuilder_finalize(builderRef);
        }

        public LLVMMetadataRef CreateFile(string filename, string directory)
        {
            return new LLVMMetadataRef(dibuilder_create_file(builderRef, filename, directory));
        }

        public LLVMMetadataRef CreateCompileUnit(LLVMMetadataRef file, string producer, bool isOptimized)
        {
            return new LLVMMetadataRef(dibuilder_create_compile_unit(builderRef, file.Pointer, producer, isOptimized));
        }
    }
}
