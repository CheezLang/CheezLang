namespace Cheez.Compiler
{
    public enum Operator
    {
        Add,
        Subtract,
        Multiply,
        Divide,

        Less,
        LessEqual,
        Greater,
        GreaterEqual,
        Equal,
        NotEqual,

        And,
        Or
    }

    public static class BinaryOperatorExtensions
    {
        public static int GetPrecedence(this Operator self)
        {
            switch (self)
            {
                case Operator.Or:
                    return 1;

                case Operator.And:
                    return 2;

                case Operator.Less:
                case Operator.LessEqual:
                case Operator.Greater:
                case Operator.GreaterEqual:
                case Operator.Equal:
                case Operator.NotEqual:
                    return 3;

                case Operator.Add:
                case Operator.Subtract:
                    return 5;

                case Operator.Multiply:
                case Operator.Divide:
                    return 10;

                default:
                    throw new System.Exception();
            }
        }
    }
}
