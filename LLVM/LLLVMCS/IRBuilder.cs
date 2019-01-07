using System;

namespace LLVMCS
{
    public struct IRBuilder
    {
        unsafe private void* instance;
        private Context Context;

        public IRBuilder(Context context)
        {
            unsafe { this.instance = null; } // TODO
            this.Context = context;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #region Utility

        public void PositionBefore(ValueRef inst)
        {
            throw new NotImplementedException();
        }

        public void PositionAtEnd(BasicBlockRef block)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Build methods

        public ValueRef Alloca(TypeRef type, string name = "")
        {
            throw new NotImplementedException();
        }

        public ValueRef IntCast(ValueRef value, TypeRef type, string name = "")
        {
            throw new NotImplementedException();
        }

        public ValueRef ZExtOrBitCast(ValueRef value, TypeRef type, string name = "")
        {
            throw new NotImplementedException();
        }

        public ValueRef PtrToInt(ValueRef value, TypeRef type, string name = "")
        {
            throw new NotImplementedException();
        }

        public ValueRef Call(ValueRef func, params ValueRef[] valueRef)
        {
            throw new NotImplementedException();
        }

        public ValueRef Call(string name, ValueRef func, params ValueRef[] valueRef)
        {
            throw new NotImplementedException();
        }

        public void Ret(ValueRef v)
        {
            throw new NotImplementedException();
        }

        public void RetVoid()
        {
            throw new NotImplementedException();
        }

        public void Store(ValueRef val, ValueRef ptr)
        {
            throw new NotImplementedException();
        }

        public void Br(BasicBlockRef block)
        {
            throw new NotImplementedException();
        }

        public void Unreachable()
        {
            throw new NotImplementedException();
        }

        public ValueRef Load(ValueRef v, string name = "")
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
