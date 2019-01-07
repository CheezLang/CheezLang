using System.Runtime.InteropServices;

namespace LLVMCS
{
    internal static class DLL
    {
        internal const string LLVM_DLL_NAME = "LLVMWrapper";
        internal const CallingConvention LLVM_DLL_CALLING_CONVENTION = CallingConvention.Cdecl;
        internal const CharSet LLVM_DLL_CHAR_SET = CharSet.Ansi;
    }
}
