using System.Runtime.InteropServices;

namespace LLVMCS
{
    public struct Target
    {
        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void* llvm_get_target(string tt);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void llvm_initialize_all_targets();

        unsafe internal void* instance;

        unsafe public Target(void* i)
        {
            instance = i;
        }

        public static Target FromTargetTriple(string tt)
        {
            unsafe { return new Target(llvm_get_target(tt)); }
        }

        public static void InitializeAll()
        {
            unsafe { llvm_initialize_all_targets(); }
        }
    }

    public struct TargetMachine
    {
        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void* llvm_create_target_machine(void* target, string tt, string cpu, string features);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void llvm_delete_target_machine(void* targetMachine);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static bool llvm_emit_object_code(void* targetMachine, void* module, string filename);

        unsafe internal void* instance;

        public TargetMachine(Target target, string targetTriple, string cpu = "generic", string features = "")
        {
            unsafe
            {
                instance = null;
                instance = llvm_create_target_machine(target.instance, targetTriple, cpu, features);
            }
        }

        public void Dispose()
        {
            unsafe
            {
                if (instance != null)
                    llvm_delete_target_machine(instance);
                instance = null;
            }
        }

        public bool EmitModule(Module module, string filename)
        {
            unsafe
            {
                return llvm_emit_object_code(instance, module.instance, filename);
            }
        }
    }
}
