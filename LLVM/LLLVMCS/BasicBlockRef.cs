using System;

namespace LLVMCS
{
    public class BasicBlockRef
    {
        unsafe internal void* instance;

        unsafe public BasicBlockRef(void* instance)
        {
            this.instance = instance;
        }

        public bool IsNull()
        {
            unsafe { return instance == null; }
        }

        public ValueRef GetLastInstruction()
        {
            throw new NotImplementedException();
        }

        public BasicBlockRef GetNextBasicBlock()
        {
            throw new NotImplementedException();
        }

        public ValueRef GetFirstInstruction()
        {
            throw new NotImplementedException();
        }

        public BasicBlockRef GetTerminator()
        {
            throw new NotImplementedException();
        }
    }
}
