using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Expressions.Types;
using Cheez.Ast.Statements;
using Cheez.Extras;
using Cheez.Parsing;
using Cheez.Types;
using Cheez.Types.Abstract;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;
using Cheez.Util;

namespace Cheez
{
    public partial class Workspace
    {
        public class TypeInferenceContext
        {
            public List<AstFunctionDecl> newPolyFunctions;
            public List<AstDecl> newPolyDeclarations;
            public HashSet<AstDecl> dependencies;
            public bool resolve_poly_expr_to_concrete_type;
            public bool forceInfer = false;
            public CheezType functionExpectedReturnType = null;
            public bool is_global = false;
        }

        private bool IsLiteralType(CheezType t)
        {
            return t == IntType.LiteralType || t == FloatType.LiteralType || t == CheezType.StringLiteral;
        }

        private CheezType UnifyTypes(CheezType concrete, CheezType literal)
        {
            if (concrete is ReferenceType r)
                concrete = r.TargetType;
                
            if (concrete is IntType && literal is IntType) return concrete;
            if (concrete is FloatType && literal is IntType) return concrete;
            if (concrete is FloatType && literal is FloatType) return concrete;
            if ((concrete == CheezType.String || concrete == CheezType.CString) && literal == CheezType.StringLiteral) return concrete;
            return LiteralTypeToDefaultType(literal);
        }

        private CheezType LiteralTypeToDefaultType(CheezType literalType, CheezType expected = null)
        {
            // :hack
            if (expected == CheezType.Void) expected = null;

            if (literalType == IntType.LiteralType)
            {
                if (expected != null && !(expected is IntType || expected is FloatType)) return IntType.DefaultType;
                return expected ?? IntType.DefaultType;
            }
            else if (literalType == FloatType.LiteralType)
            {
                if (expected != null && !(expected is FloatType)) return FloatType.DefaultType;
                return expected ?? FloatType.DefaultType;
            }
            else if (literalType == CheezType.StringLiteral) return CheezType.String;
            else if (literalType == PointerType.NullLiteralType)
            {
                if (expected is TraitType)
                    return expected;

                return PointerType.GetPointerType(CheezType.Any);
            }

            return literalType;
        }

        private void ConvertLiteralTypeToDefaultType(AstExpression expr, CheezType expected)
        {
            expr.Type = LiteralTypeToDefaultType(expr.Type, expected);
        }

        private AstExpression InferTypeSilent(AstExpression expr, CheezType expected, out SilentErrorHandler errorHandler)
        {
            errorHandler = new SilentErrorHandler();
            PushErrorHandler(errorHandler);
            expr = InferType(expr, expected);
            PopErrorHandler();
            return expr;
        }

        private AstExpression InferType(AstExpression expr, CheezType expected, bool resolve_poly_expr_to_concrete_type = false, HashSet<AstDecl> dependencies = null, bool forceInfer = false)
        {
            var context = new TypeInferenceContext
            {
                newPolyFunctions = new List<AstFunctionDecl>(),
                //newPolyDeclarations = new List<AstDecl>(),
                resolve_poly_expr_to_concrete_type = resolve_poly_expr_to_concrete_type,
                dependencies = dependencies,
                forceInfer = forceInfer
            };
            var newExpr = InferTypeHelper(expr, expected, context);

            if (context.newPolyDeclarations?.Count > 0)
            {
                ResolveTypeDeclarations(context.newPolyDeclarations);
            }

            if (context.newPolyFunctions.Count > 0)
                AnalyseFunctions(context.newPolyFunctions);

            return newExpr;
        }

        private AstExpression InferTypeHelper(AstExpression expr, CheezType expected, TypeInferenceContext context)
        {
            if (expected != null)
                expr.SetFlag(ExprFlags.ValueRequired, true);
            // @todo: remove, just for testing
            if (!expr.GetFlag(ExprFlags.ValueRequired)
                && !(expr.Parent is AstFunctionDecl)
                && !(expr.Parent is AstExprStmt)
                && !(expr.Parent is AstWhileStmt))
            {
                //expr.Type = CheezType.Void;
                //return expr;
            }

            if (!(context?.forceInfer ?? false) && expr.TypeInferred)
                return expr;
            expr.TypeInferred = true;

            if (expected == CheezType.Code)
            {
                expr.Scope = new Scope($"code", expr.Scope);
                expr.Type = CheezType.Code;
                expr.Value = expr;
                return expr;
            }

            expr.Type = CheezType.Error;

            switch (expr)
            {
                case AstNullExpr n:
                    return InferTypesNullExpr(n, expected);

                case AstBoolExpr b:
                    return InferTypeBoolExpr(b);

                case AstNumberExpr n:
                    return InferTypesNumberExpr(n, expected);

                case AstStringLiteral s:
                    return InferTypesStringLiteral(s, expected);

                case AstCharLiteral ch:
                    return InferTypesCharLiteral(ch, expected);

                case AstIdExpr i:
                    return InferTypesIdExpr(i, expected, context);

                case AstAddressOfExpr ao:
                    return InferTypeAddressOf(ao, expected, context);

                case AstDereferenceExpr de:
                    return InferTypeDeref(de, expected, context);

                case AstTupleExpr t:
                    return InferTypeTupleExpr(t, expected, context);

                case AstStructValueExpr s:
                    return InferTypeStructValueExpr(s, expected, context);

                case AstNaryOpExpr b:
                    return InferTypesNaryExpr(b, expected, context);

                case AstBinaryExpr b:
                    return InferTypesBinaryExpr(b, expected, context);

                case AstUnaryExpr u:
                    return InferTypeUnaryExpr(u, expected, context);

                case AstCallExpr c:
                    return InferTypeCallExpr(c, expected, context);

                case AstDotExpr d:
                    return InferTypeDotExpr(d, expected, context);

                case AstArrayAccessExpr d:
                    return InferTypeIndexExpr(d, expected, context);

                case AstTempVarExpr d:
                    return InferTypeTempVarExpr(d, expected, context);

                case AstSymbolExpr s:
                    return InferTypeSymbolExpr(s);

                case AstBlockExpr b:
                    return InferTypeBlock(b, expected, context);

                case AstIfExpr i:
                    return InferTypeIfExpr(i, expected, context);

                case AstCompCallExpr c:
                    return InferTypeCompCall(c, expected, context);

                case AstCastExpr cast:
                    return InferTypeCast(cast, expected, context);

                case AstEmptyExpr e:
                    return e;

                case AstUfcFuncExpr ufc:
                    return InferTypeUfcFuncExpr(ufc);

                case AstArrayExpr arr:
                    return InferTypeArrayExpr(arr, expected, context);

                case AstArgument arg:
                    return InferTypeArgExpr(arg, expected, context);

                case AstReferenceTypeExpr p:
                    return InferTypeReferenceTypeExpr(p, expected, context);

                case AstSliceTypeExpr p:
                    return InferTypeSliceTypeExpr(p, context);

                case AstArrayTypeExpr p:
                    return InferTypeArrayTypeExpr(p, context);

                case AstFunctionTypeExpr func:
                    return InferTypeFunctionTypeExpr(func, context);

                case AstImplTraitTypeExpr implTrait:
                    return InferTypeImplTraitTypeExpr(implTrait, context);

                case AstTypeRef typeRef:
                    return InferTypeTypeRefExpr(typeRef);

                case AstDefaultExpr def:
                    return InferTypeDefaultExpr(def, expected, context);

                case AstMatchExpr m:
                    return InferTypeMatchExpr(m, expected, context);

                case AstEnumValueExpr e:
                    return InferTypeEnumValueExpr(e, expected, context);

                case AstLambdaExpr l:
                    return InferTypeLambdaExpr(l, expected, context);

                case AstFunctionRef f:
                    return InferTypeFunctionRef(f, expected, context);

                case AstBreakExpr b:
                    return InferTypeBreak(b);

                case AstContinueExpr b:
                    return InferTypeContinue(b);

                case AstRangeExpr r:
                    return InferTypeRangeExpr(r, expected, context);

                default:
                    throw new NotImplementedException();
            }
        }

        private AstExpression InferTypeRangeExpr(AstRangeExpr r, CheezType expected, TypeInferenceContext context)
        {
            r.From.AttachTo(r);
            r.To.AttachTo(r);

            r.From.SetFlag(ExprFlags.ValueRequired, true);
            r.From = InferTypeHelper(r.From, null, context);
            ConvertLiteralTypeToDefaultType(r.From, IntType.DefaultType);
            r.From = Deref(r.From, context);

            r.To.SetFlag(ExprFlags.ValueRequired, true);
            r.To = InferTypeHelper(r.To, r.From.Type, context);
            ConvertLiteralTypeToDefaultType(r.To, IntType.DefaultType);
            r.To = Deref(r.To, context);

            if (r.From.Type != r.To.Type)
            {
                ReportError(r, $"Types of start and end don't match, start: {r.From.Type}, end: {r.To.Type}");
                return r;
            }

            if (r.From.Type is CheezTypeType)
            {
                r.Type = CheezType.Type;
                r.Value = RangeType.GetRangeType(r.From.Value as CheezType);
                return r;
            }

            if (!(r.From.Type is IntType))
            {
                ReportError(r, $"Types of start and end must be int");
                return r;
            }

            r.Type = RangeType.GetRangeType(r.From.Type);
            return r;
        }

        private AstExpression InferTypeContinue(AstContinueExpr cont)
        {
            // @todo: maybe add separate flag, but i think this should work
            cont.SetFlag(ExprFlags.Breaks, true);
            var sym = cont.Scope.GetContinue(cont.Label?.Name);
            if (sym == null)
                ReportError(cont, $"Did not find a loop matching this continue");
            else if (sym is AstWhileStmt loop)
                cont.Loop = loop;
            else if (sym is AstExpression action)
            {
                action = action.Clone();
                action.Parent = cont.Parent;
                action = InferTypeSilent(action, null, out var errs);
                if (errs.HasErrors)
                    ReportError(cont.Location, "Failed to continue", errs.Errors, ("continue action defined here:", action.Location));
                return action;
            }
            else WellThatsNotSupposedToHappen();

            cont.Type = CheezType.Void;
            return cont;
        }

        private AstExpression InferTypeBreak(AstBreakExpr br)
        {
            br.SetFlag(ExprFlags.Breaks, true);
            var sym = br.Scope.GetBreak(br.Label?.Name);
            if (sym == null)
                ReportError(br, $"Did not find a loop matching this break");
            else if (sym is AstWhileStmt loop)
                br.Loop = loop;
            else if (sym is AstExpression action)
            {
                action = action.Clone();
                action.Parent = br.Parent;
                action = InferTypeSilent(action, null, out var errs);
                if (errs.HasErrors)
                    ReportError(br.Location, "Failed to break", errs.Errors, ("break action defined here:", action.Location));
                return action;
            }
            else WellThatsNotSupposedToHappen();

            br.Type = CheezType.Void;
            return br;
        }

        private AstExpression InferTypeFunctionRef(AstFunctionRef f, CheezType expected, TypeInferenceContext context)
        {
            f.Type = f.Declaration.Type;
            return f;
        }

        private AstExpression InferTypeImplTraitTypeExpr(AstImplTraitTypeExpr implTrait, TypeInferenceContext context)
        {
            throw new NotImplementedException();
        }

        private AstExpression InferTypeLambdaExpr(AstLambdaExpr expr, CheezType expected, TypeInferenceContext context)
        {
            var paramScope = new Scope("||", expr.Scope);
            var subScope = new Scope("lambda", paramScope);

            var funcType = expected as FunctionType;

            for (int i = 0; i < expr.Parameters.Count; i++)
            {
                var param = expr.Parameters[i];
                CheezType ex = (funcType != null && i < funcType.Parameters.Length) ? funcType.Parameters[i].type : null;

                param.Scope = paramScope;
                if (param.TypeExpr != null)
                {
                    param.TypeExpr.Scope = paramScope;
                    param.TypeExpr = ResolveType(param.TypeExpr, context, out var t);
                    param.Type = t;
                }
                else
                {
                    param.Type = ex;
                }

                if (param.Type == null)
                {
                    ReportError(param, $"Failed to infer type of lambda parameter");
                }

                param.Scope.DefineSymbol(param);
            }

            var expectedRetType = funcType?.ReturnType;
            if (expectedRetType == CheezType.Void)
                expectedRetType = null;

            expr.Body.AttachTo(expr);
            expr.Body.Scope = subScope;
            expr.Body = InferTypeHelper(expr.Body, expectedRetType, context);
            ConvertLiteralTypeToDefaultType(expr.Body, funcType?.ReturnType);

            var retType = expr.Body.Type;
            if (funcType?.ReturnType == CheezType.Void)
                retType = CheezType.Void;

            expr.Type = new FunctionType(
                expr.Parameters.Select(p => (p.Name.Name, p.Type, (AstExpression)null)).ToArray(),
                retType,
                FunctionType.CallingConvention.Default);

            return expr;
        }

        private AstExpression InferTypeEnumValueExpr(AstEnumValueExpr expr, CheezType expected, TypeInferenceContext context)
        {
            if (expr.Argument != null)
            {
                if (expr.Member.AssociatedTypeExpr == null)
                {
                    ReportError(expr, $"The enum member '{expr.Member.Name}' does not take an argument");
                    return expr;
                }

                var at = expr.Member.AssociatedTypeExpr.Value as CheezType;

                expr.Argument.AttachTo(expr);
                expr.Argument = InferType(expr.Argument, at);
                ConvertLiteralTypeToDefaultType(expr.Argument, at);

                if (expr.EnumDecl.Type is GenericEnumType g)
                {
                    if (expected is EnumType enumType && enumType.DeclarationTemplate == g.Declaration)
                    {
                        expr.EnumDecl = enumType.Declaration;
                        expr.Member = enumType.Declaration.Members.First(m => m.Name.Name == expr.Member.Name.Name);
                        at = expr.Member.AssociatedTypeExpr.Value as CheezType;
                    }
                    else
                    {
                        // create instance
                        var args = new List<(CheezType type, object value)>();

                        // collect poly types
                        var pt = new Dictionary<string, CheezType>();
                        CollectPolyTypes(expr.Member.AssociatedType, expr.Argument.Type, pt);

                        foreach (var param in g.Declaration.Parameters)
                        {
                            if (pt.TryGetValue(param.Name.Name, out var t))
                            {
                                args.Add((CheezType.Type, t));
                            }
                        }

                        if (args.Count == g.Declaration.Parameters.Count)
                        {
                            var instance = InstantiatePolyEnum(g.Declaration, args, context.newPolyDeclarations, expr.Location);
                            if (instance != null)
                            {
                                expr.EnumDecl = instance;
                                expr.Member = instance.Members.First(m => m.Name.Name == expr.Member.Name.Name);
                                at = expr.Member.AssociatedTypeExpr.Value as CheezType;
                            }
                        }
                        else
                        {
                            ReportError(expr, $"Can't infer type of enum value");
                            return expr;
                        }
                    }
                }

                if (at != null)
                {
                    expr.Argument = HandleReference(expr.Argument, at, context);
                    expr.Argument = CheckType(expr.Argument, at);
                }
            }
            else
            {
                if (expr.IsComplete && expr.Member.AssociatedTypeExpr != null)
                {
                    ReportError(expr, $"The enum member '{expr.Member.Name}' requires an argument of type {expr.Member.AssociatedTypeExpr.Value}");
                    return expr;
                }

                if (expr.EnumDecl.Type is GenericEnumType g)
                {
                    if (expected is EnumType enumType && enumType.DeclarationTemplate == g.Declaration)
                    {
                        expr.EnumDecl = enumType.Declaration;
                        expr.Member = enumType.Declaration.Members.First(m => m.Name.Name == expr.Member.Name.Name);
                    }
                    else if (expr.IsComplete)
                    {
                        ReportError(expr, $"Can't infer type of enum value");
                        return expr;
                    }
                }
            }

            expr.Type = expr.EnumDecl.Type;

            return expr;
        }

