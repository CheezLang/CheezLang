using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Extras;
using Cheez.Types;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cheez
{
    public partial class Workspace
    {
        private void ResolveVariableDecl(AstVariableDecl v)
        {
            Debug.Assert(v.Name != null);

            if (v.TypeExpr != null)
            {
                v.TypeExpr.AttachTo(v);
                v.TypeExpr.SetFlag(ExprFlags.ValueRequired, true);
                v.TypeExpr = ResolveTypeNow(v.TypeExpr, out var t);
                v.Type = t;
            }

            // initializer stuff
            if (v.Initializer == null)
            {
                if (v.GetFlag(StmtFlags.GlobalScope))
                    ReportError(v, $"Global variables must have an initializer");
            }
            else
            {
                v.Initializer.AttachTo(v);
                v.Initializer.SetFlag(ExprFlags.ValueRequired, true);
                v.Initializer = InferType(v.Initializer, v.Type);
                ConvertLiteralTypeToDefaultType(v.Initializer, v.Type);

                if (v.TypeExpr != null)
                {
                    v.Initializer = HandleReference(v.Initializer, v.Type, null);
                    v.Initializer = CheckType(v.Initializer, v.Type);
                }
                else if (v.Initializer is AstAddressOfExpr add && add.Reference)
                {
                    // do nothing
                }
                else if (v.Initializer.Type is ReferenceType)
                {
                    v.Initializer = Deref(v.Initializer, null);
                }
            }

            if (v.TypeExpr == null)
                v.Type = v.Initializer.Type;

            switch (v.Type)
            {
                case VoidType _:
                case SumType _:
                    ReportError(v, $"Variable can't have type {v.Type}");
                    break;
            }

            //if (vardecl.Type is SumType)
            //{
            //    ReportError(vardecl.Pattern, $"Invalid type for variable declaration: {vardecl.Type}");
            //}
        }

        private IEnumerable<AstVariableDecl> SplitVariableDeclaration(AstVariableDecl v)
        {
            switch (v.Pattern)
            {
                case AstIdExpr name:
                    // ok, do nothing
                    v.Name = name;
                    break;

                case AstTupleExpr t:
                    v.Name = new AstIdExpr(GetUniqueName(t.ToString()), false, t);

                    // create new declarations for sub patterns
                    var index = 0;
                    foreach (var subPattern in t.Values)
                    {
                        var init = new AstArrayAccessExpr(
                            new AstVariableRef(v, v.Initializer),
                            new AstNumberExpr(NumberData.FromBigInt(index), Location: v.Initializer));
                        var sub = new AstVariableDecl(subPattern, null, init, Location: v);

                        yield return sub;
                        index += 1;
                    }

                    v.Pattern = v.Name;
                    break;

                default:
                    ReportError(v.Pattern, $"Invalid pattern in variable declaration");
                    break;
            }
        }

        private IEnumerable<AstConstantDeclaration> SplitConstantDeclaration(AstConstantDeclaration v)
        {
            switch (v.Pattern)
            {
                case AstIdExpr name:
                    // ok, do nothing
                    v.Name = name;
                    break;

                case AstTupleExpr t:
                    v.Name = new AstIdExpr(GetUniqueName(t.ToString()), false, t);

                    // create new declarations for sub patterns
                    var index = 0;
                    foreach (var subPattern in t.Values)
                    {
                        var init = new AstArrayAccessExpr(
                            new AstConstantRef(v, v.Initializer),
                            new AstNumberExpr(NumberData.FromBigInt(index), Location: v.Initializer));
                        var sub = new AstConstantDeclaration(subPattern, null, init, Location: v);

                        yield return sub;
                        index += 1;
                    }

                    v.Pattern = v.Name;
                    break;

                default:
                    ReportError(v.Pattern, $"Invalid pattern in variable declaration");
                    break;
            }
        }
    }
}
