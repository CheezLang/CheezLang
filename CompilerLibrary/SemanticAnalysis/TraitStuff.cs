using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Types;
using Cheez.Types.Abstract;
using Cheez.Types.Complex;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Cheez
{
    public partial class Workspace
    {
        private AstExpression InferTypeTraitTypeExpr(AstTraitTypeExpr expr)
        {
            if (expr.IsPolyInstance)
            {

            }
            else
            {
                expr.SubScope = new Scope("trait", expr.Scope);
                if (expr.Parent is AstConstantDeclaration c)
                    expr.Name = c.Name.Name;
            }

            // setup scopes and separate members
            foreach (var decl in expr.Declarations)
            {
                decl.Scope = expr.SubScope;

                switch (decl)
                {
                    case AstConstantDeclaration con when con.Initializer is AstFuncExpr func:
                        func.Name = con.Name.Name;
                        expr.Functions.Add(func);
                        break;

                    case AstConstantDeclaration con:
                        ReportError(con, $"Not supported yet");
                        break;

                    case AstVariableDecl mem:
                        expr.Members.Add(new AstTraitMember(mem, false, expr.Members.Count));
                        break;
                }
            }

            if (expr.IsPolymorphic)
            {
                // @todo
                foreach (var p in expr.Parameters)
                {
                    p.Scope = expr.Scope;
                    p.TypeExpr.Scope = expr.Scope;
                    p.TypeExpr = ResolveTypeNow(p.TypeExpr, out var t);
                    p.Type = t;

                    ValidatePolymorphicParameterType(p, p.Type);

                    expr.SubScope.DefineTypeSymbol(p.Name.Name, new PolyType(p.Name.Name, true));
                }

                expr.Type = CheezType.Type;
                expr.Value = new GenericTraitType(expr);
                return expr;
            }

            expr.Type = CheezType.Type;
            expr.Value = new TraitType(expr);
            AddTrait(expr);
            // mTypesRequiredAtRuntimeQueue.Enqueue(expr.TraitType);
            return expr;
        }

        private void ComputeTraitMembers(AstTraitTypeExpr trait)
        {
            if (trait.IsPolymorphic)
                return;
            if (trait.MembersComputed)
                return;
            trait.MembersComputed = true;

            trait.SubScope.DefineTypeSymbol("Self", new SelfType(trait.Value as CheezType));

            foreach (var v in trait.Members)
            {
                var decl = v.Decl;
                decl.TypeExpr.Scope = trait.SubScope;
                decl.TypeExpr = ResolveTypeNow(decl.TypeExpr, out var type);
                decl.Type = type;

                var res = trait.SubScope.DefineSymbol(decl);
                if (!res.ok)
                {
                    (string, ILocation)? detail = null;
                    if (res.other != null) detail = ("Other declaration here:", res.other);
                    ReportError(decl.Name, $"A symbol with name '{decl.Name.Name}' already exists in current scope", detail);
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

                switch (f.SelfType)
                {
                    case SelfParamType.None:
                    case SelfParamType.Value:
                        f.ExcludeFromVTable = true;
                        break;
                }

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
        }

        private AstTraitTypeExpr InstantiatePolyTrait(AstTraitTypeExpr decl, List<(CheezType type, object value)> args, ILocation location = null)
        {
            if (args.Any(a => a.type == CheezType.Type && (a.value as CheezType).IsErrorType))
                WellThatsNotSupposedToHappen();

            if (args.Count != decl.Parameters.Count)
            {
                if (location != null)
                    ReportError(location, "Polymorphic instantiation has wrong number of arguments.", ("Declaration here:", decl));
                else
                    ReportError("Polymorphic instantiation has wrong number of arguments.", ("Declaration here:", decl));
                return null;
            }

            AstTraitTypeExpr instance = null;

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
                instance = decl.Clone() as AstTraitTypeExpr;
                instance.SubScope = new Scope($"trait <poly>", instance.Scope);
                instance.IsPolyInstance = true;
                instance.IsGeneric = false;
                instance.Template = decl;
                instance.Name = decl.Name;
                decl.PolymorphicInstances.Add(instance);

                Debug.Assert(instance.Parameters.Count == args.Count);

                for (int i = 0; i < instance.Parameters.Count; i++)
                {
                    var param = instance.Parameters[i];
                    var arg = args[i];
                    param.Type = arg.type;
                    param.Value = arg.value;
                    instance.SubScope.DefineConstant(param.Name.Name, arg.type, arg.value);
                }

                instance = InferType(instance, null) as AstTraitTypeExpr;
                ComputeTraitMembers(instance);

                //if (instances != null)
                //    instances.Add(instance);
                //else
                //{
                //    ResolveTypeDeclaration(instance);
                //}
            }

            return instance;
        }

        public bool TypeHasTrait(CheezType type, TraitType trait)
        {
            UpdateTypeImplMap();
            return trait.Declaration.Implementations.ContainsKey(type);
        }
    }
}
