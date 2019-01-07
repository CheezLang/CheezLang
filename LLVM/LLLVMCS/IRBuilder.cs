using System;
using System.Runtime.InteropServices;

namespace LLVMCS
{
    public struct IRBuilder
    {
        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void* llvm_create_ir_builder();

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void llvm_delete_ir_builder(void* builder);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void llvm_ir_builder_position_at_end(void* builder, void* bb);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void llvm_ir_builder_position_before(void* builder, void* inst);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void* llvm_ir_builder_create_alloca(void* builder, void* type, string name);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void* llvm_ir_builder_create_br(void* builder, void* dest);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void* llvm_ir_builder_create_ret_void(void* builder);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void* llvm_ir_builder_create_ret(void* builder, void* value);

        [DllImport(DLL.LLVM_DLL_NAME, CallingConvention = DLL.LLVM_DLL_CALLING_CONVENTION, CharSet = DLL.LLVM_DLL_CHAR_SET)]
        private unsafe extern static void* llvm_ir_builder_create_call(void* builder, void* callee, void*[] args, int argCount);

        unsafe private void* instance;

        unsafe public IRBuilder(void* ptr)
        {
            unsafe { this.instance = ptr; }
        }

        public static IRBuilder Create()
        {
            unsafe {
                return new IRBuilder(llvm_create_ir_builder());
            }
        }

        public static IRBuilder Create(BasicBlockRef bb)
        {
            var ir = Create();
            ir.PositionAtEnd(bb);
            return ir;
        }

        public void Dispose()
        {
            unsafe
            {
                if (instance != null)
                    llvm_delete_ir_builder(instance);
                instance = null;
            }
        }

        #region Utility

        public void PositionBefore(ValueRef inst)
        {
            unsafe { llvm_ir_builder_position_before(instance, inst.instance); }
        }

        public void PositionAtEnd(BasicBlockRef block)
        {
            unsafe { llvm_ir_builder_position_at_end(instance, block.instance); }
        }

        #endregion

        #region Build methods

        public ValueRef Alloca(TypeRef type, string name = "")
        {
            unsafe { return new ValueRef(llvm_ir_builder_create_alloca(instance, type.instance, name)); }
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

        public ValueRef Call(ValueRef func, params ValueRef[] arguments)
        {
            unsafe
            {
                void*[] args = new void*[arguments.Length];
                for (int i = 0; i < args.Length; i++) args[i] = arguments[i].instance;
                return new ValueRef(llvm_ir_builder_create_call(instance, func.instance, args, args.Length));
            }
        }

        public ValueRef Call(string name, ValueRef func, params ValueRef[] valueRef)
        {
            throw new NotImplementedException();
        }

        public ValueRef Ret(ValueRef v)
        {
            unsafe { return new ValueRef(llvm_ir_builder_create_ret(instance, v.instance)); }
        }

        public ValueRef RetVoid()
        {
            unsafe { return new ValueRef(llvm_ir_builder_create_ret_void(instance)); }
        }

        public void Store(ValueRef val, ValueRef ptr)
        {
            throw new NotImplementedException();
        }

        public ValueRef Br(BasicBlockRef block)
        {
            unsafe { return new ValueRef(llvm_ir_builder_create_br(instance, block.instance)); }
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
