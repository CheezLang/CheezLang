using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Expressions.Types;
using Cheez.Ast.Statements;
using Cheez.Extras;
using Cheez.Types;
using Cheez.Types.Abstract;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Cheez
{
    public partial class Workspace
    {
        public AstExpression ResolveType(AstExpression expr, TypeInferenceContext context, out CheezType type)
        {
            expr = InferTypeHelper(expr, CheezType.Type, context);
            return ResolveTypeHelper(expr, out type);
        }

        public AstExpression ResolveType(AstExpression expr, List<AstDecl> newPolyDecls, out CheezType type)
        {
            expr = InferTypeHelper(expr, CheezType.Type, new TypeInferenceContext
            {
                newPolyDeclarations = newPolyDecls
            });
            return ResolveTypeHelper(expr, out type);
        }

        public AstExpression ResolveTypeNow(AstExpression expr, out CheezType type, bool resolve_poly_expr_to_concrete_type = false, HashSet<AstDecl> dependencies = null, bool forceInfer = false)
        {
            expr = InferType(expr, CheezType.Type, resolve_poly_expr_to_concrete_type, dependencies, forceInfer);
            return ResolveTypeHelper(expr, out type);
        }

        public AstExpression ResolveTypeHelper(AstExpression expr, out CheezType type)
        {
            expr = InferType(expr, CheezType.Type);
            if (expr.Type.IsErrorType)
            {
                type = CheezType.Error;
                return expr;
            }

            if (expr.Type != CheezType.Type)
            {
                ReportError(expr, $"Expected type");
                type = CheezType.Error;
                return expr;
            }
            else
            {
                type = expr.Value as CheezType;
            }
            return expr;
        }

        public static void CollectPolyTypes(CheezType param, CheezType arg, Dictionary<string, CheezType> result)
        {
            switch (param)
            {
                case PolyType i:
                    if (i.IsDeclaring && !result.ContainsKey(i.Name))
                    {
                        if (arg is ReferenceType r)
                            result[i.Name] = r.TargetType;
                        else
                            result[i.Name] = arg;
                    }
                    break;

                case PointerType p:
                    {
                        if (arg is PointerType t)
                            CollectPolyTypes(p.TargetType, t.TargetType, result);
                        break;
                    }

                case SliceType p:
                    {
                        if (arg is SliceType t)
                            CollectPolyTypes(p.TargetType, t.TargetType, result);
                        if (arg is ArrayType a)
                            CollectPolyTypes(p.TargetType, a.TargetType, result);
                        break;
                    }

                case ArrayType p:
                    {
                        if (arg is ArrayType t)
                            CollectPolyTypes(p.TargetType, t.TargetType, result);
                        break;
                    }

                case TupleType te:
                    {
                        if (arg is TupleType tt)
                        {
                            if (te.Members.Length == tt.Members.Length)
                            {
                                for (var i = 0; i < te.Members.Length; i++)
                                {
                                    CollectPolyTypes(te.Members[i].type, tt.Members[i].type, result);
                                }
                            }
                        }
                        break;
                    }

                case StructType str:
                    {
                        if (arg is StructType tt)
                        {
                            if (str.Arguments.Length == tt.Arguments.Length)
                            {
                                for (var i = 0; i < str.Arguments.Length; i++)
                                {
                                    CollectPolyTypes(str.Arguments[i], tt.Arguments[i], result);
                                }
                            }
                        }
                        break;
                    }

                case TraitType str:
                    {
                        if (arg is TraitType tt)
                        {
                            if (str.Arguments.Length == tt.Arguments.Length)
                            {
                                for (var i = 0; i < str.Arguments.Length; i++)
                                {
                                    CollectPolyTypes(str.Arguments[i], tt.Arguments[i], result);
                                }
                            }
                        }
                        break;
                    }

                case EnumType str:
                    {
                        if (arg is EnumType tt)
                        {
                            if (str.Arguments.Length == tt.Arguments.Length)
                            {
                                for (var i = 0; i < str.Arguments.Length; i++)
                                {
                                    CollectPolyTypes(str.Arguments[i], tt.Arguments[i], result);
                                }
                            }
                        }
                        break;
                    }

                case ReferenceType r:
                    {
                        if (arg is ReferenceType r2)
                            CollectPolyTypes(r.TargetType, r2.TargetType, result);
                        else
                            CollectPolyTypes(r.TargetType, arg, result);
                        break;
                    }

                case FunctionType f:
                    {
                        if (arg is FunctionType f2 && f.Parameters.Length == f2.Parameters.Length)
                        {
                            for (int i = 0; i < f.Parameters.Length; i++)
                            {
                                CollectPolyTypes(f.Parameters[i].type, f2.Parameters[i].type, result);
                            }

                            if (f.ReturnType != null && f2.ReturnType != null)
                                CollectPolyTypes(f.ReturnType, f2.ReturnType, result);
                        }
                        break;
                    }

                case VoidType _:
                case FloatType _:
                case BoolType _:
                case CheezTypeType _:
                case CharType _:
                case IntType _: break;
                case CodeType _: break;

                case ErrorType _: break;

                case CheezType t:
                    throw new NotImplementedException();
            }
        }

        private void ResolveTypeDeclaration(AstDecl decl)
        {
            ResolveTypeDeclarations(new List<AstDecl> { decl });
        }

        private void ResolveTypeDeclarations(List<AstDecl> declarations)
        {
            var done = new List<AstDecl>();

            // make a copy of declarations
            declarations = new List<AstDecl>(declarations);

            var nextInstances = new List<AstDecl>();

            int i = 0;
            while (i < MaxPolyStructResolveStepCount && declarations.Count != 0)
            {
                foreach (var instance in declarations)
                {
                    switch (instance)
                    {
                        case AstEnumDecl e:
                            if (!e.IsPolymorphic)
                                mEnums.Add(e);
                            ResolveEnum(e, nextInstances);
                            break;

                        case AstStructDecl s:
                            if (!s.IsPolymorphic)
                                mStructs.Add(s);
                            ResolveStruct(s, nextInstances);
                            break;

                        case AstTraitDeclaration trait:
                            if (!trait.IsPolymorphic)
                                mTraits.Add(trait);
                            Pass3Trait(trait);
                            break;
                    }
                }
                done.AddRange(declarations);
                declarations.Clear();

                var t = declarations;
                declarations = nextInstances;
                nextInstances = t;

                i++;
            }

            if (i == MaxPolyStructResolveStepCount)
            {
                var details = declarations.Select(str => ("Here:", str.Location)).ToList();
                ReportError($"Detected a potential infinite loop in polymorphic declarations after {MaxPolyStructResolveStepCount} steps", details);
            }

            CalculateEnumAndStructSizes(done);

            foreach (var d in done)
            {
                if (d is AstStructDecl s)
                    ResolveStructMemberInitializers(s);
            }
        }

        // impl
        private AstImplBlock InstantiatePolyImplNew(AstImplBlock decl, Dictionary<string, CheezType> args, ILocation location = null)
        {
            AstImplBlock instance = null;

            // check for existing instance
            var concreteTrait = decl.Trait != null ? InstantiatePolyType(decl.Trait, args, location) : null;
            var concreteTarget = InstantiatePolyType(decl.TargetType, args, location);

            foreach (var pi in decl.PolyInstances)
            {
                if (pi.Trait == concreteTrait && pi.TargetType == concreteTarget)
                {
                    instance = pi;
                    break;
                }
            }

            if (instance == null)
            {
                instance = decl.Clone() as AstImplBlock;
                instance.SubScope = new Scope($"impl <poly>", instance.Scope);
                instance.IsPolyInstance = true;
                instance.IsPolymorphic = false;
                instance.Parameters = null;
                instance.Conditions = null;
                instance.Template = decl;
                decl.PolyInstances.Add(instance);

                foreach (var kv in args)
                {
                    instance.SubScope.DefineTypeSymbol(kv.Key, kv.Value);
                }


                if (instance.TraitExpr != null)
                    Pass3TraitImpl(instance);
                else
                    Pass3Impl(instance);

                // @TODO: does this work?
                GlobalScope.unresolvedImpls.Enqueue(instance);
            }

            return instance;
        }

        // trait
        private AstTraitDeclaration InstantiatePolyTrait(AstTraitDeclaration decl, List<(CheezType type, object value)> args, List<AstDecl> instances = null, ILocation location = null)
        {
            if (args.Count != decl.Parameters.Count)
            {
                if (location != null)
                    ReportError(location, "Polymorphic instantiation has wrong number of arguments.", ("Declaration here:", decl));
                else
                    ReportError("Polymorphic instantiation has wrong number of arguments.", ("Declaration here:", decl));
                return null;
            }

            AstTraitDeclaration instance = null;

            // check if instance already exists
            foreach (var pi in decl.PolymorphicInstances)
            {
                Debug.Assert(pi.Parameters.Count == args.Count);

                bool eq = true;
                for (int i = 0; i < pi.Parameters.Count; i++)
                {
                    var param = pi.Parameters[i];
                    var arg = args[i];
                    if (!param.Value.Equals(arg.value))
                    {
                        eq = false;
                        break;
                    }
                }

                if (eq)
                {
                    instance = pi;
                    break;
                }
            }

            // instatiate type
            if (instance == null)
            {
                instance = decl.Clone() as AstTraitDeclaration;
                instance.SubScope = new Scope($"trait {decl.Name.Name}<poly>", instance.Scope);
                instance.IsPolyInstance = true;
                instance.IsPolymorphic = false;
                instance.Template = decl;
                decl.PolymorphicInstances.Add(instance);

                Debug.Assert(instance.Parameters.Count == args.Count);

                for (int i = 0; i < instance.Parameters.Count; i++)
                {
                    var param = instance.Parameters[i];
                    var arg = args[i];
                    param.Type = arg.type;
                    param.Value = arg.value;

                    // TODO: non type parameters
                    instance.SubScope.DefineTypeSymbol(param.Name.Name, param.Value as CheezType);
                }

                instance.Type = new TraitType(instance);

                if (instances != null)
                    instances.Add(instance);
                else
                {
                    ResolveTypeDeclaration(instance);
                }
            }

            return instance;
        }

        // enum

        private AstEnumDecl InstantiatePolyEnum(AstEnumDecl decl, List<(CheezType type, object value)> args, List<AstDecl> instances = null, ILocation location = null)
        {
            if (args.Count != decl.Parameters.Count)
            {
                if (location != null)
                    ReportError(location, "Polymorphic instantiation has wrong number of arguments.", ("Declaration here:", decl));
                else
                    ReportError("Polymorphic instantiation has wrong number of arguments.", ("Declaration here:", decl));
                return null;
            }

            AstEnumDecl instance = null;

            // check if instance already exists
            foreach (var pi in decl.PolymorphicInstances)
            {
                Debug.Assert(pi.Parameters.Count == args.Count);

                bool eq = true;
                for (int i = 0; i < pi.Parameters.Count; i++)
                {
                    var param = pi.Parameters[i];
                    var arg = args[i];
                    if (param.Value != arg.value)
                    {
                        eq = false;
                        break;
                    }
                }

                if (eq)
                {
                    instance = pi;
                    break;
                }
            }

            // instatiate type
            if (instance == null)
            {
                instance = decl.Clone() as AstEnumDecl;
                instance.SubScope = new Scope($"enum {decl.Name.Name}<poly>", instance.Scope);
                instance.IsPolyInstance = true;
                instance.IsPolymorphic = false;
                instance.Template = decl;
                decl.PolymorphicInstances.Add(instance);

                Debug.Assert(instance.Parameters.Count == args.Count);

                for (int i = 0; i < instance.Parameters.Count; i++)
                {
                    var param = instance.Parameters[i];
                    var arg = args[i];
                    param.Type = arg.type;
                    param.Value = arg.value;

                    // TODO: non type parameters
                    instance.SubScope.DefineTypeSymbol(param.Name.Name, param.Value as CheezType);
                }

                instance.Type = new EnumType(instance);

                if (instances != null)
                    instances.Add(instance);
                else
                {
                    ResolveTypeDeclaration(instance);
                }
            }

            return instance;
        }

        private void ResolveEnum(AstEnumDecl @enum, List<AstDecl> instances = null)
        {
            //
            var names = new HashSet<string>();

            @enum.TagType = IntType.DefaultType;

            if (@enum.EnumType != null)
            {
                // regular enum (not poly)
                @enum.EnumType.TagType = @enum.TagType;
            }

            foreach (var p in @enum.Parameters)
            {
                @enum.SubScope.DefineTypeSymbol(p.Name.Name, p.Value as CheezType);
            }

            int value = 0;
            foreach (var mem in @enum.Members)
            {
                if (names.Contains(mem.Name.Name))
                    ReportError(mem.Name, $"Duplicate enum member '{mem.Name}'");

                if (mem.AssociatedTypeExpr != null)
                {
                    @enum.HasAssociatedTypes = true;
                    mem.AssociatedTypeExpr.Scope = @enum.SubScope;
                    mem.AssociatedTypeExpr = ResolveType(mem.AssociatedTypeExpr, instances, out var t);
                }

                if (mem.Value == null)
                {
                    mem.Value = new AstNumberExpr(value, Location: mem.Name);
                }

                mem.Value.Scope = @enum.SubScope;
                mem.Value = InferType(mem.Value, @enum.TagType);
                ConvertLiteralTypeToDefaultType(mem.Value, @enum.TagType);
                if (!mem.Value.IsCompTimeValue || !(mem.Value.Type is IntType))
                {
                    ReportError(mem.Value, $"The value of an enum member must be a compile time integer");
                }
                else
                {
                    value = (int)((NumberData)mem.Value.Value).IntValue + 1;
                }
            }

            if (@enum.HasAssociatedTypes)
            {
                // TODO: check if all values are unique
            }

            //@enum.Scope.DefineBinaryOperator("==", );
            //@enum.Scope.DefineBinaryOperator("!=", );
        }

        // struct
        private AstStructDecl InstantiatePolyStruct(AstStructDecl decl, List<(CheezType type, object value)> args, List<AstDecl> instances = null, ILocation location = null)
        {
            if (args.Count != decl.Parameters.Count)
            {
                if (location != null)
                    ReportError(location, "Polymorphic struct instantiation has wrong number of arguments.", ("Declaration here:", decl));
                else
                    ReportError("Polymorphic struct instantiation has wrong number of arguments.", ("Declaration here:", decl));
                return null;
            }

            AstStructDecl instance = null;

            // check if instance already exists
            foreach (var pi in decl.PolymorphicInstances)
            {
                Debug.Assert(pi.Parameters.Count == args.Count);

                bool eq = true;
                for (int i = 0; i < pi.Parameters.Count; i++)
                {
                    var param = pi.Parameters[i];
                    var arg = args[i];
                    if (param.Value != arg.value)
                    {
                        eq = false;
                        break;
                    }
                }

                if (eq)
                {
                    instance = pi;
                    break;
                }
            }

            // instatiate type
            if (instance == null)
            {
                instance = decl.Clone() as AstStructDecl;
                instance.SubScope = new Scope($"struct {decl.Name.Name}<poly>", instance.Scope);
                instance.IsPolyInstance = true;
                instance.IsPolymorphic = false;
                instance.Template = decl;
                decl.PolymorphicInstances.Add(instance);

                Debug.Assert(instance.Parameters.Count == args.Count);

                for (int i = 0; i < instance.Parameters.Count; i++)
                {
                    var param = instance.Parameters[i];
                    var arg = args[i];
                    param.Type = arg.type;
                    param.Value = arg.value;

                    // TODO: what if arg.value is not a type?
                    instance.SubScope.DefineTypeSymbol(param.Name.Name, param.Value as CheezType);
                }

                instance.Type = new StructType(instance);

                if (instances != null)
                    instances.Add(instance);
                else
                {
                    ResolveTypeDeclaration(instance);
                }
            }

            return instance;
        }
        

        private void ResolveStruct(AstStructDecl @struct, List<AstDecl> instances = null)
        {
            foreach (var p in @struct.Parameters)
            {
                @struct.SubScope.DefineTypeSymbol(p.Name.Name, p.Value as CheezType);
            }

            // resolve member types
            int index = 0;
            foreach (var member in @struct.Members)
            {
                member.Index = index++;
                member.TypeExpr.Scope = @struct.SubScope;
                member.TypeExpr = ResolveType(member.TypeExpr, instances, out var t);
                member.Type = t;
            }
        }

        private void ResolveStructMemberInitializers(AstStructDecl @struct)
        {
            foreach (var member in @struct.Members)
            {
                if (member.Initializer == null)
                {
                    switch (member.Type)
                    {
                        case EnumType _:
                        case ReferenceType _:
                            @struct.SetFlag(StmtFlags.NoDefaultInitializer);
                            break;

                        default:
                            member.Initializer = new AstDefaultExpr(member.Name.Location);
                            break;
                    }
                }

                if (member.Initializer != null)
                {
                    member.Initializer.Scope = @struct.SubScope;
                    member.Initializer = InferType(member.Initializer, member.Type);
                    ConvertLiteralTypeToDefaultType(member.Initializer, member.Type);
                    member.Initializer = CheckType(member.Initializer, member.Type);
                }
            }
        }

        private AstFunctionDecl InstantiatePolyImplFunction(
            GenericFunctionType func,
            Dictionary<string, CheezType> polyTypes,
            Dictionary<string, (CheezType type, object value)> constArgs,
            List<AstFunctionDecl> instances = null,
            ILocation location = null)
        {
            var impl = func.Declaration.ImplBlock;
            var implInstance = InstantiatePolyImplNew(impl, polyTypes, location);
            return implInstance.Functions.FirstOrDefault(f => f.Name.Name == func.Declaration.Name.Name);
        }

        private AstFunctionDecl InstantiatePolyFunction(
            GenericFunctionType func,
            Dictionary<string, CheezType> polyTypes,
            Dictionary<string, (CheezType type, object value)> constArgs,
            List<AstFunctionDecl> instances = null,
            ILocation location = null)
        {
            AstFunctionDecl instance = null;

            if (func.Declaration.ImplBlock != null && func.Declaration.ImplBlock.Trait != null)
            {
                return InstantiatePolyImplFunction(func, polyTypes, constArgs, instances, location);
            }

            // check if instance already exists
            foreach (var pi in func.Declaration.PolymorphicInstances)
            {
                bool eq = true;

                if (pi.ConstParameters.Count == constArgs.Count && pi.PolymorphicTypes.Count == polyTypes.Count)
                {
                    foreach (var ca in constArgs)
                    {
                        if (!(pi.ConstParameters.TryGetValue(ca.Key, out var a) && a.type == ca.Value.type && a.value.Equals(ca.Value.value)))
                        {
                            eq = false;
                            break;
                        }
                        else if (a.value is TupleType t1 && ca.Value.value is TupleType t2 && t1.Members.Length == t2.Members.Length)
                        {
                            bool e = true;
                            for (int i = 0; i < t1.Members.Length; i++)
                            {
                                if (t1.Members[i].name != t2.Members[i].name)
                                {
                                    e = false;
                                    break;
                                }
                            }
                            if (!e)
                            {
                                eq = false;
                                break;
                            }
                        }
                    }
                    foreach (var pt in polyTypes)
                    {
                        if (!(pi.PolymorphicTypes.TryGetValue(pt.Key, out var t) && t == pt.Value))
                        {
                            eq = false;
                            break;
                        }
                    }
                }
                else
                {
                    eq = false;
                }
                
                if (eq)
                {
                    instance = pi;
                    break;
                }
            }

            // instatiate type
            if (instance == null)
            {
                instance = func.Declaration.Clone() as AstFunctionDecl;
                instance.Template = func.Declaration;
                instance.IsPolyInstance = true;
                instance.IsGeneric = false;
                instance.PolymorphicTypes = polyTypes;
                instance.ConstParameters = constArgs;
                instance.ImplBlock = func.Declaration.ImplBlock;
                instance.SelfType = func.Declaration.SelfType;
                func.Declaration.PolymorphicInstances.Add(instance);

                if (instance.ImplBlock != null)
                {
                    var targetType = instance.ImplBlock.TargetType;
                    var inst = InstantiatePolyType(targetType, polyTypes, location);
                    instance.ConstScope.DefineTypeSymbol("Self", inst);
                }

                foreach (var pt in constArgs)
                {
                    instance.ConstScope.DefineConstant(pt.Key, pt.Value.type, pt.Value.value);
                }

                foreach (var pt in polyTypes)
                {
                    instance.ConstScope.DefineTypeSymbol(pt.Key, pt.Value);
                }

                foreach (var p in instance.Parameters)
                {
                    p.Scope = instance.SubScope;
                    p.TypeExpr.Scope = p.Scope;
                    p.TypeExpr = ResolveTypeNow(p.TypeExpr, out var t, true);
                    p.Type = t;
                    //p.Type = InstantiatePolyType(p.Type, polyTypes);

                    if (p.Name?.IsPolymorphic ?? false)
                    {
                        var (type, value) = constArgs[p.Name.Name];
                        if (type != p.Type)
                        {
                            ReportError(p, $"Type of const argument ({type}) does not match type of parameter ({p.Type})");
                        }
                        else
                        {
                            switch (p.Type)
                            {
                                case IntType _:
                                case FloatType _:
                                case BoolType _:
                                case CharType _:
                                case CheezTypeType _:
                                case CodeType _:
                                    break;

                                case CheezType tt when tt == CheezType.String || tt == CheezType.CString:
                                    break;

                                case ErrorType _:
                                    break;

                                default:
                                    ReportError(p.TypeExpr, $"The type '{p.Type}' is not allowed here.");
                                    break;
                            }
                        }
                    }
                }

                // return types
                //if (instance.ReturnTypeExpr != null)
                //{
                //    instance.ReturnTypeExpr.Scope = instance.SubScope;
                //    instance.ReturnTypeExpr.TypeExpr.Scope = instance.SubScope;
                //    instance.ReturnTypeExpr.TypeExpr = ResolveTypeNow(instance.ReturnTypeExpr.TypeExpr, out var t, true);
                //    instance.ReturnTypeExpr.Type = t;
                //}

                //remove constant params
                instance.Parameters = instance.Parameters.Where(p => !(p.Name?.IsPolymorphic ?? false)).ToList();
                ResolveFunctionSignature(instance, null);

                //instance.Type = new FunctionType(instance);


                if (instances != null)
                    instances.Add(instance);
            }

            return instance;
        }

        private CheezType InstantiatePolyType(CheezType poly, Dictionary<string, CheezType> concreteTypes, ILocation location)
        {
            if (!poly.IsPolyType)
                return poly;

            switch (poly)
            {
                case PolyType p:
                    if (concreteTypes.TryGetValue(p.Name, out var t)) return t;
                    return p;

                case TupleType tuple:
                    {
                        var args = tuple.Members.Select(m => (m.name, InstantiatePolyType(m.type, concreteTypes, location)));
                        return TupleType.GetTuple(args.ToArray());
                    }

                case StructType s:
                    {
                        if (s.Declaration.Template != null)
                            throw new Exception("must be null");
                        var args = s.Arguments.Select(a => (CheezType.Type, (object)InstantiatePolyType(a, concreteTypes, location))).ToList();
                        //var args = s.Declaration.Parameters.Select(p => (p.Type, (object)concreteTypes[p.Name.Name])).ToList();
                        var instance = InstantiatePolyStruct(s.Declaration, args, location: location);
                        return instance.Type;
                    }

                case TraitType s:
                    {
                        if (s.Declaration.Template != null)
                            throw new Exception("must be null");

                        if (s.Arguments.Count() != s.Declaration.Parameters.Count())
                            throw new Exception("argument count must match");

                        var args = s.Arguments.Select(a => InstantiatePolyType(a, concreteTypes, location)).ToList();
                        var zipped = args.Zip(s.Declaration.Parameters, (type, param) => (param.Type, (object)type)).ToList();

                        var instance = InstantiatePolyTrait(s.Declaration, zipped, location: location);
                        return instance.Type;
                    }

                case EnumType s:
                    {
                        if (s.Declaration.Template != null)
                            throw new Exception("must be null");
                        var args = s.Declaration.Parameters.Select(p => (p.Type, (object)concreteTypes[p.Name.Name])).ToList();
                        var instance = InstantiatePolyEnum(s.Declaration, args, location: location);
                        return instance.Type;
                    }

                case PointerType p:
                    return PointerType.GetPointerType(InstantiatePolyType(p.TargetType, concreteTypes, location));

                default: throw new NotImplementedException();
            }
        }
    }
}
