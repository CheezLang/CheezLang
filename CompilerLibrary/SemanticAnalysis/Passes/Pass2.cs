using Cheez.Ast;
using Cheez.Ast.Statements;
using Cheez.Types;
using Cheez.Types.Abstract;
using Cheez.Types.Primitive;
using System.Collections.Generic;
using System.Linq;

namespace Cheez
{
    public partial class Workspace
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
                waitingList.Clear();
                dependencies.Clear();

                bool processedDecls = false;
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
                    else
                    {
                        processedDecls = true;
                    }
                }

                if (!processedDecls || waitingList.Count == 0)
                    break;

                typeDeclarations.AddRange(waitingList);
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
                    Message = "Cyclic dependencies in global type declarations:",
                    Details = details
                };
                ReportError(error);
            }
        }

        private HashSet<AstDecl> Pass2TypeAlias(AstTypeAliasDecl alias)
        {
            var deps = new HashSet<AstDecl>();
            
            var newType = ResolveTypeHelper(alias.TypeExpr, deps);
            if (newType != alias.Type && !(newType is AliasType))
            {
                alias.Type = newType;
                alias.Scope.ChangeTypeOfDeclaration(alias);
            }

            alias.Type = newType;
            return deps;
        }

        private HashSet<AstDecl> Pass2PolyStruct(AstStructDecl @struct)
        {
            var deps = new HashSet<AstDecl>();

            foreach (var param in @struct.Parameters)
            {
                param.TypeExpr.Scope = @struct.Scope;
                var newType = ResolveTypeHelper(param.TypeExpr, deps);

                if (newType is AbstractType)
                {
                    continue;
                }

                param.Type = newType;

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
                        ReportError(param.TypeExpr, $"The type '{param.Type}' is not allowed here.");
                        break;
                }
            }

            return deps;
        }
    }
}
