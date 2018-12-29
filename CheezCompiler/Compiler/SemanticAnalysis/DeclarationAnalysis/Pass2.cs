using Cheez.Compiler.Ast;
using System.Collections.Generic;
using System.Linq;

namespace Cheez.Compiler.SemanticAnalysis.DeclarationAnalysis
{
    public partial class DeclarationAnalyzer
    {
        /// <summary>
        /// Pass 2: resolve types
        /// </summary>
        private void Pass2()
        {
            List<AstDecl> typeDeclarations = new List<AstDecl>();
            typeDeclarations.AddRange(mTypeDefs);
            typeDeclarations.AddRange(mPolyStructs);
            // typeDeclarations.AddRange(mConsts); // TODO

            List<AstDecl> waitingList = new List<AstDecl>();

            var dependencies = new Dictionary<AstDecl, HashSet<AstDecl>>();

            while (true)
            {
                bool changes = false;
                for (int i = typeDeclarations.Count - 1; i >= 0; i--)
                {
                    var decl = typeDeclarations[i];
                    typeDeclarations.RemoveAt(i);

                    HashSet<AstDecl> deps = null;
                    switch (decl)
                    {
                        case AstTypeAliasDecl alias:
                            deps = Pass2TypeAlias(alias);
                            break;

                        case AstStructDecl @struct:
                            deps = Pass2PolyStruct(@struct);
                            break;
                    }

                    if (deps.Count != 0)
                    {
                        waitingList.Add(decl);
                        dependencies.Add(decl, deps);
                    }
                }

                if (!changes || waitingList.Count == 0)
                    break;

                typeDeclarations.AddRange(waitingList);
                dependencies.Clear();
            }

            if (waitingList.Count > 0)
            {
                var details = new List<(string, ILocation)>();
                foreach (var decl in waitingList)
                {
                    if (dependencies.TryGetValue(decl, out var deps))
                    {
                        string locations = string.Join(", ", deps.Select(d => $"{d.Name.Name} ({d.Location.Beginning})"));
                        string message = $"Depends on {locations}";
                        details.Add((message, decl.Location));
                    }
                    else
                    {
                        details.Add(("Depends on other declarations", decl.Location));
                    }
                }
                var error = new Error
                {
                    Message = "Failed to compile due to cyclic dependencies in global type declarations:",
                    Details = details
                };
                mWorkspace.ReportError(error);
            }
        }

        private HashSet<AstDecl> Pass2TypeAlias(AstTypeAliasDecl alias)
        {
            var deps = new HashSet<AstDecl>();

            alias.Type = mWorkspace.ResolveType(alias.TypeExpr, deps);

            return deps;
        }

        private HashSet<AstDecl> Pass2PolyStruct(AstStructDecl @struct)
        {
            foreach (var param in @struct.Parameters)
            {
                param.TypeExpr.Scope = @struct.Scope;
                param.Type = mWorkspace.ResolveType(param.TypeExpr);

                switch (param.Type)
                {
                    case IntType _:
                    case FloatType _:
                    case CheezTypeType _:
                    case BoolType _:
                    case CharType _:
                        break;

                    case ErrorType _:
                        break;

                    default:
                        mWorkspace.ReportError(param.TypeExpr, $"The type '{param.Type}' is not allowed here.");
                        break;
                }
            }

            return new HashSet<AstDecl>();
        }
    }
}
