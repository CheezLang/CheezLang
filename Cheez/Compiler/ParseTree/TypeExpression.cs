using Cheez.Compiler.Ast;
using Cheez.Compiler.Parsing;

namespace Cheez.Compiler.ParseTree
{
    public class PTTypeExpr : PTExpr
    {
        public PTTypeExpr(TokenLocation beg, TokenLocation end) : base(beg, end)
        {
        }

        public override AstExpression CreateAst()
        {
            return new AstTypeExpr(this);
        }
    }

    public class PTNamedTypeExpr : PTTypeExpr
    {
        public string Name { get; set; }

        public PTNamedTypeExpr(TokenLocation beg, TokenLocation end, string name) : base(beg, end)
        {
            this.Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class PTPointerTypeExpr : PTTypeExpr
    {
        public PTTypeExpr TargetType { get; set; }

        public PTPointerTypeExpr(TokenLocation beg, TokenLocation end, PTTypeExpr target) : base(beg, end)
        {
            this.TargetType = target;
        }

        public override string ToString()
        {
            return $"{TargetType}*";
        }
    }

    public class PTArrayTypeExpr : PTTypeExpr
    {
        public PTTypeExpr ElementType { get; set; }

        public PTArrayTypeExpr(TokenLocation beg, TokenLocation end, PTTypeExpr target) : base(beg, end)
        {
            this.ElementType = target;
        }

        public override string ToString()
        {
            return $"{ElementType}[]";
        }
    }

}
