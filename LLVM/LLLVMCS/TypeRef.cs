using System.Runtime.InteropServices;

namespace LLVMCS
{
    public struct TypeRef
    {
        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void* llvm_pointer_type(void* target);

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

        public TypeRef GetPointerTo()
        {
            unsafe { return new TypeRef(llvm_pointer_type(instance)); }
        }
    }
}
