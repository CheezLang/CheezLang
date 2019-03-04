﻿using System;
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
        private void ConvertLiteralTypeToDefaultType(AstExpression expr, CheezType expected = null)
        {
            if (expr.Type == IntType.LiteralType)
            {
                if (expected != null && !(expected is IntType)) throw new Exception("Can't convert int to non-int type");
                expr.Type = expected ?? IntType.DefaultType;
            }
            else if (expr.Type == FloatType.LiteralType)
            {
                if (expected != null && !(expected is FloatType)) throw new Exception("Can't convert float to non-float type");
                expr.Type = expected ?? FloatType.DefaultType;
            }
            else if (expr.Type == CheezType.StringLiteral) expr.Type = CheezType.CString; // TODO: change default back to CheezType.String
        }

        private bool InferType(AstExpression expr, CheezType expected, HashSet<AstSingleVariableDecl> unresolvedDependencies = null, HashSet<AstSingleVariableDecl> allDependencies = null, Dictionary<string, CheezType> polyTypeMap = null)
        {
            List<AstFunctionDecl> newInstances = new List<AstFunctionDecl>();
            bool changes = InferTypes(expr, expected, unresolvedDependencies, allDependencies, newInstances, polyTypeMap);

            if (newInstances.Count > 0)
            {
                AnalyzeFunctions(newInstances);
            }

            return changes;
        }

        private bool InferTypes(AstExpression expr, CheezType expected, HashSet<AstSingleVariableDecl> unresolvedDependencies, HashSet<AstSingleVariableDecl> allDependencies, List<AstFunctionDecl> newInstances, Dictionary<string, CheezType> polyTypeMap = null)
        {
            var previousType = expr.Type;
            expr.Type = CheezType.Error;

            if (expected is PolyType pt && polyTypeMap.TryGetValue(pt.Name, out var concreteType))
            {
                expected = concreteType;
            }

            switch (expr)
            {
                case AstBoolExpr b:
                    b.Type = CheezType.Bool;
                    b.Value = b.BoolValue;
                    break;

                case AstNumberExpr n:
                    InferTypesNumberExpr(n, expected);
                    break;

                case AstStringLiteral s:
                    InferTypesStringLiteral(s, expected);
                    break;

                case AstCharLiteral ch:
                    InferTypesCharLiteral(ch, expected);
                    break;

                case AstIdExpr i:
                    InferTypesIdExpr(i, expected, unresolvedDependencies, allDependencies);
                    break;

                case AstBinaryExpr b:
                    InferTypesBinaryExpr(b, expected, unresolvedDependencies, allDependencies, newInstances);
                    break;

                case AstStructValueExpr s:
                    InferTypeStructValueExpr(s, expected, unresolvedDependencies, allDependencies, newInstances);
                    break;

                case AstUnaryExpr u:
                    InferTypeUnaryExpr(u, expected, unresolvedDependencies, allDependencies, newInstances);
                    break;

                case AstCallExpr c:
                    InferTypeCallExpr(c, expected, unresolvedDependencies, allDependencies, newInstances);
                    break;

                case AstTupleExpr t:
                    InferTypeTupleExpr(t, expected, unresolvedDependencies, allDependencies, newInstances);
                    break;

                case AstDotExpr d:
                    InferTypeDotExpr(d, expected, unresolvedDependencies, allDependencies, newInstances);
                    break;

                case AstArrayAccessExpr d:
                    InferTypeIndexExpr(d, expected, unresolvedDependencies, allDependencies, newInstances);
                    break;

                default:
                    throw new NotImplementedException();
            }

            if (expected is PolyType p && p.IsDeclaring)
            {
                polyTypeMap[p.Name] = expr.Type;
            }

            return previousType != expr.Type;
        }

        private void InferTypeIndexExpr(AstArrayAccessExpr expr, CheezType expected, HashSet<AstSingleVariableDecl> unresolvedDependencies, HashSet<AstSingleVariableDecl> allDependencies, List<AstFunctionDecl> newInstances)
        {
            expr.SubExpression.Scope = expr.Scope;
            InferTypes(expr.SubExpression, null, unresolvedDependencies, allDependencies, newInstances);

            expr.Indexer.Scope = expr.Scope;
            InferTypes(expr.Indexer, null, unresolvedDependencies, allDependencies, newInstances);

            if ((unresolvedDependencies?.Count ?? 0) != 0)
            {
                return;
            }

            ConvertLiteralTypeToDefaultType(expr.Indexer);

            if (expr.SubExpression.Type is ErrorType || expr.Indexer.Type is ErrorType)
                return;

            switch (expr.SubExpression.Type)
            {
                case TupleType tuple:
                    {
                        if (!(expr.Indexer.Type is IntType) || expr.Indexer.Value == null)
                        {
                            ReportError(expr.Indexer, $"The index must be a constant int");
                            return;
                        }

                        var index = ((NumberData)expr.Indexer.Value).ToLong();
                        if (index < 0 || index >= tuple.Members.Length)
                        {
                            ReportError(expr.Indexer, $"The index is out of range");
                            return;
                        }

                        expr.Type = tuple.Members[index].type;
                        break;
                    }

                default: throw new NotImplementedException();
            }
        }

        private void InferTypeDotExpr(AstDotExpr expr, CheezType expected, HashSet<AstSingleVariableDecl> unresolvedDependencies, HashSet<AstSingleVariableDecl> allDependencies, List<AstFunctionDecl> newInstances)
        {
            expr.Left.Scope = expr.Scope;
            InferTypes(expr.Left, null, unresolvedDependencies, allDependencies, newInstances);

            if (expr.Left.Type is ErrorType)
                return;

            var sub = expr.Right.Name;
            switch (expr.Left.Type)
            {

                default: throw new NotImplementedException();
            }
        }

        private void InferTypeTupleExpr(AstTupleExpr expr, CheezType expected, HashSet<AstSingleVariableDecl> unresolvedDependencies, HashSet<AstSingleVariableDecl> allDependencies, List<AstFunctionDecl> newInstances)
        {
            TupleType tupleType = expected as TupleType;
            if (tupleType?.Members?.Length != expr.Values.Count) tupleType = null;

            var members = new (string, CheezType type)[expr.Values.Count];
            for (int i = 0; i < expr.Values.Count; i++)
            {
                var v = expr.Values[i];
                v.Scope = expr.Scope;

                var e = tupleType?.Members[i].type;
                InferTypes(v, e, unresolvedDependencies, allDependencies, newInstances);

                // TODO: do somewhere else
                ConvertLiteralTypeToDefaultType(v);

                members[i].type = v.Type;
            }

            expr.Type = TupleType.GetTuple(members);
        }

        private void InferTypeCallExpr(AstCallExpr expr, CheezType expected, HashSet<AstSingleVariableDecl> unresolvedDependencies, HashSet<AstSingleVariableDecl> allDependencies, List<AstFunctionDecl> newInstances)
        {
            expr.Function.Scope = expr.Scope;
            InferTypes(expr.Function, null, unresolvedDependencies, allDependencies, newInstances);

            switch (expr.Function.Type)
            {
                case FunctionType f:
                    {
                        InferRegularFunctionCall(f, expr, expected, unresolvedDependencies, allDependencies, newInstances);
                        break;
                    }

                case GenericFunctionType g:
                    {
                        InferGenericFunctionCall(g, expr, expected, unresolvedDependencies, allDependencies, newInstances);
                        break;
                    }

                case ConstParamFunctionType c:
                    {
                        InferConstParamFunctionCall(c, expr, expected, unresolvedDependencies, allDependencies, newInstances);
                        break;
                    }

                case ErrorType _: return;
                default: throw new NotImplementedException();
            }
        }

        private void InferConstParamFunctionCall(ConstParamFunctionType func, AstCallExpr expr, CheezType expected, HashSet<AstSingleVariableDecl> unresolvedDependencies, HashSet<AstSingleVariableDecl> allDependencies, List<AstFunctionDecl> newInstances)
        {
            if (!CheckAndMatchArgsToParams(expr, func.Parameters, false))
                return;

            expr.Arguments.Sort((a, b) => a.Index - b.Index);

            if (expr.Arguments.Count != func.Declaration.Parameters.Count) throw new NotImplementedException();

            var constArgs = new Dictionary<string, (CheezType, object)>();
            var newArgs = new List<AstArgument>();

            for (int i = 0; i < expr.Arguments.Count; i++)
            {
                var a = expr.Arguments[i];
                var p = func.Declaration.Parameters[i];

                if (p.Name?.IsPolymorphic ?? false)
                {
                    Debug.Assert(p.Type != null);

                    a.Scope = expr.Scope;
                    a.Expr.Scope = a.Scope;

                    InferTypes(a.Expr, p.Type, unresolvedDependencies, allDependencies, newInstances);
                    ConvertLiteralTypeToDefaultType(a.Expr);
                    a.Type = a.Expr.Type;

                    if (a.Expr.Value == null)
                    {
                        ReportError(a, $"Argument must be a constant {p.Type}");
                    }

                    constArgs[p.Name.Name] = (p.Type, a.Expr.Value);
                }
                else
                {
                    newArgs.Add(a);
                }
            }

            expr.Arguments = newArgs;

            var instance = InstantiateConstParamFunction(constArgs, func, newInstances);

            switch (instance.Type)
            {
                case FunctionType f:
                        InferRegularFunctionCall(f, expr, expected, unresolvedDependencies, allDependencies, newInstances);
                        break;

                case GenericFunctionType g:
                        InferGenericFunctionCall(g, expr, expected, unresolvedDependencies, allDependencies, newInstances);
                        break;

                case ErrorType _: return;
                default: throw new NotImplementedException();
            }
        }

        private bool CheckAndMatchArgsToParams(AstCallExpr expr, (string name, CheezType type)[] parameters, bool varArgs)
        {
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
                    var index = parameters.IndexOf(p => p.name == arg.Name.Name);
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

            // TODO: create missing arguments

            //
            if (map.Count < parameters.Length)
            {
                // TODO: report missing arguments
                ReportError(expr, $"Not enough arguments");
                return false;
            }

            return true;
        }

        private void InferGenericFunctionCall(GenericFunctionType func, AstCallExpr expr, CheezType expected, HashSet<AstSingleVariableDecl> unresolvedDependencies, HashSet<AstSingleVariableDecl> allDependencies, List<AstFunctionDecl> newInstances)
        {
            if (!CheckAndMatchArgsToParams(expr, func.Parameters, false))
                return;

            expr.Arguments.Sort((a, b) => a.Index - b.Index);

            // match arguments and parameter types
            var pairs = expr.Arguments.Select(arg => (arg.Index < func.Parameters.Length ? func.Parameters[arg.Index].type : null, arg));
            (CheezType type, AstArgument arg)[] args = pairs.ToArray();
            var polyTypeMap = new Dictionary<string, CheezType>();
            foreach (var (type, arg) in args)
            {
                arg.Scope = expr.Scope;
                arg.Expr.Scope = arg.Scope;

                var expectedType = type;
                if (expectedType.IsPolyType) expectedType = null;

                InferTypes(arg.Expr, expectedType, unresolvedDependencies, allDependencies, newInstances);
                ConvertLiteralTypeToDefaultType(arg.Expr);
                arg.Type = arg.Expr.Type;

                if (type.IsPolyType) ExtractPolyTypes(arg.Expr, type, polyTypeMap);
            }


            // find or create instance
            var instance = InstantiatePolyFunction(polyTypeMap, func, newInstances);

            expr.Declaration = instance;
            expr.SetFlag(ExprFlags.IsLValue, instance.FunctionType.ReturnType is PointerType);
        }

        private void InferRegularFunctionCall(FunctionType func, AstCallExpr expr, CheezType expected, HashSet<AstSingleVariableDecl> unresolvedDependencies, HashSet<AstSingleVariableDecl> allDependencies, List<AstFunctionDecl> newInstances)
        {
            if (!CheckAndMatchArgsToParams(expr, func.Parameters, func.VarArgs))
                return;

            expr.Arguments.Sort((a, b) => a.Index - b.Index);

            // match arguments and parameter types
            var pairs = expr.Arguments.Select(arg => (arg.Index < func.Parameters.Length ? func.Parameters[arg.Index].type : null, arg));
            (CheezType type, AstArgument arg)[] args = pairs.ToArray();
            foreach (var (type, arg) in args)
            {
                arg.Scope = expr.Scope;
                arg.Expr.Scope = arg.Scope;
                InferTypes(arg.Expr, type, unresolvedDependencies, allDependencies, newInstances);
                ConvertLiteralTypeToDefaultType(arg.Expr);
                arg.Type = arg.Expr.Type;

                // TODO: check types
            }

            // :hack
            expr.SetFlag(ExprFlags.IsLValue, func.ReturnType is PointerType);
            expr.Type = func.ReturnType;
            expr.Declaration = func.Declaration;
        }

        private void InferTypeUnaryExpr(AstUnaryExpr expr, CheezType expected, HashSet<AstSingleVariableDecl> unresolvedDependencies, HashSet<AstSingleVariableDecl> allDependencies, List<AstFunctionDecl> newInstances)
        {
            // TODO: return changes
            InferType(expr.SubExpr, null);

            // unary minus with constant number
            if (expr.Operator == "-")
            {
                if (expr.SubExpr.Type is IntType || expr.SubExpr.Type is FloatType)
                {
                    ConvertLiteralTypeToDefaultType(expr.SubExpr, expected);
                    expr.Type = expr.SubExpr.Type;
                    expr.Value = ((NumberData)expr.SubExpr.Value).Negate();
                }
            }
        }

        private void InferTypeStructValueExpr(AstStructValueExpr expr, CheezType expected, HashSet<AstSingleVariableDecl> unresolvedDependencies, HashSet<AstSingleVariableDecl> allDependencies, List<AstFunctionDecl> newInstances)
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
                ReportError(expr, $"Failed to infer type for expression");
                expr.Type = CheezType.Error;
                return;
            }
            else if (expr.Type == CheezType.Error)
            {
                return;
            }

            var type = expr.Type as StructType;
            if (type == null)
            {
                ReportError(expr.TypeExpr, $"This expression is not a struct but a '{expr.Type}'");
                return;
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
                    InferTypes(mi.Value, mem.Type, unresolvedDependencies, allDependencies, newInstances);
                    ConvertLiteralTypeToDefaultType(mi.Value);

                    mi.Name = new AstIdExpr(mem.Name.Name, false, mi.Value);
                    mi.Index = i;

                    // TODO: check types match
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
                    InferTypes(mi.Value, mem.Type, unresolvedDependencies, allDependencies, newInstances);
                    ConvertLiteralTypeToDefaultType(mi.Value);

                    // TODO: check types match
                }
            }
            else
            {
                ReportError(expr, $"Either all or no values must have a name");
            }
        }

        private void InferTypesBinaryExpr(AstBinaryExpr b, CheezType expected, HashSet<AstSingleVariableDecl> unresolvedDependencies, HashSet<AstSingleVariableDecl> allDependencies, List<AstFunctionDecl> newInstances)
        {
            b.Left.Scope = b.Scope;
            b.Right.Scope = b.Scope;

            InferTypes(b.Left, null, unresolvedDependencies, allDependencies, newInstances);
            InferTypes(b.Right, null, unresolvedDependencies, allDependencies, newInstances);

            var at = new List<AbstractType>();
            if (b.Left.Type is AbstractType at1) at.Add(at1);
            if (b.Right.Type is AbstractType at2) at.Add(at2);
            if (at.Count > 0)
            {
                b.Type = new CombiType(at);
            }
            else
            {
                // TODO: find matching operator

                // @hack
                b.Type = expected;
            }
        }

        private void InferTypesIdExpr(AstIdExpr i, CheezType expected, HashSet<AstSingleVariableDecl> unresolvedDependencies, HashSet<AstSingleVariableDecl> allDependencies)
        {
            var sym = i.Scope.GetSymbol(i.Name);
            if (sym == null)
            {
                ReportError(i, $"Unknown symbol '{i.Name}'");
                i.Type = CheezType.Error;
                return;
            }

            i.Symbol = sym;
            i.SetFlag(ExprFlags.IsLValue, true);

            if (sym is AstSingleVariableDecl var)
            {
                i.Type = var.Type;
                if (i.Type is AbstractType)
                    unresolvedDependencies?.Add(var);

                allDependencies?.Add(var);
            }
            else if (sym is AstParameter p)
            {
                i.Type = p.Type;
            }
            else if (sym is TypeSymbol ct)
            {
                i.Type = CheezType.Type;
                i.Value = ct.Type;
            }
            else if (sym is AstDecl decl)
            {
                i.Type = decl.Type;
            }
            else if (sym is ConstSymbol c)
            {
                i.Type = c.Type;
                i.Value = c.Value;
            }
            else
            {
                ReportError(i, $"'{i.Name}' is not a valid variable");
            }
        }

        private void InferTypesCharLiteral(AstCharLiteral s, CheezType expected)
        {
            s.Type = CheezType.Char;
            s.CharValue = s.RawValue[0];
            s.Value = s.CharValue;
        }

        private void InferTypesStringLiteral(AstStringLiteral s, CheezType expected)
        {
            if (expected == CheezType.String || expected == CheezType.CString) s.Type = expected;
            else s.Type = CheezType.StringLiteral;
        }

        private void InferTypesNumberExpr(AstNumberExpr expr, CheezType expected)
        {
            if (expr.Data.Type == NumberData.NumberType.Int)
            {
                if (expected != null && (expected is IntType || expected is FloatType)) expr.Type = expected;
                else expr.Type = IntType.LiteralType;
                expr.Value = expr.Data;
            }
            else
            {
                if (expected != null && expected is FloatType) expr.Type = expected;
                else expr.Type = FloatType.LiteralType;
                expr.Value = expr.Data;
            }
        }

        private void ExtractPolyTypes(AstExpression expr, CheezType type, Dictionary<string, CheezType> map)
        {
            switch (type)
            {
                case PolyType p:
                    if (map.TryGetValue(p.Name, out var current))
                    {
                        if (expr.Type != current)
                        {
                            ReportError(expr, $"This expression has type '{expr.Type}' which doesn't match '${p.Name}' of type '{current}'");
                        }
                    }
                    else
                    {
                        map[p.Name] = expr.Type;
                    }
                    break;

                default: throw new NotImplementedException();
            }
        }
    }
}