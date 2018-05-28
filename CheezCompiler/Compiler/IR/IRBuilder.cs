
using System;

namespace Cheez.Compiler.IR
{
    public class IRBuilder
    {
        private IRFunction function;
        private IRBasicBlock currentBlock;

        private int unnamedCounter = 0;

        public IRBuilder(IRFunction func)
        {
            this.function = func;
        }

        public void SetBasicBlock(IRBasicBlock bb)
        {
            this.currentBlock = bb;
        }

        private string NameOrDefault(string name)
        {
            return name ?? $"%{unnamedCounter++}";
        }

        public IRValue BuildConstInt(long value, CheezType type, string name = null)
        {
            name = NameOrDefault(name);

            var inst = new IRInstIntConstant(value, type);
            currentBlock.AddInstruction(inst);
            if (!function.RegisterValue(name, inst.Value))
                return null;

            inst.Value.name = name;
            return inst.Value;
        }

        public void BuildPrint(IRValue c)
        {
            var inst = new IRInstPrint();
            inst.Value = c;
            currentBlock.AddInstruction(inst);
        }

        public IRValue BuildAdd(IRValue a, IRValue b, string name = null)
        {
            name = NameOrDefault(name);

            var inst = new IRInstAdd(a, b);
            currentBlock.AddInstruction(inst);
            if (!function.RegisterValue(name, inst.Value))
                return null;

            inst.Value.name = name;
            return inst.Value;
        }
    }
}
