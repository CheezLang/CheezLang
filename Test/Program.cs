using System.Runtime.InteropServices;
using LLVMSharp;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //var mod = CreateLLVMModule();

            //var modId = LLVM.GetModuleIdentifier(mod, out var len);
            //System.Console.WriteLine($"modId: {modId}, len: {len}");

            ////var mod = LLVM.ModuleCreateWithName("uiaeuiaeuiae");
            //System.Console.WriteLine($" C# mod ref: {mod.Pointer.ToInt64()}");
            //foooo(mod);

        }

        [DllImport("LLVMBinding.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void foooo(LLVMModuleRef mod);

        [DllImport("LLVMBinding.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern LLVMModuleRef CreateLLVMModule();
    }
}
