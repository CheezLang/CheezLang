using Cheez.Ast.Expressions;
using Cheez.Ast.Expressions.Types;
using Cheez.Ast.Statements;
using Cheez.Types.Complex;
using Cheez.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cheez.Types.Abstract
{
    public abstract class AbstractType : CheezType
    {
        public override bool IsErrorType => true;
        public override bool IsComptimeOnly => true;

        protected AbstractType() : base(0, 1, false) { }
    }

    public class SelfType : AbstractType
    {
        public override bool IsPolyType => false;
        public CheezType traitType { get; }

        public SelfType(CheezType traitType)
        {
            this.traitType = traitType;
        }

        public override string ToString()
        {
            return "<Self>";
        }
    }

    public class VarDeclType : AbstractType
    {
        public override bool IsPolyType => false;
        public AstVariableDecl Declaration { get; }

        public VarDeclType(AstVariableDecl decl)
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

    public class GenericFunctionType : CheezType
    {
        public AstFuncExpr Declaration { get; }
        public override bool IsPolyType => false;
        public override bool IsErrorType => false;

        public GenericFunctionType(AstFuncExpr decl)
            : base(0, 1, false)
        {
            Declaration = decl;
        }

        public override string ToString()
        {
            return new RawAstPrinter(null).VisitFunctionSignature(Declaration);
        }

        public override int Match(CheezType concrete, Dictionary<string, (CheezType type, object value)> polyTypes)
        {
            throw new NotImplementedException();
        }
    }

    public class GenericType : CheezType
    {
        public override bool IsErrorType => false;
        public override bool IsPolyType => true;

        public AstGenericExpr Expression { get; }

        public GenericType(AstGenericExpr expr)
        {
            Expression = expr;
        }

        public override string ToString()
        {
            return "generic " + Expression.ToString();
        }
    }

    public class GenericStructType : CheezType
    {
        //public AstStructDecl Declaration { get; }
        public AstStructTypeExpr Declaration { get; }
        public (CheezType type, object value)[] Arguments { get; }
        public override bool IsPolyType => true;
        public override bool IsErrorType => false;
        public override bool IsCopy { get; }
        public string Name { get; }

        public GenericStructType(AstStructTypeExpr decl, bool copy, string name, (CheezType type, object value)[] arguments = null)
            : base(0, 1, false)
        {
            Declaration = decl;
            IsCopy = copy;
            Name = name;
            Arguments = arguments ?? decl.Parameters.Select(p => (p.Type, p.Value)).ToArray();
        }

        public override int Match(CheezType concrete, Dictionary<string, (CheezType type, object value)> polyTypes)
        {
            if (concrete is StructType str)
            {
                if (this.Declaration != str.DeclarationTemplate)
                    return -1;

                int score = 0;
                for (int i = 0; i < Arguments.Length; i++)
                {
                    int s = Workspace.PolyValuesMatch(this.Arguments[i], (null, str.Arguments[i]), polyTypes);
                    if (s == -1)
                        return -1;
                    score += s;
                }
                return score;
            }

            return -1;
        }
    }

    public class GenericEnumType : CheezType
    {
        public AstEnumTypeExpr Declaration { get; }
        public override bool IsPolyType => true;
        public override bool IsErrorType => false;
        public string Name { get; }
        public (CheezType type, object value)[] Arguments { get; }

        public GenericEnumType(AstEnumTypeExpr decl, string name, (CheezType type, object value)[] arguments = null)
            : base(0, 1, false)
        {
            Declaration = decl;
            Name = name;
            Arguments = arguments ?? decl.Parameters.Select(p => (p.Type, p.Value)).ToArray();
        }

        public override int Match(CheezType concrete, Dictionary<string, (CheezType type, object value)> polyTypes)
        {
            if (concrete is EnumType str)
            {
                if (this.Declaration != str.DeclarationTemplate)
                    return -1;

                int score = 0;
                for (int i = 0; i < Arguments.Length; i++)
                {
                    int s = Workspace.PolyValuesMatch(this.Arguments[i], (null, str.Arguments[i]), polyTypes);
                    if (s == -1)
                        return -1;
                    score += s;
                }
                return score;
            }

            return -1;
        }
    }

    public class GenericTraitType : CheezType
    {
        public AstTraitTypeExpr Declaration { get; }
        public override bool IsPolyType => true;
        public override bool IsErrorType => false;
        public (CheezType type, object value)[] Arguments { get; }

        public GenericTraitType(AstTraitTypeExpr decl, (CheezType type, object value)[] arguments = null)
            : base(0, 1, false)
        {
            Declaration = decl;
            Arguments = arguments ?? decl.Parameters.Select(p => (p.Type, p.Value)).ToArray();
        }

        public override int Match(CheezType concrete, Dictionary<string, (CheezType type, object value)> polyTypes)
        {
            if (concrete is TraitType str)
            {
                if (this.Declaration != str.DeclarationTemplate)
                    return -1;

                int score = 0;
                for (int i = 0; i < Arguments.Length; i++)
                {
                    int s = Workspace.PolyValuesMatch(this.Arguments[i], (null, str.Arguments[i]), polyTypes);
                    if (s == -1)
                        return -1;
                    score += s;
                }
                return score;
            }

            return -1;
        }
    }

    public class ErrorType : CheezType
    {
        public static ErrorType Instance { get; } = new ErrorType();
        public override bool IsPolyType => false;
        public override string ToString() => "<Error Type>";
        public override bool IsErrorType => true;

        private ErrorType() : base(0, 1, false) { }
    }

    public class PolyType : CheezType
    {
        public string Name { get; }
        public override bool IsPolyType => true;
        public override bool IsErrorType => false;

        public bool IsDeclaring { get; } = false;

        public PolyType(string name, bool declaring = false)
            : base(0, 1, false)
        {
            this.Name = name;
            this.IsDeclaring = declaring;
        }

        public override string ToString() => "$" + Name;

        public override int Match(CheezType concrete, Dictionary<string, (CheezType type, object value)> polyTypes)
        {
            if (polyTypes != null && polyTypes.TryGetValue(Name, out var v))
                return (v.value as CheezType).Match(concrete, polyTypes);
            return 1;
        }
    }

    public class PolyValueType : CheezType
    {
        private static readonly PolyValueType _instance = new PolyValueType();
        public static PolyValueType Instance => _instance;
        public override bool IsPolyType => true;
        public override bool IsErrorType => false;
        
        private PolyValueType()
            : base(0, 1, false)
        {
        }

        public override string ToString() => "poly_value_t";

        public override int Match(CheezType concrete, Dictionary<string, (CheezType type, object value)> polyTypes)
        {
            if (this == concrete)
                return 0;
            return 1;
        }
    }
}
