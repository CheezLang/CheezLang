using Cheez.Compiler.Ast;

namespace Cheez.Compiler.SemanticAnalysis.DeclarationAnalysis
{
    public partial class DeclarationAnalyzer
    {
        /// <summary>
        /// pass 3: rest of typedefs
        /// </summary>
        private void Pass3()
        {
            // typedefs
            foreach (var t in mTypeDefs)
            {
                if (t.Type == null)
                {
                    Pass3TypeAlias(t);
                }
            }
        }

        private void Pass3TypeAlias(AstTypeAliasDecl alias)
        {
            alias.Type = mWorkspace.ResolveType(alias.TypeExpr);

            var res = alias.Scope.DefineTypeSymbolNew(alias.Name.Name, alias.Type, alias);
            if (!res.ok)
            {
                (string, ILocation)? detail = null;
                if (res.other != null) detail = ("Other declaration here:", res.other);
                mWorkspace.ReportError(alias.Name, $"A symbol with name '{alias.Name.Name}' already exists in current scope", detail);
            }
        }
    }
}
