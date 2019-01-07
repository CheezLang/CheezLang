using System;
using System.Runtime.InteropServices;

namespace LLVMCS
{
    public struct TypeRef
    {
        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void* llvm_pointer_type(void* target);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void* llvm_void_type();

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void* llvm_int_type(int sizeInBits);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void* llvm_float_type(int sizeInBits);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void* llvm_named_struct(string name);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void llvm_struct_set_body(void* @struct, void*[] types, int count, bool packed);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void* llvm_function_type(void* returnType, void*[] argTypes, int count);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void* llvm_function_type_varargs(void* returnType, void*[] argTypes, int count);

        unsafe internal void* instance;

        unsafe internal TypeRef(void* instance)
        {
            this.instance = instance;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TypeRef))
            {
                return false;
            }

            var @ref = (TypeRef)obj;
            return this == @ref;
        }

        public override int GetHashCode()
        {
            unsafe { return (int)instance; }
        }

        public static bool operator ==(TypeRef t1, TypeRef t2)
        {
            unsafe { return t1.instance == t2.instance; }
        }

        public static bool operator !=(TypeRef t1, TypeRef t2)
        {
            unsafe { return t1.instance != t2.instance; }
        }

        // types
        public TypeRef GetPointerTo()
        {
            unsafe { return new TypeRef(llvm_pointer_type(instance)); }
        }

        public static TypeRef GetIntType(int sizeInBits)
        {
            unsafe { return new TypeRef(llvm_int_type(sizeInBits)); }
        }

        public static TypeRef GetFloatType(int sizeInBits)
        {
            unsafe { return new TypeRef(llvm_float_type(sizeInBits)); }
        }

        public static TypeRef GetVoidType()
        {
            unsafe { return new TypeRef(llvm_void_type()); }
        }

        public static TypeRef GetPointerType(TypeRef target)
        {
            unsafe { return new TypeRef(llvm_pointer_type(target.instance)); }
        }

        public static TypeRef GetNamedStruct(string name)
        {
            unsafe { return new TypeRef(llvm_named_struct(name)); }
        }

        public static void SetStructBody(TypeRef @struct, params TypeRef[] body)
        {
            unsafe
            {
                void*[] bodyRaw = new void*[body.Length];
                for (int i = 0; i < body.Length; i++) bodyRaw[i] = body[i].instance;
                llvm_struct_set_body(@struct.instance, bodyRaw, bodyRaw.Length, false);
            }
        }

        public static void SetStructBodyPacked(TypeRef @struct, params TypeRef[] body)
        {
            unsafe
            {
                void*[] bodyRaw = new void*[body.Length];
                for (int i = 0; i < body.Length; i++) bodyRaw[i] = body[i].instance;
                llvm_struct_set_body(@struct.instance, bodyRaw, bodyRaw.Length, true);
            }
        }

        public static TypeRef GetFunctionType(TypeRef returnType, params TypeRef[] args)
        {
            unsafe
            {
                void*[] paramTypes = new void*[args.Length];
                for (int i = 0; i < paramTypes.Length; i++) paramTypes[i] = args[i].instance;
                return new TypeRef(llvm_function_type(returnType.instance, paramTypes, paramTypes.Length));
            }
        }

        public static TypeRef GetFunctionTypeVarargs(TypeRef returnType, params TypeRef[] args)
        {
            unsafe
            {
                void*[] paramTypes = new void*[args.Length];
                for (int i = 0; i < paramTypes.Length; i++) paramTypes[i] = args[i].instance;
                return new TypeRef(llvm_function_type_varargs(returnType.instance, paramTypes, paramTypes.Length));
            }
        }

        public static TypeRef GetArrayType(TypeRef typeRef, int length)
        {
            throw new NotImplementedException();
        }
    }
}
