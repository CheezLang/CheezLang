
using System;
using System.Runtime.InteropServices;

namespace LLVMCS
{
    public struct ValueRef
    {
        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void llvm_value_set_linkage(void* val, int linkage);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void* llvm_value_append_basic_block(string name, void* function);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void* llvm_value_get_first_basic_block(void* function);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void* llvm_const_int(void* type, ulong value);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void* llvm_const_int_signed(void* type, long value);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static bool llvm_value_verify_function(void* function);

        unsafe internal void* instance;

        unsafe internal ValueRef(void* instance)
        {
            this.instance = instance;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ValueRef))
            {
                return false;
            }

            var @ref = (ValueRef)obj;
            return this == @ref;
        }

        public override int GetHashCode()
        {
            unsafe { return (int)instance; }
        }

        public static bool operator ==(ValueRef t1, ValueRef t2)
        {
            unsafe { return t1.instance == t2.instance; }
        }

        public static bool operator !=(ValueRef t1, ValueRef t2)
        {
            unsafe { return t1.instance != t2.instance; }
        }

        public void SetAlignment(int alignment)
        {
            throw new NotImplementedException();
        }

        public void SetInitializer(ValueRef var)
        {
            throw new NotImplementedException();
        }

        public void SetLinkage(LinkageTypes linkage)
        {
            unsafe { llvm_value_set_linkage(instance, (int)linkage); }
        }

        public TypeRef Type()
        {
            throw new NotImplementedException();
        }

        #region function stuff

        public void SetCallConv(LLVMCallConv callConv)
        {
            throw new NotImplementedException();
        }

        public void AddFunctionAttribute(LLVMAttributeKind kind)
        {
            throw new NotImplementedException();
        }

        public bool VerifyFunction()
        {
            unsafe { return llvm_value_verify_function(instance); }
        }

        public ValueRef GetParam(int i)
        {
            throw new NotImplementedException();
        }

        public BasicBlockRef GetFirstBasicBlock()
        {
            unsafe { return new BasicBlockRef(llvm_value_get_first_basic_block(instance)); }
        }

        public BasicBlockRef AppendBasicBlock(string name)
        {
            unsafe { return new BasicBlockRef(llvm_value_append_basic_block(name, instance)); }
        }

        #endregion


        #region constants

        public static ValueRef ConstInt(TypeRef type, long value)
        {
            unsafe { return new ValueRef(llvm_const_int_signed(type.instance, value)); }
        }

        public static ValueRef ConstUInt(TypeRef type, ulong value)
        {
            unsafe { return new ValueRef(llvm_const_int(type.instance, value)); }
        }

        public static ValueRef ConstFloat(TypeRef typeRef, double v)
        {
            throw new NotImplementedException();
        }

        public static ValueRef ConstNullPointer(TypeRef typeRef)
        {
            throw new NotImplementedException();
        }

        public static ValueRef ConstArray(TypeRef typeRef, ValueRef[] vals)
        {
            throw new NotImplementedException();
        }

        public static ValueRef ConstZeroArray(TypeRef typeRef)
        {
            throw new NotImplementedException();
        }

        public static ValueRef ConstStruct(params ValueRef[] valueRef)
        {
            throw new NotImplementedException();
        }

        public static ValueRef ConstStructPacked(params ValueRef[] valueRef)
        {
            throw new NotImplementedException();
        }

        public static ValueRef ConstIntToPtr(ValueRef valueRef, TypeRef typeRef)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
