using System.Runtime.InteropServices;

namespace LLVMCS
{
    public static class Linker
    {
        [DllImport("LLVMLinker", CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        public extern static bool llvm_link_coff(string[] argv, int argc);

        [DllImport("LLVMLinker", CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        public extern static bool llvm_link_elf(string[] argv, int argc);
    }
}
