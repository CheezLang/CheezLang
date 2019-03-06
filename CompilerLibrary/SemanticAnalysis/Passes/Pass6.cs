using Cheez.Ast;
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
        /// function bodies and variables
        /// </summary>
        private void Pass6()
        {
            List<AstVariableDecl> varDeclarations = new List<AstVariableDecl>();
            varDeclarations.AddRange(mVariables);

            List<AstVariableDecl> waitingList = new List<AstVariableDecl>();

            var dependencies = new Dictionary<AstVariableDecl, HashSet<AstSingleVariableDecl>>();

            while (true)
            {
                waitingList.Clear();
                dependencies.Clear();

                bool processedDecls = false;
                for (int i = varDeclarations.Count - 1; i >= 0; i--)
                {
                    var decl = varDeclarations[i];
                    varDeclarations.RemoveAt(i);

                    var deps = Pass6VariableDeclaration(decl);

                    if (deps.Count != 0)
                    {
                        waitingList.Add(decl);
                        dependencies.Add(decl, deps);
                    }
                    else
                    {
                        processedDecls = true;
                    }
                }

                if (!processedDecls || waitingList.Count == 0)
                    break;

                varDeclarations.AddRange(waitingList);
            }

            if (waitingList.Count > 0)
            {
                var details = new List<(string, ILocation)>();
                foreach (var decl in waitingList)
                {
                    if (dependencies.TryGetValue(decl, out var deps))
                    {
                        string locations = string.Join("\n", deps.Select(d => $" - {d.Name.Name} ({d.Location.Beginning})"));
                        string message = $"{decl.Location.Beginning} depends on\n{locations}";
                        details.Add((message, decl.Location));
                    }
                    else
                    {
                        details.Add(("Depends on other declarations", decl.Location));
                    }
                }
                var error = new Error
                {
                    Message = "Cyclic dependencies in global variable declarations:",
                    Details = details
                };
                ReportError(error);
            }
        }

        private HashSet<AstSingleVariableDecl> Pass6VariableDeclaration(AstVariableDecl v)
        {
            if (v.TypeExpr == null && v.Initializer == null)
            {
                ReportError(v, $"A variable needs to have at least a type annotation or an initializer");
                return new HashSet<AstSingleVariableDecl>();
            }

            var deps = new HashSet<AstSingleVariableDecl>();

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

                InferType(v.Initializer, v.TypeExpr?.Type, deps, allDeps);

                if (allDeps.Count > 0)
                    v.Dependencies = new List<AstSingleVariableDecl>(allDeps);

                if (v.TypeExpr != null)
                {
                    // TODO: check if can assign
                }

                ConvertLiteralTypeToDefaultType(v.Initializer);
                var newType = v.Initializer.Type;

                v.Type = newType;
            }

            if (deps.Count == 0)
                AssignTypesAndValuesToSubdecls(v.Pattern, v.Type, v.Initializer);

            return deps;
        }

        private void AssignTypesAndValuesToSubdecls(AstExpression pattern, CheezType type, AstExpression initializer)
        {
            if (pattern is AstIdExpr id)
            {
                var decl = id.Symbol as AstSingleVariableDecl;
                decl.Type = type;
                decl.Value = initializer?.Value;

                if (decl.Initializer != null && decl.Initializer != initializer)
                    InferType(decl.Initializer, type);
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
