using Cheez.Ast.Expressions;
using Cheez.Ast.Expressions.Types;
using Cheez.Extras;
using Cheez.Types;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;
using Cheez.Visitors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Cheez.Ast.Statements
{
    public abstract class AstDecl : AstStatement, ISymbol
    {
        public AstIdExpr Name { get; set; }
        public CheezType Type { get; set; }

        public HashSet<AstDecl> Dependencies { get; set; } = new HashSet<AstDecl>();

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

        public bool IsReturnParam { get; set; } = false;

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
        public TReturn Accept<TReturn, TData>(IVisitor<TReturn, TData> visitor, TData data = default) => visitor.VisitParameter(this, data);

        public override string ToString()
        {
            return new RawAstPrinter(null).VisitParameter(this);
        }
    }

    public class AstConstantDeclaration : AstDecl
    {
        public AstExpression Pattern { get; set; }
        public AstExpression TypeExpr { get; set; }
        public AstExpression Initializer { get; set; }

        public object Value { get; set; }

        public AstConstantDeclaration(AstExpression pattern, AstExpression typeExpr, AstExpression init, ILocation Location = null)
            : base(pattern is AstIdExpr ? (pattern as AstIdExpr) : (new AstIdExpr(pattern.ToString(), false, pattern.Location)), null, Location)
        {
            this.Pattern = pattern;
            this.TypeExpr = typeExpr;
            this.Initializer = init;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitConstantDeclaration(this, data);

        public override AstStatement Clone()
            => CopyValuesTo(new AstConstantDeclaration(
                Pattern.Clone(),
                TypeExpr?.Clone(),
                Initializer.Clone()));
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
        public AstFunctionDecl Template { get; set; } = null;

        public SelfParamType SelfType { get; set; } = SelfParamType.None;
        public bool IsGeneric { get; set; } = false;
        public bool IsPolyInstance { get; set; } = false;
        public List<ILocation> InstantiatedAt { get; private set; } = null;
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
        public override TReturn Accept<TReturn, TData>(IVisitor<TReturn, TData> visitor, TData data = default) => visitor.VisitFunctionDecl(this, data);

        public override AstStatement Clone()
        {
            var copy = CopyValuesTo(new AstFunctionDecl(
                Name.Clone() as AstIdExpr,
                Parameters.Select(p => p.Clone()).ToList(),
                ReturnTypeExpr?.Clone(),
                Body?.Clone() as AstBlockExpr, 
                ParameterLocation: ParameterLocation));
            copy.ConstScope = new Scope($"fn$ {Name.Name}", copy.Scope);
            copy.SubScope = new Scope($"fn {Name.Name}", copy.ConstScope);
            return copy;
        }

        public void AddInstantiatedAt(ILocation loc)
        {
            if (InstantiatedAt == null)
            {
                InstantiatedAt = new List<ILocation>();
            }

            InstantiatedAt.Add(loc);
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
        public override TReturn Accept<TReturn, TData>(IVisitor<TReturn, TData> visitor, TData data = default) => visitor.VisitStructDecl(this, data);

        public override AstStatement Clone() => CopyValuesTo(new AstStructDecl(Name.Clone() as AstIdExpr, Parameters?.Select(p => p.Clone()).ToList(), Members.Select(m => m.Clone()).ToList()));
    }

    public class AstStructMemberNew
    {

        public bool IsPublic { get; }
        public bool IsReadOnly { get; }
        public AstVariableDecl Decl { get; }
        public string Name => Decl.Name.Name;
        public CheezType Type => Decl.Type;
        public ILocation Location => Decl.Location;
        public int Index { get; }


        public AstStructMemberNew(AstVariableDecl decl, bool pub, bool readOnly, int index)
        {
            Decl = decl;
            IsPublic = pub;
            IsReadOnly = readOnly;
            Index = index;
        }
    }

    public class AstStructTypeExpr : AstExpression
    {
        public string Name { get; set; } = "#anonymous";
        public List<AstParameter> Parameters { get; set; }
        public List<AstDecl> Declarations { get; }
        public List<AstStructMemberNew> Members { get; set; }

        public StructType StructType => Value as StructType;

        public AstStructTypeExpr Template { get; set; } = null;

        public Scope SubScope { get; set; }

        public bool _isPolymorphic { get; set; }
        public override bool IsPolymorphic => _isPolymorphic;

        public bool IsPolyInstance { get; set; }

        public List<AstStructTypeExpr> PolymorphicInstances { get; } = new List<AstStructTypeExpr>();

        public List<TraitType> Traits { get; } = new List<TraitType>();
        public List<AstDirective> Directives { get; protected set; }

        public AstStructTypeExpr(List<AstParameter> param, List<AstDecl> declarations, List<AstDirective> Directives = null, ILocation Location = null)
            : base(Location)
        {
            this.Parameters = param ?? new List<AstParameter>();
            this.Declarations = declarations;
            this._isPolymorphic = Parameters.Count > 0;
            this.Directives = Directives;
        }

        [DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IVisitor<TReturn, TData> visitor, TData data = default) => visitor.VisitStructTypeExpr(this, data);

        public override AstExpression Clone() => CopyValuesTo(
            new AstStructTypeExpr(
                Parameters.Select(p => p.Clone()).ToList(),
                Declarations.Select(m => m.Clone() as AstDecl).ToList(),
                Directives.Select(d => d.Clone()).ToList()));

        public bool HasDirective(string name) => Directives?.Find(d => d.Name.Name == name) != null;

        public AstDirective GetDirective(string name)
        {
            return Directives?.FirstOrDefault(d => d.Name.Name == name);
        }

        public bool TryGetDirective(string name, out AstDirective dir)
        {
            dir = Directives?.FirstOrDefault(d => d.Name.Name == name);
            return dir != null;
        }
    }


    public class AstEnumMemberNew
    {

        public AstVariableDecl Decl { get; }
        public string Name => Decl.Name.Name;
        public CheezType AssociatedType => Decl.Type;
        public AstExpression AssociatedTypeExpr => Decl.TypeExpr;
        public ILocation Location => Decl.Location;
        public int Index { get; }
        public NumberData Value { get; set; }


        public AstEnumMemberNew(AstVariableDecl decl, int index)
        {
            this.Decl = decl;
            this.Index = index;
        }
    }

    public class AstEnumTypeExpr : AstExpression
    {
        public string Name { get; set; } = "#anonymous";
        public List<AstParameter> Parameters { get; set; }
        public List<AstDecl> Declarations { get; }
        public List<AstEnumMemberNew> Members { get; set; }

        public EnumType EnumType => Value as EnumType;
        public IntType TagType { get; set; }

        public AstEnumTypeExpr Template { get; set; } = null;

        public Scope SubScope { get; set; }

        public bool _isPolymorphic { get; set; }
        public override bool IsPolymorphic => _isPolymorphic;

        public bool IsPolyInstance { get; set; }

        public List<AstEnumTypeExpr> PolymorphicInstances { get; } = new List<AstEnumTypeExpr>();

        public List<TraitType> Traits { get; } = new List<TraitType>();
        public List<AstDirective> Directives { get; protected set; }

        public bool MembersComputed { get; set; } = false;

        public AstEnumTypeExpr(List<AstParameter> param, List<AstDecl> declarations, List<AstDirective> Directives = null, ILocation Location = null)
            : base(Location)
        {
            this.Parameters = param ?? new List<AstParameter>();
            this.Declarations = declarations;
            this._isPolymorphic = Parameters.Count > 0;
            this.Directives = Directives;
        }

        [DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IVisitor<TReturn, TData> visitor, TData data = default) => visitor.VisitEnumTypeExpr(this, data);

        public override AstExpression Clone() => CopyValuesTo(
            new AstEnumTypeExpr(
                Parameters.Select(p => p.Clone()).ToList(),
                Declarations.Select(m => m.Clone() as AstDecl).ToList(),
                Directives.Select(d => d.Clone()).ToList()));

        public bool HasDirective(string name) => Directives?.Find(d => d.Name.Name == name) != null;

        public AstDirective GetDirective(string name)
        {
            return Directives?.FirstOrDefault(d => d.Name.Name == name);
        }

        public bool TryGetDirective(string name, out AstDirective dir)
        {
            dir = Directives?.FirstOrDefault(d => d.Name.Name == name);
            return dir != null;
        }
    }

    #endregion

    #region Trait

    public class AstTraitDeclaration : AstDecl, ITypedSymbol
    {
        public List<AstParameter> Parameters { get; set; }

        public List<AstFunctionDecl> Functions { get; }
        public List<AstStructMember> Variables { get; }

        public Dictionary<CheezType, AstImplBlock> Implementations { get; } = new Dictionary<CheezType, AstImplBlock>();

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

        public override TReturn Accept<TReturn, TData>(IVisitor<TReturn, TData> visitor, TData data = default) => visitor.VisitTraitDecl(this, data);

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
        public ILocation Location { get; set; }

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

    public class ImplConditionAny : ImplCondition
    {
        public AstExpression Expr { get; set; }

        public ImplConditionAny(AstExpression expr, ILocation Location)
            : base(Location)
        {
            this.Expr = expr;
        }

        public override ImplCondition Clone() => new ImplConditionAny(Expr.Clone(), Location);
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
        public bool IsPolyInstance { get; set; } = false;
        public bool IsPolymorphic { get; set; } = false;

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

        public override TReturn Accept<TReturn, TData>(IVisitor<TReturn, TData> visitor, TData data = default) => visitor.VisitImplDecl(this, data);

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

        public AstSingleVariableDecl(AstIdExpr name, AstExpression typeExpr, AstVariableDecl parent, ILocation Location) : base(name, Location: Location)
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

        public HashSet<AstSingleVariableDecl> VarDependencies { get; set; }

        public List<AstSingleVariableDecl> SubDeclarations { get; set; } = new List<AstSingleVariableDecl>();

        public AstVariableDecl(AstExpression pattern, AstExpression typeExpr, AstExpression init, List<AstDirective> Directives = null, ILocation Location = null)
            : base(pattern is AstIdExpr ? (pattern as AstIdExpr) : (new AstIdExpr(pattern.ToString(), false, pattern.Location)), Directives, Location)
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
                TypeExpr?.Clone(),
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