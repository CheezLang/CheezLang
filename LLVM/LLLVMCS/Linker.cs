using System.Runtime.InteropServices;

namespace LLVMCS
{
    public static class Linker
    {
        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        public extern static bool llvm_link_coff(string[] argv, int argc);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        public extern static bool llvm_link_elf(string[] argv, int argc);
    }
}
