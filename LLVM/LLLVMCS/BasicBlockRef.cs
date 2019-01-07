using System;
using System.Collections.Generic;
using System.Text;

namespace LLVMCS
{
    public class BasicBlockRef
    {
        unsafe private void* instance;

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