        private AstExpression InferTypeMatchExpr(AstMatchExpr expr, CheezType expected, TypeInferenceContext context)
        {
            expr.SubExpression.SetFlag(ExprFlags.ValueRequired, true);
            expr.SubExpression.AttachTo(expr);
            expr.SubExpression = InferTypeHelper(expr.SubExpression, null, context);

            if (expr.SubExpression.Type.IsErrorType)
                return expr;

            ConvertLiteralTypeToDefaultType(expr.SubExpression, null);
            if (expr.SubExpression.Type is ReferenceType)
                expr.SubExpression = Deref(expr.SubExpression, context);

            if (expr.SubExpression.GetFlag(ExprFlags.IsLValue))
            {
                var tmp = new AstTempVarExpr(expr.SubExpression, true);
                tmp.AttachTo(expr);
                tmp.SetFlag(ExprFlags.IsLValue, true);
                expr.SubExpression = InferTypeHelper(tmp, null, context);
            }
            else
            {
                var tmp = new AstTempVarExpr(expr.SubExpression);
                tmp.AttachTo(expr);
                tmp.SetFlag(ExprFlags.IsLValue, true);
                expr.SubExpression = InferTypeHelper(tmp, null, context);
            }
            
            expr.IsSimpleIntMatch = true;

            if (expr.Cases.Count == 0)
            {
                ReportError(expr, $"match expression must have at least one case");
                return expr;
            }

            foreach (var c in expr.Cases)
            {
                c.SubScope = new Scope("case", expr.Scope);

                // pattern
                c.Pattern.AttachTo(expr);
                c.Pattern.Scope = c.SubScope;
                c.Pattern.SetFlag(ExprFlags.ValueRequired, true);
                c.Pattern = MatchPatternWithType(c, c.Pattern, expr.SubExpression);

                if (c.Pattern.Type?.IsErrorType ?? false)
                {
                    c.Body.Type = CheezType.Error;
                    continue;
                }

                // condition
                if (c.Condition != null)
                {
                    c.Condition.AttachTo(expr);
                    c.Condition.Scope = c.SubScope;
                    c.Condition = InferTypeHelper(c.Condition, CheezType.Bool, context);
                    ConvertLiteralTypeToDefaultType(c.Condition, CheezType.Bool);
                    if (c.Condition.Type is ReferenceType)
                        c.Condition = Deref(c.Condition, context);
                    c.Condition = CheckType(c.Condition, CheezType.Bool);
                }

                // body
                c.Body.AttachTo(expr);
                c.Body.Scope = c.SubScope;
                c.Body.SetFlag(ExprFlags.ValueRequired, expr.GetFlag(ExprFlags.ValueRequired));
                c.Body = InferTypeHelper(c.Body, expected, context);
                ConvertLiteralTypeToDefaultType(c.Body, expected);

                if (expected != null)
                {
                    c.Body = HandleReference(c.Body, expected, context);
                    c.Body = CheckType(c.Body, expected);
                }

                if (!(c.Pattern is AstNumberExpr || c.Pattern is AstCharLiteral) || c.Condition != null)
                    expr.IsSimpleIntMatch = false;
            }

            expr.Type = SumType.GetSumType(expr.Cases.Select(c => c.Body.Type).ToArray());
            if (!(expr.Type is IntType || expr.Type is CharType))
                expr.IsSimpleIntMatch = false;

            return expr;
        }

        private AstExpression MatchPatternWithType(
            AstMatchCase cas,
            AstExpression pattern,
            AstExpression value)
        {
            if (value.Type is ReferenceType)
            {
                //pattern.SetFlag(ExprFlags.PatternRefersToReference, true);
            }

            switch (pattern)
            {
                case AstIdExpr id:
                    {
                        if (id.IsPolymorphic)
                        {
                            AstExpression tmpVar = new AstTempVarExpr(value, false);
                            tmpVar.Replace(value);
                            tmpVar = InferType(tmpVar, null);

                            id.Type = value.Type;
                            id.Scope.DefineUse(id.Name, tmpVar, false, out var use);
                            id.Symbol = use;
                        }
                        else
                        {
                            var newPattern = InferType(id, value.Type);
                            if (newPattern != pattern)
                                return MatchPatternWithType(cas, newPattern, value);

                            if (pattern.Type.IsErrorType)
                                return id;

                            if (id.Type != value.Type)
                                break;
                            if (!id.IsCompTimeValue)
                            {
                                ReportError(id, $"Must be constant");
                            }
                        }
                        return id;
                    }

                case AstCharLiteral n:
                    {
                        InferType(n, value.Type);
                        ConvertLiteralTypeToDefaultType(n, value.Type);
                        if (pattern.Type.IsErrorType)
                            return pattern;

                        if (n.Type != value.Type)
                            break;
                        return n;
                    }

                case AstNumberExpr n:
                    {
                        InferType(n, value.Type);
                        ConvertLiteralTypeToDefaultType(n, value.Type);
                        if (pattern.Type.IsErrorType)
                            return pattern;

                        if (n.Type != value.Type)
                            break;
                        return n;
                    }

                case AstTupleExpr te:
                    {
                        if (value.Type is TupleType tt)
                        {
                            if (te.Values.Count != tt.Members.Length)
                                break;
                            for (int i = 0; i < tt.Members.Length; i++)
                            {
                                var p = te.Values[i];
                                p.AttachTo(te);
                                AstExpression v = new AstArrayAccessExpr(value, new AstNumberExpr(i, Location: value.Location), value.Location);
                                v.AttachTo(value);
                                v = InferType(v, tt.Members[i].type);
                                te.Values[i] = MatchPatternWithType(cas, p, v);
                            }

                            te.Type = tt;

                            return te;
                        }
                        pattern.Type = CheezType.Error;
                        break;
                    }

                case AstDotExpr dot:
                    {
                        var d = InferType(dot, null);
                        if (d is AstEnumValueExpr e)
                        {
                            if (pattern.Type.IsErrorType)
                                return pattern;
                            if (e.Type != value.Type)
                                break;
                            return d;
                        }
                        else
                            break;
                    }

                case AstEnumValueExpr ev:
                    {
                        if (pattern.Type.IsErrorType)
                            return pattern;
                        if (ev.Type != value.Type)
                            break;
                        return ev;
                    }

                case AstCallExpr call:
                    {
                        call.FunctionExpr.AttachTo(call);
                        var d = InferType(call.FunctionExpr, value.Type);
                        if (d is AstEnumValueExpr e)
                        {
                            call.Type = e.Type;
                            if (e.Type != value.Type)
                                break;

                            if (call.Arguments.Count == 1)
                            { 
                                e.Argument = call.Arguments[0].Expr;
                                AstExpression sub = new AstDotExpr(value, new AstIdExpr(e.Member.Name.Name, false, call.Location), false, call.Location);
                                sub.AttachTo(value);
                                sub.SetFlag(ExprFlags.ValueRequired, pattern.GetFlag(ExprFlags.ValueRequired));
                                sub = InferType(sub, null);
                                e.Argument.AttachTo(e);
                                e.Argument = MatchPatternWithType(cas, e.Argument, sub);
                            }
                            else if (call.Arguments.Count > 1)
                            {
                                e.Argument = new AstTupleExpr(
                                    call.Arguments.Select(a => new AstParameter(null, a.Expr, null, a.Location)).ToList(),
                                    call.Location);

                                //e.Argument = MatchPatternWithType(cas, e.Argument, ...);
                            }

                            return d;
                        }
                        else
                        {
                            break;
                        }
                    }

                    case AstReferenceTypeExpr r:
                    {
                        r.Target.AttachTo(r);

                        if (r.Target is AstIdExpr id && id.IsPolymorphic)
                        {
                            var val = new AstAddressOfExpr(value, value.Location);
                            val.Reference = true;

                            AstExpression tmpVar = new AstTempVarExpr(val, false);
                            tmpVar.Replace(r);
                            tmpVar = InferType(tmpVar, null);

                            id.Type = tmpVar.Type;
                            id.Scope.DefineUse(id.Name, tmpVar, false, out var use);
                            id.Symbol = use;

                            r.Type = id.Type;
                            return r;
                        }
                        else
                        {
                            ReportError(r, $"invalid pattern");
                        }
                        return r;
                    }

            }

            //if (pattern.Type?.IsErrorType ?? false)
                ReportError(pattern, $"Can't match type {value.Type} to pattern {pattern}");
            pattern.Type = CheezType.Error;
            return pattern;
        }

        private AstExpression InferTypeDefaultExpr(AstDefaultExpr expr, CheezType expected, TypeInferenceContext context)
        {
            expr.IsCompTimeValue = true;
            if (expected == null)
            {
                ReportError(expr, $"Can't infer type of default expression");
                return expr;
            }

            if (expected is ReferenceType r)
            {
                ReportError(expr, $"Can't default initialize a reference");
                return expr;
            }

            if (expected is EnumType e)
            {
                ReportError(expr, $"Can't default initialize an enum");
                return expr;
            }

            if (expected is StructType s)
            {
                if (s.Declaration.GetFlag(StmtFlags.NoDefaultInitializer))
                {
                    ReportError(expr, $"Can't default initialize struct {s}");
                    return expr;
                }
            }

            //if (expected is ArrayType a)
            //{
            //    ReportError(expr, $"Can't default initialize an array");
            //    return expr;
            //}

            expr.Type = expected;

            //switch (expr.Type)
            //{
            //    case StructType s:
            //        {
            //            var sv = new AstStructValueExpr(new AstTypeRef(s, expr.Location), new List<AstStructMemberInitialization>(), expr.Location);
            //            sv.Replace(expr);
            //            return InferTypeHelper(sv, expected, context);
            //        }
            //}

            return expr;
        }

