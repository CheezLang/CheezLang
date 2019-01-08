namespace LLVMCS
{
    public abstract class LLVMRef
    {
        unsafe internal void* instance;

        ~LLVMRef()
        {
            Dispose();
        }

        public void Dispose()
        {
            unsafe
            {
                if (instance != null)
                {
                    OnDispose(instance);
                }
                instance = null;
            }
        }

        unsafe protected abstract void OnDispose(void* instance);
    }
}
