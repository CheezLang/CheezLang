namespace Cheez.Compiler.IR
{
    public class IRInterpreter
    {
        public void Interpret(IRBasicBlock block)
        {

        }
    }

    public class IRValue
    {
        public CheezType type;
        public object value;

        public string name;

        public override string ToString()
        {
            return name;
        }
    }
}