        private AstExpression InferTypesNaryExpr(AstNaryOpExpr expr, CheezType expected, TypeInferenceContext context)
        {
            if (expr.ActualOperator == null)
            {
                for (int i = 0; i < expr.Arguments.Count; i++)
                {
                    expr.Arguments[i].AttachTo(expr);
                    expr.Arguments[i] = InferTypeHelper(expr.Arguments[i], null, context);
                    ConvertLiteralTypeToDefaultType(expr.Arguments[i], null);
                }

                var argTypes = expr.Arguments.Select(a => a.Type);
                var ops = expr.Scope.GetNaryOperators(expr.Operator, argTypes.ToArray());


                if (ops.Count == 0)
                {
                    ReportError(expr,
                        $"No operator '{expr.Operator}' matches the types ({string.Join(", ", argTypes)})");
                    return expr;
                }
                else if (ops.Count > 1)
                {
                    // TODO: show matching operators
                    ReportError(expr, $"Multiple operators '{expr.Operator}' match the types ({string.Join(", ", argTypes)})");
                    return expr;
                }

                var op = ops[0];

                for (int i = 0; i < expr.Arguments.Count; i++)
                {
                    expr.Arguments[i] = HandleReference(expr.Arguments[i], op.ArgTypes[i], context);
                    expr.Arguments[i] = CheckType(expr.Arguments[i], op.ArgTypes[i]);
                }

                expr.ActualOperator = op;
            }

            if (expr.ActualOperator is UserDefinedNaryOperator user)
            {
                var args = expr.Arguments.Select(a =>
                    new AstArgument(a, Location: a)).ToList();
                var func = new AstSymbolExpr(user.Declaration);
                var call = new AstCallExpr(func, args, expr.Location);
                call.Replace(expr);
                return InferType(call, expected);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private AstExpression InferTypeTypeRefExpr(AstTypeRef expr)
        {
            expr.Type = CheezType.Type;
            return expr;
        }

        private AstExpression InferTypeSymbolExpr(AstSymbolExpr s)
        {
            s.Type = s.Symbol.Type;
            s.SetFlag(ExprFlags.IsLValue, true);
            return s;
        }

        private AstExpression InferTypeTempVarExpr(AstTempVarExpr expr, CheezType expected, TypeInferenceContext context)
        {
            if (expr.Expr.Type == null)
                expr.Expr = InferTypeHelper(expr.Expr, expected, context);
            expr.Type = expr.Expr.Type;
            return expr;
        }

        private AstExpression InferTypeBoolExpr(AstBoolExpr expr)
        {
            expr.Type = CheezType.Bool;
            expr.Value = expr.BoolValue;
            return expr;
        }

        private AstExpression InferTypeArgExpr(AstArgument arg, CheezType expected, TypeInferenceContext context)
        {
            arg.Expr.AttachTo(arg);
            arg.Expr = InferTypeHelper(arg, expected, context);
            arg.Type = arg.Expr.Type;
            arg.Value = arg.Expr.Value;
            arg.IsCompTimeValue = arg.Expr.IsCompTimeValue;
            return arg;
        }

        private AstExpression InferTypeReferenceTypeExpr(AstReferenceTypeExpr p, CheezType expected, TypeInferenceContext context)
        {
            p.Target.AttachTo(p);
            p.Target = InferTypeHelper(p.Target, expected, context);
            if (p.Target.Type == CheezType.Type)
            {
                p.Type = CheezType.Type;
                p.Value = ReferenceType.GetRefType(p.Target.Value as CheezType);
            }
            else
            {
                var r = new AstAddressOfExpr(p.Target, p);
                r.Replace(p);
                r.Reference = true;
                return InferTypeHelper(r, p.Target.Type, context);
            }
            return p;
        }

        private AstExpression InferTypeSliceTypeExpr(AstSliceTypeExpr p, TypeInferenceContext context)
        {
            p.Target.AttachTo(p);
            p.Target.SetFlag(ExprFlags.ValueRequired, true);
            p.Target = InferTypeHelper(p.Target, CheezType.Type, context);
            if (p.Target.Type == CheezType.Type)
            {
                p.Type = CheezType.Type;
                p.Value = SliceType.GetSliceType(p.Target.Value as CheezType);
            }
            else
            {
                ReportError(p, $"Can't create a reference type to non type.");
            }
            return p;
        }

        private AstExpression InferTypeArrayTypeExpr(AstArrayTypeExpr p, TypeInferenceContext context)
        {
            p.Target.AttachTo(p);
            p.Target.SetFlag(ExprFlags.ValueRequired, true);
            p.Target = InferTypeHelper(p.Target, CheezType.Type, context);
            if (p.Target.Type == CheezType.Type)
            {
                p.Type = CheezType.Type;
                p.Value = SliceType.GetSliceType(p.Target.Value as CheezType);
            }
            else
            {
                ReportError(p, $"Can't create a reference type to non type.");
                return p;
            }

            p.SizeExpr.AttachTo(p);
            p.SizeExpr.SetFlag(ExprFlags.ValueRequired, true);
            p.SizeExpr = InferType(p.SizeExpr, IntType.DefaultType);
            ConvertLiteralTypeToDefaultType(p.SizeExpr, IntType.DefaultType);

            if (!p.SizeExpr.IsCompTimeValue || !(p.SizeExpr.Type is IntType))
            {
                ReportError(p.SizeExpr, "Index must be a constant int");
                return p;
            }
            else
            {
                int v = (int)((NumberData)p.SizeExpr.Value).IntValue;
                p.Type = CheezType.Type;
                p.Value = ArrayType.GetArrayType(p.Target.Value as CheezType, v);
                return p;
            }
        }

        private AstExpression InferTypeFunctionTypeExpr(AstFunctionTypeExpr func, TypeInferenceContext context)
        {
            for (int i = 0; i < func.ParameterTypes.Count; i++)
            {
                func.ParameterTypes[i].AttachTo(func);
                func.ParameterTypes[i].SetFlag(ExprFlags.ValueRequired, true);
                func.ParameterTypes[i] = ResolveType(func.ParameterTypes[i], context, out var t);

            }

            CheezType ret = CheezType.Void;

            if (func.ReturnType != null)
            {
                func.ReturnType.SetFlag(ExprFlags.ValueRequired, true);
                func.ReturnType.AttachTo(func);
                func.ReturnType = ResolveType(func.ReturnType, context, out var t);
                ret = t;
            }
            
            if ((func.ReturnType?.Type?.IsErrorType ?? false) || func.ParameterTypes.Any(t => t.Type.IsErrorType))
            {
                return func;
            }

            var paramTypes = func.ParameterTypes.Select(
                p => ((string)null, p.Value as CheezType, (AstExpression)null)).ToArray();

            var cc = FunctionType.CallingConvention.Default;

            if (func.HasDirective("stdcall"))
                cc = FunctionType.CallingConvention.Stdcall;

            func.Type = CheezType.Type;
            func.Value = new FunctionType(paramTypes, ret, cc);
            return func;
        }

        private AstExpression InferTypeArrayExpr(AstArrayExpr expr, CheezType expected, TypeInferenceContext context)
        {
            CheezType subExpected = null;
            if (expected != null)
            {
                if (expected is ArrayType arr)
                    subExpected = arr.TargetType;
                else if (expected is SliceType s)
                    subExpected = s.TargetType;
            }

            var type = subExpected;

            for (int i = 0; i < expr.Values.Count; i++)
            {
                expr.Values[i].Scope = expr.Scope;
                expr.Values[i] = InferType(expr.Values[i], subExpected);
                ConvertLiteralTypeToDefaultType(expr.Values[i], subExpected);


                if (type == null)
                {
                    type = expr.Values[i].Type;
                    if (type is ReferenceType r)
                        type = r.TargetType;
                }

                expr.Values[i] = HandleReference(expr.Values[i], type, context);
                expr.Values[i] = CheckType(expr.Values[i], type);
            }

            if (type == null)
            {
                ReportError(expr, $"Failed to infer type for array expression");
                expr.Type = CheezType.Error;
                return expr;
            }

            expr.Type = ArrayType.GetArrayType(type, expr.Values.Count);
            return expr;
        }

        private AstExpression InferTypeUfcFuncExpr(AstUfcFuncExpr expr)
        {
            expr.Type = expr.FunctionDecl.Type;
            return expr;
        }

        private AstExpression InferTypesNullExpr(AstNullExpr expr, CheezType expected)
        {
            expr.IsCompTimeValue = true;
            if (expected is PointerType)
                expr.Type = expected;
            else if (expected is SliceType)
                expr.Type = expected;
            else
                expr.Type = PointerType.NullLiteralType;// PointerType.GetPointerType(CheezType.Any);
            return expr;
        }

        private AstExpression InferTypeCast(AstCastExpr cast, CheezType expected, TypeInferenceContext context)
        {
            CheezType subExpected = null;
            if (cast.TypeExpr != null)
            {
                cast.TypeExpr.SetFlag(ExprFlags.ValueRequired, true);
                cast.TypeExpr.AttachTo(cast);
                cast.TypeExpr = ResolveTypeNow(cast.TypeExpr, out var type);
                cast.Type = type;
                subExpected = cast.Type;
            }
            else if (expected != null)
            {
                cast.Type = expected;
            }
            else
            {
                ReportError(cast, $"Auto cast not possible here");
            }

            cast.SubExpression.SetFlag(ExprFlags.ValueRequired, true);
            cast.SubExpression.Scope = cast.Scope;
            cast.SubExpression = InferTypeHelper(cast.SubExpression, subExpected, context);
            ConvertLiteralTypeToDefaultType(cast.SubExpression, cast.Type);

            if (cast.SubExpression.Type.IsErrorType || cast.Type.IsErrorType)
                return cast;

            if (cast.SubExpression.Type == cast.Type)
                return cast.SubExpression;

            cast.SubExpression = HandleReference(cast.SubExpression, cast.Type, context);

            var to = cast.Type;
            var from = cast.SubExpression.Type;

            // check for trait cast
            if (to is TraitType t)
            {
                if (!cast.SubExpression.GetFlag(ExprFlags.IsLValue))
                {
                    var tmp = new AstTempVarExpr(cast.SubExpression);
                    cast.SubExpression = InferTypeHelper(tmp, cast.SubExpression.Type, context);

                    // @TODO: make this an error
                    ReportError(cast.Location, $"Can't cast a non-lvalue to a trait");
                    return cast;
                }

                if (t.Declaration.Implementations.ContainsKey(from))
                    return cast;

                if (t.Declaration.IsPolyInstance)
                {
                    var impls = GetImplsForType(from, t);

                    if (impls.Count == 0)
                    {
                        ReportError(cast, $"Can't cast {from} to {to} because it doesn't implement the trait");
                        return cast;
                    }
                    else if (impls.Count > 1)
                    {
                        throw new Exception("Shouldn't happen i guess?");
                    }

                    return cast;
                }
                else
                {
                    var template = t.Declaration.FindMatchingImplementation(from);
                    if (template == null)
                    {
                        ReportError(cast, $"Can't cast {from} to {to} because it doesn't implement the trait");
                        return cast;
                    }

                    var polyTypes = new Dictionary<string, CheezType>();
                    CollectPolyTypes(template.TargetType, from, polyTypes);

                    var impl = InstantiatePolyImplNew(template, polyTypes);
                    return cast;
                }
            }

            else if ((to is PointerType && from is PointerType) ||
                (to is IntType && from is PointerType) ||
                (to is PointerType && from is IntType) ||
                (to is PointerType p1 && from is ArrayType a1 && p1.TargetType == a1.TargetType) ||
                (to is IntType && from is IntType) ||
                (to is FloatType && from is FloatType) ||
                (to is FloatType && from is IntType) ||
                (to is IntType && from is FloatType) ||
                (to is IntType && from is BoolType) ||
                (to is IntType && from is CharType) ||
                (to is CharType && from is IntType) ||
                (to is SliceType s && from is PointerType p && s.TargetType == p.TargetType) ||
                (to is SliceType s2 && from is ArrayType a && a.TargetType == s2.TargetType) ||
                (to is IntType && from is EnumType) ||
                (to is FunctionType && from is FunctionType) ||
                (to is BoolType && from is FunctionType) ||
                (to is FunctionType && from is PointerType p2 && p2.TargetType == CheezType.Any))
            {
                return cast;
            }

            ReportError(cast, $"Can't convert from type {from} to type {to}");

            return cast;
        }

        private AstExpression InferTypeDeref(AstDereferenceExpr expr, CheezType expected, TypeInferenceContext context)
        {
            CheezType subExpect = null;
            if (expected != null) subExpect = PointerType.GetPointerType(expected);

            expr.SubExpression.AttachTo(expr);
            expr.SubExpression.SetFlag(ExprFlags.ValueRequired, true);
            expr.SubExpression = InferTypeHelper(expr.SubExpression, subExpect, context);

            if (expr.Reference)
            {
                if (expr.SubExpression.Type is ReferenceType r)
                {
                    expr.Type = r.TargetType;
                }
                else
                {
                    ReportError(expr, $"Can't dereference non reference type {expr.SubExpression.Type}");
                }
            }
            else
            {
                if (expr.SubExpression.Type is ReferenceType r)
                {
                    expr.SubExpression = Deref(expr.SubExpression, context);
                }
                if (expr.SubExpression.Type is PointerType p)
                {
                    expr.Type = p.TargetType;
                }
                else if (!expr.SubExpression.Type.IsErrorType)
                {
                    ReportError(expr, $"Can't dereference non pointer type {expr.SubExpression.Type}");
                }
            }


            expr.SetFlag(ExprFlags.IsLValue, true);
            return expr;
        }

        private AstExpression InferTypeIfExpr(AstIfExpr expr, CheezType expected, TypeInferenceContext context)
        {
            if (expr.ElseCase == null)
                expr.ElseCase = new AstBlockExpr(new List<AstStatement>(), new Location(expr.IfCase.End));

            if (expr.IsConstIf)
                expr.SubScope = expr.Scope;
            else
            {
                expr.SubScope = new Scope("if", expr.Scope);
            }

            if (expr.PreAction != null)
            {
                expr.PreAction.Scope = expr.SubScope;
                expr.PreAction.Parent = expr;
                AnalyseVariableDecl(expr.PreAction);
            }

            expr.Condition.SetFlag(ExprFlags.ValueRequired, true);
            expr.Condition.AttachTo(expr, expr.SubScope);
            expr.Condition = InferTypeHelper(expr.Condition, CheezType.Bool, context);
            ConvertLiteralTypeToDefaultType(expr.Condition, CheezType.Bool);

            if (expr.Condition.Type is ReferenceType)
                expr.Condition = Deref(expr.Condition, context);

            expr.Condition = CheckType(expr.Condition, CheezType.Bool, $"Condition of if statement must be either a bool or a pointer but is {expr.Condition.Type}");

            if (expr.IsConstIf)
            {
                if (expr.Condition.Value == null)
                {
                    ReportError(expr.Condition, $"Condition must be a compile time constant");
                    return expr;
                }

                var cond = (bool)expr.Condition.Value;
                if (cond)
                {
                    expr.IfCase.Replace(expr);
                    expr.IfCase.SetFlag(ExprFlags.Anonymous, true);
                    return InferTypeHelper(expr.IfCase, expected, context);
                }
                else if (expr.ElseCase != null)
                {
                    expr.ElseCase.Replace(expr);
                    expr.ElseCase.SetFlag(ExprFlags.Anonymous, true);
                    return InferTypeHelper(expr.ElseCase, expected, context);
                }
                else
                {
                    var emptyBlock = new AstBlockExpr(
                            new List<AstStatement>(), expr.Condition.Location);
                    emptyBlock.Replace(expr);
                    return InferTypeHelper(emptyBlock, expected, context);
                }
            }

            expr.IfCase.SetFlag(ExprFlags.ValueRequired, expr.GetFlag(ExprFlags.ValueRequired));
            expr.IfCase.AttachTo(expr, expr.SubScope);
            expr.IfCase = InferTypeHelper(expr.IfCase, expected, context);
            ConvertLiteralTypeToDefaultType(expr.IfCase, expected);

            if (expr.ElseCase != null)
            {
                expr.ElseCase.SetFlag(ExprFlags.ValueRequired, expr.GetFlag(ExprFlags.ValueRequired));
                expr.ElseCase.AttachTo(expr, expr.SubScope);
                expr.ElseCase = InferTypeHelper(expr.ElseCase, expected, context);
                ConvertLiteralTypeToDefaultType(expr.ElseCase, expected);
                
                if (expr.IfCase.Type == expr.ElseCase.Type)
                {
                    expr.Type = expr.IfCase.Type;
                }
                else
                {
                    expr.Type = SumType.GetSumType(expr.IfCase.Type, expr.ElseCase.Type);
                }

                if (expr.IfCase.GetFlag(ExprFlags.Returns) && expr.ElseCase.GetFlag(ExprFlags.Returns))
                {
                    expr.SetFlag(ExprFlags.Returns, true);
                }

                expr.SetFlag(ExprFlags.Breaks,
                    expr.IfCase.GetFlag(ExprFlags.Breaks) && expr.ElseCase.GetFlag(ExprFlags.Breaks));
            }
            else
            {
                expr.Type = CheezType.Void;
            }

            return expr;
        }

        private AstExpression InferTypeAddressOf(AstAddressOfExpr expr, CheezType expected, TypeInferenceContext context)
        {
            CheezType subExpected = null;
            if (expected is PointerType p)
                subExpected = p.TargetType;
            else if (expected == CheezType.Type)
                subExpected = expected;

            expr.SubExpression.SetFlag(ExprFlags.ValueRequired, true);
            expr.SubExpression.AttachTo(expr);
            expr.SubExpression = InferTypeHelper(expr.SubExpression, subExpected, context);

            if (expr.SubExpression.Type.IsErrorType)
                return expr;

            // handle type expression
            if (expr.SubExpression.Type == CheezType.Type)
            {
                var subType = expr.SubExpression.Value as CheezType;
                expr.Type = CheezType.Type;
                expr.Value = PointerType.GetPointerType(subType);
                return expr;
            }

            if (expr.Reference)
            {
                if (!expr.SubExpression.GetFlag(ExprFlags.IsLValue))
                {
                    // create temp variable
                    //var tmpVar = new AstTempVarExpr(expr.SubExpression);
                    //tmpVar.AttachTo(expr);
                    //expr.SubExpression = InferType(tmpVar, null);

                    ReportError(expr, $"Can't create a reference to non l-value of type '{expr.SubExpression.Type}'");
                    expr.Type = CheezType.Error;
                    return expr;
                }

                expr.Type = ReferenceType.GetRefType(expr.SubExpression.Type);
                expr.SetFlag(ExprFlags.IsLValue, true);
            }
            else
            {
                if (expr.SubExpression.Type is ReferenceType)
                {
                    expr.SubExpression = Deref(expr.SubExpression, context);
                }

                if (!expr.SubExpression.GetFlag(ExprFlags.IsLValue))
                {
                    ReportError(expr, $"Can't take the address of non l-value of type '{expr.SubExpression.Type}'");
                    expr.Type = CheezType.Error;
                    return expr;
                }

                expr.Type = PointerType.GetPointerType(expr.SubExpression.Type);
            }

            return expr;
        }

        private AstExpression InferTypeCompCall(AstCompCallExpr expr, CheezType expected, TypeInferenceContext context)
        {
            AstExpression InferArg(int index, CheezType e)
            {
                var arg = expr.Arguments[index];
                arg.Expr.SetFlag(ExprFlags.ValueRequired, true);
                arg.AttachTo(expr);
                arg.Expr.AttachTo(arg);
                arg.Expr = InferTypeHelper(arg.Expr, e, context);

                ConvertLiteralTypeToDefaultType(arg.Expr, e);
                if (e != null)
                {
                    arg.Expr = HandleReference(arg.Expr, e, context);
                    arg.Expr = CheckType(arg.Expr, e);
                }

                return arg.Expr;
            }

            switch (expr.Name.Name)
            {
                case "dup":
                    {
                        if (expr.Arguments.Count < 1 || expr.Arguments.Count > 2)
                        {
                            ReportError(expr.Location, "@dup takes one or two argument2");
                            return expr;
                        }

                        if (expected is ArrayType arr)
                        {
                            var val = InferArg(0, arr.TargetType);

                            var size = arr.Length;
                            if (expr.Arguments.Count == 2)
                            {
                                var sizeExpr = InferArg(1, IntType.DefaultType);
                                if (!sizeExpr.IsCompTimeValue)
                                {
                                    ReportError(expr.Arguments[1], "Argument must be a constant int");
                                    return expr;
                                }
                                size = (int)((NumberData)sizeExpr.Value).IntValue;
                            }

                            expr.Type = ArrayType.GetArrayType(arr.TargetType, size);
                        }
                        else
                        {
                            var val = InferArg(0, null);

                            if (expr.Arguments.Count == 2)
                            {
                                var sizeExpr = InferArg(1, IntType.DefaultType);
                                if (!sizeExpr.IsCompTimeValue)
                                {
                                    ReportError(expr.Arguments[1], "Argument must be a constant int");
                                    return expr;
                                }
                                var size = (int)((NumberData)sizeExpr.Value).IntValue;
                                expr.Type = ArrayType.GetArrayType(val.Type, size);
                            }
                            else
                            {
                                ReportError(expr, $"Failed to infer size from context, please provide the size as a second argument");
                                return expr;
                            }
                        }

                        return expr;
                    }

                case "is_default_constructable":
                    {
                        if (expr.Arguments.Count != 1)
                        {
                            ReportError(expr.Location, "@is_default_constructable takes exactly one argument");
                            return expr;
                        }

                        var arg = InferArg(0, CheezType.Type);
                        if (arg.Type is CheezTypeType)
                        {
                            var type = arg.Value as CheezType;
                            expr.Type = CheezType.Bool;
                            expr.Value = type.IsDefaultConstructable;
                            return expr;
                        }
                        else
                        {
                            ReportError(arg, "argument must be a type");
                            return expr;
                        }
                    }

                case "log_symbol_status":
                    {
                        if (expr.Arguments.Count != 1)
                        {
                            ReportError(expr.Location, "@log_symbol_status takes exactly one argument");
                            return expr;
                        }

                        var arg = expr.Arguments[0].Expr;
                        if (!(arg is AstIdExpr id))
                        {
                            ReportError(arg.Location, $"argument must be an identifier");
                            return expr;
                        }

                        expr.SetFlag(ExprFlags.IgnoreInCodeGen, true);
                        expr.Type = CheezType.Void;
                        break;
                    }

                case "set_break_and_continue":
                    {
                        if (expr.Arguments.Count != 1)
                        {
                            ReportError(expr.Location, "@set_break_and_continue takes exactly one argument");
                            return expr;
                        }

                        var labelExpr = expr.Arguments[0].Expr;
                        if (!(labelExpr is AstIdExpr label))
                        {
                            ReportError(labelExpr, $"Argument must be an identifier");
                            return expr;
                        }
                        expr.Scope.OverrideBreakName(label.Name);
                        expr.Scope.OverrideContinueName(label.Name);

                        expr.Type = CheezType.Void;
                        expr.SetFlag(ExprFlags.IgnoreInCodeGen, true);
                        return expr;
                    }

                case "code":
                    {
                        if (expr.Arguments.Count != 1)
                        {
                            ReportError(expr.Location, "@code takes exactly one argument");
                            return expr;
                        }

                        var arg = expr.Arguments[0].Expr;
                        arg.Scope = new Scope($"code", expr.Scope);
                        expr.Type = CheezType.Code;
                        expr.Value = arg;
                        return expr;
                    }

                case "insert":
                    {
                        if (expr.Arguments.Count == 0)
                        {
                            ReportError(expr.Location, "@insert takes at least one argument");
                            return expr;
                        }
                        var code = InferArg(0, null);

                        if (code.Value == null)
                        {
                            ReportError(expr, $"argument is not constant");
                            return expr;
                        }

                        code = code.Value as AstExpression;
                        code = code.Clone();
                        code.Scope = new Scope("insert{}", code.Scope);

                        var links = expr.Arguments.Where(a => a.Name?.Name == "link").ToList();
                        foreach (var link in links)
                        {
                            if (link.Expr is AstArrayExpr arr)
                            {
                                foreach (var varToLink in arr.Values)
                                {
                                    if (varToLink is AstIdExpr varName)
                                    {
                                        varName.SetFlag(ExprFlags.ValueRequired, true);
                                        varName.AttachTo(expr);
                                        InferTypeHelper(varName, null, context);
                                        if (varName.Symbol != null)
                                        {
                                            code.Scope.DefineSymbol(varName.Symbol);

                                            // @todo: do this in other pass
                                            //var status = expr.Scope.GetSymbolStatus(varName.Symbol);
                                            //code.Scope.SetSymbolStatus(varName.Symbol, status.kind, status.location);
                                        }
                                    }
                                    else
                                    {
                                        ReportError(link.Expr, $"Argument to link array must be an identifier");
                                    }
                                }
                            }
                            else
                            {
                                ReportError(link.Expr, $"Argument to link must be an array");
                            }
                        }

                        var _breaks = expr.Arguments.Where(a => a.Name?.Name == "_break").ToArray();
                        if (_breaks.Length > 1)
                        {
                            ReportError(expr, $"Only one argument can be named '_break'");
                        }
                        else if (_breaks.Length == 1)
                        {
                            var _break = _breaks[0];
                            var action = _break.Expr;
                            action.AttachTo(expr);
                            code.Scope.DefineBreak(null, action);
                        }

                        // continue
                        var _continues = expr.Arguments.Where(a => a.Name?.Name == "_continue").ToArray();
                        if (_continues.Length > 1)
                        {
                            ReportError(expr, $"Only one argument can be named '_continue'");
                        }
                        else if (_continues.Length == 1)
                        {
                            var _continue = _continues[0];
                            var action = _continue.Expr;
                            action.AttachTo(expr);
                            code.Scope.DefineContinue(null, action);
                        }

                        code.Parent = expr;
                        code.Scope.LinkedScope = expr.Scope;
                        code.Value = null;
                        code = InferTypeHelper(code, expected, context);

                        return code;
                    }

                case "link":
                    {
                        if (expr.Arguments.Count != 1)
                        {
                            ReportError(expr.Location, "@link takes exacty one argument");
                            return expr;
                        }

                        var arg = expr.Arguments[0].Expr;
                        arg.Scope = expr.Scope.LinkedScope;
                        arg.SetFlag(ExprFlags.Anonymous, true);
                        arg.SetFlag(ExprFlags.Link, true);
                        arg.SetFlag(ExprFlags.ValueRequired, true);

                        if (arg.Scope == null)
                        {
                            ReportError(expr, "There is no scope linked to the current scope or any of its parents");
                            return expr;
                        }

                        arg = InferTypeHelper(arg, arg.Type, context);
                        return arg;
                    }

                case "cast":
                    {
                        if (expr.Arguments.Count != 2)
                        {
                            ReportError(expr.Location, "@cast requires two arguments (type and value)");
                            return expr;
                        }

                        var targetTypeExpr = InferArg(0, null);
                        if (targetTypeExpr.Type != CheezType.Type)
                        {
                            ReportError(targetTypeExpr.Location, $"First argument of @cast has to be a type but is {targetTypeExpr.Type}");
                            return expr;
                        }

                        var targetType = (CheezType)targetTypeExpr.Value;
                        var value = InferArg(1, null);

                        var cast = new AstCastExpr(
                            new AstTypeRef(targetType, targetTypeExpr.Location),
                            value,
                            expr.Location);
                        cast.Replace(expr);

                        return InferTypeHelper(cast, expected, context);
                    }

                case "panic":
                    {
                        if (expr.Arguments.Count != 1)
                        {
                            ReportError(expr, $"@panic requires one argument");
                            return expr;
                        }

                        InferArg(0, CheezType.String);

                        expr.Type = CheezType.Void;
                        break;
                    }

                case "log":
                    {
                        for (int i = 0; i < expr.Arguments.Count; i++)
                            InferArg(i, null);

                        var text = string.Join("", expr.Arguments.Select(a => a.Value?.ToString()));
                        Console.WriteLine($"[@log] {text}");
                        expr.Type = CheezType.Void;
                        break;
                    }

                case "assert":
                    {
                        string msg = "Assertion failed";

                        if (expr.Arguments.Count < 1 || expr.Arguments.Count > 2)
                        {
                            ReportError(expr, $"Wrong number of arguments to @assert(condition: bool, message: string = \"{msg}\")");
                            return expr;
                        }

                        InferArg(0, CheezType.Bool);

                        if (expr.Arguments.Count > 1)
                        {
                            var arg = InferArg(1, CheezType.String);
                            if (!arg.IsCompTimeValue)
                            {
                                ReportError(arg, $"Argument must be a compile time constant");
                                return expr;
                            }
                        }

                        expr.Type = CheezType.Void;
                        break;
                    }


                case "static_assert":
                    {

                        AstExpression cond = null, message = null;
                        if (expr.Arguments.Count >= 1)
                            cond = expr.Arguments[0].Expr;
                        if (expr.Arguments.Count >= 2)
                            message = expr.Arguments[1].Expr;

                        if (cond == null || expr.Arguments.Count > 2)
                        {
                            ReportError(expr, $"Wrong number of arguments");
                            return expr;
                        }

                        // infer types of arguments
                        cond.AttachTo(expr);
                        cond = InferType(cond, CheezType.Bool);

                        if (message != null)
                        {
                            message.AttachTo(expr);
                            message = InferType(message, CheezType.Bool);
                        }

                        // check types of arguments
                        if (cond.Type.IsErrorType || (message?.Type?.IsErrorType ?? false))
                            return expr;

                        if (cond.Type != CheezType.Bool || !cond.IsCompTimeValue)
                        {
                            ReportError(cond, $"Condition of @static_assert must be a constant bool");
                            return expr;
                        }

                        if (message != null && !(message.Value is string v))
                        {
                            ReportError(message, $"Message of @static_assert must be a constant string");
                            return expr;
                        }

                        var actualMessage = (message?.Value as string) ?? "Static assertion failed";

                        // check condition
                        if (!(bool)cond.Value)
                            ReportError(expr, actualMessage);

                        expr.Type = CheezType.Void;
                        return expr;
                    }

                case "sizeof":
                    {
                        if (expr.Arguments.Count != 1)
                        {
                            ReportError(expr, $"@sizeof takes one argument");
                            return expr;
                        }
                        
                        var arg = InferArg(0, CheezType.Type);
                        if (arg.Type.IsErrorType)
                            return expr;

                        if (arg.Type != CheezType.Type)
                        {
                            ReportError(arg, $"Argument must be a type but is '{arg.Type}'");
                            return expr;
                        }

                        var type = (CheezType)arg.Value;

                        var num = new AstNumberExpr(type.Size, Location: expr.Location);
                        num.SetFlag(ExprFlags.ValueRequired, expr.GetFlag(ExprFlags.ValueRequired));
                        return InferTypeHelper(num, null, context);
                    }

                case "alignof":
                    {
                        if (expr.Arguments.Count != 1)
                        {
                            ReportError(expr, $"@alignof takes one argument");
                            return expr;
                        }

                        var arg = InferArg(0, CheezType.Type);
                        if (arg.Type.IsErrorType)
                            return expr;

                        if (arg.Type != CheezType.Type)
                        {
                            ReportError(arg, $"Argument must be a type but is '{arg.Type}'");
                            return expr;
                        }

                        var type = (CheezType)arg.Value;

                        var num = new AstNumberExpr(type.Alignment, Location: expr.Location);
                        num.SetFlag(ExprFlags.ValueRequired, expr.GetFlag(ExprFlags.ValueRequired));
                        return InferTypeHelper(num, null, context);
                    }

                case "tuple_type_member":
                    {
                        if (expr.Arguments.Count != 2)
                        {
                            ReportError(expr, $"@tuple_type_member requires two arguments (tuple type, int)");
                            return expr;
                        }

                        var type = InferArg(0, CheezType.Type);
                        if ((type.Value as CheezType)?.IsPolyType ?? false)
                        {
                            expr.Type = CheezType.Type;
                            expr.Value = new PolyType(expr.ToString());
                            return expr;
                        }
                        {
                            var arg = expr.Arguments[1];
                            arg.AttachTo(expr);
                            arg.Expr.AttachTo(arg);
                            arg.Expr = InferTypeHelper(arg.Expr, IntType.DefaultType, context);
                            if (arg.Expr.Type is CheezType && arg.Expr.Value is CheezType t && t.IsPolyType)
                            {
                                expr.Type = CheezType.Type;
                                expr.Value = new PolyType(expr.ToString());
                                return expr;
                            }
                        }
                        var index = InferArg(1, IntType.DefaultType);

                        if (type.Value is PolyType || index.Value is PolyType)
                        {
                            expr.Type = CheezType.Type;
                            expr.Value = new PolyType($"tuple_type_member({type.Value}, {index.Value})");
                            return expr;
                        }

                        if (type.Type != CheezType.Type || !(type.Value is TupleType))
                        {
                            if (type.Value is PolyType)
                            {
                                expr.Type = CheezType.Type;
                                expr.Value = new PolyType($"tuple_type_member({type.Value}, {index.Value})");
                                //expr.Value = expr.Arguments[0].Type;
                                return expr;
                            }
                            ReportError(type, $"This argument must be a tuple type, got {type.Type} '{type.Value}'");
                            return expr;
                        }
                        if (!(index.Type is IntType) || !index.IsCompTimeValue)
                        {
                            ReportError(index, $"This argument must be a constant int, got {index.Type} '{index.Value}'");
                            return expr;
                        }

                        var tuple = type.Value as TupleType;
                        var indexInt = ((NumberData)index.Value).ToLong();

                        if (indexInt < 0 || indexInt >= tuple.Members.Length)
                        {
                            ReportError(index, $"Index '{index}' is out of range. Index must be between [0, {tuple.Members.Length})");
                            return expr;
                        }

                        expr.Type = CheezType.Type;
                        expr.Value = tuple.Members[indexInt].type;

                        break;
                    }

                case "typeof":
                    {
                        if (expr.Arguments.Count != 1)
                        {
                            ReportError(expr, $"@typeof takes one argument");
                            return expr;
                        }

                        var arg = InferArg(0, null);
                        if (arg.Type.IsErrorType)
                            return expr;

                        var result = new AstTypeRef(arg.Type, expr);
                        result.AttachTo(expr);
                        result.SetFlag(ExprFlags.ValueRequired, expr.GetFlag(ExprFlags.ValueRequired));
                        return InferTypeHelper(result, null, context);
                    }

                case "typename":
                    {
                        if (expr.Arguments.Count != 1)
                        {
                            ReportError(expr, $"@typename takes one argument");
                            return expr;
                        }

                        var arg = InferArg(0, CheezType.Type);
                        if (arg.Type.IsErrorType)
                            return expr;

                        if (arg.Type != CheezType.Type)
                        {
                            ReportError(arg, $"Argument must be a type but is '{arg.Type}'");
                            return expr;
                        }

                        var type = (CheezType)arg.Value;

                        var result = new AstStringLiteral(type.ToString(), Location: expr);
                        result.AttachTo(expr);
                        return InferTypeHelper(result, null, context);
                    }

                case "alloca":
                    {
                        if (expr.Arguments.Count != 2)
                        {
                            ReportError(expr, $"@alloca takes two arguments");
                            return expr;
                        }

                        var argType = InferArg(0, CheezType.Type);
                        var argSize = InferArg(1, IntType.DefaultType);

                        if (argSize.Type.IsErrorType || argType.Type.IsErrorType)
                            return expr;

                        if (argType.Type is CheezType)
                            expr.Type = PointerType.GetPointerType(argType.Value as CheezType);
                        else
                            ReportError(argSize, $"Argument must be a type");

                        if (!(argSize.Type is IntType))
                            ReportError(argSize, $"Argument must be an int but is '{argSize.Type}'");

                        return expr;
                    }

                case "bin_or":
                    return HandleComptimeBitwiseOperator(expr.Name.Name, expr, context, -1, values =>
                    {
                        BigInteger result = 0;
                        foreach (var v in values)
                            result |= v.IntValue;
                        return result;
                    });

                case "bin_xor":
                    return HandleComptimeBitwiseOperator(expr.Name.Name, expr, context, -1, values =>
                    {
                        BigInteger result = 0;
                        foreach (var v in values)
                            result ^= v.IntValue;
                        return result;
                    });

                case "bin_and":
                    return HandleComptimeBitwiseOperator(expr.Name.Name, expr, context, -1, values =>
                    {
                        BigInteger result = 0;
                        foreach (var v in values)
                            result &= v.IntValue;
                        return result;
                    });

                case "bin_lsl":
                    return HandleComptimeBitwiseOperator(expr.Name.Name, expr, context, 2, values =>
                    {
                        BigInteger result = values.First().IntValue;
                        int shift = (int)values.Skip(1).First().ToLong();
                        return result << shift;
                    });

                case "bin_lsr":
                    return HandleComptimeBitwiseOperator(expr.Name.Name, expr, context, 2, values =>
                    {
                        BigInteger result = values.First().IntValue;
                        int shift = (int)values.Skip(1).First().ToLong();
                        return result >> shift;
                    });

                default: ReportError(expr.Name, $"Unknown intrinsic '{expr.Name.Name}'"); break;
            }
            return expr;
        }

        private AstExpression HandleComptimeBitwiseOperator(string name, AstCompCallExpr expr, TypeInferenceContext context, int requiredArgs, Func<IEnumerable<NumberData>, NumberData> compute = null)
        {
            if (requiredArgs >= 0 && expr.Arguments.Count != requiredArgs)
            {
                ReportError(expr, $"@{name} requires {requiredArgs} arguments");
                return expr;
            }
            else if (requiredArgs < 0 && expr.Arguments.Count == 0)
            {
                ReportError(expr, $"@{name} requires at least one argument");
                return expr;
            }

            var ok = true;

            CheezType expectedArgType = null;
            for (int i = 0; i < expr.Arguments.Count; i++)
            {
                var arg = expr.Arguments[i];
                arg.AttachTo(expr);
                arg.Expr.AttachTo(arg);
                arg.Expr.SetFlag(ExprFlags.ValueRequired, true);
                expr.Arguments[i].Expr = InferType(arg.Expr, null);

                if (arg.Expr.Type != IntType.LiteralType && expectedArgType == null)
                    expectedArgType = arg.Expr.Type;
            }

            if (expectedArgType is ReferenceType r)
                expectedArgType = r.TargetType;

            for (int i = 0; i < expr.Arguments.Count; i++)
            {
                var arg = expr.Arguments[i];
                ConvertLiteralTypeToDefaultType(arg.Expr, expectedArgType);
                arg.Expr = Deref(arg.Expr, context);
                if (arg.Expr.Type.IsErrorType)
                {
                    ok = false;
                    continue;
                }

                if (!(arg.Expr.Type is IntType it))
                {
                    ReportError(arg, $"Argument must be of type int");
                    return expr;
                }
            }

            if (!ok)
                return expr;

            // check if all args have the same type
            foreach (var arg in expr.Arguments)
            {
                if (expectedArgType == null)
                    expectedArgType = arg.Expr.Type;
                if (arg.Expr.Type != expectedArgType)
                {
                    ReportError(arg.Location, $"Argument is of type '{arg.Expr.Type}' but must be of type '{expectedArgType}' (determined from first argument)");
                }
            }

            // calculate value if all args are comptime values
            if (expr.Arguments.All(arg => arg.Expr.IsCompTimeValue))
            {
                var values = from arg in expr.Arguments select (NumberData)arg.Expr.Value;
                var result = compute(values);
                return InferTypeHelper(new AstNumberExpr(result, Location: expr.Location), expectedArgType, context);
            }

            expr.Type = expr.Arguments[0].Expr.Type;
            return expr;
        }

        private AstExpression InferTypeBlock(AstBlockExpr expr, CheezType expected, TypeInferenceContext context)
        {
            if (expr.GetFlag(ExprFlags.Anonymous))
                expr.SubScope = expr.Scope;
            else
            {
                expr.SubScope = new Scope("{}", expr.Scope);
            }

            int end = expr.Statements.Count;

            if (expr.GetFlag(ExprFlags.ValueRequired) && expr.Statements.LastOrDefault() is AstExprStmt) --end;

            for (int i = 0; i < end; i++)
            {
                var stmt = expr.Statements[i];
                stmt.Scope = expr.SubScope;
                stmt.Parent = expr;
                expr.Statements[i] = stmt = AnalyseStatement(stmt);

                if (stmt.GetFlag(StmtFlags.Returns))
                    expr.SetFlag(ExprFlags.Returns, true);

                if (stmt.GetFlag(StmtFlags.Breaks))
                    expr.SetFlag(ExprFlags.Breaks, true);
            }

            if (end < expr.Statements.Count && expr.Statements.LastOrDefault() is AstExprStmt exprStmt)
            {
                exprStmt.Expr.SetFlag(ExprFlags.ValueRequired, true);

                exprStmt.Scope = expr.SubScope;
                exprStmt.Parent = expr;
                exprStmt.Expr.AttachTo(exprStmt);
                exprStmt.Expr = InferTypeHelper(exprStmt.Expr, expected, context);
                ConvertLiteralTypeToDefaultType(exprStmt.Expr, expected);
                expr.Type = exprStmt.Expr.Type;

                AnalyseExprStatement(exprStmt, true, false);

                expr.SetFlag(ExprFlags.IsLValue, exprStmt.Expr.GetFlag(ExprFlags.IsLValue));

                if (exprStmt.GetFlag(StmtFlags.Returns))
                    expr.SetFlag(ExprFlags.Returns, true);

                if (exprStmt.GetFlag(StmtFlags.Breaks))
                    expr.SetFlag(ExprFlags.Breaks, true);

            }
            else
            {
                expr.Type = CheezType.Void;
            }

            //if (!expr.GetFlag(ExprFlags.Anonymous) && !expr.GetFlag(ExprFlags.DontApplySymbolStatuses))
            //{
            //    // copy initialized symbols
            //    expr.SubScope.ApplyInitializedSymbolsToParent();
            //}

            return expr;
        }

        private AstExpression InferTypeIndexExpr(AstArrayAccessExpr expr, CheezType expected, TypeInferenceContext context)
        {
            expr.SubExpression.SetFlag(ExprFlags.SetAccess, expr.GetFlag(ExprFlags.SetAccess));

            expr.SubExpression.SetFlag(ExprFlags.ValueRequired, true);
            expr.SubExpression.Scope = expr.Scope;
            expr.SubExpression = InferTypeHelper(expr.SubExpression, null, context);

            expr.Indexer.SetFlag(ExprFlags.ValueRequired, true);
            expr.Indexer.Scope = expr.Scope;
            expr.Indexer = InferTypeHelper(expr.Indexer, null, context);

            ConvertLiteralTypeToDefaultType(expr.SubExpression, null);
            ConvertLiteralTypeToDefaultType(expr.Indexer, null);

            if (expr.SubExpression.Type is ErrorType || expr.Indexer.Type is ErrorType)
                return expr;

            if (expr.SubExpression.Type is ReferenceType)
            {
                expr.SubExpression = Deref(expr.SubExpression, context);
            }

            switch (expr.SubExpression.Type)
            {
                case TupleType tuple:
                    {
                        if (!(expr.Indexer.Type is IntType) || expr.Indexer.Value == null)
                        {
                            ReportError(expr.Indexer, $"The index must be a constant int");
                            return expr;
                        }

                        var index = ((NumberData)expr.Indexer.Value).ToLong();
                        if (index < 0 || index >= tuple.Members.Length)
                        {
                            ReportError(expr.Indexer, $"The index '{index}' is out of range. Index must be between [0, {tuple.Members.Length})");
                            return expr;
                        }

                        expr.Type = tuple.Members[index].type;
                        break;
                    }

                case PointerType ptr:
                    {
                        expr.Indexer = Deref(expr.Indexer, context);
                        if (expr.Indexer.Type is IntType)
                        {
                            expr.SetFlag(ExprFlags.IsLValue, true);
                            expr.Type = ptr.TargetType;
                        }
                        else
                        {
                            ReportError(expr.Indexer, $"The index into a pointer must be an int but is '{expr.Indexer.Type}'");
                        }
                        break;
                    }

                case SliceType slice:
                    {
                        expr.Indexer = Deref(expr.Indexer, context);
                        if (expr.Indexer.Type is IntType)
                        {
                            expr.SetFlag(ExprFlags.IsLValue, true);
                            expr.Type = slice.TargetType;
                        }
                        else if (expr.Indexer.Type is RangeType r && r.TargetType is IntType)
                        {
                            expr.SetFlag(ExprFlags.IsLValue, false);
                            expr.Type = slice;
                        }
                        else
                        {
                            ReportError(expr.Indexer, $"The index into a slice can't be '{expr.Indexer.Type}'");
                        }
                        break;
                    }

                case ArrayType arr:
                    {
                        expr.Indexer = Deref(expr.Indexer, context);
                        if (expr.Indexer.Type is IntType)
                        {
                            expr.SetFlag(ExprFlags.IsLValue, true);
                            expr.Type = arr.TargetType;

                            if (expr.Indexer.IsCompTimeValue)
                            {
                                var val = (NumberData)expr.Indexer.Value;
                                if (val < 0 || val >= arr.Length)
                                    ReportError(expr.Indexer, $"The index is out of range. Must be in [0, {arr.Length-1}]");
                            }
                        }
                        else
                        {
                            ReportError(expr.Indexer, $"The index into an array must be an int but is '{expr.Indexer.Type}'", ($"'{expr.SubExpression}' is of type '{arr}'", null));
                        }
                        break;
                    }

                default:
                    {
                        if (expr.GetFlag(ExprFlags.AssignmentTarget))
                        {
                            expr.TypeInferred = false;
                            expr.Type = CheezType.Void;
                            return expr;
                        }

                        var left = expr.SubExpression;
                        var right = expr.Indexer;

                        // resolve impls
                        GetImplsForType(left.Type);
                        GetImplsForType(right.Type);

                        var ops = expr.Scope.GetBinaryOperators("[]", left.Type, right.Type);

                        // :temp
                        // check if an operator is defined in an impl with *Self
                        if (ops.Count == 0)
                        {
                            ops = expr.Scope.GetBinaryOperators("[]", PointerType.GetPointerType(left.Type), right.Type);
                            left = new AstAddressOfExpr(left, left);
                        }

                        if (ops.Count == 1)
                        {
                            var opCall = new AstBinaryExpr("[]", left, right, expr);
                            opCall.Scope = expr.Scope;
                            return InferType(opCall, expected);
                        }
                        else if (ops.Count > 1)
                        {
                            ReportError(expr, $"Multiple operators '[]' match the types {left.Type} and {right.Type}");
                        }
                    }

                    ReportError(expr, $"Type {expr.SubExpression.Type} has no operator []");
                    break;
            }

            return expr;
        }

        private AstExpression InferTypeDotExpr(AstDotExpr expr, CheezType expected, TypeInferenceContext context)
        {
            expr.Left.SetFlag(ExprFlags.ValueRequired, true);
            expr.Left.Scope = expr.Scope;
            expr.Left = InferTypeHelper(expr.Left, null, context);
            ConvertLiteralTypeToDefaultType(expr.Left, null);

            if (expr.Left.Type.IsErrorType)
                return expr;

            if (!expr.IsDoubleColon)
            {
                while (expr.Left.Type is PointerType p)
                {
                    var newLeft = new AstDereferenceExpr(expr.Left, expr.Left.Location);
                    newLeft.AttachTo(expr.Left);
                    expr.Left = InferType(newLeft, p.TargetType);
                }

                if (expr.Left.Type is ReferenceType r)
                {
                    expr.Left = Deref(expr.Left, context);
                }
            }

            var sub = expr.Right.Name;
            switch (expr.Left.Type)
            {
                case RangeType range when !expr.IsDoubleColon:
                    {
                        var name = expr.Right.Name;

                        if (name == "start" || name == "end")
                        {
                            expr.SetFlag(ExprFlags.IsLValue, expr.Left.GetFlag(ExprFlags.IsLValue));

                            expr.Type = range.TargetType;
                            return expr;
                        }
                        else
                        {
                            ReportError(expr, $"type {range} has no field '{name}'");
                            return expr;
                        }
                    }

                case EnumType @enum when !expr.IsDoubleColon:
                    {
                        var memName = expr.Right.Name;
                        var mem = @enum.Declaration.Members.FirstOrDefault(m => m.Name.Name == memName);

                        if (mem == null)
                        {
                            ReportError(expr, $"Type '{@enum}' has no member '{memName}'", ("Maybe use :: instead of . if you want to call a function", null));
                            return expr;
                        }

                        if (mem.AssociatedTypeExpr == null)
                        {
                            ReportError(expr, $"Enum member '{memName}' of enum {@enum} has no associated value");
                            return expr;
                        }

                        expr.Type = mem.AssociatedTypeExpr.Value as CheezType;
                        expr.SetFlag(ExprFlags.IsLValue, true);
                        break;
                    }

                case TupleType tuple when !expr.IsDoubleColon:
                    {
                        var memName = expr.Right.Name;
                        var memberIndex = tuple.Members.IndexOf(m => m.name == memName);
                        if (memberIndex == -1)
                        {
                            ReportError(expr, $"The tuple '{tuple}' has no member '{memName}'");
                            return expr;
                        }

                        expr.Type = tuple.Members[memberIndex].type;
                        expr.SetFlag(ExprFlags.IsLValue, expr.Left.GetFlag(ExprFlags.IsLValue));
                        break;
                    }

                case SliceType slice when !expr.IsDoubleColon:
                    {
                        expr.SetFlag(ExprFlags.IsLValue, true);
                        var name = expr.Right.Name;
                        if (name == "data")
                        {
                            expr.Type = slice.ToPointerType();
                        }
                        else if (name == "length")
                        {
                            expr.Type = IntType.GetIntType(8, true);
                        }
                        else
                        {
                            // TODO: check for impl functions
                            ReportError(expr, $"No subscript '{name}' exists for slice type {slice}");
                        }
                        break;
                    }

                case ArrayType arr when !expr.IsDoubleColon:
                    {
                        var name = expr.Right.Name;
                        if (name == "data")
                        {
                            expr.Type = arr.ToPointerType();
                            expr.SetFlag(ExprFlags.IsLValue, true);
                        }
                        else if (name == "length")
                        {
                            expr.Type = IntType.GetIntType(8, true);
                        }
                        else
                        {
                            ReportError(expr, $"No subscript '{name}' exists for array type {arr}");
                        }
                        break;
                    }

                case StructType s when !expr.IsDoubleColon:
                    {
                        var name = expr.Right.Name;
                        var index = s.GetIndexOfMember(name);
                        if (index == -1)
                        {
                            return GetImplFunctions(expr, s, name, context);
                        }

                        var member = s.Declaration.Members[index];

                        // check wether we have private access to this struct
                        if (currentFunction.ImplBlock?.TargetType is StructType sx && sx.DeclarationTemplate == s.DeclarationTemplate)
                        {
                            // we are in an impl block for the struct type
                            // -> we always have access to all members
                        }
                        else
                        {
                            // we only have access to public fields
                            if (expr.GetFlag(ExprFlags.SetAccess))
                            {
                                // set
                                if (!member.IsPublic)
                                    ReportError(expr, $"The member '{member.Name}' of struct '{s}' is private and can't be accessed from here.",
                                        ("Member declared here:", member.Location));
                                else if (member.IsReadOnly)
                                    ReportError(expr, $"The member '{member.Name}' of struct '{s}' can only be read from here.",
                                        ("Member declared here:", member.Location));
                            }
                            else
                            {
                                // get
                                if (!member.IsPublic)
                                    ReportError(expr, $"The member '{member.Name}' of struct '{s}' is private and can't be accessed from here.",
                                        ("Member declared here:", member.Location));
                            }
                        }

                        expr.Type = member.Type;

                        expr.SetFlag(ExprFlags.IsLValue, expr.Left.GetFlag(ExprFlags.IsLValue));
                        break;
                    }

                case TraitType t when !expr.IsDoubleColon:
                    {
                        var name = expr.Right.Name;
                        var func = t.Declaration.Functions.FirstOrDefault(f => f.Name.Name == name);

                        if (func == null)
                        {
                            var mem = t.Declaration.Variables.FirstOrDefault(v => v.Name.Name == name);

                            if (mem == null)
                            {
                                ReportError(expr.Right, $"Trait '{t.Declaration.Name}' has no function or member '{name}'");
                                break;
                            }

                            expr.Type = mem.Type;
                            return expr;
                        }

                        var ufc = new AstUfcFuncExpr(expr.Left, func);
                        ufc.Replace(expr);
                        ufc.SetFlag(ExprFlags.ValueRequired, expr.GetFlag(ExprFlags.ValueRequired));
                        return InferTypeHelper(ufc, null, context);
                    }

                case CheezTypeType _ when !expr.IsDoubleColon && expr.Left.Value is EnumType @enum:
                    {
                        if (@enum.Members.TryGetValue(expr.Right.Name, out var m))
                        {
                            expr.Type = @enum;

                            var mem = @enum.Declaration.Members.First(x => x.Name.Name == expr.Right.Name);
                            var eve = new AstEnumValueExpr(@enum.Declaration, mem, loc: expr.Location);
                            eve.Replace(expr);
                            return eve;
                        }
                        else
                        {
                            ReportError(expr, $"Enum {@enum} has no member '{expr.Right}'");
                        }
                        break;
                    }

                case CheezTypeType _ when expr.IsDoubleColon:
                    {
                        var t = expr.Left.Value as CheezType;
                        var funcs = GetImplFunctions(t, expr.Right.Name, expected);

                        if (funcs.Count == 0)
                        {
                            ReportError(expr.Right, $"Type '{t}' has no function '{expr.Right.Name}'");
                            break;
                        }
                        else if (funcs.Count > 1)
                        {
                            var details = funcs.Select(f => ("Possible candidate:", f.Name.Location));
                            ReportError(expr.Right, $"Ambigious call to function '{expr.Right.Name}'", details);
                            break;
                        }

                        expr.Type = funcs[0].Type;
                        break;
                    }

                case CheezTypeType _:
                    ReportError(expr.Left, $"Invalid value on left side of '.': '{expr.Left.Value}'");
                    break;

                case ErrorType _: return expr;

                case CheezType c when expr.IsDoubleColon:
                    {
                        var name = expr.Right.Name;
                        return GetImplFunctions(expr, c, name, context);
                    }
                default: ReportError(expr, $"Invalid expression on left side of '.'"); break;
            }

            return expr;
        }

        private AstExpression InferTypeTupleExpr(AstTupleExpr expr, CheezType expected, TypeInferenceContext context)
        {
            TupleType tupleType = expected as TupleType;
            if (tupleType?.Members?.Length != expr.Values.Count) tupleType = null;

            int typeMembers = 0;

            var members = new (string name, CheezType type)[expr.Values.Count];
            for (int i = 0; i < expr.Values.Count; i++)
            {
                var v = expr.Values[i];
                v.SetFlag(ExprFlags.ValueRequired, true);
                v.AttachTo(expr);

                var e = tupleType?.Members[i].type;
                v = expr.Values[i] = InferTypeHelper(v, e, context);
                ConvertLiteralTypeToDefaultType(v, e);

                members[i].type = v.Type;

                if (v.Type == CheezType.Type)
                {
                    typeMembers++;
                    members[i].type = v.Value as CheezType;
                    members[i].name = expr.Types[i].Name?.Name;
                }
            }

            if (typeMembers == expr.Values.Count)
            {
                expr.Type = CheezType.Type;
                expr.Value = TupleType.GetTuple(members);
            }
            else if (typeMembers != 0)
            {
                ReportError(expr, $"A tuple can't have types as members");
            }
            else
            {
                expr.Type = TupleType.GetTuple(members);
            }

            return expr;
        }

        private AstExpression ExpandMacro(AstCallExpr call, TypeInferenceContext context)
        {
            var macro = call.Declaration;
            var code = macro.Body.Clone() as AstBlockExpr;
            code.Parent = call.Parent;
            code.Scope = new Scope("macro {}", macro.ConstScope);
            code.Scope.LinkedScope = call.Scope;
            code.SetFlag(ExprFlags.FromMacroExpansion, true);

            // define arguments
            var links = call.Arguments.Select((arg, index) =>
            {
                var param = macro.Parameters[index];
                var link = new AstCompCallExpr(
                    new AstIdExpr("link", false, arg.Location),
                    new List<AstArgument> { arg }, arg.Location);
                bool isConst = arg.Type.IsComptimeOnly || arg.Expr.Value != null;
                var varDecl = new AstVariableDecl(param.Name, null, link, isConst, Location: arg.Location);
                return varDecl;
            });
            code.Statements.InsertRange(0, links);

            var errHandler = new SilentErrorHandler();
            PushErrorHandler(errHandler);
            code.SetFlag(ExprFlags.ValueRequired, true);
            var newExpr = InferTypeHelper(code, null, context);
            PopErrorHandler();

            if (errHandler.HasErrors)
            {
                ReportError(call.Location, "Failed to expand macro", errHandler.Errors, ("Macro defined here:", macro.Name.Location));
            }

            return newExpr;
        }

        private AstExpression InferTypeCallExpr(AstCallExpr expr, CheezType expected, TypeInferenceContext context)
        {
            expr.FunctionExpr.AttachTo(expr);

            {
                var prev = context.functionExpectedReturnType;
                context.functionExpectedReturnType = expected;
                expr.FunctionExpr.SetFlag(ExprFlags.ValueRequired, true);
                expr.FunctionExpr = InferTypeHelper(expr.FunctionExpr, null, context);
                context.functionExpectedReturnType = prev;
            }

            switch (expr.FunctionExpr.Type)
            {
                case FunctionType f:
                    {
                        var newExpr = InferRegularFunctionCall(f, expr, expected, context);

                        // check if it is a macro call
                        if (!newExpr.Type.IsErrorType && newExpr is AstCallExpr call && call.Declaration != null && call.Declaration.GetFlag(StmtFlags.IsMacroFunction))
                        {
                            return ExpandMacro(call, context);
                        }

                        return newExpr;
                    }

                case GenericFunctionType g:
                    {
                        var newExpr = InferGenericFunctionCall(g, expr, expected, context);

                        // check if it is a macro call
                        if (!newExpr.Type.IsErrorType && newExpr is AstCallExpr call
                            && call.Declaration.Template.GetFlag(StmtFlags.IsMacroFunction))
                        {
                            return ExpandMacro(call, context);
                        }

                        return newExpr;
                    }

                case GenericEnumType @enum:
                    {
                        var e = expr.FunctionExpr as AstEnumValueExpr;
                        Debug.Assert(e != null);

                        var assType = e.Member.AssociatedType;

                        if (expr.Arguments.Count == 1)
                        {
                            e.Argument = expr.Arguments[0].Expr;

                        }
                        else if (expr.Arguments.Count > 1)
                        {
                            var p = expr.Arguments.Select(a => new AstParameter(null, a.Expr, null, a.Location)).ToList();
                            e.Argument = new AstTupleExpr(p, new Location(expr.Arguments));
                        }

                        e.TypeInferred = false;
                        return InferTypeHelper(e, expected, context);
                    }

                case EnumType @enum:
                    {
                        var e = expr.FunctionExpr as AstEnumValueExpr;
                        Debug.Assert(e != null);

                        var assType = e.Member.AssociatedType;

                        if (expr.Arguments.Count == 1)
                        {
                            e.Argument = expr.Arguments[0].Expr;

                        }
                        else if (expr.Arguments.Count > 1)
                        {
                            var p = expr.Arguments.Select(a => new AstParameter(null, a.Expr, null, a.Location)).ToList();
                            e.Argument = new AstTupleExpr(p, new Location(expr.Arguments));
                        }

                        if (e.Argument != null)
                        {
                            e.Argument.AttachTo(e);
                            e.Argument = InferTypeHelper(e.Argument, assType, context);
                            ConvertLiteralTypeToDefaultType(e.Argument, assType);
                            e.Argument = HandleReference(e.Argument, assType, context);
                            e.Argument = CheckType(e.Argument, assType);
                            
                        }

                        return e;
                    }

                case CheezTypeType type when expr.FunctionExpr.Value is GenericStructType ||
                                        expr.FunctionExpr.Value is GenericTraitType ||
                                        expr.FunctionExpr.Value is GenericEnumType:
                    {
                        return InferTypeGenericTypeCallExpr(expr, context);
                    }

                    // this is a cast
                case CheezTypeType _:
                    {
                        if (expr.Arguments.Count != 1)
                        {
                            ReportError(expr.Location, "Cast requires exactly one argument!");
                            return expr;
                        }

                        var targetType = (CheezType)expr.FunctionExpr.Value;
                        var cast = new AstCastExpr(new AstTypeRef(targetType, expr.FunctionExpr.Location),
                            expr.Arguments[0].Expr,
                            expr.Location);
                        cast.Replace(expr);
                        return InferTypeHelper(cast, expected, context);
                    }

                case ErrorType _: return expr;

                default: ReportError(expr.FunctionExpr, $"Type '{expr.FunctionExpr.Type}' is not callable"); break;
            }

            return expr;
        }

        private AstExpression InferTypeGenericTypeCallExpr(AstCallExpr expr, TypeInferenceContext context)
        {
            bool anyArgIsPoly = false;

            foreach (var arg in expr.Arguments)
            {
                arg.AttachTo(expr);
                arg.Expr.AttachTo(arg);
                arg.Expr.SetFlag(ExprFlags.ValueRequired, true);
                arg.Expr = InferTypeHelper(arg.Expr, CheezType.Type, context);
                arg.Type = arg.Expr.Type;
                arg.Value = arg.Expr.Value;

                if (arg.Type == CheezType.Type)
                {
                    var argType = arg.Value as CheezType;
                    if (argType.IsPolyType) anyArgIsPoly = true;
                }
                else
                {
                    ReportError(arg, $"Non type arguments in poly struct type not implemented yet.");
                    return expr;
                }
            }

            if (expr.FunctionExpr.Value is GenericStructType strType)
            {
                if (anyArgIsPoly)
                {
                    expr.Type = CheezType.Type;
                    expr.Value = new StructType(strType.Declaration, expr.Arguments.Select(a => a.Value as CheezType).ToArray());
                    return expr;
                }

                // instantiate struct
                var args = expr.Arguments.Select(a => (a.Type, a.Value)).ToList();
                var instance = InstantiatePolyStruct(strType.Declaration, args, context.newPolyDeclarations, expr);
                expr.Type = CheezType.Type;
                expr.Value = instance?.Type ?? CheezType.Error;

                // this causes impls to be calculated for this type if not done yet
                //if (instance.Type != null)
                //    GetImplsForType(instance.Type);

                return expr;
            }
            else if (expr.FunctionExpr.Value is GenericEnumType @enum)
            {
                if (anyArgIsPoly)
                {
                    expr.Type = CheezType.Type;
                    expr.Value = new EnumType(@enum.Declaration, expr.Arguments.Select(a => a.Value as CheezType).ToArray());
                    return expr;
                }

                // instantiate enum
                var args = expr.Arguments.Select(a => (a.Type, a.Value)).ToList();
                var instance = InstantiatePolyEnum(@enum.Declaration, args, context.newPolyDeclarations, expr);
                expr.Type = CheezType.Type;
                expr.Value = instance?.Type ?? CheezType.Error;

                // this causes impls to be calculated for this type if not done yet
                //if (instance.Type != null)
                //    GetImplsForType(instance.Type);

                return expr;
            }
            else if (expr.FunctionExpr.Value is GenericTraitType trait)
            {
                if (anyArgIsPoly)
                {
                    expr.Type = CheezType.Type;
                    expr.Value = new TraitType(trait.Declaration, expr.Arguments.Select(a => a.Value as CheezType).ToArray());
                    return expr;
                }

                // instantiate trait
                var args = expr.Arguments.Select(a => (a.Type, a.Value)).ToList();
                var instance = InstantiatePolyTrait(trait.Declaration, args, context.newPolyDeclarations, expr);
                expr.Type = CheezType.Type;
                expr.Value = instance?.Type ?? CheezType.Error;

                // this causes impls to be calculated for this type if not done yet
                //if (instance.Type != null)
                //    GetImplsForType(instance.Type);

                return expr;
            }
            else
            {
                ReportError(expr.FunctionExpr, $"This type must be a polymorphic type but is '{expr.FunctionExpr.Value}'");
                return expr;
            }
        }

        private bool CheckAndMatchArgsToParams(
            List<AstArgument> arguments,
            (string Name, CheezType Type, AstExpression DefaultValue)[] parameters,
            bool varArgs)
        {
            // check for too many arguments
            if (arguments.Count > parameters.Length && !varArgs)
                return false;

            // match arguments to parameters
            var map = new Dictionary<int, AstArgument>();
            bool allowUnnamed = true;
            bool ok = true;
            for (int i = 0; i < arguments.Count; i++)
            {
                var arg = arguments[i];
                if (arg.Name == null)
                {
                    if (!allowUnnamed)
                    {
                        ok = false;
                        ReportError(arg, $"Unnamed arguments are not allowed after named arguments");
                        break;
                    }

                    map[i] = arg;
                    arg.Index = i;
                }
                else
                {
                    var index = parameters.IndexOf(p => p.Name == arg.Name.Name);
                    if (map.TryGetValue(index, out var other))
                    {

                        ReportError(arg, $"This argument maps to the same parameter ({i}) as '{other}'");
                        ok = false;
                        break;
                    }
                    // TODO: check if index != -1

                    map[index] = arg;
                    arg.Index = index;
                }
            }

            if (!ok)
                return false;

            // create missing arguments
            for (int i = 0; i < parameters.Length; i++)
            {
                if (map.ContainsKey(i))
                    continue;
                var p = parameters[i];
                if (p.DefaultValue == null)
                {
                    return false;
                }
                var arg = new AstArgument(p.DefaultValue.Clone(), Location: p.DefaultValue.Location);
                arg.IsDefaultArg = true;
                arg.Index = i;
                arguments.Add(arg);
            }

            arguments.Sort((a, b) => a.Index - b.Index);

            if (arguments.Count < parameters.Length)
                return false;

            return true;
        }

        private bool CheckAndMatchArgsToParams(
            AstCallExpr expr, 
            (string Name, CheezType Type, AstExpression DefaultValue)[] parameters, 
            bool varArgs)
        {
            // create self argument for ufc
            if (expr.FunctionExpr is AstUfcFuncExpr ufc)
            {
                AstArgument selfArg = new AstArgument(ufc.SelfArg, Location: expr.FunctionExpr);
                expr.Arguments.Insert(0, selfArg);
                expr.UnifiedFunctionCall = true;
            }

            // check for too many arguments
            if (expr.Arguments.Count > parameters.Length && !varArgs)
            {
                (string, ILocation)? detail = null;
                if (expr.FunctionExpr is AstIdExpr id)
                {
                    ILocation loc = id.Symbol.Location;
                    if (id.Symbol is AstFunctionDecl fd)
                        loc = new Location(fd.Name.Beginning, fd.ParameterLocation.End);
                    detail = ("Function defined here:", loc);
                }
                ReportError(expr, $"Too many arguments. Expected {parameters.Length}, got {expr.Arguments.Count}", detail);
                return false;
            }

            // match arguments to parameters
            var map = new Dictionary<int, AstArgument>();
            bool allowUnnamed = true;
            bool ok = true;
            for (int i = 0; i < expr.Arguments.Count; i++)
            {
                var arg = expr.Arguments[i];
                if (arg.Name == null)
                {
                    if (!allowUnnamed)
                    {
                        ok = false;
                        ReportError(arg, $"Unnamed arguments are not allowed after named arguments");
                        break;
                    }

                    map[i] = arg;
                    arg.Index = i;
                }
                else
                {
                    var index = parameters.IndexOf(p => p.Name == arg.Name.Name);
                    if (map.TryGetValue(index, out var other))
                    {

                        ReportError(arg, $"This argument maps to the same parameter ({i}) as '{other}'");
                        ok = false;
                        break;
                    }
                    // TODO: check if index != -1

                    map[index] = arg;
                    arg.Index = index;
                }
            }

            if (!ok)
                return false;

            // create missing arguments
            for (int i = 0; i < parameters.Length; i++)
            {
                if (map.ContainsKey(i))
                    continue;
                var p = parameters[i];
                if (p.DefaultValue == null)
                {
                    ReportError(expr, $"Call misses parameter ({i}) '{p.Name}' of type '{p.Type}'.");
                    ok = false;
                    continue;
                }
                var arg = new AstArgument(p.DefaultValue.Clone(), Location: p.DefaultValue.Location);
                arg.IsDefaultArg = true;
                arg.Index = i;
                expr.Arguments.Add(arg);
            }

            expr.Arguments.Sort((a, b) => a.Index - b.Index);

            if (expr.Arguments.Count < parameters.Length)
                return false;

            return true;
        }

        private AstExpression InferGenericFunctionCall(GenericFunctionType func, AstCallExpr expr, CheezType expected, TypeInferenceContext context)
        {
            var decl = func.Declaration;

            var par = func.Declaration.Parameters.Select(p => (p.Name?.Name, p.Type, p.DefaultValue));
            if (!CheckAndMatchArgsToParams(expr, par.ToArray(), false))
                return expr;

            // match arguments and parameter types
            var pairs = expr.Arguments.Select(arg => (arg.Index < decl.Parameters.Count ? decl.Parameters[arg.Index] : null, arg));
            (AstParameter param, AstArgument arg)[] args = pairs.ToArray();

            // infer types of arguments
            foreach (var (param, arg) in args)
            {
                arg.Expr.SetFlag(ExprFlags.ValueRequired, true);
                arg.AttachTo(expr);
                arg.Expr.AttachTo(arg, expr.Scope);

                var ex = param.Type;
                if (ex.IsPolyType)
                    ex = null;
                
                arg.IsConstArg = param.Name?.IsPolymorphic ?? false;
                if (arg.IsDefaultArg && !arg.IsConstArg)
                    continue;
                arg.Expr = InferTypeHelper(arg.Expr, ex, context);
                ConvertLiteralTypeToDefaultType(arg.Expr, ex);
                arg.Type = arg.Expr.Type;
            }

            // collect polymorphic types and const arguments
            var polyTypes = new Dictionary<string, CheezType>();
            var constArgs = new Dictionary<string, (CheezType type, object value)>();
            var newArgs = new List<AstArgument>();

            if (func.Declaration.ImplBlock != null)
            {
                if (expr.FunctionExpr is AstDotExpr dot)
                {
                    if (dot.IsDoubleColon && dot.Left.Type is CheezType)
                    {
                        var type = dot.Left.Value as CheezType;
                        CollectPolyTypes(func.Declaration.ImplBlock.TargetType, type, polyTypes);
                    }
                }
                else
                {
                    if (expr.UnifiedFunctionCall)
                    {
                        var selfType = expr.Arguments[0].Type;

                        switch (func.Declaration.SelfType)
                        {
                            case SelfParamType.Reference:
                                if (selfType is ReferenceType r)
                                    CollectPolyTypes(func.Declaration.ImplBlock.TargetType, r, polyTypes);
                                else
                                    CollectPolyTypes(func.Declaration.ImplBlock.TargetType, selfType, polyTypes);
                                break;

                            case SelfParamType.Value:
                                CollectPolyTypes(func.Declaration.ImplBlock.TargetType, selfType, polyTypes);
                                break;

                            default: throw new NotImplementedException();
                        }
                    }
                    else
                    {
                        foreach (var a in expr.Arguments)
                        {
                            CollectPolyTypes(func.Declaration.ImplBlock.TargetType, a.Type, polyTypes);
                        }
                    }
                }
            }

            foreach (var (param, arg) in args)
            {
                if (!arg.IsDefaultArg)
                    CollectPolyTypes(param.Type, arg.Type, polyTypes);

                if (param.Name?.IsPolymorphic ?? false)
                {
                    if (arg.Expr.Value == null)
                    {
                        ReportError(arg, $"The expression must be a compile time constant");
                        return expr; // :hack
                    }
                    else
                    {
                        constArgs[param.Name.Name] = (arg.Expr.Type, arg.Expr.Value);
                    }
                }
                else
                {
                    newArgs.Add(arg);
                }
            }

            expr.Arguments = newArgs;

            // TODO: check if all poly types have been found
            if (expr.Arguments.Any(a => a.Type?.IsErrorType ?? false))
                return expr;
            
            // find or create instance
            var instance = InstantiatePolyFunction(func, polyTypes, constArgs, context.newPolyFunctions, expr);


            // check parameter types
            Debug.Assert(expr.Arguments.Count == instance.Parameters.Count);
            
            if (instance.Type.IsPolyType)
            {
                // error in function declaration
                expr.Type = CheezType.Error;
                return expr;
            }

            for (int i = 0; i < expr.Arguments.Count; i++)
            {
                var a = expr.Arguments[i];
                var p = instance.Parameters[i];


                if (a.IsDefaultArg && !a.IsConstArg)
                {
                    var ex = p.Type;
                    a.Expr = InferTypeHelper(a.Expr, ex, context);
                    ConvertLiteralTypeToDefaultType(a.Expr, ex);
                    a.Type = a.Expr.Type;
                }


                if (a.Type.IsErrorType)
                    continue;

                a.Expr = HandleReference(a.Expr, p.Type, context);
                a.Type = a.Expr.Type;

                a.Expr = CheckType(a.Expr, p.Type, $"Type of argument ({a.Type}) does not match type of parameter ({p.Type})");
            }

            expr.Declaration = instance;
            expr.FunctionType = instance.FunctionType;
            expr.Type = instance.FunctionType.ReturnType;
            expr.SetFlag(ExprFlags.IsLValue, instance.FunctionType.ReturnType is PointerType);

            return expr;
        }

        private AstExpression InferRegularFunctionCall(FunctionType func, AstCallExpr expr, CheezType expected, TypeInferenceContext context)
        {
            // check if call is from trait to non ref self param function
            if (func.Declaration?.Trait != null)
            {
                if (func.Declaration.SelfType == SelfParamType.Value)
                    ReportError(expr, $"Can't call trait function with non ref Self param");
                if (func.Declaration.SelfType == SelfParamType.None)
                    ReportError(expr, $"Can't call trait function with non ref Self param");

                if (func.Declaration.GetFlag(StmtFlags.ExcludeFromVtable))
                    ReportError(expr, $"Can't call trait function because it is excluded from the vtable");
            }

            //var par = func.Declaration.Parameters.Select(p => (p.Name?.Name, p.Type, p.DefaultValue)).ToArray();
            var par = func.Parameters;
            if (!CheckAndMatchArgsToParams(expr, par, func.VarArgs))
                return expr;

            // match arguments and parameter types
            var pairs = expr.Arguments.Select(arg => (arg.Index < func.Parameters.Length ? func.Parameters[arg.Index].type : null, arg));
            (CheezType type, AstArgument arg)[] args = pairs.ToArray();
            foreach (var (type, arg) in args)
            {
                arg.Expr.SetFlag(ExprFlags.ValueRequired, true);
                arg.AttachTo(expr);
                arg.Expr.AttachTo(arg, expr.Scope);
                arg.Expr = InferTypeHelper(arg.Expr, type, context);
                ConvertLiteralTypeToDefaultType(arg.Expr, type);
                arg.Type = arg.Expr.Type;

                if (arg.Type.IsErrorType)
                    continue;

                if (func.VarArgs && arg.Index >= func.Parameters.Length)
                {
                    if (arg.Type is ReferenceType r)
                    {
                        arg.Expr = Deref(arg.Expr, context);
                    }
                }
                else
                {
                    arg.Expr = HandleReference(arg.Expr, type, context);
                    arg.Expr = CheckType(arg.Expr, type, $"Type of argument ({arg.Expr.Type}) does not match type of parameter ({type})");
                    arg.Type = arg.Expr.Type;
                }
            }

            // :hack
            expr.SetFlag(ExprFlags.IsLValue, func.ReturnType is ReferenceType);
            expr.Type = func.ReturnType;
            expr.Declaration = func.Declaration;
            expr.FunctionType = func;

            return expr;
        }


        private AstExpression InferTypeStructValueExpr(AstStructValueExpr expr, CheezType expected, TypeInferenceContext context)
        {
            if (expr.TypeExpr != null)
            {
                expr.TypeExpr.AttachTo(expr);
                expr.TypeExpr = ResolveTypeNow(expr.TypeExpr, out var t);
                expr.Type = t;
            }
            else
            {
                expr.Type = expected;
            }

            if (expr.Type == null)
            {
                ReportError(expr, $"Failed to infer type for struct expression");
                expr.Type = CheezType.Error;
                return expr;
            }
            else if (expr.Type == CheezType.Error)
            {
                return expr;
            }

            var type = expr.Type as StructType;
            if (type == null)
            {
                ReportError(expr, $"Failed to infer type for struct value expression");
                expr.Type = CheezType.Error;
                return expr;
            }

            if (type.Size == -1)
            {
                ReportError(expr, 
                    $"Can't create an instance of this struct because the member types have not yet been computed. This may be a bug in the compiler. This error can happen if you use a struct literal in a constant context which is not allowed.");
                expr.Type = CheezType.Error;
                return expr;
            }

            // 
            int namesProvided = 0;
            foreach (var m in expr.MemberInitializers)
            {
                if (m.Name != null)
                {
                    if (!type.Declaration.Members.Any(m2 => m2.Name.Name == m.Name.Name))
                    {
                        ReportError(m.Name, $"'{m.Name}' is not a member of struct {type.Declaration.Name}");
                    }
                    namesProvided++;
                }
            }

            var inits = new HashSet<string>();
            if (namesProvided == 0)
            {
                for (int i = 0; i < expr.MemberInitializers.Count && i < type.Declaration.Members.Count; i++)
                {
                    var mi = expr.MemberInitializers[i];
                    var mem = type.Declaration.Members[i];
                    inits.Add(mem.Name.Name);

                    mi.Value.AttachTo(expr);
                    mi.Value = InferTypeHelper(mi.Value, mem.Type, context);
                    ConvertLiteralTypeToDefaultType(mi.Value, mem.Type);

                    mi.Name = new AstIdExpr(mem.Name.Name, false, mi.Value);
                    mi.Index = i;

                    if (mi.Value.Type.IsErrorType) continue;
                    mi.Value = HandleReference(mi.Value, mem.Type, context);
                    mi.Value = CheckType(mi.Value, mem.Type);
                }
            }
            else if (namesProvided == expr.MemberInitializers.Count)
            {
                for (int i = 0; i < expr.MemberInitializers.Count && i < type.Declaration.Members.Count; i++)
                {
                    var mi = expr.MemberInitializers[i];
                    var memIndex = type.Declaration.Members.FindIndex(m => m.Name.Name == mi.Name.Name);
                    if (memIndex < 0 || memIndex >= type.Declaration.Members.Count)
                    {
                        mi.Value.Type = CheezType.Error;
                        continue;
                    }


                    var mem = type.Declaration.Members[memIndex];
                    inits.Add(mem.Name.Name);

                    mi.Index = memIndex;

                    mi.Value.AttachTo(expr);
                    mi.Value = InferTypeHelper(mi.Value, mem.Type, context);
                    ConvertLiteralTypeToDefaultType(mi.Value, mem.Type);

                    if (mi.Value.Type.IsErrorType) continue;
                    mi.Value = HandleReference(mi.Value, mem.Type, context);
                    mi.Value = CheckType(mi.Value, mem.Type);
                }
            }
            else
            {
                ReportError(expr, $"Either all or no values must have a name");
            }

            // create missing values
            foreach (var mem in type.Declaration.Members)
            {
                if (!inits.Contains(mem.Name.Name))
                {
                    if (mem.Initializer == null)
                    {
                        ReportError(expr, $"You must provide an initial value for member '{mem.Name.Name}' because it can't be default initialized");
                        continue;
                    }

                    var mi = new AstStructMemberInitialization(new AstIdExpr(mem.Name.Name, false, expr.Location), mem.Initializer, expr.Location);
                    mi.Index = mem.Index;
                    expr.MemberInitializers.Add(mi);
                }
            }

            return expr;
        }

        private AstExpression InferTypeUnaryExpr(AstUnaryExpr expr, CheezType expected, TypeInferenceContext context)
        {
            expr.SubExpr.SetFlag(ExprFlags.ValueRequired, true);
            expr.SubExpr.Scope = expr.Scope;
            expr.SubExpr = InferTypeHelper(expr.SubExpr, null, context);

            if (expr.SubExpr.Type.IsErrorType)
                return expr;
            
            if (expr.SubExpr.Type is AbstractType at1)
            {
                expr.Type = expr.SubExpr.Type;
            }
            else
            {
                var ops = expr.Scope.GetUnaryOperators(expr.Operator, expr.SubExpr.Type);

                if (ops.Count == 0)
                {
                    ReportError(expr, $"No operator '{expr.Operator}' matches the type {expr.SubExpr.Type}");
                    return expr;
                }
                else if (ops.Count > 1)
                {
                    ReportError(expr, $"Multiple operators '{expr.Operator}' match the type {expr.SubExpr.Type}");
                    return expr;
                }

                var op = ops[0];

                expr.SubExpr = HandleReference(expr.SubExpr, op.SubExprType, context);
                if (!op.SubExprType.IsPolyType)
                    expr.SubExpr = CheckType(expr.SubExpr, op.SubExprType);

                if (op is UserDefinedUnaryOperator user)
                {
                    var args = new List<AstArgument>() {
                        new AstArgument(expr.SubExpr, Location: expr.SubExpr.Location),
                    };
                    var func = new AstSymbolExpr(user.Declaration);
                    var call = new AstCallExpr(func, args, expr.Location);
                    call.Replace(expr);
                    return InferType(call, expected);
                }

                expr.ActualOperator = op;

                if (expr.SubExpr.IsCompTimeValue)
                {
                    expr.Value = op.Execute(expr.SubExpr.Value);
                }

                // @hack
                expr.Type = op.ResultType;
            }

            return expr;
        }

        private AstExpression InferTypesBinaryExpr(AstBinaryExpr expr, CheezType expected, TypeInferenceContext context)
        {
            expr.Left.SetFlag(ExprFlags.ValueRequired, true);
            expr.Right.SetFlag(ExprFlags.ValueRequired, true);

            expr.Left.Scope = expr.Scope;
            expr.Right.Scope = expr.Scope;

            expr.Left = InferTypeHelper(expr.Left, null, context);
            expr.Right = InferTypeHelper(expr.Right, null, context);

            if (expr.Left.Type.IsErrorType || expr.Right.Type.IsErrorType)
                return expr;

            var at = new List<AbstractType>();
            if (expr.Left.Type is AbstractType at1) at.Add(at1);
            if (expr.Right.Type is AbstractType at2) at.Add(at2);
            if (at.Count > 0)
            {
                expr.Type = new CombiType(at);
            }
            else
            {
                // convert literal types to concrete types
                if (IsLiteralType(expr.Left.Type) && IsLiteralType(expr.Right.Type))
                {

                }
                else if (IsLiteralType(expr.Left.Type))
                {
                    expr.Left.Type = UnifyTypes(expr.Right.Type, expr.Left.Type);
                }
                else if (IsLiteralType(expr.Right.Type))
                {
                    expr.Right.Type = UnifyTypes(expr.Left.Type, expr.Right.Type);
                }

                // before we search for operators, make sure that all impls for both arguments have been matched

                if (expr.Left.Type is StructType || expr.Left.Type is EnumType)
                    GetImplsForType(expr.Left.Type);
                if (expr.Right.Type is StructType || expr.Right.Type is EnumType)
                    GetImplsForType(expr.Right.Type);


                var ops = expr.Scope.GetBinaryOperators(expr.Operator, expr.Left.Type, expr.Right.Type);

                if (ops.Count == 0)
                {
                    ReportError(expr, $"No operator '{expr.Operator}' matches the types {expr.Left.Type} and {expr.Right.Type}");
                    return expr;
                }
                else if (ops.Count > 1)
                {
                    // TODO: show matching operators
                    ReportError(expr, $"Multiple operators '{expr.Operator}' match the types {expr.Left.Type} and {expr.Right.Type}");
                    return expr;
                }

                var op = ops[0];

                expr.Left = HandleReference(expr.Left, op.LhsType, context);
                expr.Right = HandleReference(expr.Right, op.RhsType, context);
                if (!op.LhsType?.IsPolyType ?? false)
                    expr.Left = CheckType(expr.Left, op.LhsType);
                if (!op.RhsType?.IsPolyType ?? false)
                    expr.Right = CheckType(expr.Right, op.RhsType);

                if (op is UserDefinedBinaryOperator user)
                {
                    var args = new List<AstArgument>() {
                        new AstArgument(expr.Left, Location: expr.Left.Location),
                        new AstArgument(expr.Right, Location: expr.Right.Location)
                    };
                    var func = new AstSymbolExpr(user.Declaration);
                    var call = new AstCallExpr(func, args, expr.Location);
                    call.Replace(expr);
                    return InferType(call, expected);
                }

                expr.ActualOperator = op;

                if (expr.Left.Value != null && expr.Right.Value != null)
                    expr.Value = op.Execute(expr.Left.Value, expr.Right.Value);

                // @hack
                expr.Type = op.ResultType;
            }

            return expr;
        }

        private AstExpression InferTypesIdExpr(AstIdExpr expr, CheezType expected, TypeInferenceContext context)
        {
            if (expr.IsPolymorphic && !context.resolve_poly_expr_to_concrete_type)
            {
                expr.Type = CheezType.Type;
                expr.Value = new PolyType(expr.Name, true);
                return expr;
            }

            var sym = expr.Scope.GetSymbol(expr.Name);
            if (sym == null)
            {
                ReportError(expr, $"Unknown symbol '{expr.Name}'");
                return expr;
            }

            expr.Symbol = sym;

            if (context.dependencies != null && sym is AstDecl decl && decl.Type is AbstractType)
            {
                context.dependencies.Add(decl);
            }

            if (sym is AstSingleVariableDecl var)
            {
                expr.Type = var.Type;
                expr.SetFlag(ExprFlags.IsLValue, true);

                if (var.Type is VarDeclType && !context.is_global)
                {
                    ReportError(expr, $"Can't use variable '{var.Name}' before it is declared");
                }

                if (var.Constant)
                {
                    expr.IsCompTimeValue = true;
                    expr.Value = var.Value;
                }
            }
            else if (sym is AstParameter p)
            {
                expr.Type = p.Type;
                expr.SetFlag(ExprFlags.IsLValue, true);
            }
            else if (sym is TypeSymbol ct)
            {
                expr.Type = CheezType.Type;
                expr.Value = ct.Type;
            }
            else if (sym is AstStructDecl str)
            {
                expr.Type = CheezType.Type;
                expr.Value = str.Type;
            }
            else if (sym is AstEnumDecl @enum)
            {
                expr.Type = CheezType.Type;
                expr.Value = @enum.Type;
            }
            else if (sym is AstTraitDeclaration trait)
            {
                expr.Type = CheezType.Type;
                expr.Value = trait.Type;
            }
            else if (sym is AstTypeAliasDecl typedef)
            {
                expr.Type = CheezType.Type;
                expr.Value = typedef.Type;
            }
            else if (sym is AstFunctionDecl func)
            {
                expr.Type = func.Type;
                if (func.SelfType != SelfParamType.None)
                {
                    var ufc = new AstUfcFuncExpr(new AstIdExpr("self", false, expr), func);
                    ufc.Replace(expr);
                    ufc.SetFlag(ExprFlags.ValueRequired, expr.GetFlag(ExprFlags.ValueRequired));
                    return InferTypeHelper(ufc, null, default);
                }
            }
            else if (sym is ConstSymbol c)
            {
                expr.Type = c.Type;
                expr.Value = c.Value;
            }
            else if (sym is Using u)
            {
                if (u.Replace)
                {
                    var e = u.Expr.Clone();
                    e.Replace(expr);
                    e.Location = expr.Location;
                    e = InferTypeHelper(e, expected, context);
                    return e;
                }

                expr.Type = u.Type;
                expr.SetFlag(ExprFlags.IsLValue, true);
            }
            else
            {
                ReportError(expr, $"'{expr.Name}' is not a valid variable");
            }

            return expr;
        }

        private AstExpression InferTypesCharLiteral(AstCharLiteral expr, CheezType expected)
        {
            expr.Type = CheezType.Char;
            expr.CharValue = expr.RawValue[0];
            expr.Value = expr.CharValue;

            return expr;
        }

        private AstExpression InferTypesStringLiteral(AstStringLiteral expr, CheezType expected)
        {
            if (expr.Suffix != null)
            {
                if (expr.Suffix == "c") expr.Type = CheezType.CString;
                else
                {
                    // TODO: overridable suffixes
                    ReportError(expr, $"Unknown suffix '{expr.Suffix}'");
                }
            }
            else if (expected == CheezType.String || expected == CheezType.CString) expr.Type = expected;
            else if (expected is ArrayType arr && arr.TargetType == CheezType.Char)
            {
                expr.Type = ArrayType.GetArrayType(CheezType.Char, expr.StringValue.Length);
            }
            else expr.Type = CheezType.StringLiteral;

            return expr;
        }

        private AstExpression InferTypesNumberExpr(AstNumberExpr expr, CheezType expected)
        {
            if (expr.Data.Type == NumberData.NumberType.Int)
            {
                if (expr.Suffix != null)
                {
                    switch (expr.Suffix)
                    {
                        case "u8": expr.Type = IntType.GetIntType(1, false); break;
                        case "u16": expr.Type = IntType.GetIntType(2, false); break;
                        case "u32": expr.Type = IntType.GetIntType(4, false); break;
                        case "u64": expr.Type = IntType.GetIntType(8, false); break;
                        case "i8": expr.Type = IntType.GetIntType(1, true); break;
                        case "i16": expr.Type = IntType.GetIntType(2, true); break;
                        case "i32": expr.Type = IntType.GetIntType(4, true); break;
                        case "i64": expr.Type = IntType.GetIntType(8, true); break;
                        default: ReportError(expr, $"Unknown suffix '{expr.Suffix}'"); break;
                    }
                }
                else if (expected != null && (expected is IntType || expected is FloatType)) expr.Type = expected;
                else expr.Type = IntType.LiteralType;
                expr.Value = expr.Data;
            }
            else
            {
                if (expr.Suffix != null)
                {
                    if (expr.Suffix == "d")
                        expr.Type = FloatType.GetFloatType(8);
                    else if (expr.Suffix == "f")
                        expr.Type = FloatType.GetFloatType(4);
                    else ReportError(expr, $"Unknown suffix '{expr.Suffix}'");
                }
                else if (expected != null && expected is FloatType) expr.Type = expected;
                else expr.Type = FloatType.LiteralType;
                expr.Value = expr.Data;
            }

            return expr;
        }

        private AstExpression HandleReference(AstExpression expr, CheezType expected, TypeInferenceContext context)
        {
            var fromIsRef = expr.Type is ReferenceType;
            var toIsRef = expected is ReferenceType;
            if (toIsRef && !fromIsRef)
                return Ref(expr, context);
            if (!toIsRef && fromIsRef)
                return Deref(expr, context);

            return expr;
        }

        private AstExpression Deref(AstExpression expr, TypeInferenceContext context)
        {
            if (expr.Type is ReferenceType)
            {
                var deref = new AstDereferenceExpr(expr, expr);
                deref.Reference = true;
                deref.AttachTo(expr);
                deref.SetFlag(ExprFlags.ValueRequired, expr.GetFlag(ExprFlags.ValueRequired));

                if (context == null)
                    return InferType(deref, null);
                else
                    return InferTypeHelper(deref, null, context);
            }
            return expr;
        }

        private AstExpression Ref(AstExpression expr, TypeInferenceContext context)
        {
            var deref = new AstAddressOfExpr(expr, expr);
            deref.Reference = true;
            deref.AttachTo(expr);
            deref.SetFlag(ExprFlags.ValueRequired, expr.GetFlag(ExprFlags.ValueRequired));

            if (context == null)
                return InferType(deref, null);
            else
                return InferTypeHelper(deref, null, context);
        }

        private AstExpression CheckType(AstExpression expr, CheezType to, string errorMsg = null)
        {
            if (expr.Type.IsErrorType || to.IsErrorType)
                return expr;

            var from = expr.Type;

            if (from == to)
                return expr;

            var cast = new AstCastExpr(new AstTypeRef(to), expr, expr.Location);
            cast.Scope = expr.Scope;

            // TODO: only do this for implicit casts
            if (to is SliceType s && from is PointerType p && s.TargetType == p.TargetType)
                return InferType(cast, to);

            if (to is PointerType p2 && p2.TargetType == CheezType.Any && from is PointerType)
                return InferType(cast, to);

            if (to is IntType i1 && from is IntType i2 && i1.Signed == i2.Signed && i1.Size >= i2.Size)
                return InferType(cast, to);

            if (to is FloatType f1 && from is FloatType f2 && f1.Size >= f2.Size)
                return InferType(cast, to);

            if (to is SliceType s2 && from is ArrayType a && a.TargetType == s2.TargetType)
                return InferType(cast, to);

            if (to is BoolType && from is FunctionType)
                return InferType(cast, to);

            if (to is FunctionType && from is PointerType p3 && p3.TargetType == CheezType.Any)
                return InferType(cast, to);

            if (to is TraitType trait)
            {
                return InferType(cast, to);
            }

            ReportError(expr, errorMsg ?? $"Can't implicitly convert {from} to {to}");
            return expr;
        }

        private AstImplBlock GetTraitImpl(Scope scope, CheezType trait)
        {
            while (scope != null)
            {
                foreach (var impl in scope.Impls)
                {
                    if (impl.Trait == null &&
                        CheezType.TypesMatch(impl.TargetType, trait))
                    {

                        return impl;
                    }
                }

                scope = scope.Parent;
            }

            return null;
        }

        private AstExpression GetImplFunctions(AstDotExpr expr, CheezType type, string functionName, TypeInferenceContext context)
        {
            var result = GetImplFunctions(type, functionName, context.functionExpectedReturnType);
            
            if (result.Count == 0)
            {
                ReportError(expr.Right, $"Type '{type}' has no impl function '{functionName}'");
                return expr;
            }
            else if (result.Count > 1)
            {
                var details = result.Select(f => ("Possible candidate:", f.Name.Location));
                ReportError(expr.Right, $"Ambigious call to impl function '{functionName}'", details);
                return expr;
            }

            var ufc = new AstUfcFuncExpr(expr.Left, result[0]);
            ufc.Replace(expr);
            ufc.SetFlag(ExprFlags.ValueRequired, expr.GetFlag(ExprFlags.ValueRequired));
            return InferTypeHelper(ufc, null, context);
        }

        private List<AstFunctionDecl> GetImplFunctions(CheezType type, string functionName, CheezType expected)
        {
            var resultNormal = new List<AstFunctionDecl>();
            var resultNormal2 = new List<AstFunctionDecl>();
            var resultTrait = new List<AstFunctionDecl>();
            var resultTrait2 = new List<AstFunctionDecl>();

            // only search for non reference types
            if (type is ReferenceType r)
                type = r.TargetType;

            foreach (var impl in GetImplsForType(type))
            {
                if (impl.Trait == null)
                {
                    var func = impl.Functions.FirstOrDefault(f => f.Name.Name == functionName);
                    if (func != null)
                        if (Utilities.Implies(expected != null, func.ReturnType == expected))
                            resultNormal.Add(func);
                        else
                            resultNormal2.Add(func);
                }
                else
                {
                    var func = impl.Functions.FirstOrDefault(f => f.Name.Name == functionName);
                    if (func != null)
                        if (Utilities.Implies(expected != null, func.ReturnType == expected))
                            resultTrait.Add(func);
                        else
                            resultTrait2.Add(func);
                }
            }

            if (resultNormal.Count > 0)
                return resultNormal;
            if (resultNormal2.Count > 0)
                return resultNormal2;
            if (resultTrait.Count > 0)
                return resultTrait;
            return resultTrait2;
        }
    }
}
