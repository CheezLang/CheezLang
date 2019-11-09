using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Extras;
using Cheez.Types;
using Cheez.Types.Abstract;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace Cheez
{
    public partial class Workspace
    {
        private AstExpression InferTypeEnumTypeExpr(AstEnumTypeExpr expr)
        {
            if (expr.IsPolyInstance)
            {

            }
            else
            {
                expr.SubScope = new Scope("enum", expr.Scope);
                if (expr.Parent is AstConstantDeclaration c)
                    expr.Name = c.Name.Name;
            }

            bool isCopy = false;
            if (!expr.IsPolymorphic && expr.TryGetDirective("copy", out var d))
            {
                isCopy = true;
                foreach (var arg in d.Arguments)
                {
                    arg.AttachTo(expr, expr.SubScope);
                    ResolveTypeNow(arg, out var t);
                    isCopy &= t.IsCopy;
                }
            }

            expr.TagType = IntType.GetIntType(8, false);
            if (expr.TryGetDirective("tag_type", out var bt))
            {
                if (bt.Arguments.Count != 1)
                {
                    ReportError(bt, $"#tag_type requires one argument");
                }
                else
                {
                    var arg = bt.Arguments[0];
                    arg.AttachTo(expr);
                    arg = InferType(arg, CheezType.Type);

                    if (!arg.Type.IsErrorType)
                    {
                        if (arg.Type != CheezType.Type)
                        {
                            ReportError(arg, $"Argument must be an int type");
                        }
                        else if (arg.Value is IntType i)
                        {
                            expr.TagType = i;
                        }
                        else
                        {
                            ReportError(arg, $"Argument must be an int type");
                        }
                    }

                }
            }

            // setup scopes and separate members
            expr.Members = new List<AstEnumMemberNew>();
            foreach (var decl in expr.Declarations)
            {
                decl.Scope = expr.SubScope;

                switch (decl)
                {
                    case AstConstantDeclaration con:
                        break;
                    case AstVariableDecl mem:
                        expr.Members.Add(new AstEnumMemberNew(mem, expr.Members.Count));
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
                expr.Value = new GenericEnumType(expr, expr.Name);
                return expr;
            }

            foreach (var decl in expr.Declarations)
            {
                if (decl is AstConstantDeclaration con)
                {
                    AnalyseConstantDeclaration(con);
                }
            }

            expr.Type = CheezType.Type;
            expr.Value = new EnumType(expr, isCopy);
            return expr;
        }

        private void ComputeEnumMembers(AstEnumTypeExpr expr)
        {
            if (expr.MembersComputed)
                return;
            expr.MembersComputed = true;

            BigInteger value = 0;
            var usedValues = new HashSet<BigInteger>();

            foreach (var mem in expr.Members)
            {
                var memDecl = mem.Decl;

                if (!(memDecl.Pattern is AstIdExpr memName))
                {
                    ReportError(memDecl.Pattern, $"Only single names allowed");
                    continue;
                }

                if (memDecl.TypeExpr != null)
                {
                    memDecl.TypeExpr.AttachTo(memDecl);
                    memDecl.TypeExpr = ResolveTypeNow(memDecl.TypeExpr, out var t);
                    memDecl.Type = t;

                    // @todo: check if type is valid as enum member, eg no void
                }

                if (memDecl.Initializer != null)
                {
                    memDecl.Initializer.AttachTo(memDecl);
                    memDecl.Initializer = InferType(memDecl.Initializer, expr.TagType);
                    memDecl.Initializer = CheckType(memDecl.Initializer, expr.TagType);

                    if (memDecl.Initializer.Type is IntType i)
                    {
                        if (memDecl.Initializer.IsCompTimeValue)
                        {
                            value = ((NumberData)memDecl.Initializer.Value).IntValue;
                            CheckValueRangeForType(i, memDecl.Initializer.Value, memDecl.Initializer);
                        }
                        else
                        {
                            ReportError(memDecl.Initializer, $"Value of enum member has to be an constant integer");
                        }
                    }
                }

                if (!expr.IsPolymorphic && expr.EnumType.IsCopy && (!memDecl.Type?.IsCopy ?? false))
                {
                    ReportError(memDecl, "Member is not copyable");
                }

                if (usedValues.Contains(value))
                    ReportError(memDecl, $"Member has value {value} which is already being used by another member");
                usedValues.Add(value);

                if (memDecl.Type != null)
                    ComputeTypeMembers(memDecl.Type);
                mem.Value = NumberData.FromBigInt(value);

                value += 1;
            }
        }

        private AstEnumTypeExpr InstantiatePolyEnum(AstEnumTypeExpr decl, List<(CheezType type, object value)> args, ILocation location = null)
        {
            if (args.Count != decl.Parameters.Count)
            {
                if (location != null)
                    ReportError(location, "Polymorphic instantiation has wrong number of arguments.", ("Declaration here:", decl));
                else
                    ReportError("Polymorphic instantiation has wrong number of arguments.", ("Declaration here:", decl));
                return null;
            }

            AstEnumTypeExpr instance = null;

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
                instance = decl.Clone() as AstEnumTypeExpr;
                instance.SubScope = new Scope($"enum {decl.Name}<poly>", instance.Scope);
                instance.IsPolyInstance = true;
                instance.IsGeneric = false;
                instance.Template = decl;
                decl.PolymorphicInstances.Add(instance);
                instance.Name = decl.Name;

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

                instance = InferType(instance, null) as AstEnumTypeExpr;

                //instance.Type = new EnumType(instance);

                //if (instances != null)
                //    instances.Add(instance);
                //else
                //{
                //    ResolveTypeDeclaration(instance);
                //}
            }

            return instance;
        }

    }
}
