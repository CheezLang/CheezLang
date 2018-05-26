namespace Cheez.Compiler
{
    public interface IOperator
    {
        CheezType LhsType { get; }
        CheezType RhsType { get; }
        CheezType ResultType { get; }
        string Name { get; }

        int Accepts(CheezType lhs, CheezType rhs);
    }

    public interface IUnaryOperator
    {
        CheezType SubExprType { get; }
        CheezType ResultType { get; }
        string Name { get; }

        int Accepts(CheezType sub);
    }

    public class BuiltInOperator : IOperator
    {
        public CheezType LhsType { get; private set; }
        public CheezType RhsType { get; private set; }
        public CheezType ResultType { get; private set; }

        public string Name { get; private set; }


        public BuiltInOperator(string name, CheezType resType, CheezType lhs, CheezType rhs)
        {
            Name = name;
            ResultType = resType;
            LhsType = lhs;
            RhsType = rhs;
        }

        public int Accepts(CheezType lhs, CheezType rhs)
        {
            throw new System.NotImplementedException();
        }

        public override string ToString()
        {
            return $"({ResultType}) {LhsType} {Name} {RhsType}";
        }
    }

    public class BuiltInUnaryOperator : IUnaryOperator
    {
        public CheezType SubExprType { get; private set; }
        public CheezType ResultType { get; private set; }

        public string Name { get; private set; }


        public BuiltInUnaryOperator(string name, CheezType resType, CheezType sub)
        {
            Name = name;
            ResultType = resType;
            SubExprType = sub;
        }

        public override string ToString()
        {
            return $"({ResultType}) {Name} {SubExprType}";
        }

        public int Accepts(CheezType sub)
        {
            throw new System.NotImplementedException();
        }
    }
}
