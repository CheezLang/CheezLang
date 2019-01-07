using System;
using System.Runtime.InteropServices;

namespace LLVMCS
{
    public struct DataLayout : IDisposable
    {
        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void* llvm_data_layout_create_1(string layout);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void* llvm_create_data_layout_2(void* module);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void* llvm_delete_data_layout(void* dataLayout);

        internal unsafe void* instance;

        public DataLayout(string layout)
        {
            unsafe
            {
                instance = llvm_data_layout_create_1(layout);
            }
        }

        public DataLayout(Module module)
        {
            unsafe
            {
                instance = llvm_create_data_layout_2(module.instance);
            }
        }

        public void Dispose()
        {
            unsafe
            {
                if (instance != null)
                    llvm_delete_data_layout(instance);
                instance = null;
            }
        }

        public int GetAlignmentOf(TypeRef type)
        {
            throw new NotImplementedException();
        }
    }
}
