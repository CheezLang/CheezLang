using System;
using System.Collections.Generic;
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
        private void ConvertLiteralTypeToDefaultType(AstExpression expr)
        {
            if (expr.Type == IntType.LiteralType) expr.Type = IntType.DefaultType;
            else if (expr.Type == FloatType.LiteralType) expr.Type = FloatType.DefaultType;
            else if (expr.Type == CheezType.StringLiteral) expr.Type = CheezType.String;
        }

        private bool InferTypes(AstExpression expr, CheezType expected, HashSet<AstSingleVariableDecl> unresolvedDependencies = null, HashSet<AstSingleVariableDecl> allDependencies = null, Dictionary<string, CheezType> polyTypeMap = null)
        {
            var previousType = expr.Type;

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
                    InferTypesBinaryExpr(b, expected, unresolvedDependencies, allDependencies);
                    break;

                case AstStructValueExpr s:
                    InferTypeStructValueExpr(s, expected, unresolvedDependencies, allDependencies);
                    break;

                case AstUnaryExpr u:
                    InferTypeUnaryExpr(u, expected, unresolvedDependencies, allDependencies);
                    break;

                case AstCallExpr c:
                    InferTypeCallExpr(c, expected, unresolvedDependencies, allDependencies);
                    break;

                case AstTupleExpr t:
                    InferTypeTupleExpr(t, expected, unresolvedDependencies, allDependencies);
                    break;

                case AstDotExpr d:
                    InferTypeDotExpr(d, expected, unresolvedDependencies, allDependencies);
                    break;

                case AstArrayAccessExpr d:
                    InferTypeIndexExpr(d, expected, unresolvedDependencies, allDependencies);
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

        private void InferTypeIndexExpr(AstArrayAccessExpr expr, CheezType expected, HashSet<AstSingleVariableDecl> unresolvedDependencies, HashSet<AstSingleVariableDecl> allDependencies)
        {
            expr.SubExpression.Scope = expr.Scope;
            InferTypes(expr.SubExpression, null, unresolvedDependencies, allDependencies);

            expr.Indexer.Scope = expr.Scope;
            InferTypes(expr.Indexer, null, unresolvedDependencies, allDependencies);

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

        private void InferTypeDotExpr(AstDotExpr expr, CheezType expected, HashSet<AstSingleVariableDecl> unresolvedDependencies, HashSet<AstSingleVariableDecl> allDependencies)
        {
            expr.Left.Scope = expr.Scope;
            InferTypes(expr.Left, null, unresolvedDependencies, allDependencies);

            if (expr.Left.Type is ErrorType)
                return;

            var sub = expr.Right.Name;
            switch (expr.Left.Type)
            {

                default: throw new NotImplementedException();
            }
        }

        private void InferTypeTupleExpr(AstTupleExpr expr, CheezType expected, HashSet<AstSingleVariableDecl> unresolvedDependencies, HashSet<AstSingleVariableDecl> allDependencies)
        {
            TupleType tupleType = expected as TupleType;
            if (tupleType?.Members?.Length != expr.Values.Count) tupleType = null;

            var members = new (string, CheezType type)[expr.Values.Count];
            for (int i = 0; i < expr.Values.Count; i++)
            {
                var v = expr.Values[i];
                v.Scope = expr.Scope;

                var e = tupleType?.Members[i].type;
                InferTypes(v, e, unresolvedDependencies, allDependencies);

                // TODO: do somewhere else
                ConvertLiteralTypeToDefaultType(v);

                members[i].type = v.Type;
            }

            expr.Type = TupleType.GetTuple(members);
        }

        private void InferTypeCallExpr(AstCallExpr expr, CheezType expected, HashSet<AstSingleVariableDecl> unresolvedDependencies, HashSet<AstSingleVariableDecl> allDependencies)
        {
            expr.Function.Scope = expr.Scope;
            InferTypes(expr.Function, null, unresolvedDependencies, allDependencies);

            switch (expr.Function.Type)
            {
                case FunctionType f:
                    {
                        InferRegularFunctionCall(f, expr, expected, unresolvedDependencies, allDependencies);
                        break;
                    }

                case GenericFunctionType g:
                    {
                        InferGenericFunctionCall(g, expr, expected, unresolvedDependencies, allDependencies);
                        break;
                    }

                case ErrorType _: return;
                default: throw new NotImplementedException();
            }
        }

        private bool CheckAndMatchArgsToParams(AstCallExpr expr, (string name, CheezType type)[] parameters)
        {
            if (expr.Arguments.Count > parameters.Length)
            {
                (string, ILocation)? detail = null;
                if (expr.Function is AstIdExpr id)
                {
                    ILocation loc = id.Symbol.Location;
                    if (id.Symbol is CompTimeVariable ct && ct.Declaration is AstFunctionDecl fd)
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

                    var param = parameters[i];
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
                ReportError(expr, $"Not enough arguments");
                return false;
            }

            return true;
        }

        private void InferGenericFunctionCall(GenericFunctionType func, AstCallExpr expr, CheezType expected, HashSet<AstSingleVariableDecl> unresolvedDependencies, HashSet<AstSingleVariableDecl> allDependencies)
        {
            if (!CheckAndMatchArgsToParams(expr, func.Parameters))
                return;

            expr.Arguments.Sort((a, b) => a.Index - b.Index);

            // TODO: types and such
            var pairs = expr.Arguments.Select(arg => (func.Parameters[arg.Index].type, arg));
            // TODO: return type
            (CheezType type, AstArgument arg)[] args = pairs.ToArray();

            // match arguments and parameter types
            var polyTypeMap = new Dictionary<string, CheezType>();

            while (true)
            {
                bool changes = false;

                foreach (var (type, arg) in args)
                {
                    arg.Scope = expr.Scope;
                    arg.Expr.Scope = arg.Scope;

                    changes |= InferTypes(arg.Expr, type, unresolvedDependencies, allDependencies, polyTypeMap);

                    arg.Type = arg.Expr.Type;
                    // TODO: check types
                }

                if (!changes)
                    break;
            }

            // find or create instance
            var instance = InstantiatePolyFunction(polyTypeMap, func);

            expr.Declaration = instance;
            expr.SetFlag(ExprFlags.IsLValue, instance.FunctionType.ReturnType is PointerType);
        }

        private void InferRegularFunctionCall(FunctionType func, AstCallExpr expr, CheezType expected, HashSet<AstSingleVariableDecl> unresolvedDependencies, HashSet<AstSingleVariableDecl> allDependencies)
        {
            expr.Type = func.ReturnType;
            if (!CheckAndMatchArgsToParams(expr, func.Parameters))
                return;

            expr.Arguments.Sort((a, b) => a.Index - b.Index);

            var pairs = expr.Arguments.Select(arg => (func.Parameters[arg.Index].type, arg));
            // TODO: return type
            (CheezType type, AstArgument arg)[] args = pairs.ToArray();

            // match arguments and parameter types
            foreach (var (type, arg) in args)
            {
                arg.Scope = expr.Scope;
                arg.Expr.Scope = arg.Scope;
                InferTypes(arg.Expr, type, unresolvedDependencies, allDependencies);
                arg.Type = arg.Expr.Type;
                // TODO: check types
            }

            // :hack
            expr.SetFlag(ExprFlags.IsLValue, func.ReturnType is PointerType);
        }

        private void InferTypeUnaryExpr(AstUnaryExpr expr, CheezType expected, HashSet<AstSingleVariableDecl> unresolvedDependencies, HashSet<AstSingleVariableDecl> allDependencies)
        {
            throw new NotImplementedException();
        }

        private void InferTypeStructValueExpr(AstStructValueExpr expr, CheezType expected, HashSet<AstSingleVariableDecl> unresolvedDependencies, HashSet<AstSingleVariableDecl> allDependencies)
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
                    InferTypes(mi.Value, mem.Type, unresolvedDependencies, allDependencies);
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
                    InferTypes(mi.Value, mem.Type, unresolvedDependencies, allDependencies);
                    ConvertLiteralTypeToDefaultType(mi.Value);

                    // TODO: check types match
                }
            }
            else
            {
                ReportError(expr, $"Either all or no values must have a name");
            }
        }

        private void InferTypesBinaryExpr(AstBinaryExpr b, CheezType expected, HashSet<AstSingleVariableDecl> unresolvedDependencies, HashSet<AstSingleVariableDecl> allDependencies)
        {
            b.Left.Scope = b.Scope;
            b.Right.Scope = b.Scope;

            InferTypes(b.Left, null, unresolvedDependencies, allDependencies);
            InferTypes(b.Right, null, unresolvedDependencies, allDependencies);

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
            else if (sym is CompTimeVariable ct)
            {
                i.Type = ct.Value as CheezType;
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
    }
}
