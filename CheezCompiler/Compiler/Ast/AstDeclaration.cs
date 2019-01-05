using Cheez.Compiler.Parsing;
using Cheez.Compiler.Visitor;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Cheez.Compiler.Ast
{

    public abstract class AstDecl : AstStatement
    {
        public AstIdExpr Name { get; set; }
        public CheezType Type { get; set; }

        public AstDecl(AstIdExpr name, List<AstDirective> Directives = null, ILocation Location = null) : base(Directives, Location)
        {
            this.Name = name;
        }
    }

    public class AstParameter : ITypedSymbol, ILocation
    {
        public ILocation Location { get; private set; }
        public TokenLocation Beginning => Location?.Beginning;
        public TokenLocation End => Location?.End;


        public AstIdExpr Name { get; }
        public CheezType Type { get; set; }
        public AstTypeExpr TypeExpr { get; set; }
        public Scope Scope { get; set; }

        public object Value { get; set; }

        public bool IsConstant => true;

        public AstParameter(AstIdExpr name, AstTypeExpr typeExpr, ILocation Location = null)
        {
            this.Location = Location;
            Name = name;
            this.TypeExpr = typeExpr;
        }

        public AstParameter Clone() => new AstParameter(Name?.Clone() as AstIdExpr, TypeExpr.Clone() as AstTypeExpr);
    }

    #region Function Declaration

    public interface ITempVariable
    {
        AstIdExpr Name { get; } // can be null
        CheezType Type { get; }
    }

    public class AstFunctionDecl : AstDecl, ITypedSymbol
    {
        public Scope HeaderScope { get; set; }
        public Scope SubScope { get; set; }

        public List<AstParameter> Parameters { get; }
        public List<AstParameter> ReturnValues { get; }

        public FunctionType FunctionType => Type as FunctionType;

        public AstBlockStmt Body { get; private set; }

        public List<AstFunctionDecl> PolymorphicInstances { get; } = new List<AstFunctionDecl>();

        public bool RefSelf { get; set; }
        public bool IsGeneric { get; set; }

        public bool IsConstant => true;

        public bool IsPolyInstance { get; set; } = false;
        public Dictionary<string, AstExpression> PolymorphicTypeExprs { get; internal set; }
        public Dictionary<string, CheezType> PolymorphicTypes { get; internal set; }

        public AstImplBlock ImplBlock { get; set; }
        public AstFunctionDecl TraitFunction { get; internal set; }

        public AstFunctionDecl(AstIdExpr name,
            List<AstIdExpr> generics,
            List<AstParameter> parameters,
            List<AstParameter> returns,
            AstBlockStmt body = null, 
            List<AstDirective> Directives = null, 
            bool refSelf = false, ILocation Location = null)
            : base(name, Directives, Location)
        {
            this.Parameters = parameters;
            this.ReturnValues = returns ?? new List<AstParameter>();
            this.Body = body;
            this.RefSelf = refSelf;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default(D)) => visitor.VisitFunctionDecl(this, data);

        public override AstStatement Clone() => CopyValuesTo(new AstFunctionDecl(Name.Clone() as AstIdExpr,
                null,
                Parameters.Select(p => p.Clone()).ToList(),
                ReturnValues.Select(p => p.Clone()).ToList(),
                Body?.Clone() as AstBlockStmt));
    }

    #endregion

    #region Struct Declaration

    public class AstMemberDecl : ILocation
    {
        public ILocation Location { get; private set; }
        public TokenLocation Beginning => Location?.Beginning;
        public TokenLocation End => Location?.End;


        public AstIdExpr Name { get; }
        public AstExpression Initializer { get; set; }
        public AstTypeExpr TypeExpr { get; set; }
        public CheezType Type { get; set; }

        public AstMemberDecl(AstIdExpr name, AstTypeExpr typeExpr, AstExpression init, ILocation Location = null)
        {
            this.Location = Location;
            this.Name = name;
            this.TypeExpr = typeExpr;
            this.Initializer = init;
        }

        public AstMemberDecl Clone() => new AstMemberDecl(Name.Clone() as AstIdExpr, TypeExpr.Clone() as AstTypeExpr, Initializer?.Clone());
    }

    public class AstStructDecl : AstDecl, ITypedSymbol
    {
        public List<AstMemberDecl> Members { get; }
        public List<AstParameter> Parameters { get; set; }

        public Scope SubScope { get; set; }

        public bool IsPolymorphic { get; set; }
        public bool IsPolyInstance { get; set; }

        public bool IsConstant => true;

        public List<AstStructDecl> PolymorphicInstances { get; } = new List<AstStructDecl>();

        //public List<AstImplBlock> Implementations { get; } = new List<AstImplBlock>();
        public List<TraitType> Traits { get; } = new List<TraitType>();

        public AstStructDecl(AstIdExpr name, List<AstParameter> param, List<AstMemberDecl> members, List<AstDirective> Directives = null, ILocation Location = null)
            : base(name, Directives, Location)
        {
            this.Parameters = param ?? new List<AstParameter>();
            this.Members = members;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitStructDecl(this, data);

        public override AstStatement Clone() => CopyValuesTo(new AstStructDecl(Name.Clone() as AstIdExpr, Parameters?.Select(p => p.Clone()).ToList(), Members.Select(m => m.Clone()).ToList()));
    }

    #endregion

    #region Trait

    public class AstTraitDeclaration : AstDecl, ITypedSymbol
    {
        public List<AstParameter> Parameters { get; set; }

        public List<AstFunctionDecl> Functions { get; }
        public List<AstFunctionDecl> FunctionInstances { get; }

        public bool IsConstant => true;

        public bool IsPolymorphic { get; set; }
        public bool IsPolyInstance { get; set; }

        public AstTraitDeclaration(AstIdExpr name, List<AstParameter> parameters, List<AstFunctionDecl> functions, ILocation Location = null)
            : base(name, Location: Location)
        {
            this.Parameters = parameters;
            this.Functions = functions;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitTraitDecl(this, data);

        public override AstStatement Clone() => CopyValuesTo(new AstTraitDeclaration(Name.Clone() as AstIdExpr, Parameters.Select(p => p.Clone()).ToList(), Functions.Select(f => f.Clone() as AstFunctionDecl).ToList()));
    }

    public class AstImplBlock : AstStatement
    {
        public CheezType TargetType { get; set; }
        public AstTypeExpr TargetTypeExpr { get; set; }
        public AstTypeExpr TraitExpr { get; set; }

        public TraitType Trait { get; set; }

        public List<AstFunctionDecl> Functions { get; }
        public List<AstFunctionDecl> FunctionInstances { get; } = new List<AstFunctionDecl>();

        public Scope SubScope { get; set; }

        public AstImplBlock(AstTypeExpr targetTypeExpr, AstTypeExpr traitExpr, List<AstFunctionDecl> functions, ILocation Location = null) : base(Location: Location)
        {
            this.TargetTypeExpr = targetTypeExpr;
            this.Functions = functions;
            this.TraitExpr = traitExpr;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitImplDecl(this, data);

        public override AstStatement Clone() => CopyValuesTo(new AstImplBlock(TargetTypeExpr.Clone() as AstTypeExpr, TraitExpr.Clone() as AstTypeExpr, Functions.Select(f => f.Clone() as AstFunctionDecl).ToList()));
    }

    #endregion

    #region Variable Declarion

    public class AstSingleVariableDecl : AstDecl, ITypedSymbol
    {
        public bool IsConstant => false;
        public AstTypeExpr TypeExpr { get; set; }

        public AstVariableDecl VarDeclaration { get; set; }

        public AstSingleVariableDecl(AstIdExpr name, AstTypeExpr typeExpr, AstVariableDecl parent, ILocation Location) : base(name, Location: Location)
        {
            TypeExpr = typeExpr;
            VarDeclaration = parent;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            throw new System.NotImplementedException();
        }

        public override AstStatement Clone()
        {
            throw new System.NotImplementedException();
        }
    }


    public class AstVariableDecl : AstStatement
    {
        public AstExpression Pattern { get; set; }
        public AstTypeExpr TypeExpr { get; set; }
        public AstExpression Initializer { get; set; }
        public Scope SubScope { get; set; }

        public bool IsConstant { get; set; } = false;

        public CheezType Type { get; set; } = null;

        public List<AstSingleVariableDecl> Dependencies { get; set; }

        public List<AstSingleVariableDecl> SubDeclarations = new List<AstSingleVariableDecl>();

        public AstVariableDecl(AstExpression pattern, AstTypeExpr typeExpr, AstExpression init, List<AstDirective> Directives = null, ILocation Location = null) 
            : base(Directives, Location)
        {
            this.Pattern = pattern;
            this.TypeExpr = typeExpr;
            this.Initializer = init;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default(D)) => visitor.VisitVariableDecl(this, data);

        public override AstStatement Clone()
            => CopyValuesTo(new AstVariableDecl(
                Pattern.Clone(),
                TypeExpr?.Clone() as AstTypeExpr,
                Initializer?.Clone()));
    }

    #endregion

    #region Enum

    public class AstEnumMember : ILocation
    {
        public ILocation Location { get; private set; }
        public TokenLocation Beginning => Location?.Beginning;
        public TokenLocation End => Location?.End;


        public AstIdExpr Name { get; }
        public AstExpression Value { get; }
        public CheezType Type { get; set; }

        public AstEnumMember(AstIdExpr name, AstExpression value, ILocation Location = null)
        {
            this.Location = Location;
            this.Name = name;
            this.Value = value;
        }

        public AstEnumMember Clone() => new AstEnumMember(Name.Clone() as AstIdExpr, Value?.Clone());
    }

    public class AstEnumDecl : AstDecl, INamed
    {
        public List<AstEnumMember> Members { get; }

        public AstEnumDecl(AstIdExpr name, List<AstEnumMember> members, List<AstDirective> Directive = null, ILocation Location = null)
            : base(name, Directive, Location)
        {
            this.Members = members;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitEnumDecl(this, data);

        public override AstStatement Clone() => CopyValuesTo(new AstEnumDecl(Name.Clone() as AstIdExpr, Members.Select(m => m.Clone()).ToList()));
    }

    #endregion

    #region Type Alias

    public class AstTypeAliasDecl : AstDecl, ITypedSymbol
    {
        public AstTypeExpr TypeExpr { get; set; }

        public bool IsConstant => false;

        public AstTypeAliasDecl(AstIdExpr name, AstTypeExpr typeExpr, List<AstDirective> Directives = null, ILocation Location = null)
            : base(name, Directives, Location)
        {
            this.TypeExpr = typeExpr;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitTypeAliasDecl(this, data);

        public override AstStatement Clone() => CopyValuesTo(new AstTypeAliasDecl(Name.Clone() as AstIdExpr, TypeExpr.Clone() as AstTypeExpr));
    }

    #endregion
}
