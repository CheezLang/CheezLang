using System;
using System.Runtime.InteropServices;
using System.Text;

namespace LLVMCS
{
    public class Module
    {
        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void* llvm_create_module(string name, void* contextPtr);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void llvm_delete_module(void* ptr);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void llvm_module_set_target_triple(void* ptr, string name);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void llvm_module_get_target_triple(void* ptr, sbyte** data, int* size);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static bool llvm_module_print_to_file(void* ptr, string path);

        unsafe internal void* instance;
        public Context Context { get; private set; }

        public string Name { get; }

        public Module(string name)
        {
            this.Name = name;
            Context = new Context();

            unsafe
            {
                instance = llvm_create_module(name, Context.instance);
            }
        }

        public Module(string name, Context context)
        {
            this.Name = name;
            this.Context = context;
            unsafe
            {
                instance = llvm_create_module(name, context.instance);
            }
        }

        ~Module()
        {
            Dispose();
            Context.Dispose();
        }

        public void Dispose()
        {
            unsafe
            {
                if (instance != null)
                {
                    llvm_delete_module(instance);
                }
                instance = null;
            }
        }

        public void SetTargetTriple(string targetTriple)
        {
            unsafe
            {
                llvm_module_set_target_triple(instance, targetTriple);
            }
        }

        public string GetTargetTriple()
        {
            unsafe
            {
                sbyte* data = null;
                int length = 0;
                llvm_module_get_target_triple(instance, &data, &length);
                
                var targetTriple = new string(data, 0, length, Encoding.ASCII);
                return targetTriple;
            }
        }

        public void PrintToFile(string filename)
        {
            unsafe
            {
                if (!llvm_module_print_to_file(instance, filename))
                {
                    throw new Exception($"Failed to print module '{Name}' to file '{filename}'");
                }
            }
        }

        public ValueRef AddFunction(string v, TypeRef ltype)
        {
            throw new NotImplementedException();
        }

        public ValueRef AddGlobal(TypeRef type, string name)
        {
            throw new NotImplementedException();
        }
    }
}
