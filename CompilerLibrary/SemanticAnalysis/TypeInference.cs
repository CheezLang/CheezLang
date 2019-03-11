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
                if (expected != null && !(expected is IntType)) throw new Exception("Can't convert int to non-int type");
                return expected ?? IntType.DefaultType;
            }
            else if (literalType == FloatType.LiteralType)
            {
                if (expected != null && !(expected is FloatType)) throw new Exception("Can't convert float to non-float type");
                return expected ?? FloatType.DefaultType;
            }
            else if (literalType == CheezType.StringLiteral) return CheezType.String;

            return literalType;
        }

        private void ConvertLiteralTypeToDefaultType(AstExpression expr, CheezType expected = null)
        {
            expr.Type = LiteralTypeToDefaultType(expr.Type, expected);
        }

        private void InferType(AstExpression expr, CheezType expected)
        {
            List<AstFunctionDecl> newInstances = new List<AstFunctionDecl>();
            InferTypeHelper(expr, expected, newInstances);

            if (newInstances.Count > 0)
                AnalyseFunctions(newInstances);
        }

        private void InferTypeHelper(AstExpression expr, CheezType expected, List<AstFunctionDecl> newInstances)
        {
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
                    else
                        n.Type = PointerType.GetPointerType(CheezType.Any);
                    break;

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
                    InferTypesIdExpr(i, expected);
                    break;

                case AstBinaryExpr b:
                    InferTypesBinaryExpr(b, expected, newInstances);
                    break;

                case AstStructValueExpr s:
                    InferTypeStructValueExpr(s, expected, newInstances);
                    break;

                case AstUnaryExpr u:
                    InferTypeUnaryExpr(u, expected, newInstances);
                    break;

                case AstCallExpr c:
                    InferTypeCallExpr(c, expected, newInstances);
                    break;

                case AstTupleExpr t:
                    InferTypeTupleExpr(t, expected, newInstances);
                    break;

                case AstDotExpr d:
                    InferTypeDotExpr(d, expected, newInstances);
                    break;

                case AstArrayAccessExpr d:
                    InferTypeIndexExpr(d, expected, newInstances);
                    break;

                case AstTempVarExpr d:
                    if (d.Expr.Type == null)
                        InferTypeHelper(d.Expr, expected, newInstances);
                    d.Type = d.Expr.Type;
                    break;

                case AstSymbolExpr s:
                    s.Type = s.Symbol.Type;
                    s.SetFlag(ExprFlags.IsLValue, true);
                    break;

                case AstBlockExpr b:
                    InferTypeBlock(b, expected, newInstances);
                    break;

                case AstIfExpr i:
                    InferTypeIfExpr(i, expected, newInstances);
                    break;

                case AstCompCallExpr c:
                    InferTypeCompCall(c, expected, newInstances);
                    break;

                case AstAddressOfExpr ao:
                    InferTypeAddressOf(ao, expected, newInstances);
                    break;

                case AstDereferenceExpr de:
                    InferTypeDeref(de, expected, newInstances);
                    break;

                case AstCastExpr cast:
                    InferTypeCast(cast, expected, newInstances);
                    break;

                default:
                    throw new NotImplementedException();
            }

            //if (expected is PolyType p && p.IsDeclaring)
            //{
            //    polyTypeMap[p.Name] = expr.Type;
            //}
        }

        private void InferTypeCast(AstCastExpr cast, CheezType expected, List<AstFunctionDecl> newInstances)
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
            InferTypeHelper(cast.SubExpression, cast.Type, newInstances);

            if (cast.SubExpression.Type.IsErrorType)
                return;

            // TODO: check if cast possible
            var to = cast.Type;
            var from = cast.SubExpression.Type;
            if ((to is PointerType && from is PointerType) ||
                (to is IntType && from is PointerType) ||
                (to is PointerType && from is IntType) ||
                (to is IntType && from is IntType) ||
                (to is FloatType && from is FloatType) ||
                (to is FloatType && from is IntType) ||
                (to is IntType && from is FloatType))
            {
                // ok
            }
            else
            {
                ReportError(cast, $"Can't convert from type {from} to type {to}");
            }
        }

        private void InferTypeDeref(AstDereferenceExpr de, CheezType expected, List<AstFunctionDecl> newInstances)
        {
            CheezType subExpect = null;
            if (expected != null) subExpect = PointerType.GetPointerType(expected);

            de.SubExpression.Scope = de.Scope;
            InferTypeHelper(de.SubExpression, subExpect, newInstances);

            if (de.SubExpression.Type is PointerType p)
            {
                de.Type = p.TargetType;
            }
            else if (!de.SubExpression.Type.IsErrorType)
            {
                ReportError(de, $"Can't dereference non pointer type {de.SubExpression.Type}");
            }
        }

        private void InferTypeIfExpr(AstIfExpr iff, CheezType expected, List<AstFunctionDecl> newInstances)
        {
            iff.SubScope = new Scope("if", iff.Scope);
            if (iff.PreAction != null)
            {
                iff.PreAction.Scope = iff.SubScope;
                iff.PreAction.Parent = iff;
                AnalyseVariableDecl(iff.PreAction);
            }

            iff.Condition.Scope = iff.SubScope;
            iff.Condition.Parent = iff;
            InferTypeHelper(iff.Condition, CheezType.Bool, newInstances);
            ConvertLiteralTypeToDefaultType(iff.Condition);

            if (iff.Condition.Type != CheezType.Bool && !(iff.Condition.Type is PointerType) && !iff.Condition.Type.IsErrorType)
            {
                ReportError(iff.Condition, $"Condition of if statement must be either a bool or a pointer but is {iff.Condition.Type}");
            }

            iff.IfCase.Scope = iff.SubScope;
            iff.IfCase.Parent = iff;
            InferTypeHelper(iff.IfCase, expected, newInstances);
            ConvertLiteralTypeToDefaultType(iff.IfCase, expected);

            if (iff.ElseCase != null)
            {
                iff.ElseCase.Scope = iff.SubScope;
                iff.ElseCase.Parent = iff;
                InferTypeHelper(iff.ElseCase, expected, newInstances);
                ConvertLiteralTypeToDefaultType(iff.ElseCase, expected);
                
                if (iff.IfCase.Type == iff.ElseCase.Type)
                {
                    iff.Type = iff.IfCase.Type;
                }
                else
                {
                    iff.Type = new SumType(iff.IfCase.Type, iff.ElseCase.Type);
                }

                if (iff.IfCase.GetFlag(ExprFlags.Returns) && iff.ElseCase.GetFlag(ExprFlags.Returns))
                {
                    iff.SetFlag(ExprFlags.Returns, true);
                }
            }
            else
            {
                iff.Type = CheezType.Void;
            }
        }

        private void InferTypeAddressOf(AstAddressOfExpr ao, CheezType expected, List<AstFunctionDecl> newInstances)
        {
            CheezType subExpected = null;
            if (expected is PointerType p)
            {
                subExpected = p.TargetType;
            }

            ao.SubExpression.Scope = ao.Scope;
            InferTypeHelper(ao.SubExpression, subExpected, newInstances);

            // check wether sub is an lvalue

            ao.Type = PointerType.GetPointerType(ao.SubExpression.Type);
        }

        private void InferTypeCompCall(AstCompCallExpr expr, CheezType expected, List<AstFunctionDecl> newInstances)
        {
            switch (expr.Name.Name)
            {
                case "tuple_type_member":
                    {
                        if (expr.Arguments.Count != 2)
                        {
                            ReportError(expr, $"@tuple_type_member requires two arguments (tuple type, int)");
                            return;
                        }

                        expr.Arguments[0].Scope = expr.Scope;
                        expr.Arguments[1].Scope = expr.Scope;
                        InferTypeHelper(expr.Arguments[0], CheezType.Type, newInstances);
                        InferTypeHelper(expr.Arguments[1], IntType.DefaultType, newInstances);

                        if (expr.Arguments[0].Type != CheezType.Type || !(expr.Arguments[0].Value is TupleType))
                        {
                            ReportError(expr.Arguments[0], $"This argument must be a tuple type, got {expr.Arguments[0].Type} '{expr.Arguments[0].Value}'");
                            return;
                        }
                        if (!(expr.Arguments[1].Type is IntType) || expr.Arguments[1].Value == null)
                        {
                            ReportError(expr.Arguments[1], $"This argument must be a constant int, got {expr.Arguments[1].Type} '{expr.Arguments[1].Value}'");
                            return;
                        }

                        var tuple = expr.Arguments[0].Value as TupleType;
                        var index = ((NumberData)expr.Arguments[1].Value).ToLong();

                        if (index < 0 || index >= tuple.Members.Length)
                        {
                            ReportError(expr.Arguments[1], $"Index '{index}' is out of range. Index must be between [0, {tuple.Members.Length})");
                            return;
                        }

                        expr.Type = CheezType.Type;
                        expr.Value = tuple.Members[index].type;

                        break;
                    }

                default: ReportError(expr.Name, $"Unknown intrinsic '{expr.Name.Name}'"); break;
            }
        }

        private void InferTypeBlock(AstBlockExpr block, CheezType expected, List<AstFunctionDecl> newInstances)
        {
            block.SubScope = new Scope("{}", block.Scope);

            int end = block.Statements.Count;
            if (block.Statements.LastOrDefault() is AstExprStmt) --end;

            for (int i = 0; i < end; i++)
            {
                var stmt = block.Statements[i];
                stmt.Scope = block.SubScope;
                stmt.Parent = block;
                AnalyseStatement(stmt);

                if (stmt.GetFlag(StmtFlags.Returns))
                    block.SetFlag(ExprFlags.Returns, true);
            }

            if (block.Statements.LastOrDefault() is AstExprStmt expr)
            {
                expr.Expr.Scope = block.SubScope;
                InferTypeHelper(expr.Expr, expected, newInstances);
                ConvertLiteralTypeToDefaultType(expr.Expr, expected);
                block.Type = expr.Expr.Type;

                AnalyseExprStatement(expr, true, false);
            }
            else
            {
                block.Type = CheezType.Void;
            }

            foreach (var symbol in block.SubScope.InitializedSymbols)
            {
                block.Scope.SetInitialized(symbol);
            }

        }

        private void InferTypeIndexExpr(AstArrayAccessExpr expr, CheezType expected, List<AstFunctionDecl> newInstances)
        {
            expr.SubExpression.Scope = expr.Scope;
            InferTypeHelper(expr.SubExpression, null, newInstances);

            expr.Indexer.Scope = expr.Scope;
            InferTypeHelper(expr.Indexer, null, newInstances);

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
                            ReportError(expr.Indexer, $"The index '{index}' is out of range. Index must be between [0, {tuple.Members.Length})");
                            return;
                        }

                        expr.Type = tuple.Members[index].type;
                        break;
                    }

                default:
                    // TODO: seach for overloaded operator
                    ReportError(expr.SubExpression, $"Type {expr.SubExpression.Type} has no operator []");
                    break;
            }
        }

        private void InferTypeDotExpr(AstDotExpr expr, CheezType expected, List<AstFunctionDecl> newInstances)
        {
            expr.Left.Scope = expr.Scope;
            InferTypeHelper(expr.Left, null, newInstances);

            if (expr.Left.Type.IsErrorType)
                return;

            var sub = expr.Right.Name;
            switch (expr.Left.Type)
            {
                case TupleType tuple:
                    {
                        var memName = expr.Right.Name;
                        var memberIndex = tuple.Members.IndexOf(m => m.name == memName);
                        if (memberIndex == -1)
                        {
                            ReportError(expr, $"The tuple '{tuple}' has no member '{memName}'");
                            return;
                        }

                        expr.Type = tuple.Members[memberIndex].type;
                        break;
                    }

                case SliceType slice:
                    {
                        var name = expr.Right.Name;
                        if (name == "data")
                        {
                            expr.Type = slice.ToPointerType();
                        }
                        else if (name == "length")
                        {
                            expr.Type = IntType.GetIntType(4, true);
                        }
                        else
                        {
                            // TODO: check for impl functions
                            ReportError(expr, $"No subscript '{name}' exists for slice type {slice}");
                        }
                        break;
                    }

            default: throw new NotImplementedException();
            }
        }

        private void InferTypeTupleExpr(AstTupleExpr expr, CheezType expected, List<AstFunctionDecl> newInstances)
        {
            TupleType tupleType = expected as TupleType;
            if (tupleType?.Members?.Length != expr.Values.Count) tupleType = null;

            var members = new (string, CheezType type)[expr.Values.Count];
            for (int i = 0; i < expr.Values.Count; i++)
            {
                var v = expr.Values[i];
                v.Scope = expr.Scope;

                var e = tupleType?.Members[i].type;
                InferTypeHelper(v, e, newInstances);

                // TODO: do somewhere else
                ConvertLiteralTypeToDefaultType(v);

                members[i].type = v.Type;
            }

            expr.Type = TupleType.GetTuple(members);
        }

        private void InferTypeCallExpr(AstCallExpr expr, CheezType expected, List<AstFunctionDecl> newInstances)
        {
            expr.Function.Scope = expr.Scope;
            InferTypeHelper(expr.Function, null, newInstances);

            switch (expr.Function.Type)
            {
                case FunctionType f:
                    {
                        InferRegularFunctionCall(f, expr, expected, newInstances);
                        break;
                    }

                case GenericFunctionType g:
                    {
                        InferGenericFunctionCall(g, expr, expected, newInstances);
                        break;
                    }

                case ErrorType _: return;
                default: throw new NotImplementedException();
            }
        }

        private bool CheckAndMatchArgsToParams(AstCallExpr expr, List<AstParameter> parameters, bool varArgs)
        {
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

        private void InferGenericFunctionCall(GenericFunctionType func, AstCallExpr expr, CheezType expected, List<AstFunctionDecl> newInstances)
        {
            var decl = func.Declaration;

            if (!CheckAndMatchArgsToParams(expr, decl.Parameters, false))
                return;

            // match arguments and parameter types
            var pairs = expr.Arguments.Select(arg => (arg.Index < decl.Parameters.Count ? decl.Parameters[arg.Index] : null, arg));
            (AstParameter param, AstArgument arg)[] args = pairs.ToArray();

            // infer types of arguments
            foreach (var (param, arg) in args)
            {
                arg.Scope = expr.Scope;
                arg.Expr.Scope = arg.Scope;

                InferTypeHelper(arg.Expr, null, newInstances);
                ConvertLiteralTypeToDefaultType(arg.Expr);
                arg.Type = arg.Expr.Type;
            }

            // collect polymorphic types and const arguments
            var polyTypes = new Dictionary<string, CheezType>();
            var constArgs = new Dictionary<string, (CheezType type, object value)>();
            var newArgs = new List<AstArgument>();
            foreach (var (param, arg) in args)
            {
                CollectPolyTypes(arg, param.TypeExpr, arg.Type, polyTypes);

                if (param.Name?.IsPolymorphic ?? false)
                {
                    if (arg.Expr.Value == null)
                    {
                        ReportError(arg, $"The expression must be a compile time constant");
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
            var instance = InstantiatePolyFunction(func, polyTypes, constArgs, newInstances);

            // check parameter types
            Debug.Assert(expr.Arguments.Count == instance.Parameters.Count);

            if (instance.Type.IsPolyType)
            {
                // error in function declaration
                expr.Type = CheezType.Error;
                return;
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
        }

        private void InferRegularFunctionCall(FunctionType func, AstCallExpr expr, CheezType expected, List<AstFunctionDecl> newInstances)
        {
            if (!CheckAndMatchArgsToParams(expr, func.Declaration.Parameters, func.VarArgs))
                return;

            // match arguments and parameter types
            var pairs = expr.Arguments.Select(arg => (arg.Index < func.Parameters.Length ? func.Parameters[arg.Index].type : null, arg));
            (CheezType type, AstArgument arg)[] args = pairs.ToArray();
            foreach (var (type, arg) in args)
            {
                arg.Scope = expr.Scope;
                arg.Expr.Scope = arg.Scope;
                InferTypeHelper(arg.Expr, type, newInstances);
                ConvertLiteralTypeToDefaultType(arg.Expr);
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
        }

        private void InferTypeUnaryExpr(AstUnaryExpr expr, CheezType expected, List<AstFunctionDecl> newInstances)
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

        private void InferTypeStructValueExpr(AstStructValueExpr expr, CheezType expected, List<AstFunctionDecl> newInstances)
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
                    InferTypeHelper(mi.Value, mem.Type, newInstances);
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
                    InferTypeHelper(mi.Value, mem.Type, newInstances);
                    ConvertLiteralTypeToDefaultType(mi.Value);

                    // TODO: check types match
                }
            }
            else
            {
                ReportError(expr, $"Either all or no values must have a name");
            }
        }

        private void InferTypesBinaryExpr(AstBinaryExpr b, CheezType expected, List<AstFunctionDecl> newInstances)
        {
            b.Left.Scope = b.Scope;
            b.Right.Scope = b.Scope;

            InferTypeHelper(b.Left, null, newInstances);
            InferTypeHelper(b.Right, null, newInstances);

            var at = new List<AbstractType>();
            if (b.Left.Type is AbstractType at1) at.Add(at1);
            if (b.Right.Type is AbstractType at2) at.Add(at2);
            if (at.Count > 0)
            {
                b.Type = new CombiType(at);
            }
            else
            {
                // convert literal types to concrete types
                if (IsLiteralType(b.Left.Type) && IsLiteralType(b.Right.Type))
                {

                }
                else if (IsLiteralType(b.Left.Type))
                {
                    b.Left.Type = UnifyTypes(b.Right.Type, b.Left.Type);
                }
                else if (IsLiteralType(b.Right.Type))
                {
                    b.Right.Type = UnifyTypes(b.Left.Type, b.Right.Type);
                }

                var ops = b.Scope.GetOperators(b.Operator, b.Left.Type, b.Right.Type);

                if (ops.Count == 0)
                {
                    ReportError(b, $"No operator matches the types {b.Left.Type} and {b.Right.Type}");
                    return;
                }
                else if (ops.Count > 1)
                {
                    // TODO: show matching operators
                    ReportError(b, $"Multiple operators match the types {b.Left.Type} and {b.Right.Type}");
                    return;
                }

                var op = ops[0];
                b.ActualOperator = op;

                if (b.Left.Value != null && b.Right.Value != null)
                    b.Value = op.Execute(b.Left.Value, b.Right.Value);

                // @hack
                b.Type = op.ResultType;
            }
        }

        private void InferTypesIdExpr(AstIdExpr i, CheezType expected)
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
            else if (sym is Using u)
            {
                // TODO:
                i.Type = u.Type;
                //throw new NotImplementedException();
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
            if (s.Suffix != null)
            {
                if (s.Suffix == "c") s.Type = CheezType.CString;
                else
                {
                    // TODO: overridable suffixes
                    ReportError(s, $"Unknown suffix '{s.Suffix}'");
                }
            }
            else if (expected == CheezType.String || expected == CheezType.CString) s.Type = expected;
            else s.Type = CheezType.StringLiteral;
        }

        private void InferTypesNumberExpr(AstNumberExpr expr, CheezType expected)
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
