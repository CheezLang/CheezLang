using System.Numerics;

namespace Cheez.Compiler.IR
{
    public class IRInstruction
    {
        public IRValue Value { get; set; }

        public IRInstruction()
        {
            Value = new IRValue();
        }
    }

    // temp
    public class IRInstPrint : IRInstruction
    {
        public override string ToString()
        {
            return $"_ = print {Value}";
        }
    }

    public class IRInstIntConstant : IRInstruction
    {
        private BigInteger value;

        public IRInstIntConstant(long l, CheezType type)
        {
            this.value = l;
            Value.type = type;
        }

        public override string ToString()
        {
            return $"{Value} = <{Value.type}> const {value}";
        }
    }

    public class IRInstAdd : IRInstruction
    {
        private IRValue value1;
        private IRValue value2;

        public IRInstAdd(IRValue a, IRValue b)
        {
            value1 = a;
            value2 = b;

            Value.type = a.type;
        }

        public override string ToString()
        {
            return $"{Value} = <{Value.type}> iadd {value1}, {value2}";
        }
    }

    public class IRInstBranch : IRInstruction
    {
        public override string ToString()
        {
            return "br";
        }
    }
}
