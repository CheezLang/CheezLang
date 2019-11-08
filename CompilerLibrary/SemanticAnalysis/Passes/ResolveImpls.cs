using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Types;
using Cheez.Types.Abstract;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;
using Cheez.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cheez
{
    /// <summary>
    /// This pass resolves the types of struct members
    /// </summary>
    public partial class Workspace
    {        

        private bool SizeOfTypeDependsOnSelfType(CheezType type)
        {
            switch (type)
            {
                case SelfType _:
                    return true;

                case StructType str:
                    ComputeStructMembers(str.Declaration);
                    return str.Declaration.Members.Any(m => SizeOfTypeDependsOnSelfType(m.Type));

                case TupleType t:
                    return t.Members.Any(m => SizeOfTypeDependsOnSelfType(m.type));

                case EnumType en:
                    ComputeEnumMembers(en.Declaration);
                    return en.Declaration.Members.Any(m => m.AssociatedType != null ? SizeOfTypeDependsOnSelfType(m.AssociatedType) : false);

                case ArrayType t:
                    return SizeOfTypeDependsOnSelfType(t.TargetType);

                case RangeType r:
                    return SizeOfTypeDependsOnSelfType(r.TargetType);

                case ReferenceType _:
                case PointerType _:
                case SliceType _:
                case FunctionType _:
                case TraitType _:
                case VoidType _:
                case PolyType _:
                case IntType _:
                case FloatType _:
                case BoolType _:
                case CharType _:
                    return false;

                case CheezTypeType _:
                    return false;

                case GenericStructType _:
                case GenericEnumType _:
                case GenericTraitType _:
                case GenericFunctionType _:
                    return false;

                case ErrorType _:
                    return false;

                default:
                    throw new NotImplementedException();
            }
        }

        private void Pass3Trait(AstTraitDeclaration trait)
        {
            if (trait.GetFlag(StmtFlags.MembersComputed))
                return;

            foreach (var p in trait.Parameters)
            {
                trait.SubScope.DefineTypeSymbol(p.Name.Name, p.Value as CheezType);
            }
            trait.SubScope.DefineTypeSymbol("Self", new SelfType(trait.Type));

            foreach (var v in trait.Variables)
            {
                v.TypeExpr.Scope = trait.SubScope;
                v.TypeExpr = ResolveTypeNow(v.TypeExpr, out var type);
                v.Type = type;

                var res = trait.SubScope.DefineSymbol(v);
                if (!res.ok)
                {
                    (string, ILocation)? detail = null;
                    if (res.other != null) detail = ("Other declaration here:", res.other);
                    ReportError(v.Name, $"A symbol with name '{v.Name.Name}' already exists in current scope", detail);
                }
            }

            foreach (var f in trait.Functions)
            {
                f.Trait = trait;
                f.Scope = trait.SubScope;
                f.ConstScope = new Scope($"fn$ {f.Name}", f.Scope);
                f.SubScope = new Scope($"fn {f.Name}", f.ConstScope);

                InferTypeFuncExpr(f);
                CheckForSelfParam(f);

                foreach (var p in f.Parameters)
                {
                    if (SizeOfTypeDependsOnSelfType(p.Type))
                    {
                        f.ExcludeFromVTable = true;
                    }
                }

                if (SizeOfTypeDependsOnSelfType(f.ReturnType))
                {
                    f.ExcludeFromVTable = true;
                }

                // TODO: for now don't allow default implemenation
                if (f.Body != null)
                {
                    ReportError(f.ParameterLocation, $"Trait functions can't have an implementation");
                }
            }

            trait.SetFlag(StmtFlags.MembersComputed);
        }

        private void Pass3TraitImpl(AstImplBlock impl)
        {
            impl.TraitExpr.Scope = impl.SubScope;

            if (impl.IsPolymorphic)
            {
                // setup scopes
                foreach (var param in impl.Parameters)
                {
                    if (param.Name != null)
                        impl.SubScope.DefineTypeSymbol(param.Name.Name, new PolyType(param.Name.Name, true));

                    param.Scope = impl.Scope;
                    param.TypeExpr.Scope = impl.Scope;
                    param.TypeExpr.SetFlag(ExprFlags.ValueRequired, true);
                    param.TypeExpr = ResolveTypeNow(param.TypeExpr, out var newType, forceInfer: true);

                    if (newType is AbstractType)
                    {
                        continue;
                    }

                    param.Type = newType;

                    switch (param.Type)
                    {
                        case CheezTypeType _:
                            param.Value = new PolyType(param.Name.Name, true);
                            break;

                        case IntType _:
                        case FloatType _:
                        case BoolType _:
                        case CharType _:
                            break;

                        case ErrorType _:
                            break;

                        default:
                            ReportError(param.TypeExpr, $"The type '{param.Type}' is not allowed here.");
                            break;
                    }
                }
            }

            impl.TraitExpr.SetFlag(ExprFlags.ValueRequired, true);
            impl.TraitExpr = ResolveTypeNow(impl.TraitExpr, out var type, resolve_poly_expr_to_concrete_type: true);


            if (type is TraitType tt)
            {
                impl.Trait = tt;
            }
            else
            {
                if (!type.IsErrorType)
                    ReportError(impl.TraitExpr, $"{type} is not a trait");
                impl.Trait = new TraitErrorType();
                return;
            }

            impl.TargetTypeExpr.SetFlag(ExprFlags.ValueRequired, true);
            impl.TargetTypeExpr.Scope = impl.SubScope;
            impl.TargetTypeExpr = ResolveTypeNow(impl.TargetTypeExpr, out var t, resolve_poly_expr_to_concrete_type: !impl.IsPolymorphic);
            impl.TargetType = t;

            if (impl.Conditions != null)
            {
                if (impl.Parameters == null)
                    ReportError(new Location(impl.Conditions.First().Location.Beginning, impl.Conditions.Last().Location.End), $"An impl block can't have a condition without parameters");

                foreach (var cond in impl.Conditions)
                {
                    cond.Scope = impl.SubScope;
                }
            }

            if (impl.IsPolymorphic)
                return;

            impl.SubScope.DefineTypeSymbol("Self", impl.TargetType);

            // register impl in trait
            if (impl.Trait.Declaration.Implementations.TryGetValue(impl.TargetType, out var otherImpl))
            {
                ReportError(impl.TargetTypeExpr, $"There already exists an implementation of trait {impl.Trait} for type {impl.TargetType}",
                    ("Other implementation here:", otherImpl.TargetTypeExpr));
            }
            impl.Trait.Declaration.Implementations[impl.TargetType] = impl;

            // @TODO: should not be necessary
            AddTraitForType(impl.TargetType, impl);

            // handle functions
            foreach (var f in impl.Functions)
            {
                f.Scope = impl.SubScope;
                f.ConstScope = new Scope($"fn$ {f.Name}", f.Scope);
                f.SubScope = new Scope($"fn {f.Name}", f.ConstScope);
                f.ImplBlock = impl;

                InferTypeFuncExpr(f);
                CheckForSelfParam(f);
                impl.Scope.DefineImplFunction(f);
                impl.SubScope.DefineSymbol(f);

                if (f.Body == null)
                {
                    ReportError(f.ParameterLocation, $"Function must have an implementation");
                }
            }



            // check if type has required members
            if (impl.Trait.Declaration.Variables.Count > 0) do
            {
                if (!(impl.TargetType is StructType str))
                {
                    ReportError(impl.TargetTypeExpr, $"Can't implement trait '{impl.Trait}' for non struct type '{impl.TargetType}' because the trait requires members",
                        ("Trait defined here:", impl.Trait.Declaration.Name.Location));
                    break;
                }

                var strDecl = str.Declaration;

                foreach (var v in impl.Trait.Declaration.Variables)
                {
                    ComputeStructMembers(strDecl);
                    var member = strDecl.Members.FirstOrDefault(m => m.Name == v.Name.Name);
                    if (member == null)
                    {
                        ReportError(impl.TraitExpr, $"Can't implement trait '{impl.Trait}' for type '{impl.TargetType}' because it misses member '{v.Name.Name}: {v.Type}'",
                            ("Trait member defined here:", v.Location));
                        continue;
                    }
                    if (!member.IsPublic || member.IsReadOnly)
                    {
                        ReportError(impl.TraitExpr, $"Can't implement trait '{impl.Trait}' for type '{impl.TargetType}' because member '{member.Name}: {member.Type}' is not public",
                            ("Struct member defined here:", member.Location));
                        continue;
                    }
                    if (member.Type != v.Type)
                    {
                        ReportError(impl.TraitExpr, $"Can't implement trait '{impl.Trait}' for type '{impl.TargetType}' because '{member.Name}' has a different type than the trait member",
                            ("Struct member defined here:", member.Decl.TypeExpr.Location),
                            ("Trait member defined here:", v.TypeExpr.Location));
                        continue;
                    }
                }
            } while (false);

            // match functions against trait functions
            foreach (var traitFunc in impl.Trait.Declaration.Functions)
            {
                // find matching function
                bool found = false;
                foreach (var func in impl.Functions)
                {
                    if (func.Name != traitFunc.Name)
                        continue;

                    func.TraitFunction = traitFunc;
                    found = true;

                    if (func.SelfType != traitFunc.SelfType)
                    {
                        ReportError(func.ParameterLocation, 
                            $"The self parameter of this function doesn't match the trait functions self parameter",
                            ("Trait function defined here:", traitFunc.ParameterLocation));
                        continue;
                    }

                    if (func.Parameters.Count != traitFunc.Parameters.Count)
                    {
                        ReportError(func.ParameterLocation, $"This function must take the same parameters as the corresponding trait function", ("Trait function defined here:", traitFunc));
                        continue;
                    }

                    int i = 0;
                    // if first param is self param, skip it
                    if (func.SelfType != SelfParamType.None)
                        i = 1;

                    for (; i < func.Parameters.Count; i++)
                    {
                        var fp = func.Parameters[i];
                        var tp = traitFunc.Parameters[i];

                        if (tp.Type is SelfType)
                        {
                            if (fp.Type != impl.TargetType)
                                ReportError(
                                    fp.TypeExpr,
                                    $"Type of parameter '{fp.Type}' must be the implemented type '{impl.TargetType}'",
                                    ("Trait function parameter type defined here:", tp.TypeExpr));
                        }
                        else if (!CheezType.TypesMatch(fp.Type, tp.Type))
                        {
                            ReportError(
                                fp.TypeExpr,
                                $"Type of parameter '{fp.Type}' must match the type of the trait functions parameter '{tp.Type}'",
                                ("Trait function parameter type defined here:", tp.TypeExpr));
                        }
                    }

                    // check return type
                    if (traitFunc.ReturnType is SelfType)
                    {
                        if (func.ReturnType != impl.TargetType)
                            ReportError(
                                func.ReturnTypeExpr?.Location ?? func.ParameterLocation,
                                $"Return type '{func.ReturnType}' must be the implemented type '{impl.TargetType}'",
                                ("Trait function parameter type defined here:", traitFunc.ReturnTypeExpr?.Location ?? traitFunc.ParameterLocation));
                    } else if (!CheezType.TypesMatch(func.ReturnType, traitFunc.ReturnType))
                    {
                        ReportError(
                            func.ReturnTypeExpr?.Location ?? func.ParameterLocation,
                            $"Return type must match the trait functions return type",
                            ("Trait function parameter type defined here:", traitFunc.ReturnTypeExpr?.Location ?? traitFunc.ParameterLocation));
                    }
                }

                if (!found)
                {
                    ReportError(
                        impl.TargetTypeExpr,
                        $"Missing implementation for trait function '{traitFunc.Name}'",
                        ("Trait function defined here:", traitFunc));
                }
            }
        }

        private void Pass3Impl(AstImplBlock impl)
        {
            Log($"Pass3Impl {impl.Accept(new SignatureAstPrinter())}", $"poly = {impl.IsPolymorphic}");
            PushLogScope();

            try
            {

                // check if there are parameters
                if (impl.IsPolymorphic)
                {
                    // setup scopes
                    foreach (var param in impl.Parameters)
                    {
                        if (param.Name != null)
                            impl.SubScope.DefineTypeSymbol(param.Name.Name, new PolyType(param.Name.Name, true));

                        param.Scope = impl.Scope;
                        param.TypeExpr.Scope = impl.Scope;
                        param.TypeExpr.SetFlag(ExprFlags.ValueRequired, true);
                        param.TypeExpr = ResolveTypeNow(param.TypeExpr, out var newType, forceInfer: true);

                        if (newType is AbstractType)
                        {
                            continue;
                        }

                        param.Type = newType;

                        switch (param.Type)
                        {
                            case CheezTypeType _:
                                param.Value = new PolyType(param.Name.Name, true);
                                break;

                            case IntType _:
                            case FloatType _:
                            case BoolType _:
                            case CharType _:
                                break;

                            case ErrorType _:
                                break;

                            default:
                                ReportError(param.TypeExpr, $"The type '{param.Type}' is not allowed here.");
                                break;
                        }
                    }
                }

                impl.TargetTypeExpr.SetFlag(ExprFlags.ValueRequired, true);
                impl.TargetTypeExpr.Scope = impl.SubScope;
                impl.TargetTypeExpr = ResolveTypeNow(impl.TargetTypeExpr, out var t, resolve_poly_expr_to_concrete_type: !impl.IsPolymorphic);
                impl.TargetType = t;

                // @TODO: check if target type expr contains poly names such as '$T', these are not allowed
                if (false)
                {
                    ReportError(impl.TargetTypeExpr, $"Target type of impl can't be polymorphic");
                }

                // @TODO: does it make sense to allow conditions on impl blocks without parameters?
                // for now don't allow these

                if (impl.Conditions != null)
                {
                    if (impl.Parameters == null)
                        ReportError(new Location(impl.Conditions.First().Location.Beginning, impl.Conditions.Last().Location.End), $"An impl block can't have a condition without parameters");

                    foreach (var cond in impl.Conditions)
                    {
                        cond.Scope = impl.SubScope;
                    }
                }

                if (impl.IsPolymorphic)
                    return;

                impl.SubScope.DefineTypeSymbol("Self", impl.TargetType);

                foreach (var f in impl.Functions)
                {
                    f.Scope = impl.SubScope;
                    f.ConstScope = new Scope($"fn$ {f.Name}", f.Scope);
                    f.SubScope = new Scope($"fn {f.Name}", f.ConstScope);
                    f.ImplBlock = impl;

                    InferTypeFuncExpr(f);
                    CheckForSelfParam(f);
                    impl.Scope.DefineImplFunction(f);
                    impl.SubScope.DefineSymbol(f);
                }
            }
            finally
            {
                PopLogScope();
                Log($"Finished Pass3Impl {impl.Accept(new SignatureAstPrinter())}", $"poly = {impl.IsPolymorphic}");
            }
        }
    }
}
