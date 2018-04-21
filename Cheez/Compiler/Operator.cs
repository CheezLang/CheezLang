namespace Cheez.Compiler
{
    public enum Operator
    {
        Add,
        Subtract,
        Multiply,
        Divide,
    }

    public static class BinaryOperatorExtensions
    {
        public static int GetPrecedence(this Operator self)
        {
            switch (self)
            {
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
