using Cheez.Ast.Statements;
using System.Collections.Generic;

namespace Cheez.Types.Abstract
{
    public abstract class AbstractType : CheezType { }

    public class VarDeclType : AbstractType
    {
        public override bool IsPolyType => false;
        public AstSingleVariableDecl Declaration { get; }

        public VarDeclType(AstSingleVariableDecl decl)
        {
            Declaration = decl;
        }

        public override string ToString() => $"<var decl> {Declaration.Name.Name}";
    }

    public class CombiType : AbstractType
    {
        public override bool IsPolyType => false;
        public List<AbstractType> SubTypes { get; }

        public CombiType(List<AbstractType> decls)
        {
            SubTypes = decls;
        }

        public override string ToString() => $"<decls> ({string.Join(", ", SubTypes)})";
    }

    public class AliasType : AbstractType
    {
        public override bool IsPolyType => false;
        public AstTypeAliasDecl Declaration { get; }

        public AliasType(AstTypeAliasDecl decl)
        {
            Declaration = decl;
        }

        public override string ToString() => $"<type alias> {Declaration.Name.Name}";
    }

    public class GenericFunctionType : CheezType
    {
        public AstFunctionDecl Declaration { get; }

        public override bool IsPolyType => false;

        public GenericFunctionType(AstFunctionDecl decl)
        {
            Declaration = decl;
        }
    }

    public class GenericStructType : CheezType
    {
        public AstStructDecl Declaration { get; }

        public override bool IsPolyType => false;

        public GenericStructType(AstStructDecl decl)
        {
            Declaration = decl;
        }
    }

    public class GenericTraitType : CheezType
    {
        public AstTraitDeclaration Declaration { get; }

        public override bool IsPolyType => false;

        public GenericTraitType(AstTraitDeclaration decl)
        {
            Declaration = decl;
        }
    }

    public class ErrorType : CheezType
    {
        public static ErrorType Instance { get; } = new ErrorType { Size = 0 };
        public override bool IsPolyType => false;
        public override string ToString() => "<Error Type>";
    }

    public class PolyType : CheezType
    {
        public string Name { get; }
        public override bool IsPolyType => true;

        public PolyType(string name)
        {
            this.Name = name;
        }

        public override string ToString() => "$" + Name;
    }
}
