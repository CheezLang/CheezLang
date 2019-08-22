using Cheez.Ast.Expressions;
using Cheez.Ast.Expressions.Types;
using Cheez.Types;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;
using Cheez.Visitors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Cheez.Ast.Statements
{
    public enum DependencyKind
    {
        Type,
        Value
    }

    public abstract class AstDecl : AstStatement, ISymbol
    {
        public AstIdExpr Name { get; set; }
        public CheezType Type { get; set; }

        public HashSet<(DependencyKind kind, AstDecl decl)> Dependencies { get; set; } = new HashSet<(DependencyKind, AstDecl)>();

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

        public AstIdExpr Name { get; set; }
        public CheezType Type { get; set; }
        public AstExpression TypeExpr { get; set; }
        public AstExpression DefaultValue { get; set; }

        public Scope Scope { get; set; }

        public ISymbol Symbol { get; set; } = null;

        public object Value { get; set; }

        public bool IsConstant => true;

        public AstParameter(AstIdExpr name, AstExpression typeExpr, AstExpression defaultValue, ILocation Location = null)
        {
            this.Location = Location;
            this.Name = name;
            this.TypeExpr = typeExpr;
            this.DefaultValue = defaultValue;
        }

        public AstParameter Clone()
        {
            return new AstParameter(Name?.Clone() as AstIdExpr, TypeExpr?.Clone(), DefaultValue?.Clone(), Location);
        }

        [DebuggerStepThrough]
        public T Accept<T, D>(IVisitor<T, D> visitor, D data = default(D)) => visitor.VisitParameter(this, data);

        public override string ToString()
        {
            return new RawAstPrinter(null).VisitParameter(this);
        }
    }

    #region Function Declaration

    public enum SelfParamType
    {
        None,
        Value,
        Reference
    }

    public class AstFunctionDecl : AstDecl, ITypedSymbol
    {
        public Scope ConstScope { get; set; }
        public Scope SubScope { get; set; }

        public List<AstParameter> Parameters { get; set; }
        public AstParameter ReturnTypeExpr { get; }

        public FunctionType FunctionType => Type as FunctionType;
        public CheezType ReturnType => ReturnTypeExpr?.Type ?? CheezType.Void;

        public AstBlockExpr Body { get; private set; }

        public List<AstFunctionDecl> PolymorphicInstances { get; } = new List<AstFunctionDecl>();

        public SelfParamType SelfType { get; set; } = SelfParamType.None;
        public bool IsGeneric { get; set; } = false;
        public bool IsConstant => true;
        public bool IsPolyInstance { get; set; } = false;
        public AstTraitDeclaration Trait { get; set; } = null;

        public Dictionary<string, CheezType> PolymorphicTypes { get; internal set; }
        public Dictionary<string, (CheezType type, object value)> ConstParameters { get; internal set; }

        private AstImplBlock _ImplBlock;
        public AstImplBlock ImplBlock
        {
            get => _ImplBlock;
            set
            {
                _ImplBlock = value;
            }
        }
        public AstFunctionDecl TraitFunction { get; internal set; }
        public ILocation ParameterLocation { get; internal set; }

        public AstFunctionDecl(AstIdExpr name,
            List<AstIdExpr> generics,
            List<AstParameter> parameters,
            AstParameter returns,
            AstBlockExpr body = null,
            List<AstDirective> Directives = null,
            ILocation Location = null,
            ILocation ParameterLocation = null)
            : base(name, Directives, Location)
        {
            this.Parameters = parameters;
            this.ReturnTypeExpr = returns;
            this.Body = body;
            this.ParameterLocation = ParameterLocation;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default(D)) => visitor.VisitFunctionDecl(this, data);

        public override AstStatement Clone()
        {
            var copy = CopyValuesTo(new AstFunctionDecl(Name.Clone() as AstIdExpr,
                null,
                Parameters.Select(p => p.Clone()).ToList(),
                ReturnTypeExpr?.Clone(),
                Body?.Clone() as AstBlockExpr, ParameterLocation: ParameterLocation));
            copy.ConstScope = new Scope("fn$", copy.Scope);
            copy.SubScope = new Scope("fn", copy.ConstScope);
            return copy;
        }
    }

    #endregion

    #region Struct Declaration

    public class AstStructMember : ISymbol
    {
        internal bool IsPublic;
        internal bool IsReadOnly;

        public ILocation Location { get; private set; }
        public TokenLocation Beginning => Location?.Beginning;
        public TokenLocation End => Location?.End;

        public AstIdExpr Name { get; }
        public AstExpression Initializer { get; set; }
        public AstExpression TypeExpr { get; set; }
        public CheezType Type { get; set; }

        public int Index { get; set; }

        public AstStructMember(AstIdExpr name, AstExpression typeExpr, AstExpression init, ILocation Location = null)
        {
            this.Location = Location;
            this.Name = name;
            this.TypeExpr = typeExpr;
            this.Initializer = init;
        }

        public AstStructMember Clone()
            => new AstStructMember(Name.Clone() as AstIdExpr, TypeExpr.Clone(), Initializer?.Clone())
            {
                IsPublic = IsPublic,
                IsReadOnly = IsReadOnly,
                Location = Location
            };

        public override string ToString()
        {
            return new RawAstPrinter(null).VisitStructMember(this);
        }
    }

    public class AstStructDecl : AstDecl, ITypedSymbol
    {
        public List<AstStructMember> Members { get; }
        public List<AstParameter> Parameters { get; set; }

        public AstStructDecl Template { get; set; } = null;

        public Scope SubScope { get; set; }

        public bool IsPolymorphic { get; set; }
        public bool IsPolyInstance { get; set; }

        public bool IsConstant => true;

        public List<AstStructDecl> PolymorphicInstances { get; } = new List<AstStructDecl>();

        //public List<AstImplBlock> Implementations { get; } = new List<AstImplBlock>();
        public List<TraitType> Traits { get; } = new List<TraitType>();

        public AstStructDecl(AstIdExpr name, List<AstParameter> param, List<AstStructMember> members, List<AstDirective> Directives = null, ILocation Location = null)
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
        public List<AstStructMember> Variables { get; }

        public Dictionary<CheezType, AstImplBlock> Implementations { get; } = new Dictionary<CheezType, AstImplBlock>();

        public bool IsConstant => true;

        public bool IsPolymorphic { get; set; }
        public bool IsPolyInstance { get; set; }

        public List<AstTraitDeclaration> PolymorphicInstances { get; } = new List<AstTraitDeclaration>();
        public AstTraitDeclaration Template { get; set; } = null;

        public Scope SubScope { get; set; }

        public AstTraitDeclaration(
            AstIdExpr name,
            List<AstParameter> parameters,
            List<AstFunctionDecl> functions,
            List<AstStructMember> variables,
            ILocation Location = null)
            : base(name, Location: Location)
        {
            this.Parameters = parameters;
            this.Functions = functions;
            this.Variables = variables;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitTraitDecl(this, data);

        public override AstStatement Clone() => CopyValuesTo(
            new AstTraitDeclaration(
                Name.Clone() as AstIdExpr,
                Parameters.Select(p => p.Clone()).ToList(),
                Functions.Select(f => f.Clone() as AstFunctionDecl).ToList(),
                Variables.Select(v => v.Clone()).ToList()));

        public AstImplBlock FindMatchingImplementation(CheezType from)
        {
            foreach (var kv in Implementations)
            {
                var type = kv.Key;
                var impl = kv.Value;
                if (CheezType.TypesMatch(type, from))
                {
                    return impl;
                }
            }

            return null;
        }
    }

    public abstract class ImplCondition
    {
        public ILocation Location;

        public abstract ImplCondition Clone();

        public Scope Scope { get; set; }

        public ImplCondition(ILocation Location)
        {
            this.Location = Location;
        }
    }

    public class ImplConditionImplTrait : ImplCondition
    {
        public AstExpression type { get; set; }
        public AstExpression trait { get; set; }

        public ImplConditionImplTrait(AstExpression type, AstExpression trait, ILocation Location)
            : base(Location)
        {
            this.type = type;
            this.trait = trait;
        }

        public override ImplCondition Clone()
        {
            return new ImplConditionImplTrait(type.Clone(), trait.Clone(), Location);
        }
    }

    public class ImplConditionNotYet : ImplCondition
    {
        public ImplConditionNotYet(ILocation Location)
            : base(Location)
        { }

        public override ImplCondition Clone() => new ImplConditionNotYet(Location);
    }

    public class AstImplBlock : AstStatement
    {
        public List<AstParameter> Parameters { get; set; }

        public AstExpression TargetTypeExpr { get; set; }
        public CheezType TargetType { get; set; }

        public AstExpression TraitExpr { get; set; }
        public TraitType Trait { get; set; }

        public List<ImplCondition> Conditions { get; set; }

        public List<AstFunctionDecl> Functions { get; }

        public Scope SubScope { get; set; }

        public AstImplBlock Template { get; set; } = null;
        public List<AstImplBlock> PolyInstances { get; set; } = new List<AstImplBlock>();
        public bool IsPolyInstance = false;
        public bool IsPolymorphic = false;

        public AstImplBlock(
            List<AstParameter> parameters,
            AstExpression targetTypeExpr,
            AstExpression traitExpr,
            List<ImplCondition> conditions,
            List<AstFunctionDecl> functions,
            ILocation Location = null) : base(Location: Location)
        {
            this.Parameters = parameters;
            this.TargetTypeExpr = targetTypeExpr;
            this.TraitExpr = traitExpr;
            this.Conditions = conditions;
            this.Functions = functions;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitImplDecl(this, data);

        public override AstStatement Clone()
            => CopyValuesTo(new AstImplBlock(
                Parameters?.Select(p => p.Clone()).ToList(),
                TargetTypeExpr.Clone(),
                TraitExpr?.Clone(),
                Conditions?.Select(c => c.Clone()).ToList(),
                Functions.Select(f => f.Clone() as AstFunctionDecl).ToList()
                ));

        public override string ToString()
        {
            return Accept(new SignatureAstPrinter(true));
        }
    }

    #endregion

    #region Variable Declarion

    public class AstSingleVariableDecl : AstDecl, ITypedSymbol
    {
        public AstExpression TypeExpr { get; set; }
        public AstExpression Initializer { get; set; }

        public AstVariableDecl VarDeclaration { get; set; }

        public object Value { get; set; } = null;

        public bool Constant = false;

        public AstSingleVariableDecl(AstIdExpr name, AstExpression typeExpr, AstVariableDecl parent, bool isConst, ILocation Location) : base(name, Location: Location)
        {
            TypeExpr = typeExpr;
            VarDeclaration = parent;
            this.Constant = isConst;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            throw new System.NotImplementedException();
        }

        public override AstStatement Clone()
        {
            throw new System.NotImplementedException();
        }

        public override string ToString()
        {
            return $"@singlevar({Name.Name}, {VarDeclaration})";
        }
    }

    public class AstVariableDecl : AstDecl
    {
        public AstExpression Pattern { get; set; }
        public AstExpression TypeExpr { get; set; }
        public AstExpression Initializer { get; set; }
        public Scope SubScope { get; set; }

        public bool Constant = false;

        public HashSet<AstSingleVariableDecl> VarDependencies { get; set; }

        public List<AstSingleVariableDecl> SubDeclarations = new List<AstSingleVariableDecl>();

        public AstVariableDecl(AstExpression pattern, AstExpression typeExpr, AstExpression init, bool isConst, List<AstDirective> Directives = null, ILocation Location = null)
            : base(pattern is AstIdExpr ? (pattern as AstIdExpr) : (new AstIdExpr(pattern.ToString(), false, pattern.Location)), Directives, Location)
        {
            this.Pattern = pattern;
            this.TypeExpr = typeExpr;
            this.Initializer = init;
            this.Constant = isConst;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default(D)) => visitor.VisitVariableDecl(this, data);

        public override AstStatement Clone()
            => CopyValuesTo(new AstVariableDecl(
                Pattern.Clone(),
                TypeExpr?.Clone(),
                Initializer?.Clone(),
                Constant));
    }

    #endregion

    #region Enum

    public class AstEnumMember : ILocation
    {
        public ILocation Location { get; private set; }
        public TokenLocation Beginning => Location?.Beginning;
        public TokenLocation End => Location?.End;

        public AstIdExpr Name { get; }
        public AstExpression Value { get; set; }
        public AstExpression AssociatedTypeExpr { get; set; }
        public CheezType AssociatedType => AssociatedTypeExpr?.Value as CheezType;

        public AstEnumMember(AstIdExpr name, AstExpression assType, AstExpression value, ILocation Location = null)
        {
            this.Location = Location;
            this.Name = name;
            this.Value = value;
            this.AssociatedTypeExpr = assType;
        }

        public AstEnumMember Clone() => new AstEnumMember(Name.Clone() as AstIdExpr, AssociatedTypeExpr?.Clone(), Value?.Clone());
    }

    public class AstEnumDecl : AstDecl
    {
        public Scope SubScope { get; set; }
        public List<AstEnumMember> Members { get; }
        public List<AstParameter> Parameters { get; set; }

        public EnumType EnumType => Type as EnumType;
        public IntType TagType { get; set; }
        public bool HasAssociatedTypes { get; set; } = false;
        public bool IsPolymorphic { get; internal set; }
        public bool IsPolyInstance { get; set; }

        public List<AstEnumDecl> PolymorphicInstances { get; } = new List<AstEnumDecl>();
        public AstEnumDecl Template { get; set; } = null;

        public AstEnumDecl(AstIdExpr name, List<AstEnumMember> members, List<AstParameter> parameters, List<AstDirective> Directive = null, ILocation Location = null)
            : base(name, Directive, Location)
        {
            this.Members = members;
            this.Parameters = parameters ?? new List<AstParameter>();
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitEnumDecl(this, data);

        public override AstStatement Clone()
            => CopyValuesTo(new AstEnumDecl(
                Name.Clone() as AstIdExpr, 
                Members.Select(m => m.Clone()).ToList(),
                Parameters?.Select(p => p.Clone())?.ToList()));
    }

    #endregion

    #region Type Alias

    public class AstTypeAliasDecl : AstDecl, ITypedSymbol
    {
        public AstExpression TypeExpr { get; set; }

        public bool IsConstant => false;

        public AstTypeAliasDecl(AstIdExpr name, AstExpression typeExpr, List<AstDirective> Directives = null, ILocation Location = null)
            : base(name, Directives, Location)
        {
            this.TypeExpr = typeExpr;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitTypeAliasDecl(this, data);

        public override AstStatement Clone()
            => CopyValuesTo(new AstTypeAliasDecl(Name.Clone() as AstIdExpr, TypeExpr.Clone()));
    }

    #endregion
}