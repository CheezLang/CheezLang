using Cheez.Compiler.Parsing;
using Cheez.Compiler.Visitor;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Cheez.Compiler.Ast
{
    public class AstParameter : ITypedSymbol, ILocation
    {
        public ILocation Location { get; private set; }
        public TokenLocation Beginning => Location?.Beginning;
        public TokenLocation End => Location?.End;


        public AstIdentifierExpr Name { get; }
        public CheezType Type { get; set; }
        public AstExpression TypeExpr { get; set; }
        public Scope Scope { get; set; }

        public object Value { get; set; }

        public bool IsConstant => true;

        public AstParameter(AstIdentifierExpr name, AstExpression typeExpr, ILocation Location = null)
        {
            this.Location = Location;
            Name = name;
            this.TypeExpr = typeExpr;
        }

        public AstParameter Clone() => new AstParameter(Name?.Clone() as AstIdentifierExpr, TypeExpr.Clone());
    }

    #region Function Declaration

    public class AstFunctionParameter : ITypedSymbol, ILocation
    {
        public ILocation Location { get; private set; }
        public TokenLocation Beginning => Location?.Beginning;
        public TokenLocation End => Location?.End;


        public AstIdentifierExpr Name { get; }
        public CheezType Type { get; set; }
        public AstExpression TypeExpr { get; set; }
        public Scope Scope { get; set; }

        public bool IsConstant => false;

        public AstFunctionParameter(AstIdentifierExpr name, AstExpression typeExpr, ILocation Location = null)
        {
            this.Location = Location;
            this.Name = name;
            this.TypeExpr = typeExpr;
        }

        public AstFunctionParameter Clone() => new AstFunctionParameter(Name.Clone() as AstIdentifierExpr, TypeExpr.Clone(), Location);
    }

    public interface ITempVariable
    {
        AstIdentifierExpr Name { get; } // can be null
        CheezType Type { get; }
    }

    public class AstFunctionDecl : AstStatement, ITypedSymbol
    {
        public Parsing.IText Text { get; set; }

        public Scope HeaderScope { get; set; }
        public Scope SubScope { get; set; }

        public AstIdentifierExpr Name { get; set; }
        public List<AstFunctionParameter> Parameters { get; }
        public AstExpression ReturnTypeExpr { get; set; }
        public CheezType ReturnType { get; set; }

        //public List<AstIdentifierExpr> Generics { get; }

        public CheezType Type { get; set; }
        public FunctionType FunctionType => Type as FunctionType;

        public AstBlockStmt Body { get; private set; }

        //public List<ITempVariable> LocalVariables { get; } = new List<ITempVariable>();

        public List<AstFunctionDecl> PolymorphicInstances { get; } = new List<AstFunctionDecl>();

        public bool RefSelf { get; set; }
        public bool IsGeneric { get; set; }

        public bool IsConstant => true;

        public bool IsPolyInstance { get; set; } = false;
        public Dictionary<string, AstExpression> PolymorphicTypeExprs { get; internal set; }
        public Dictionary<string, CheezType> PolymorphicTypes { get; internal set; }

        //public CheezType ImplTarget { get; set; }
        public AstImplBlock ImplBlock { get; set; }
        public AstFunctionDecl TraitFunction { get; internal set; }

        public AstFunctionDecl(AstIdentifierExpr name,
            List<AstIdentifierExpr> generics,
            List<AstFunctionParameter> parameters,
            AstExpression returnTypeExpr,
            AstBlockStmt body = null, 
            List<AstDirective> Directives = null, 
            bool refSelf = false, ILocation Location = null)
            : base(Directives, Location)
        {
            this.Name = name;
            this.Parameters = parameters;
            this.ReturnTypeExpr = returnTypeExpr;
            this.Body = body;
            this.RefSelf = refSelf;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default(D)) => visitor.VisitFunctionDeclaration(this, data);

        public override AstStatement Clone() => CopyValuesTo(new AstFunctionDecl(Name.Clone() as AstIdentifierExpr,
                null,
                Parameters.Select(p => p.Clone()).ToList(),
                ReturnTypeExpr?.Clone(),
                Body?.Clone() as AstBlockStmt));
    }

    #endregion

    #region Type Declaration

    public class AstMemberDecl : ILocation
    {
        public ILocation Location { get; private set; }
        public TokenLocation Beginning => Location?.Beginning;
        public TokenLocation End => Location?.End;


        public AstIdentifierExpr Name { get; }
        public AstExpression Initializer { get; set; }
        public AstExpression TypeExpr { get; set; }
        public CheezType Type { get; set; }

        public AstMemberDecl(AstIdentifierExpr name, AstExpression typeExpr, AstExpression init, ILocation Location = null)
        {
            this.Location = Location;
            this.Name = name;
            this.TypeExpr = typeExpr;
            this.Initializer = init;
        }

        public AstMemberDecl Clone() => new AstMemberDecl(Name.Clone() as AstIdentifierExpr, TypeExpr.Clone(), Initializer?.Clone());
    }

    public class AstStructDecl : AstStatement, ITypedSymbol
    {
        public AstIdentifierExpr Name { get; set; }
        public List<AstMemberDecl> Members { get; }
        public List<AstParameter> Parameters { get; set; }

        public CheezType Type { get; set; }

        public Scope SubScope { get; set; }

        public bool IsPolymorphic { get; set; }
        public bool IsPolyInstance { get; set; }

        public bool IsConstant => true;

        public List<AstStructDecl> PolymorphicInstances { get; } = new List<AstStructDecl>();

        //public List<AstImplBlock> Implementations { get; } = new List<AstImplBlock>();
        public List<TraitType> Traits { get; } = new List<TraitType>();

        public AstStructDecl(AstIdentifierExpr name, List<AstParameter> param, List<AstMemberDecl> members, List<AstDirective> Directives = null, ILocation Location = null) : base(Directives, Location)
        {
            this.Name = name;
            this.Parameters = param ?? new List<AstParameter>();
            this.Members = members;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitStructDeclaration(this, data);

        public override AstStatement Clone() => CopyValuesTo(new AstStructDecl(Name.Clone() as AstIdentifierExpr, Parameters?.Select(p => p.Clone()).ToList(), Members.Select(m => m.Clone()).ToList()));
    }

    public class AstTraitDeclaration : AstStatement, ITypedSymbol
    {
        public AstIdentifierExpr Name { get; set; }
        public List<AstParameter> Parameters { get; set; }

        public List<AstFunctionDecl> Functions { get; }
        public List<AstFunctionDecl> FunctionInstances { get; }

        public CheezType Type { get; set; }
        public bool IsConstant => true;

        public bool IsPolymorphic { get; set; }
        public bool IsPolyInstance { get; set; }

        public AstTraitDeclaration(AstIdentifierExpr name, List<AstParameter> parameters, List<AstFunctionDecl> functions, ILocation Location = null) : base(Location: Location)
        {
            this.Name = name;
            this.Parameters = parameters;
            this.Functions = functions;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitTraitDeclaration(this, data);

        public override AstStatement Clone() => CopyValuesTo(new AstTraitDeclaration(Name.Clone() as AstIdentifierExpr, Parameters.Select(p => p.Clone()).ToList(), Functions.Select(f => f.Clone() as AstFunctionDecl).ToList()));
    }

    public class AstImplBlock : AstStatement
    {
        public CheezType TargetType { get; set; }
        public AstExpression TargetTypeExpr { get; set; }
        public AstExpression TraitExpr { get; set; }

        public TraitType Trait { get; set; }

        public List<AstFunctionDecl> Functions { get; }
        public List<AstFunctionDecl> FunctionInstances { get; } = new List<AstFunctionDecl>();

        public Scope SubScope { get; set; }

        public AstImplBlock(AstExpression targetTypeExpr, AstExpression traitExpr, List<AstFunctionDecl> functions, ILocation Location = null) : base(Location: Location)
        {
            this.TargetTypeExpr = targetTypeExpr;
            this.Functions = functions;
            this.TraitExpr = traitExpr;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitImplBlock(this, data);

        public override AstStatement Clone() => CopyValuesTo(new AstImplBlock(TargetTypeExpr.Clone(), TraitExpr.Clone(), Functions.Select(f => f.Clone() as AstFunctionDecl).ToList()));
    }

    #endregion

    #region Variable Declarion

    public class AstVariableDecl : AstStatement, ITypedSymbol
    {
        public AstIdentifierExpr Name { get; set; }
        public CheezType Type { get; set; }
        public AstExpression TypeExpr { get; set; }
        public AstExpression Initializer { get; set; }
        public Scope SubScope { get; set; }

        public bool IsConstant { get; set; } = false;

        public AstVariableDecl(AstIdentifierExpr name, AstExpression typeExpr, AstExpression init, List<AstDirective> Directives = null, ILocation Location = null) : base(Directives, Location)
        {
            this.Name = name;
            this.TypeExpr = typeExpr;
            this.Initializer = init;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default(D)) => visitor.VisitVariableDeclaration(this, data);

        public override AstStatement Clone() => CopyValuesTo(new AstVariableDecl(Name.Clone() as AstIdentifierExpr, TypeExpr?.Clone(), Initializer?.Clone()));
    }

    #endregion

    #region Enum

    public class AstEnumMember : ILocation
    {
        public ILocation Location { get; private set; }
        public TokenLocation Beginning => Location?.Beginning;
        public TokenLocation End => Location?.End;


        public AstIdentifierExpr Name { get; }
        public AstExpression Value { get; }
        public CheezType Type { get; set; }

        public AstEnumMember(AstIdentifierExpr name, AstExpression value, ILocation Location = null)
        {
            this.Location = Location;
            this.Name = name;
            this.Value = value;
        }

        public AstEnumMember Clone() => new AstEnumMember(Name.Clone() as AstIdentifierExpr, Value?.Clone());
    }

    public class AstEnumDecl : AstStatement, INamed
    {
        public AstIdentifierExpr Name { get; }
        public List<AstEnumMember> Members { get; }
        public CheezType Type { get; set; }

        public AstEnumDecl(AstIdentifierExpr name, List<AstEnumMember> members, List<AstDirective> Directive = null, ILocation Location = null) : base(Directive, Location)
        {
            this.Name = name;
            this.Members = members;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitEnumDeclaration(this, data);

        public override AstStatement Clone() => CopyValuesTo(new AstEnumDecl(Name.Clone() as AstIdentifierExpr, Members.Select(m => m.Clone()).ToList()));
    }

    #endregion

    #region Type Alias

    public class AstTypeAliasDecl : AstStatement
    {
        public AstIdentifierExpr Name { get; set; }
        public AstExpression TypeExpr { get; set; }
        public CheezType Type { get; set; }

        public AstTypeAliasDecl(AstIdentifierExpr name, AstExpression typeExpr, List<AstDirective> Directives = null, ILocation Location = null) : base(Directives, Location)
        {
            this.Name = name;
            this.TypeExpr = typeExpr;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitTypeAlias(this, data);

        public override AstStatement Clone() => CopyValuesTo(new AstTypeAliasDecl(Name.Clone() as AstIdentifierExpr, TypeExpr.Clone()));
    }

    #endregion
}
