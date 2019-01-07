using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace LLVMCS
{
    public class Context : IDisposable
    {
        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void* llvm_create_context();

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void* llvm_delete_context(void* instance);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void* llvm_pointer_type(void* target);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void* llvm_void_type(void* context);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void* llvm_int_type(void* context, int sizeInBits);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void* llvm_float_type(void* context, int sizeInBits);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void* llvm_named_struct(void* context, string name);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void llvm_struct_set_body(void* @struct, void*[] types, int count, bool packed);

        internal unsafe void* instance;

        public Context()
        {
            unsafe
            {
                instance = llvm_create_context();
            }
        }

        ~Context()
        {
            Dispose();
        }

        public void Dispose()
        {
            unsafe
            {
                if (instance != null)
                {
                    llvm_delete_context(instance);
                }
                instance = null;
            }
        }

        // types
        public TypeRef GetIntType(int sizeInBits)
        {
            unsafe { return new TypeRef(llvm_int_type(instance, sizeInBits)); }
        }

        public TypeRef GetFloatType(int sizeInBits)
        {
            unsafe { return new TypeRef(llvm_float_type(instance, sizeInBits)); }
        }

        public TypeRef GetVoidType()
        {
            unsafe { return new TypeRef(llvm_void_type(instance)); }
        }

        public TypeRef GetPointerType(TypeRef target)
        {
            unsafe { return new TypeRef(llvm_pointer_type(target.instance)); }
        }

        public TypeRef GetNamedStruct(string name)
        {
            unsafe { return new TypeRef(llvm_named_struct(instance, name)); }
        }

        public void SetStructBody(TypeRef @struct, params TypeRef[] body)
        {
            unsafe
            {
                void*[] bodyRaw = new void*[body.Length];
                for (int i = 0; i < body.Length; i++) bodyRaw[i] = body[i].instance;
                llvm_struct_set_body(@struct.instance, bodyRaw, bodyRaw.Length, false);
            }
        }

        public void SetStructBodyPacked(TypeRef @struct, params TypeRef[] body)
        {
            unsafe
            {
                void*[] bodyRaw = new void*[body.Length];
                for (int i = 0; i < body.Length; i++) bodyRaw[i] = body[i].instance;
                llvm_struct_set_body(@struct.instance, bodyRaw, bodyRaw.Length, true);
            }
        }

        public TypeRef GetFunctionType(TypeRef returnType, params TypeRef[] args)
        {
            throw new NotImplementedException();
        }

        public TypeRef GetFunctionTypeVarargs(TypeRef returnType, params TypeRef[] args)
        {
            throw new NotImplementedException();
        }

        public TypeRef GetArrayType(TypeRef typeRef, int length)
        {
            throw new NotImplementedException();
        }
    }
}
