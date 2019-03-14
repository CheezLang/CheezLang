using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Expressions.Types;
using Cheez.Ast.Statements;
using Cheez.Extras;
using Cheez.Types;
using Cheez.Types.Abstract;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;
using Cheez.Util;

namespace Cheez
{
    public partial class Workspace
    {
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
                if (expected != null && !(expected is IntType)) return IntType.DefaultType;
                return expected ?? IntType.DefaultType;
            }
            else if (literalType == FloatType.LiteralType)
            {
                if (expected != null && !(expected is FloatType)) return FloatType.DefaultType;
                return expected ?? FloatType.DefaultType;
            }
            else if (literalType == CheezType.StringLiteral) return CheezType.String;

            return literalType;
        }

        private void ConvertLiteralTypeToDefaultType(AstExpression expr, CheezType expected = null)
        {
            expr.Type = LiteralTypeToDefaultType(expr.Type, expected);
        }

        private AstExpression InferType(AstExpression expr, CheezType expected)
        {
            List<AstFunctionDecl> newInstances = new List<AstFunctionDecl>();
            var newExpr = InferTypeHelper(expr, expected, newInstances);

            if (newInstances.Count > 0)
                AnalyseFunctions(newInstances);

            return newExpr;
        }

        private AstExpression InferTypeHelper(AstExpression expr, CheezType expected, List<AstFunctionDecl> newInstances)
        {
            if (expr.TypeInferred)
                return expr;
            expr.TypeInferred = true;

            // :fix
            // does not work because tuple containing abstract types does currently not count as an abstract type
            // - 08.03.2019
            //if (expr.Type != null && !(expr.Type is AbstractType)) return;

            expr.Type = CheezType.Error;

            //if (expected is PolyType pt && polyTypeMap.TryGetValue(pt.Name, out var concreteType))
            //{
            //    expected = concreteType;
            //}

            switch (expr)
            {
                case AstNullExpr n:
                    if (expected is PointerType)
                        n.Type = expected;
                    else if (expected is SliceType)
                        n.Type = expected;
                    else
                        n.Type = PointerType.GetPointerType(CheezType.Any);
                    return expr;

                case AstBoolExpr b:
                    b.Type = CheezType.Bool;
                    b.Value = b.BoolValue;
                    return expr;

                case AstNumberExpr n:
                    return InferTypesNumberExpr(n, expected);

                case AstStringLiteral s:
                    return InferTypesStringLiteral(s, expected);

                case AstCharLiteral ch:
                    return InferTypesCharLiteral(ch, expected);

                case AstIdExpr i:
                    return InferTypesIdExpr(i, expected);

                case AstBinaryExpr b:
                    return InferTypesBinaryExpr(b, expected, newInstances);

                case AstStructValueExpr s:
                    return InferTypeStructValueExpr(s, expected, newInstances);

                case AstUnaryExpr u:
                    return InferTypeUnaryExpr(u, expected, newInstances);

                case AstCallExpr c:
                    return InferTypeCallExpr(c, expected, newInstances);

                case AstTupleExpr t:
                    return InferTypeTupleExpr(t, expected, newInstances);

                case AstDotExpr d:
                    return InferTypeDotExpr(d, expected, newInstances);

                case AstArrayAccessExpr d:
                    return InferTypeIndexExpr(d, expected, newInstances);

                case AstTempVarExpr d:
                    if (d.Expr.Type == null)
                        d.Expr = InferTypeHelper(d.Expr, expected, newInstances);
                    d.Type = d.Expr.Type;
                    return expr;

                case AstSymbolExpr s:
                    s.Type = s.Symbol.Type;
                    s.SetFlag(ExprFlags.IsLValue, true);
                    return expr;

                case AstBlockExpr b:
                    return InferTypeBlock(b, expected, newInstances);

                case AstIfExpr i:
                    return InferTypeIfExpr(i, expected, newInstances);

                case AstCompCallExpr c:
                    return InferTypeCompCall(c, expected, newInstances);

                case AstAddressOfExpr ao:
                    return InferTypeAddressOf(ao, expected, newInstances);

                case AstDereferenceExpr de:
                    return InferTypeDeref(de, expected, newInstances);

                case AstCastExpr cast:
                    return InferTypeCast(cast, expected, newInstances);

                case AstTypeExpr type:
                    return InferTypeTypeExpr(type, expected, newInstances);

                case AstEmptyExpr e:
                    return e;

                default:
                    throw new NotImplementedException();
            }

            //if (expected is PolyType p && p.IsDeclaring)
            //{
            //    polyTypeMap[p.Name] = expr.Type;
            //}

            return expr;
        }

        private AstExpression InferTypeTypeExpr(AstTypeExpr expr, CheezType expected, List<AstFunctionDecl> newInstances)
        {
            expr.Type = CheezType.Type;
            expr.Value = ResolveType(expr);
            return expr;
        }

        private AstExpression InferTypeCast(AstCastExpr cast, CheezType expected, List<AstFunctionDecl> newInstances)
        {
            if (cast.TypeExpr != null)
            {
                cast.TypeExpr.Scope = cast.Scope;
                cast.Type = ResolveType(cast.TypeExpr);
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
            cast.SubExpression = InferTypeHelper(cast.SubExpression, cast.Type, newInstances);

            if (cast.SubExpression.Type.IsErrorType)
                return cast;

            // TODO: check if cast possible
            var to = cast.Type;
            var from = cast.SubExpression.Type;
            if ((to is PointerType && from is PointerType) ||
                (to is IntType && from is PointerType) ||
                (to is PointerType && from is IntType) ||
                (to is IntType && from is IntType) ||
                (to is FloatType && from is FloatType) ||
                (to is FloatType && from is IntType) ||
                (to is IntType && from is FloatType) ||
                (to is IntType && from is BoolType) ||
                (to is SliceType s && from is PointerType p && s.TargetType == p.TargetType))
            {
                // ok
            }
            else
            {
                ReportError(cast, $"Can't convert from type {from} to type {to}");
            }

            return cast;
        }

        private AstExpression InferTypeDeref(AstDereferenceExpr de, CheezType expected, List<AstFunctionDecl> newInstances)
        {
            CheezType subExpect = null;
            if (expected != null) subExpect = PointerType.GetPointerType(expected);

            de.SubExpression.Scope = de.Scope;
            de.SubExpression = InferTypeHelper(de.SubExpression, subExpect, newInstances);

            if (!de.SubExpression.GetFlag(ExprFlags.IsLValue))
            {
                ReportError(de, $"Can't dereference non lvalue");
                return de;
            }

            if (de.SubExpression.Type is PointerType p)
            {
                de.Type = p.TargetType;
            }
            else if (!de.SubExpression.Type.IsErrorType)
            {
                ReportError(de, $"Can't dereference non pointer type {de.SubExpression.Type}");
            }

            de.SetFlag(ExprFlags.IsLValue, true);
            return de;
        }

        private AstExpression InferTypeIfExpr(AstIfExpr expr, CheezType expected, List<AstFunctionDecl> newInstances)
        {
            expr.SubScope = new Scope("if", expr.Scope);
            if (expr.PreAction != null)
            {
                expr.PreAction.Scope = expr.SubScope;
                expr.PreAction.Parent = expr;
                AnalyseVariableDecl(expr.PreAction);
            }

            expr.Condition.Scope = expr.SubScope;
            expr.Condition.Parent = expr;
            expr.Condition = InferTypeHelper(expr.Condition, CheezType.Bool, newInstances);
            ConvertLiteralTypeToDefaultType(expr.Condition);

            if (expr.Condition.Type != CheezType.Bool && !(expr.Condition.Type is PointerType) && !expr.Condition.Type.IsErrorType)
            {
                ReportError(expr.Condition, $"Condition of if statement must be either a bool or a pointer but is {expr.Condition.Type}");
            }

            expr.IfCase.Scope = expr.SubScope;
            expr.IfCase.Parent = expr;
            expr.IfCase = InferTypeHelper(expr.IfCase, expected, newInstances);
            ConvertLiteralTypeToDefaultType(expr.IfCase, expected);

            if (expr.ElseCase != null)
            {
                expr.ElseCase.Scope = expr.SubScope;
                expr.ElseCase.Parent = expr;
                expr.ElseCase = InferTypeHelper(expr.ElseCase, expected, newInstances);
                ConvertLiteralTypeToDefaultType(expr.ElseCase, expected);
                
                if (expr.IfCase.Type == expr.ElseCase.Type)
                {
                    expr.Type = expr.IfCase.Type;
                }
                else
                {
                    expr.Type = new SumType(expr.IfCase.Type, expr.ElseCase.Type);
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

            return expr;
        }

        private AstExpression InferTypeAddressOf(AstAddressOfExpr expr, CheezType expected, List<AstFunctionDecl> newInstances)
        {
            CheezType subExpected = null;
            if (expected is PointerType p)
            {
                subExpected = p.TargetType;
            }

            expr.SubExpression.Scope = expr.Scope;
            expr.SubExpression = InferTypeHelper(expr.SubExpression, subExpected, newInstances);

            // check wether sub is an lvalue

            expr.Type = PointerType.GetPointerType(expr.SubExpression.Type);

            return expr;
        }

        private AstExpression InferTypeCompCall(AstCompCallExpr expr, CheezType expected, List<AstFunctionDecl> newInstances)
        {
            switch (expr.Name.Name)
            {
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
                        arg = expr.Arguments[0] = InferTypeHelper(arg, CheezType.Type, newInstances);
                        if (arg.Type.IsErrorType)
                            return expr;

                        if (arg.Type != CheezType.Type)
                        {
                            ReportError(arg, $"Argument must be a type but is '{arg.Type}'");
                            return expr;
                        }

                        var type = (CheezType)arg.Value;

                        return InferTypeHelper(new AstNumberExpr(type.Size, Location: expr.Location), null, null);
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
                        expr.Arguments[0] = InferTypeHelper(expr.Arguments[0], CheezType.Type, newInstances);
                        expr.Arguments[1] = InferTypeHelper(expr.Arguments[1], IntType.DefaultType, newInstances);

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
                        if (!(expr.Arguments[1].Type is IntType) || expr.Arguments[1].Value == null)
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

                default: ReportError(expr.Name, $"Unknown intrinsic '{expr.Name.Name}'"); break;
            }
            return expr;
        }

        private AstExpression InferTypeBlock(AstBlockExpr expr, CheezType expected, List<AstFunctionDecl> newInstances)
        {
            expr.SubScope = new Scope("{}", expr.Scope);

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
                exprStmt.Expr = InferTypeHelper(exprStmt.Expr, expected, newInstances);
                ConvertLiteralTypeToDefaultType(exprStmt.Expr, expected);
                expr.Type = exprStmt.Expr.Type;

                AnalyseExprStatement(exprStmt, true, false);
            }
            else
            {
                expr.Type = CheezType.Void;
            }

            foreach (var symbol in expr.SubScope.InitializedSymbols)
            {
                expr.Scope.SetInitialized(symbol);
            }

            return expr;
        }

        private AstExpression InferTypeIndexExpr(AstArrayAccessExpr expr, CheezType expected, List<AstFunctionDecl> newInstances)
        {
            expr.SubExpression.Scope = expr.Scope;
            expr.SubExpression = InferTypeHelper(expr.SubExpression, null, newInstances);

            expr.Indexer.Scope = expr.Scope;
            expr.Indexer = InferTypeHelper(expr.Indexer, null, newInstances);

            ConvertLiteralTypeToDefaultType(expr.Indexer);

            if (expr.SubExpression.Type is ErrorType || expr.Indexer.Type is ErrorType)
                return expr;

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
                    // TODO: seach for overloaded operator
                    ReportError(expr.SubExpression, $"Type {expr.SubExpression.Type} has no operator []");
                    break;
            }

            return expr;
        }

        private AstExpression InferTypeDotExpr(AstDotExpr expr, CheezType expected, List<AstFunctionDecl> newInstances)
        {
            expr.Left.Scope = expr.Scope;
            expr.Left = InferTypeHelper(expr.Left, null, newInstances);

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
            }

            var sub = expr.Right.Name;
            switch (expr.Left.Type)
            {
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

                case SliceType slice when expr.IsDoubleColon:
                    {
                        var name = expr.Right.Name;
                        var func = expr.Scope.GetImplFunction(slice, name);

                        if (func == null)
                        {
                            ReportError(expr.Right, $"Type '{slice}' has no impl function '{name}'");
                            break;
                        }

                        expr.Type = func.Type;
                        break;
                    }

                case PointerType ptr when expr.IsDoubleColon:
                    {
                        var name = expr.Right.Name;
                        var func = expr.Scope.GetImplFunction(ptr, name);

                        if (func == null)
                        {
                            ReportError(expr.Right, $"Type '{ptr}' has no impl function '{name}'");
                            break;
                        }

                        expr.Type = func.Type;
                        break;
                    }

                case StructType s when !expr.IsDoubleColon:
                    {
                        var name = expr.Right.Name;
                        var index = s.GetIndexOfMember(name);
                        if (index == -1)
                        {
                            // check if function exists

                            var func = expr.Scope.GetImplFunction(s, name);

                            if (func == null)
                            {
                                ReportError(expr.Right, $"Struct '{s}' has no field or function '{name}'");
                                break;
                            }

                            expr.Type = func.Type;
                            break;
                        }

                        expr.Type = s.Declaration.Members[index].Type;
                        expr.SetFlag(ExprFlags.IsLValue, true);
                        break;
                    }

                case CheezTypeType _ when expr.IsDoubleColon:
                    {
                        var t = expr.Left.Value as CheezType;
                        var func = expr.Scope.GetImplFunction(t, expr.Right.Name);

                        if (func == null)
                        {
                            ReportError(expr.Right, $"Type '{t}' has no function '{expr.Right.Name}'");
                            break;
                        }

                        expr.Type = func.Type;
                        break;
                    }
                case CheezTypeType _:
                    ReportError(expr.Left, $"Invalid value on left side of '.': '{expr.Left.Value}'");
                    break;

                case ErrorType _: return expr;

                default: throw new NotImplementedException();
            }

            return expr;
        }

        private AstExpression InferTypeTupleExpr(AstTupleExpr expr, CheezType expected, List<AstFunctionDecl> newInstances)
        {
            TupleType tupleType = expected as TupleType;
            if (tupleType?.Members?.Length != expr.Values.Count) tupleType = null;

            var members = new (string, CheezType type)[expr.Values.Count];
            for (int i = 0; i < expr.Values.Count; i++)
            {
                var v = expr.Values[i];
                v.Scope = expr.Scope;

                var e = tupleType?.Members[i].type;
                v = expr.Values[i] = InferTypeHelper(v, e, newInstances);

                // TODO: do somewhere else
                ConvertLiteralTypeToDefaultType(v);

                members[i].type = v.Type;
            }

            expr.Type = TupleType.GetTuple(members);

            return expr;
        }

        private AstExpression InferTypeCallExpr(AstCallExpr expr, CheezType expected, List<AstFunctionDecl> newInstances)
        {
            expr.Function.Scope = expr.Scope;
            expr.Function = InferTypeHelper(expr.Function, null, newInstances);

            switch (expr.Function.Type)
            {
                case FunctionType f:
                    {
                        return InferRegularFunctionCall(f, expr, expected, newInstances);
                    }

                case GenericFunctionType g:
                    {
                        return InferGenericFunctionCall(g, expr, expected, newInstances);
                    }

                case ErrorType _: return expr;

                default: ReportError(expr.Function, $"This is not a callable value"); break;
            }

            return expr;
        }

        private bool CheckAndMatchArgsToParams(AstFunctionDecl decl, AstCallExpr expr, List<AstParameter> parameters, bool varArgs)
        {
            // create self argument for ufc
            if (decl != null && decl.ImplBlock != null && decl.SelfParameter)
            {
                if (expr.Function is AstDotExpr val)
                {
                    // add left side as first parameter
                    AstArgument selfArg = null;
                    if (decl.RefSelf)
                    {
                        selfArg = new AstArgument(new AstAddressOfExpr(val.Left, val.Left.Location), Location: val.Left.Location);
                    }
                    else
                    {
                        selfArg = new AstArgument(val.Left, Location: val.Left.Location);
                    }
                    expr.Arguments.Insert(0, selfArg);
                }
                else if (expr.Function is AstSymbolExpr)
                {
                    // do nothing. this was generated by the compiler in an assignment
                }
                else
                {
                    throw new NotImplementedException();
                }

                expr.UnifiedFunctionCall = true;
            }

            // check for too many arguments
            if (expr.Arguments.Count > parameters.Count && !varArgs)
            {
                (string, ILocation)? detail = null;
                if (expr.Function is AstIdExpr id)
                {
                    ILocation loc = id.Symbol.Location;
                    if (id.Symbol is AstFunctionDecl fd)
                        loc = new Location(fd.Name.Beginning, fd.ParameterLocation.End);
                    detail = ("Function defined here:", loc);
                }
                ReportError(expr, $"Too many arguments. Expected {parameters.Count}, got {expr.Arguments.Count}", detail);
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
                    var index = parameters.FindIndex(p => p.Name?.Name == arg.Name.Name);
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
            for (int i = 0; i < parameters.Count; i++)
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
                var arg = new AstArgument(p.DefaultValue, Location: p.DefaultValue.Location);
                arg.Index = i;
                expr.Arguments.Add(arg);
            }

            expr.Arguments.Sort((a, b) => a.Index - b.Index);

            if (expr.Arguments.Count < parameters.Count)
                return false;

            return true;
        }

        private AstExpression InferGenericFunctionCall(GenericFunctionType func, AstCallExpr expr, CheezType expected, List<AstFunctionDecl> newInstances)
        {
            var decl = func.Declaration;

            if (!CheckAndMatchArgsToParams(decl, expr, decl.Parameters, false))
                return expr;

            // match arguments and parameter types
            var pairs = expr.Arguments.Select(arg => (arg.Index < decl.Parameters.Count ? decl.Parameters[arg.Index] : null, arg));
            (AstParameter param, AstArgument arg)[] args = pairs.ToArray();

            // infer types of arguments
            foreach (var (param, arg) in args)
            {
                arg.Scope = expr.Scope;
                arg.Expr.Scope = arg.Scope;

                arg.Expr = InferTypeHelper(arg.Expr, null, newInstances);
                ConvertLiteralTypeToDefaultType(arg.Expr);
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
                        if (func.Declaration.RefSelf)
                        {
                            selfType = ((PointerType)selfType).TargetType;
                            CollectPolyTypes(func.Declaration.ImplBlock.TargetType, selfType, polyTypes);
                        }
                        else
                        {
                            CollectPolyTypes(func.Declaration.ImplBlock.TargetType, selfType, polyTypes);
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
            
            // find or create instance
            var instance = InstantiatePolyFunction(func, polyTypes, constArgs, newInstances, expr);

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
                if (a.Type != p.Type && !a.Type.IsErrorType)
                {
                    ReportError(a, $"Type of argument ({a.Type}) does not match type of parameter ({p.Type})");
                }
            }

            expr.Declaration = instance;
            expr.Type = instance.FunctionType.ReturnType;
            expr.SetFlag(ExprFlags.IsLValue, instance.FunctionType.ReturnType is PointerType);

            return expr;
        }

        private AstExpression InferRegularFunctionCall(FunctionType func, AstCallExpr expr, CheezType expected, List<AstFunctionDecl> newInstances)
        {

            if (!CheckAndMatchArgsToParams(func.Declaration, expr, func.Declaration.Parameters, func.VarArgs))
                return expr;

            // match arguments and parameter types
            var pairs = expr.Arguments.Select(arg => (arg.Index < func.Parameters.Length ? func.Parameters[arg.Index].type : null, arg));
            (CheezType type, AstArgument arg)[] args = pairs.ToArray();
            foreach (var (type, arg) in args)
            {
                arg.Scope = expr.Scope;
                arg.Expr.Scope = arg.Scope;
                arg.Expr = InferTypeHelper(arg.Expr, type, newInstances);
                ConvertLiteralTypeToDefaultType(arg.Expr, type);
                arg.Type = arg.Expr.Type;

                if ((!func.VarArgs || arg.Index < func.Parameters.Length) && arg.Type != type && !arg.Type.IsErrorType)
                {
                    ReportError(arg, $"Type of argument ({arg.Type}) does not match type of parameter ({type})");
                }
            }

            // :hack
            expr.SetFlag(ExprFlags.IsLValue, func.ReturnType is PointerType);
            expr.Type = func.ReturnType;
            expr.Declaration = func.Declaration;

            return expr;
        }


        private AstExpression InferTypeStructValueExpr(AstStructValueExpr expr, CheezType expected, List<AstFunctionDecl> newInstances)
        {
            if (expr.TypeExpr != null)
            {
                expr.TypeExpr.Scope = expr.Scope;
                expr.TypeExpr.Type = ResolveType(expr.TypeExpr);
                expr.Type = expr.TypeExpr.Type;
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
                ReportError(expr.TypeExpr, $"This expression is not a struct but a '{expr.Type}'");
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

            if (namesProvided == 0)
            {
                for (int i = 0; i < expr.MemberInitializers.Count; i++)
                {
                    var mi = expr.MemberInitializers[i];
                    var mem = type.Declaration.Members[i];

                    mi.Value.Scope = expr.Scope;
                    mi.Value = InferTypeHelper(mi.Value, mem.Type, newInstances);
                    ConvertLiteralTypeToDefaultType(mi.Value);

                    mi.Name = new AstIdExpr(mem.Name.Name, false, mi.Value);
                    mi.Index = i;

                    if (mi.Value.Type.IsErrorType) continue;
                    if (mi.Value.Type != mem.Type)
                    {
                        ReportError(mi.Value, $"Can't convert a value of type {mi.Value.Type} to type {mem.Type}");
                    }
                }
            }
            else if (namesProvided == expr.MemberInitializers.Count)
            {
                for (int i = 0; i < expr.MemberInitializers.Count; i++)
                {
                    var mi = expr.MemberInitializers[i];
                    var memIndex = type.Declaration.Members.FindIndex(m => m.Name.Name == mi.Name.Name);

                    if (memIndex < 0)
                    {
                        ReportError(mi.Name, $"Struct '{type}' has no member '{mi.Name.Name}'");
                        continue;
                    }

                    var mem = type.Declaration.Members[memIndex];
                    mi.Index = memIndex;

                    mi.Value.Scope = expr.Scope;
                    mi.Value = InferTypeHelper(mi.Value, mem.Type, newInstances);
                    ConvertLiteralTypeToDefaultType(mi.Value);

                    if (mi.Value.Type.IsErrorType) continue;
                    if (mi.Value.Type != mem.Type)
                    {
                        ReportError(mi.Value, $"Can't convert a value of type {mi.Value.Type} to type {mem.Type}");
                    }
                }
            }
            else
            {
                ReportError(expr, $"Either all or no values must have a name");
            }

            return expr;
        }

        private AstExpression InferTypeUnaryExpr(AstUnaryExpr expr, CheezType expected, List<AstFunctionDecl> newInstances)
        {
            expr.SubExpr.Scope = expr.Scope;

            expr.SubExpr = InferTypeHelper(expr.SubExpr, null, newInstances);

            if (expr.SubExpr.Type.IsErrorType)
                return expr;
            
            if (expr.SubExpr.Type is AbstractType at1)
            {
                expr.Type = expr.SubExpr.Type;
            }
            else
            {
                var ops = expr.Scope.GetOperators(expr.Operator, expr.SubExpr.Type);

                if (ops.Count == 0)
                {
                    ReportError(expr, $"No operator '{expr.Operator}' matches the type {expr.SubExpr.Type}");
                    return expr;
                }
                else if (ops.Count > 1)
                {
                    // TODO: show matching operators
                    ReportError(expr, $"Multiple operators '{expr.Operator}' match the type {expr.SubExpr.Type}");
                    return expr;
                }

                var op = ops[0];
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

                if (expr.SubExpr.Value != null)
                    expr.Value = op.Execute(expr.SubExpr.Value);

                // @hack
                expr.Type = op.ResultType;
            }

            return expr;
        }

        private AstExpression InferTypesBinaryExpr(AstBinaryExpr expr, CheezType expected, List<AstFunctionDecl> newInstances)
        {
            expr.Left.Scope = expr.Scope;
            expr.Right.Scope = expr.Scope;

            expr.Left = InferTypeHelper(expr.Left, null, newInstances);
            expr.Right = InferTypeHelper(expr.Right, null, newInstances);

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

                var ops = expr.Scope.GetOperators(expr.Operator, expr.Left.Type, expr.Right.Type);

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
                if (op is UserDefinedBinaryOperator user)
                {
                    var args = new List<AstArgument>() {
                        new AstArgument(expr.Left, Location: expr.Left.Location),
                        new AstArgument(expr.Right, Location: expr.Right.Location)
                    };
                    var func = new AstSymbolExpr(user.Declaration);
                    var call = new AstCallExpr(func, args, expr.Location);
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

        private AstExpression InferTypesIdExpr(AstIdExpr expr, CheezType expected)
        {
            var sym = expr.Scope.GetSymbol(expr.Name);
            if (sym == null)
            {
                ReportError(expr, $"Unknown symbol '{expr.Name}'");
                return expr;
            }

            expr.Symbol = sym;
            expr.SetFlag(ExprFlags.IsLValue, true);

            if (sym is AstSingleVariableDecl var)
            {
                expr.Type = var.Type;
            }
            else if (sym is AstParameter p)
            {
                expr.Type = p.Type;
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
            else if (sym is AstTypeAliasDecl typedef)
            {
                expr.Type = CheezType.Type;
                expr.Value = typedef.Type;
            }
            else if (sym is AstFunctionDecl func)
            {
                expr.Type = func.Type;
            }
            else if (sym is AstSingleVariableDecl decl)
            {
                expr.Type = decl.Type;
            }
            else if (sym is ConstSymbol c)
            {
                expr.Type = c.Type;
                expr.Value = c.Value;
            }
            else if (sym is Using u)
            {
                // TODO:
                expr.Type = u.Type;
                //throw new NotImplementedException();
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

        private AstExpression Cast(AstExpression expr, CheezType to)
        {
            var from = expr.Type;

            if (from == to)
                return expr;

            // TODO: only do this for implicit casts
            return InferType(new AstCastExpr(new AstTypeRef(to), expr, expr.Location), to);
        }
    }
}
