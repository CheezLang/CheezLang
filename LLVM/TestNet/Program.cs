using System;

using LLVMCS;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var module = new Module("test module");

            module.SetTargetTriple("i386-pc-win32");

            var tt = module.GetTargetTriple();

            var dl = new DataLayout(module);

            var str = TypeRef.GetNamedStruct("test_struct");
            TypeRef.SetStructBody(str, new TypeRef[]
            {
                TypeRef.GetIntType(32),
                TypeRef.GetIntType(64),
                TypeRef.GetIntType(1),
                TypeRef.GetFloatType(32).GetPointerTo()
            });

            var test = module.AddGlobal(str, "test");
            test.SetLinkage(LinkageTypes.CommonLinkage);

            module.PrintToFile("test.ll");

            dl.Dispose();
            module.Dispose();
            Console.WriteLine("ok");
        }
    }
}
