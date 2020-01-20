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

            if (v.Directives != null)
            {
                foreach (var d in v.Directives)
                {
                    foreach (var arg in d.Arguments)
                    {
                        arg.Scope = v.Scope;
                        InferType(arg, null);
                    }
                }
            }

            if (v.HasDirective("local"))
                v.SetFlag(StmtFlags.IsLocal, true);

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
                if (v.GetFlag(StmtFlags.GlobalScope) && !v.HasDirective("extern"))
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
                case IntType _:
                case FloatType _:
                case BoolType _:
                case CharType _:
                case SliceType _:
                case StringType _:
                case ArrayType _:
                case StructType _:
                case EnumType _:
                case PointerType _:
                case ReferenceType _:
                case FunctionType _:
                case TupleType _:
                case RangeType _:
                    break;

                case CheezType t when t.IsErrorType:
                    break;

                default:
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
            v.Pattern.SetFlag(ExprFlags.IsDeclarationPattern, true);
            switch (v.Pattern)
            {
                case AstCompCallExpr cc when cc.Name.Name == "id":
                    cc.AttachTo(v);
                    v.Name = InferType(cc, null) as AstIdExpr;
                    break;

                case AstIdExpr name:
                    // ok, do nothing
                    v.Name = name;
                    break;

                case AstTupleExpr t:
                    {
                        v.Name = new AstIdExpr(GetUniqueName(t.ToString()), false, t);
                        v.Pattern = v.Name;

                        //var initClone = v.Initializer.Clone();
                        //initClone.AttachTo(v);
                        //initClone.SetFlag(ExprFlags.ValueRequired, true);
                        //initClone = InferType(initClone, null);

                        //AstVariableDecl CreateSub(int index, string name)
                        //{
                        //    var init = mCompiler.ParseExpression(
                        //        $"@{name}(§init)",
                        //        new Dictionary<string, AstExpression>
                        //        {
                        //                { "init", new AstVariableRef(v, v.Initializer) }
                        //        });
                        //    var sub = new AstVariableDecl(t.Values[index], null, init, Location: v);
                        //    sub.Scope = v.Scope;
                        //    sub.SetFlags(v.GetFlags());
                        //    return sub;
                        //}
                        //if (initClone.Type is PointerType pt1 && pt1.TargetType is AnyType)
                        //{
                        //    // create new declarations for sub patterns
                        //    yield return CreateSub(0, "ptr_of_any");
                        //    yield return CreateSub(1, "type_info_of_any");
                        //    break;
                        //}
                        //else if (initClone.Type is PointerType pt2 && pt2.TargetType is TraitType)
                        //{
                        //    // create new declarations for sub patterns
                        //    yield return CreateSub(0, "ptr_of_trait");
                        //    yield return CreateSub(1, "vtable_of_trait");
                        //    break;
                        //}
                        //else
                        //{
                            // create new declarations for sub patterns
                            var index = 0;
                            foreach (var subPattern in t.Values)
                            {
                                var init = new AstArrayAccessExpr(
                                    new AstVariableRef(v, v.Initializer),
                                    new AstNumberExpr(NumberData.FromBigInt(index), Location: v.Initializer));
                                var sub = new AstVariableDecl(subPattern, null, init, Location: v);
                                sub.Scope = v.Scope;
                                sub.SetFlags(v.GetFlags());

                                yield return sub;
                                index += 1;
                            }
                            break;
                        //}
                    }

                default:
                    ReportError(v.Pattern, $"Invalid pattern in variable declaration");
                    break;
            }
        }

        private IEnumerable<AstConstantDeclaration> SplitConstantDeclaration(AstConstantDeclaration v)
        {
            v.Pattern.SetFlag(ExprFlags.IsDeclarationPattern, true);
            switch (v.Pattern)
            {
                case AstCompCallExpr cc when cc.Name.Name == "id":
                    cc.AttachTo(v);
                    v.Name = InferType(cc, null) as AstIdExpr;
                    break;

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
                        var sub = new AstConstantDeclaration(subPattern, null, init, null, Location: v);
                        sub.Scope = v.Scope;
                        sub.SetFlags(v.GetFlags());

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
