using Cheez.Ast;
using Cheez.Ast.Statements;
using Cheez.Types;
using Cheez.Types.Abstract;
using Cheez.Types.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cheez
{
    public partial class Workspace
    {
        private HashSet<AstDecl> Pass2TypeAlias(AstTypeAliasDecl alias)
        {
            var deps = new HashSet<AstDecl>();
            
            alias.TypeExpr.Scope = alias.Scope;
            alias.TypeExpr = ResolveTypeNow(alias.TypeExpr, out var newType, dependencies: deps, forceInfer: true);
            if (newType != alias.Type && !(newType is AliasType))
                alias.Type = newType;

            alias.Type = newType;
            return deps;
        }
    }
}
