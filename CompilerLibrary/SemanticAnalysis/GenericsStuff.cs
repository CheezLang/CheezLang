using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Types;
using Cheez.Types.Abstract;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Cheez
{
    public partial class Workspace
    {
        private AstExpression InferTypeGenericExpr(AstGenericExpr expr, CheezType expected, TypeInferenceContext context)
        {
            foreach (var param in expr.Parameters)
            {
                param.Scope = expr.Scope;
                param.TypeExpr.AttachTo(expr);
                param.TypeExpr = InferTypeHelper(param.TypeExpr, null, context);
                param.Type = param.TypeExpr.Value as CheezType;
                if (!ValidatePolymorphicParameterType(param.Location, param.Type))
                    return expr;
            }

            expr.Type = new GenericType(expr);
            expr.Value = new PolyValue("generic");
            return expr;
        }

        private AstExpression CallGenericExpr(AstGenericExpr expr, List<(int index, CheezType type, object value)> args, ILocation location = null)
        {
            // check if an instance already exists
            foreach (var inst in expr.PolymorphicInstances)
            {
                if (inst.args.Count != args.Count)
                    continue;

                bool allArgsMatch = true;
                for (int i = 0; i < inst.args.Count; i++)
                {
                    var a = inst.args[i];
                    var b = args[i];

                    if (a.index != b.index || a.type != b.type || a.value != b.value)
                    {
                        allArgsMatch = false;
                        break;
                    }
                }

                if (allArgsMatch)
                    return inst.expr;
            }

            // create new instance
            var instance = expr.SubExpression.Clone();
            instance.Replace(expr, new Scope("+", expr.Scope));

            foreach (var arg in args)
            {
                var param = expr.Parameters[arg.index];
                instance.Scope.DefineConstant(param.Name.Name, arg.type, arg.value);
            }

            instance = InferType(instance, null);
            expr.PolymorphicInstances.Add((args, instance));

            return instance;
        }

        private AstExpression InferTypeGenericCallExpr(GenericType genType, AstArrayAccessExpr expr, CheezType expected, TypeInferenceContext context)
        {
            var generic = genType.Expression;
            var args = expr.Arguments.Select(a => new AstArgument(a, Location: a.Location)).ToList();
            if (!CheckAndMatchArgsToParams(args, generic.Parameters, false))
                return expr;

            foreach (var arg in args)
            {
                var param = arg.Index < generic.Parameters.Count ? generic.Parameters[arg.Index] : null;
                var paramType = param?.Type;

                arg.Expr.SetFlag(ExprFlags.ValueRequired, true);
                arg.AttachTo(expr);

                arg.Expr.Parent = arg;

                // if the expression already has a scope it is because it is a default value
                if (!arg.IsDefaultArg)
                    arg.Expr.Scope = expr.Scope;

                arg.Expr = InferTypeHelper(arg.Expr, paramType, context);
                arg.Value = arg.Expr.Value;

                ConvertLiteralTypeToDefaultType(arg.Expr, paramType);
                arg.Type = arg.Expr.Type;

                if (arg.Type.IsErrorType)
                    continue;

                //arg.Expr = HandleReference(arg.Expr, paramType, context);
                arg.Expr = CheckType(arg.Expr, paramType, $"Type of argument ({arg.Expr.Type}) does not match type of parameter ({paramType})");
                arg.Type = arg.Expr.Type;
            }

            // :hack
            var argsArray = args.Select(a => (a.Index, a.Type, a.Value)).ToList();
            var instance = CallGenericExpr(generic, argsArray, expr.Location);
            return instance;
        }
    }
}
