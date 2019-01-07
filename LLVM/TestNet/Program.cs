using System;

using LLVMCS;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var module = new Module("deine mudda");

            module.SetTargetTriple("i386-pc-win32");

            var tt = module.GetTargetTriple();

            var dl = new DataLayout(module);

            var str = module.Context.GetNamedStruct("test_struct");
            module.Context.SetStructBody(str, new TypeRef[]
            {
                module.Context.GetIntType(32),
                module.Context.GetIntType(64),
                module.Context.GetIntType(1),
                module.Context.GetFloatType(32).GetPointerTo()
            });

            module.PrintToFile("test.ll");

            dl.Dispose();
            module.Dispose();
            Console.WriteLine("ok");
        }
    }
}
