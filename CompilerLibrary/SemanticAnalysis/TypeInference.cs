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
            public bool poly_from_scope;
            public bool forceInfer = false;
        }

        private bool IsLiteralType(CheezType t)
        {
            return t == IntType.LiteralType || t == FloatType.LiteralType || t == CheezType.StringLiteral;
        }

        private CheezType UnifyTypes(CheezType concrete, CheezType literal)
        {
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

        private AstExpression InferType(AstExpression expr, CheezType expected, bool poly_from_scope = false, HashSet<AstDecl> dependencies = null, bool forceInfer = false)
        {
            var context = new TypeInferenceContext
            {
                newPolyFunctions = new List<AstFunctionDecl>(),
                //newPolyDeclarations = new List<AstDecl>(),
                poly_from_scope = poly_from_scope,
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
            if (!(context?.forceInfer ?? false) && expr.TypeInferred)
                return expr;
            expr.TypeInferred = true;

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
                    return InferTypeReferenceTypeExpr(p, context);

                case AstSliceTypeExpr p:
                    return InferTypeSliceTypeExpr(p, context);

                case AstArrayTypeExpr p:
                    return InferTypeArrayTypeExpr(p, context);

                case AstFunctionTypeExpr func:
                    return InferTypeFunctionTypeExpr(func, context);

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

                case AstMacroExpr m:
                    return InferTypeMacro(m, expected, context);

                default:
                    throw new NotImplementedException();
            }
        }

        private AstExpression InferTypeMacro(AstMacroExpr expr, CheezType expected, TypeInferenceContext context)
        {
            var macro = expr.Scope.GetMacro(expr.Name.Name);

            if (macro is BuiltInMacro b)
            {
                var lexer = new ListLexer(mCompiler.GetText(expr), expr.Tokens);
                var e = b.Execute(expr, lexer, mCompiler.ErrorHandler);

                if (e == null)
                {
                    return expr;
                }

                e.Replace(expr);
                return InferTypeHelper(e, expected, context);
            }
            else
            {
                ReportError(expr.Name, $"'{expr.Name.Name}' is not a macro.");
            }

            return expr;
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

            foreach (var c in expr.Cases)
            {
                c.SubScope = new Scope("case", expr.Scope);

                // pattern
                c.Pattern.AttachTo(expr);
                c.Pattern.Scope = c.SubScope;
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


            // transform match

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
                        call.Function.AttachTo(call);
                        var d = InferType(call.Function, value.Type);
                        if (d is AstEnumValueExpr e)
                        {
                            call.Type = e.Type;
                            if (e.Type != value.Type)
                                break;

                            if (call.Arguments.Count == 1)
                            { 
                                e.Argument = call.Arguments[0].Expr;
                                AstExpression sub = new AstDotExpr(value, new AstIdExpr(e.Member.Name.Name, false), false);
                                sub.AttachTo(value);
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

        private AstExpression InferTypeReferenceTypeExpr(AstReferenceTypeExpr p, TypeInferenceContext context)
        {
            p.Target.AttachTo(p);
            p.Target = InferTypeHelper(p.Target, CheezType.Type, context);
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
            var ok = true;
            for (int i = 0; i < func.ParameterTypes.Count; i++)
            {
                func.ParameterTypes[i].AttachTo(func);
                func.ParameterTypes[i] = ResolveType(func.ParameterTypes[i], context, out var t);
            }

            CheezType ret = CheezType.Void;

            if (func.ReturnType != null)
            {
                func.ReturnType.AttachTo(func);
                func.ReturnType = ResolveType(func.ReturnType, context, out var t);
                ret = t;
            }

            if (!ok)
                return func;

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
            if (cast.TypeExpr != null)
            {
                cast.TypeExpr.AttachTo(cast);
                cast.TypeExpr = ResolveTypeNow(cast.TypeExpr, out var type);
                cast.Type = type;
            }
            else if (expected != null)
            {
                cast.Type = expected;
            }
            else
            {
                ReportError(cast, $"Auto cast not possible here");
            }

            cast.SubExpression.Scope = cast.Scope;
            cast.SubExpression = InferTypeHelper(cast.SubExpression, cast.Type, context);
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
                }

                if (t.Declaration.Implementations.ContainsKey(from))
                    return cast;

                if (t.Declaration.IsPolyInstance)
                {
                    var template = t.Declaration.Template.FindMatchingImplementation(from);
                    if (template == null)
                    {
                        ReportError(cast, $"Can't cast {from} to {to} because it doesn't implement the trait");
                        return cast;
                    }

                    var polyTypes = new Dictionary<string, CheezType>();
                    CollectPolyTypes(template.Trait, to, polyTypes);
                    CollectPolyTypes(template.TargetType, from, polyTypes);

                    var impl = InstantiatePolyImpl(template, polyTypes);
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

                    var impl = InstantiatePolyImpl(template, polyTypes);
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
            expr.SubScope = new Scope("if", expr.Scope);

            // copy initialized symbols
            foreach (var symbol in expr.Scope.InitializedSymbols)
                expr.SubScope.SetInitialized(symbol);

            if (expr.PreAction != null)
            {
                expr.PreAction.Scope = expr.SubScope;
                expr.PreAction.Parent = expr;
                AnalyseVariableDecl(expr.PreAction);
            }

            expr.Condition.Scope = expr.SubScope;
            expr.Condition.Parent = expr;
            expr.Condition = InferTypeHelper(expr.Condition, CheezType.Bool, context);
            ConvertLiteralTypeToDefaultType(expr.Condition, CheezType.Bool);

            if (expr.Condition.Type is ReferenceType)
                expr.Condition = Deref(expr.Condition, context);

            expr.Condition = CheckType(expr.Condition, CheezType.Bool, $"Condition of if statement must be either a bool or a pointer but is {expr.Condition.Type}");

            expr.IfCase.Scope = expr.SubScope;
            expr.IfCase.Parent = expr;
            expr.IfCase = InferTypeHelper(expr.IfCase, expected, context) as AstNestedExpression;
            ConvertLiteralTypeToDefaultType(expr.IfCase, expected);

            if (expr.ElseCase != null)
            {
                expr.ElseCase.Scope = expr.SubScope;
                expr.ElseCase.Parent = expr;
                expr.ElseCase = InferTypeHelper(expr.ElseCase, expected, context) as AstNestedExpression;
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
            }
            else
            {
                expr.Type = CheezType.Void;
            }

            if (expr.ElseCase != null)
            {
                foreach (var symbol in expr.IfCase.SubScope.InitializedSymbols)
                {
                    if (expr.ElseCase.SubScope.IsInitialized(symbol))
                        expr.Scope.SetInitialized(symbol);
                }
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
                    var tmpVar = new AstTempVarExpr(expr.SubExpression);
                    tmpVar.AttachTo(expr);
                    expr.SubExpression = InferType(tmpVar, null);

                    //ReportError(expr, $"Can't create a reference to the value '{expr.SubExpression}'");
                    //expr.Type = CheezType.Error;
                    //return expr;
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
                    ReportError(expr, $"Can't take the address of non lvalue");
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
                arg.AttachTo(expr);
                arg = InferTypeHelper(arg, e, context);
                ConvertLiteralTypeToDefaultType(arg, e);
                arg = HandleReference(arg, e, context);
                arg = CheckType(arg, e);

                return expr.Arguments[index] = arg;
            }

            switch (expr.Name.Name)
            {
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
                        } else
                        {
                            expr.Arguments.Add(new AstStringLiteral("Assertion failed!"));
                        }

                        expr.Type = CheezType.Void;
                        break;
                    }


                case "static_assert":
                    {
                        AstExpression cond = null, message = null;
                        if (expr.Arguments.Count >= 1)
                            cond = expr.Arguments[0];
                        if (expr.Arguments.Count >= 2)
                            message = expr.Arguments[1];

                        if (cond == null || expr.Arguments.Count > 2)
                        {
                            ReportError(expr, $"Wrong number of arguments");
                            return expr;
                        }

                        // infer types of arguments
                        cond.Scope = expr.Scope;
                        cond.Parent = expr;
                        cond = InferType(cond, CheezType.Bool);

                        if (message != null)
                        {
                            message.Scope = expr.Scope;
                            message.Parent = expr;
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

                        var arg = expr.Arguments[0];
                        arg.Scope = expr.Scope;
                        arg.Parent = expr;
                        arg = expr.Arguments[0] = InferTypeHelper(arg, CheezType.Type, context);
                        if (arg.Type.IsErrorType)
                            return expr;

                        if (arg.Type != CheezType.Type)
                        {
                            ReportError(arg, $"Argument must be a type but is '{arg.Type}'");
                            return expr;
                        }

                        var type = (CheezType)arg.Value;

                        return InferTypeHelper(new AstNumberExpr(type.Size, Location: expr.Location), null, context);
                    }

                case "alignof":
                    {
                        if (expr.Arguments.Count != 1)
                        {
                            ReportError(expr, $"@alignof takes one argument");
                            return expr;
                        }

                        var arg = expr.Arguments[0];
                        arg.Scope = expr.Scope;
                        arg.Parent = expr;
                        arg = expr.Arguments[0] = InferTypeHelper(arg, CheezType.Type, context);
                        if (arg.Type.IsErrorType)
                            return expr;

                        if (arg.Type != CheezType.Type)
                        {
                            ReportError(arg, $"Argument must be a type but is '{arg.Type}'");
                            return expr;
                        }

                        var type = (CheezType)arg.Value;

                        return InferTypeHelper(new AstNumberExpr(type.Alignment, Location: expr.Location), null, context);
                    }

                case "tuple_type_member":
                    {
                        if (expr.Arguments.Count != 2)
                        {
                            ReportError(expr, $"@tuple_type_member requires two arguments (tuple type, int)");
                            return expr;
                        }

                        expr.Arguments[0].Scope = expr.Scope;
                        expr.Arguments[1].Scope = expr.Scope;
                        expr.Arguments[0] = InferTypeHelper(expr.Arguments[0], CheezType.Type, context);
                        expr.Arguments[1] = InferTypeHelper(expr.Arguments[1], IntType.DefaultType, context);

                        if (expr.Arguments[0].Type != CheezType.Type || !(expr.Arguments[0].Value is TupleType))
                        {
                            if (expr.Arguments[0].Value is PolyType)
                            {
                                expr.Type = CheezType.Type;
                                expr.Value = expr.Arguments[0].Type;
                                return expr;
                            }
                            ReportError(expr.Arguments[0], $"This argument must be a tuple type, got {expr.Arguments[0].Type} '{expr.Arguments[0].Value}'");
                            return expr;
                        }
                        if (!(expr.Arguments[1].Type is IntType) || !expr.Arguments[1].IsCompTimeValue)
                        {
                            ReportError(expr.Arguments[1], $"This argument must be a constant int, got {expr.Arguments[1].Type} '{expr.Arguments[1].Value}'");
                            return expr;
                        }

                        var tuple = expr.Arguments[0].Value as TupleType;
                        var index = ((NumberData)expr.Arguments[1].Value).ToLong();

                        if (index < 0 || index >= tuple.Members.Length)
                        {
                            ReportError(expr.Arguments[1], $"Index '{index}' is out of range. Index must be between [0, {tuple.Members.Length})");
                            return expr;
                        }

                        expr.Type = CheezType.Type;
                        expr.Value = tuple.Members[index].type;

                        break;
                    }

                case "typeof":
                    {
                        if (expr.Arguments.Count != 1)
                        {
                            ReportError(expr, $"@typeof takes one argument");
                            return expr;
                        }

                        var arg = expr.Arguments[0];
                        arg.Scope = expr.Scope;
                        arg.Parent = expr;
                        arg = expr.Arguments[0] = InferTypeHelper(arg, null, context);
                        if (arg.Type.IsErrorType)
                            return expr;

                        var result = new AstTypeRef(arg.Type, expr);
                        result.AttachTo(expr);
                        return InferTypeHelper(result, null, context);
                    }

                case "typename":
                    {
                        if (expr.Arguments.Count != 1)
                        {
                            ReportError(expr, $"@typename takes one argument");
                            return expr;
                        }

                        var arg = expr.Arguments[0];
                        arg.Scope = expr.Scope;
                        arg.Parent = expr;
                        arg = expr.Arguments[0] = InferTypeHelper(arg, CheezType.Type, context);
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

                        var argType = expr.Arguments[0];
                        argType.AttachTo(expr);
                        argType = expr.Arguments[0] = InferTypeHelper(argType, CheezType.Type, context);

                        var argSize = expr.Arguments[1];
                        argSize.AttachTo(expr);
                        argSize = expr.Arguments[1] = InferTypeHelper(argSize, IntType.DefaultType, context);

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
                    {
                        if (expr.Arguments.Count == 0)
                        {
                            ReportError(expr, $"@bin_or requires at least one argument");
                            return expr;
                        }

                        var minSize = 1;
                        var ok = true;
                        bool allConstant = true;
                        for (int i = 0; i < expr.Arguments.Count; i++)
                        {
                            var arg = expr.Arguments[i];
                            arg.AttachTo(expr);
                            arg = expr.Arguments[i] = InferType(arg, null);
                            ConvertLiteralTypeToDefaultType(arg, null);
                            if (arg.Type.IsErrorType)
                            {
                                ok = false;
                                continue;
                            }

                            if (!arg.IsCompTimeValue)
                                allConstant = false;

                            if (arg.Type is IntType it)
                            {
                                if (it.Size > minSize)
                                    minSize = it.Size;
                            }
                            else
                            {
                                ReportError(arg, $"Argument to @bin_or must be ints");
                            }
                        }

                        if (!ok)
                            return expr;

                        if (allConstant)
                        {
                            BigInteger result = 0;
                            foreach (var a in expr.Arguments)
                            {
                                var v = ((NumberData)a.Value);
                                result |= v.IntValue;
                            }

                            expr.Value = NumberData.FromBigInt(result);
                            expr.IsCompTimeValue = true;
                        }

                        for (int i = 0; i < expr.Arguments.Count; i++)
                        {
                            var to = IntType.GetIntType(minSize, (expr.Arguments[i].Type as IntType).Signed);
                            expr.Arguments[i] = CheckType(expr.Arguments[i], to);
                        }

                        expr.Type = IntType.GetIntType(minSize, false);
                        return expr;
                    }

                case "bin_and":
                    {
                        if (expr.Arguments.Count == 0)
                        {
                            ReportError(expr, $"@bin_and requires at least one argument");
                            return expr;
                        }

                        var minSize = 1;
                        var ok = true;
                        for (int i = 0; i < expr.Arguments.Count; i++)
                        {
                            var arg = expr.Arguments[i];
                            arg.AttachTo(expr);
                            arg = expr.Arguments[i] = InferType(arg, null);
                            ConvertLiteralTypeToDefaultType(arg, null);
                            if (arg.Type.IsErrorType)
                            {
                                ok = false;
                                continue;
                            }

                            if (arg.Type is IntType it)
                            {
                                if (it.Size > minSize)
                                    minSize = it.Size;
                            }
                            else
                            {
                                ReportError(arg, $"Argument to @bin_and must be ints");
                            }
                        }

                        if (!ok)
                        {
                            return expr;
                        }

                        for (int i = 0; i < expr.Arguments.Count; i++)
                        {
                            var to = IntType.GetIntType(minSize, (expr.Arguments[i].Type as IntType).Signed);
                            expr.Arguments[i] = CheckType(expr.Arguments[i], to);
                        }

                        expr.Type = IntType.GetIntType(minSize, false);
                        return expr;
                    }

                case "bin_lsl":
                    {
                        if (expr.Arguments.Count != 2)
                        {
                            ReportError(expr, $"@bin_lsl requires two arguments");
                            return expr;
                        }

                        var ok = true;
                        for (int i = 0; i < expr.Arguments.Count; i++)
                        {
                            var arg = expr.Arguments[i];
                            arg.AttachTo(expr);
                            arg = expr.Arguments[i] = InferType(arg, null);
                            ConvertLiteralTypeToDefaultType(arg, null);
                            if (arg.Type.IsErrorType)
                            {
                                ok = false;
                                continue;
                            }

                            if (!(arg.Type is IntType it))
                            {
                                ReportError(arg, $"Argument must be ints");
                            }
                        }

                        if (!ok)
                        {
                            return expr;
                        }

                        expr.Type = expr.Arguments[0].Type;
                        return expr;
                    }

                case "bin_lsr":
                    {
                        if (expr.Arguments.Count != 2)
                        {
                            ReportError(expr, $"@bin_lsr requires two arguments");
                            return expr;
                        }

                        var ok = true;
                        for (int i = 0; i < expr.Arguments.Count; i++)
                        {
                            var arg = expr.Arguments[i];
                            arg.AttachTo(expr);
                            arg = expr.Arguments[i] = InferType(arg, null);
                            ConvertLiteralTypeToDefaultType(arg, null);
                            if (arg.Type.IsErrorType)
                            {
                                ok = false;
                                continue;
                            }

                            if (!(arg.Type is IntType it))
                            {
                                ReportError(arg, $"Argument must be ints");
                            }
                        }

                        if (!ok)
                        {
                            return expr;
                        }

                        expr.Type = expr.Arguments[0].Type;
                        return expr;
                    }

                default: ReportError(expr.Name, $"Unknown intrinsic '{expr.Name.Name}'"); break;
            }
            return expr;
        }

        private AstExpression InferTypeBlock(AstBlockExpr expr, CheezType expected, TypeInferenceContext context)
        {
            expr.SubScope = new Scope("{}", expr.Scope);

            // copy initialized symbols
            foreach (var symbol in expr.Scope.InitializedSymbols)
                expr.SubScope.SetInitialized(symbol);

            // test
            //ResolveDeclarations(expr.SubScope, expr.Statements);


            int end = expr.Statements.Count;
            if (expr.Statements.LastOrDefault() is AstExprStmt) --end;

            for (int i = 0; i < end; i++)
            {
                var stmt = expr.Statements[i];
                stmt.Scope = expr.SubScope;
                stmt.Parent = expr;
                AnalyseStatement(stmt);

                if (stmt.GetFlag(StmtFlags.Returns))
                    expr.SetFlag(ExprFlags.Returns, true);
            }

            if (expr.Statements.LastOrDefault() is AstExprStmt exprStmt)
            {
                exprStmt.Expr.Scope = expr.SubScope;
                exprStmt.Expr.Parent = exprStmt;
                exprStmt.Parent = expr;
                exprStmt.Expr = InferTypeHelper(exprStmt.Expr, expected, context);
                ConvertLiteralTypeToDefaultType(exprStmt.Expr, expected);
                expr.Type = exprStmt.Expr.Type;

                AnalyseExprStatement(exprStmt, true, false);

                expr.SetFlag(ExprFlags.IsLValue, exprStmt.Expr.GetFlag(ExprFlags.IsLValue));

                if (exprStmt.GetFlag(StmtFlags.Returns))
                    expr.SetFlag(ExprFlags.Returns, true);
            }
            else
            {
                expr.Type = CheezType.Void;
            }

            // copy initialized symbols
            foreach (var symbol in expr.SubScope.InitializedSymbols)
            {
                expr.Scope.SetInitialized(symbol);
            }

            return expr;
        }

        private AstExpression InferTypeIndexExpr(AstArrayAccessExpr expr, CheezType expected, TypeInferenceContext context)
        {
            expr.SubExpression.SetFlag(ExprFlags.SetAccess, expr.GetFlag(ExprFlags.SetAccess));

            expr.SubExpression.Scope = expr.Scope;
            expr.SubExpression = InferTypeHelper(expr.SubExpression, null, context);

            expr.Indexer.Scope = expr.Scope;
            expr.Indexer = InferTypeHelper(expr.Indexer, null, context);

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
                        if (expr.Indexer.Type is IntType)
                        {
                            expr.SetFlag(ExprFlags.IsLValue, true);
                            expr.Type = ptr.TargetType;
                        }
                        else
                        {
                            ReportError(expr.Indexer, $"The index of into a pointer must be a int but is '{expr.Indexer.Type}'");
                        }
                        break;
                    }

                case SliceType slice:
                    {
                        if (expr.Indexer.Type is IntType)
                        {
                            expr.SetFlag(ExprFlags.IsLValue, true);
                            expr.Type = slice.TargetType;
                        }
                        else
                        {
                            ReportError(expr.Indexer, $"The index of into a slice must be a int but is '{expr.Indexer.Type}'");
                        }
                        break;
                    }

                case ArrayType arr:
                    {
                        if (expr.Indexer.Type is IntType)
                        {
                            expr.SetFlag(ExprFlags.IsLValue, true);
                            expr.Type = arr.TargetType;
                        }
                        else
                        {
                            ReportError(expr.Indexer, $"The index of into an array must be a int but is '{expr.Indexer.Type}'");
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
                    newLeft.Scope = expr.Left.Scope;
                    newLeft.Parent = expr.Left;
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
                case EnumType @enum when !expr.IsDoubleColon:
                    {
                        var memName = expr.Right.Name;
                        var mem = @enum.Declaration.Members.FirstOrDefault(m => m.Name.Name == memName);

                        if (mem == null)
                        {
                            ReportError(expr, $"Type {@enum} has no member '{memName}'");
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
                            // check if function exists

                            var funcs = expr.Scope.GetImplFunction(s, name);

                            if (funcs.Count == 0)
                            {
                                ReportError(expr.Right, $"Struct '{s}' has no field or function '{name}'");
                                break;
                            }
                            else if (funcs.Count > 1)
                            {
                                var details = funcs.Select(f => ("Possible candidate:", f.Name.Location));
                                ReportError(expr.Right, $"Ambigious call to impl function '{name}'", details);
                                break;
                            }

                            var ufc = new AstUfcFuncExpr(expr.Left, funcs[0]);
                            return InferTypeHelper(ufc, null, context);
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
                        expr.SetFlag(ExprFlags.IsLValue, true);
                        break;
                    }

                case TraitType t when !expr.IsDoubleColon:
                    {
                        var name = expr.Right.Name;
                        var func = t.Declaration.Functions.FirstOrDefault(f => f.Name.Name == name);

                        if (func == null)
                        {
                            ReportError(expr.Right, $"Trait '{t.Declaration.Name}' has no function '{name}'");
                            break;
                        }

                        var ufc = new AstUfcFuncExpr(expr.Left, func);
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
                        var funcs = expr.Scope.GetImplFunction(t, expr.Right.Name);

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
                        var funcs = expr.Scope.GetImplFunction(c, name);

                        if (funcs.Count == 0)
                        {
                            ReportError(expr.Right, $"Type '{c}' has no impl function '{name}'");
                            break;
                        }
                        else if (funcs.Count > 1)
                        {
                            var details = funcs.Select(f => ("Possible candidate:", f.Name.Location));
                            ReportError(expr.Right, $"Ambigious call to function '{expr.Right.Name}'", details);
                            break;
                        }

                        var ufc = new AstUfcFuncExpr(expr.Left, funcs[0]);
                        return InferTypeHelper(ufc, null, context);
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

        private AstExpression InferTypeCallExpr(AstCallExpr expr, CheezType expected, TypeInferenceContext context)
        {
            expr.Function.AttachTo(expr);
            expr.Function = InferTypeHelper(expr.Function, null, context);

            switch (expr.Function.Type)
            {
                case FunctionType f:
                    {
                        return InferRegularFunctionCall(f, expr, expected, context);
                    }

                case GenericFunctionType g:
                    {
                        return InferGenericFunctionCall(g, expr, expected, context);
                    }

                case GenericEnumType @enum:
                    {
                        var e = expr.Function as AstEnumValueExpr;
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
                        var e = expr.Function as AstEnumValueExpr;
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

                case CheezTypeType type:
                    {
                        return InferTypeGenericTypeCallExpr(expr, context);
                    }

                case ErrorType _: return expr;

                default: ReportError(expr.Function, $"Type '{expr.Function.Type}' is not callable"); break;
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

            if (expr.Function.Value is GenericStructType strType)
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
                return expr;
            }
            else if (expr.Function.Value is GenericEnumType @enum)
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
                return expr;
            }
            else if (expr.Function.Value is GenericTraitType trait)
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
                return expr;
            }
            else
            {
                ReportError(expr.Function, $"This type must be a polymorphic type but is '{expr.Function.Value}'");
                return expr;
            }
        }

        private bool CheckAndMatchArgsToParams(
            AstCallExpr expr, 
            (string Name, CheezType Type, AstExpression DefaultValue)[] parameters, 
            bool varArgs)
        {
            // create self argument for ufc
            if (expr.Function is AstUfcFuncExpr ufc)
            {
                AstArgument selfArg = new AstArgument(ufc.SelfArg, Location: expr.Function);
                expr.Arguments.Insert(0, selfArg);
                expr.UnifiedFunctionCall = true;
            }

            // check for too many arguments
            if (expr.Arguments.Count > parameters.Length && !varArgs)
            {
                (string, ILocation)? detail = null;
                if (expr.Function is AstIdExpr id)
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
                    ReportError(expr, $"Call misses parameter {i} ({p.ToString()}).");
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
                arg.Scope = expr.Scope;
                arg.Expr.Scope = arg.Scope;

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
                if (expr.Function is AstDotExpr dot)
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
            expr.Type = instance.FunctionType.ReturnType;
            expr.SetFlag(ExprFlags.IsLValue, instance.FunctionType.ReturnType is PointerType);

            return expr;
        }

        private AstExpression InferRegularFunctionCall(FunctionType func, AstCallExpr expr, CheezType expected, TypeInferenceContext context)
        {
            //var par = func.Declaration.Parameters.Select(p => (p.Name?.Name, p.Type, p.DefaultValue)).ToArray();
            var par = func.Parameters;
            if (!CheckAndMatchArgsToParams(expr, par, func.VarArgs))
                return expr;

            // match arguments and parameter types
            var pairs = expr.Arguments.Select(arg => (arg.Index < func.Parameters.Length ? func.Parameters[arg.Index].type : null, arg));
            (CheezType type, AstArgument arg)[] args = pairs.ToArray();
            foreach (var (type, arg) in args)
            {
                arg.Scope = expr.Scope;
                arg.Expr.Scope = arg.Scope;
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
                ReportError(expr, $"This expression is not a struct but a '{expr.Type}'");
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
                if (!op.LhsType.IsPolyType)
                    expr.Left = CheckType(expr.Left, op.LhsType);
                if (!op.RhsType.IsPolyType)
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
            if (expr.IsPolymorphic && !context.poly_from_scope)
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
                if (func.SelfParameter)
                {
                    var ufc = new AstUfcFuncExpr(new AstIdExpr("self", false, expr), func);
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
            var deref = new AstDereferenceExpr(expr, expr);
            deref.Reference = true;
            deref.AttachTo(expr);
            return InferTypeHelper(deref, null, context);
        }

        private AstExpression Ref(AstExpression expr, TypeInferenceContext context)
        {
            var deref = new AstAddressOfExpr(expr, expr);
            deref.Reference = true;
            deref.AttachTo(expr);
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
    }
}
