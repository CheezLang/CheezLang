using Cheez.Ast.Expressions.Types;
using Cheez.Ast.Statements;
using System.Collections.Generic;
using System.Linq;

namespace Cheez.Types.Abstract
{
    public abstract class AbstractType : CheezType {
        public override bool IsErrorType => true;
    }

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
        public override bool IsErrorType => false;

        public GenericFunctionType(AstFunctionDecl decl)
        {
            Declaration = decl;
        }
    }

    public class GenericStructType : CheezType
    {
        public AstStructDecl Declaration { get; }
        public override bool IsPolyType => false;
        public override bool IsErrorType => false;

        public GenericStructType(AstStructDecl decl)
        {
            Declaration = decl;
        }
    }

    public class GenericEnumType : CheezType
    {
        public AstEnumDecl Declaration { get; }
        public override bool IsPolyType => false;
        public override bool IsErrorType => false;

        public GenericEnumType(AstEnumDecl decl)
        {
            Declaration = decl;
        }
    }

    public class GenericTraitType : CheezType
    {
        public AstTraitDeclaration Declaration { get; }
        public override bool IsPolyType => false;
        public override bool IsErrorType => false;

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
        public override bool IsErrorType => true;
    }

    public class PolyType : CheezType
    {
        public string Name { get; }
        public override bool IsPolyType => true;
        public override bool IsErrorType => false;

        public bool IsDeclaring = false;

        public PolyType(string name, bool declaring = false)
        {
            this.Name = name;
            this.IsDeclaring = declaring;
        }

        public override string ToString() => "$" + Name;

        public override int Match(CheezType concrete, Dictionary<string, CheezType> polyTypes)
        {
            if (polyTypes != null && polyTypes.TryGetValue(Name, out var v))
                return v.Match(concrete, polyTypes);
            return 1;
        }
    }
}
