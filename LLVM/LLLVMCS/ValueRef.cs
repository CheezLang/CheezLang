
using System;

namespace LLVMCS
{
    public struct ValueRef
    {
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

        public BasicBlockRef GetFirstBasicBlock()
        {
            throw new NotImplementedException();
        }

        public BasicBlockRef AppendBasicBlock(string v)
        {
            throw new NotImplementedException();
        }

        public ValueRef GetParam(int i)
        {
            throw new NotImplementedException();
        }

        public void SetAlignment(int alignment)
        {
            throw new NotImplementedException();
        }

        public void SetInitializer(ValueRef var)
        {
            throw new NotImplementedException();
        }

        public void SetLinkage(LLVMLinkage linkage)
        {
            throw new NotImplementedException();
        }

        public TypeRef Type()
        {
            throw new NotImplementedException();
        }

        public void SetCallConv(LLVMCallConv callConv)
        {
            throw new NotImplementedException();
        }
    }
}
