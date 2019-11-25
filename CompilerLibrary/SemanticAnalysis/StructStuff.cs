using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Types;
using Cheez.Types.Abstract;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cheez
{
    public partial class Workspace
    {

        private AstExpression InferTypeStructTypeExpr(AstStructTypeExpr expr)
        {
            if (expr.IsPolyInstance)
            {

            }
            else
            {
                expr.SubScope = new Scope("struct", expr.Scope);
                if (expr.Parent is AstConstantDeclaration c)
                    expr.Name = c.Name.Name;
            }

            bool isCopy = expr.HasDirective("copy");

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
                expr.Value = new GenericStructType(expr, isCopy, expr.Name);
                return expr;
            }

            foreach (var decl in expr.Declarations)
            {
                decl.Scope = expr.SubScope;

                if (decl is AstConstantDeclaration con)
                {
                    AnalyseConstantDeclaration(con);
                }
            }

            expr.Type = CheezType.Type;
            expr.Value = new StructType(expr, isCopy, expr.Name);
            return expr;
        }

        private void ComputeStructMembers(AstStructTypeExpr expr)
        {
            if (expr.Members != null)
                return;


            expr.Members = new List<AstStructMemberNew>();
            foreach (var decl in expr.Declarations)
            {
                if (decl is AstVariableDecl mem)
                {
                    if (!(mem.Pattern is AstIdExpr memName))
                    {
                        ReportError(mem.Pattern, $"Only single names allowed");
                        continue;
                    }

                    if (mem.Directives != null)
                        foreach (var dir in mem.Directives)
                            InferTypeAttributeDirective(dir, mem, mem.Scope);

                    if (mem.TypeExpr != null)
                    {
                        mem.TypeExpr.AttachTo(mem);
                        mem.TypeExpr = ResolveTypeNow(mem.TypeExpr, out var t);
                        mem.Type = t;

                        // @todo: check if type is valid as struct member, eg no void
                    }

                    if (mem.Initializer != null)
                    {
                        mem.Initializer.AttachTo(mem);
                        mem.Initializer = InferType(mem.Initializer, mem.Type);
                        ConvertLiteralTypeToDefaultType(mem.Initializer, mem.Type);

                        if (mem.Type == null)
                            mem.Type = mem.Initializer.Type;
                        else
                            mem.Initializer = CheckType(mem.Initializer, mem.Type);
                    }

                    if (expr.StructType.IsCopy && !mem.Type.IsCopy)
                    {
                        ReportError(mem, "Member is not copyable");
                    }

                    expr.Members.Add(new AstStructMemberNew(mem, true, false, expr.Members.Count));

                    ComputeTypeMembers(mem.Type);
                }
            }
        }

        private AstStructTypeExpr InstantiatePolyStruct(AstStructTypeExpr decl, List<(CheezType type, object value)> args, ILocation location = null)
        {
            if (args.Count != decl.Parameters.Count)
            {
                if (location != null)
                    ReportError(location, "Polymorphic struct instantiation has wrong number of arguments.", ("Declaration here:", decl));
                else
                    ReportError("Polymorphic struct instantiation has wrong number of arguments.", ("Declaration here:", decl));
                return null;
            }

            AstStructTypeExpr instance = null;

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
                instance = decl.Clone() as AstStructTypeExpr;
                instance.SubScope = new Scope($"struct.poly", instance.Scope);
                instance.IsPolyInstance = true;
                instance.IsGeneric = false;
                instance.Template = decl;
                instance.Name = decl.Name;

                // @todo
                //instance.SetFlag(StmtFlags.IsCopy, decl.GetFlag(StmtFlags.IsCopy));
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

                instance = InferType(instance, null) as AstStructTypeExpr;

                //instance.Type = new StructType(instance);

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
