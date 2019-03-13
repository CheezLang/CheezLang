﻿using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Types;
using Cheez.Types.Abstract;
using Cheez.Types.Complex;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cheez
{
    /// <summary>
    /// function bodies and variables
    /// </summary>
    public partial class Workspace
    {
        /// <summary>
        /// function bodies and global variables
        /// </summary>
        private void Pass6()
        {
            var done = new HashSet<AstVariableDecl>();
            foreach (var v in mVariables)
            {
                var path = new List<AstVariableDecl>();
                Pass6VariableDeclaration(v, done, path);
            }
        }

        private void Pass6VariableDeclaration(AstVariableDecl v, HashSet<AstVariableDecl> done = null, List<AstVariableDecl> path = null)
        {
            if (path?.FirstOrDefault() == v)
            {
                var details = new List<(string, ILocation)>();

                for (int i = 0; i < path.Count; i++)
                {
                    var v1 = path[i];
                    var v2 = path[(i + 1) % path.Count];

                    details.Add(($"{v1.Pattern} depends on {v2.Pattern}", v1.Pattern));
                }

                var error = new Error
                {
                    Message = "Cyclic dependencies in global variable declarations:",
                    Details = details
                };
                ReportError(error);
            }

            path?.Add(v);


            // check for cyclic dependencies
            if (done?.Contains(v) ?? false)
            {
                path.RemoveAt(path.Count - 1);
                return;
            }
            done?.Add(v);

            if (v.Dependencies != null)
            {
                foreach (var d in v.Dependencies)
                {
                    Pass6VariableDeclaration(d.VarDeclaration, done, path);
                }
            }

            if (v.TypeExpr == null && v.Initializer == null)
            {
                ReportError(v, $"A variable needs to have at least a type annotation or an initializer");
                path?.RemoveAt(path.Count - 1);
                return;
            }

            if (v.TypeExpr != null)
            {
                 v.TypeExpr.Scope = v.Scope;
                 v.TypeExpr.Type = ResolveType(v.TypeExpr);
                 v.Type = v.TypeExpr.Type;
            }

            if (v.Initializer != null)
            {
                v.Initializer.Scope = v.Scope;

                var allDeps = new HashSet<AstSingleVariableDecl>();

                v.Initializer = InferType(v.Initializer, v.TypeExpr?.Type);

                ConvertLiteralTypeToDefaultType(v.Initializer);

                if (v.TypeExpr != null && v.Initializer.Type != v.Type && !v.Initializer.Type.IsErrorType)
                {
                    ReportError(v, $"Can't initialize a variable of type {v.Type} with a value of type {v.Initializer.Type}");
                }

                if (v.TypeExpr == null)
                    v.Type = v.Initializer.Type;
            }

            AssignTypesAndValuesToSubdecls(v.Pattern, v.Type, v.Initializer);

            path?.RemoveAt(path.Count - 1);
        }

        private void AssignTypesAndValuesToSubdecls(AstExpression pattern, CheezType type, AstExpression initializer)
        {
            if (pattern is AstIdExpr id)
            {
                var decl = id.Symbol as AstSingleVariableDecl;
                decl.Type = type;
                decl.Value = initializer?.Value;
                decl.Initializer = initializer;

                if (decl.Type == CheezType.Void)
                    ReportError(decl.Name, $"A variable can't have type void");
            }
            else if (pattern is AstTupleExpr tuple)
            {
                if (type is TupleType tupleType)
                {
                    if (tuple.Values.Count != tupleType.Members.Length)
                    {
                        ReportError(pattern, $"Pattern does not match declared type: {type}");
                        return;
                    }

                    for (int i = 0; i < tuple.Values.Count; i++)
                    {
                        var tid = tuple.Values[i];
                        var tty = tupleType.Members[i].type;

                        AstExpression tin = null;
                        if (initializer is AstTupleExpr tupleInit)
                        {
                            tin = tupleInit.Values[i];
                        }
                        else if (initializer != null)
                        {
                            var tmp = new AstTempVarExpr(initializer);
                            tmp.SetFlag(ExprFlags.IsLValue, true);
                            tin = new AstArrayAccessExpr(tmp, new AstNumberExpr(i));
                            tin.Scope = tmp.Scope;
                            tin = InferType(tin, tty);
                        }

                        AssignTypesAndValuesToSubdecls(tid, tty, tin);
                    }
                }
                else
                {
                    ReportError(pattern, $"Pattern does not match declared type: {type}");
                }
            }
        }
    }
}
